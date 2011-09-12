/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/blobCT.fx#1 $

Copyright NVIDIA Corporation 2002
TO THE MAXIMUM EXTENT PERMITTED BY APPLICABLE LAW, THIS SOFTWARE IS PROVIDED
*AS IS* AND NVIDIA AND ITS SUPPLIERS DISCLAIM ALL WARRANTIES, EITHER EXPRESS
OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, IMPLIED WARRANTIES OF MERCHANTABILITY
AND FITNESS FOR A PARTICULAR PURPOSE.  IN NO EVENT SHALL NVIDIA OR ITS SUPPLIERS
BE LIABLE FOR ANY SPECIAL, INCIDENTAL, INDIRECT, OR CONSEQUENTIAL DAMAGES
WHATSOEVER (INCLUDING, WITHOUT LIMITATION, DAMAGES FOR LOSS OF BUSINESS PROFITS,
BUSINESS INTERRUPTION, LOSS OF BUSINESS INFORMATION, OR ANY OTHER PECUNIARY LOSS)
ARISING OUT OF THE USE OF OR INABILITY TO USE THIS SOFTWARE, EVEN IF NVIDIA HAS
BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.


Comments:
    Simple vertex animation on a plastic-like surface.
	The highlight is done using multitexture, looks better considering the
	unpredictable nature of disturbed polys.
    Do not let your kids play with this shader, you will not get your
		computer back for a while.
	Only needs DX8 hardware

******************************************************************************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=ps11;";
> = 0.8;

/************* TWEAKABLES **************/

float4x4 WorldIT : WorldInverseTranspose < string UIWidget="None"; >;
float4x4 wvp : WorldViewProjection < string UIWidget="None"; >;
float4x4 World : World < string UIWidget="None"; >;
float4x4 WorldI : WorldInverse < string UIWidget="None"; >;
float4x4 ViewInv : ViewInverse < string UIWidget="None"; >;

// float ticks : Time < string UIWidget="None"; >;

/////////////////////////////////////// tweakables 

float4 LightPos : Position
<
    string Object = "PointLight";
    string Space = "World";
> = {100.0f, 100.0f, -100.0f, 0.0f};

float4 LightColor <
    string UIName =  "Light Color";
	string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f, 1.0f};

float4 AmbiColor : Ambient
<
    string UIName =  "Ambient Light Color";
> = {0.2f, 0.2f, 0.2f, 1.0f};

float4 SurfColor : Diffuse
<
    string UIName =  "Surface Color";
    string UIWidget = "Color";
> = {0.1f, 0.5f, 0.4f, 1.0f};

//////////

float4 BlobCenter1 : Position
<
    string Space = "World";
    string Object = "PointLight"; // hack to let us pose this with FXComposer
> = {4.0f, 2.0f, 3.0f, 1.0f};

float BlobRadius1
<
    string UIWidget = "slider";
    float UIMin = 0.1;
    float UIMax = 10.0;
    float UIStep = 0.1;
> = 2.0;

float BlobGradient1
<
    string UIWidget = "slider";
    float UIMin = 0.1;
    float UIMax = 10.0;
    float UIStep = 0.1;
> = 1.0;

float BlobStrength1
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.01;
> = 1.0;

//////////////////////////////////////

texture halfAngleMap : SPECULAR
<
    string ResourceName = "ctHalf.dds";
    string ResourceType = "2D";
    string UIName =  "Map with dot-half-angle factors";
>;

sampler2D halfAngleSampler = sampler_state
{
	Texture = <halfAngleMap>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = None;
	AddressU = Clamp;
	AddressV = Clamp;
};

///////

texture normalMap : NORMAL
<
    string ResourceName = "ctNorm.dds";
    string ResourceType = "2D";
    string UIName =  "Map with dot-normal factors";
>;

sampler2D normalSampler = sampler_state
{
	Texture = <normalMap>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = None;
	AddressU = Clamp;
	AddressV = Clamp;
};

/************* DATA STRUCTS **************/

/* data from application vertex buffer */
struct appdata {
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;
    float4 Normal	: NORMAL;
};

/* data passed from vertex shader to pixel shader */
struct vertexOutput {
    float4 HPosition	: POSITION;
    float4 TexCoord0	: TEXCOORD0;
    float4 TexCoord1	: TEXCOORD1;
    float4 diffCol		: COLOR0;
};

/*********** vertex shader ******/

vertexOutput theBlobVS(appdata IN) {
    vertexOutput OUT;
    float3 worldNormal = mul(WorldIT, IN.Normal).xyz;
    worldNormal = normalize(worldNormal);
    float4 objSpacePos = float4(IN.Position.x,IN.Position.y,IN.Position.z,1.0);
    float3 worldSpacePos = mul(objSpacePos, World).xyz;

    // now apply blob
    float3 delta = worldSpacePos - BlobCenter1.xyz;
    float d = sqrt(dot(delta,delta));
    float g1 = min(BlobRadius1,BlobGradient1);
    float ramp = BlobStrength1*(1.0 - smoothstep((BlobRadius1-g1),BlobRadius1,d));
    delta = delta / d;  // in other words, now normalized
    float3 targetPos = BlobCenter1.xyz + BlobRadius1 * delta;
    worldSpacePos = lerp(worldSpacePos,targetPos,ramp);
    float ddw = dot(delta,worldNormal);
    if (ddw<0.0) { delta = -delta; }
    worldNormal = lerp(worldNormal,delta,ramp);

    // now update obj-space pos for later stuff
    float4 temp = float4(worldSpacePos.x,worldSpacePos.y,worldSpacePos.z,1.0);
    objSpacePos = mul(temp, WorldI);

    float3 LightVec = normalize(LightPos - worldSpacePos);
    float ldn = dot(LightVec,worldNormal);
    float diffComp = max(0,ldn);
    float4 diffContrib = SurfColor * ( diffComp * LightColor + AmbiColor);

    OUT.diffCol = diffContrib;
    OUT.diffCol.w = 1.0;
    // OUT.TexCoord2 = IN.UV;

    float3 EyePos = ViewInv[3].xyz;
    float3 vertToEye = normalize(EyePos - worldSpacePos);
    float3 halfAngle = normalize(vertToEye + LightVec);
    float4 halfIndices = float4(0.5+dot(LightVec,halfAngle)/2.0,
			   0.5+dot(worldNormal,halfAngle)/2.0,0.0,1.0);
    float4 normIndices = float4(0.5+dot(LightVec,worldNormal)/2.0,
			   0.5+dot(worldNormal,vertToEye)/2.0,0.0,1.0);
    OUT.TexCoord0 = halfIndices;
    OUT.TexCoord1 = normIndices;

    // transform into homogeneous-clip space
    OUT.HPosition = mul(objSpacePos, wvp);
    return OUT;
}

/********* pixel shader ********/

float4 theBlobCTPS(vertexOutput IN) : COLOR {
    float4 nspec = tex2D(halfAngleSampler,IN.TexCoord0.xy) * 
				   tex2D(normalSampler,IN.TexCoord1.xy) * LightColor;
    float4 result = IN.diffCol + nspec;
    return result;
}

/////////////////////////////// technique

technique ps11 <
	string Script = "Pass=p0;";
> {
	pass p0  <
		string Script = "Draw=geometry;";
	> {		
		VertexShader = compile vs_1_1 theBlobVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_1_1 theBlobCTPS();
	}
}

/***************************** eof ***/
