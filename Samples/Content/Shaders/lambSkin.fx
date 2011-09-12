/*********************************************************************NVMH3****
File:  $Id: //sw/devrel/SDK/MEDIA/HLSL/lambSkin_ps11_tex.fx#15 $

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
    A lambertian-like surface with light "bleed-through" -- appropriate
    for soft translucent materials like skin. The "subColor" represents
    the tinting acquired by light diffused below the surface.
	Set the "rollOff" angle to the cosine of the angle used for
    additional lighting "wraparound" -- the diffuse effect propogates based
    on the angle of LightDirection versus SurfaceNormal.
    	Versions are provided for shading in pixel or vertex shaders,
    textured or untextured.

******************************************************************************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?UntexturedPS:TexturedPS:UntexturedVS:TexturedVS;";
> = 0.8;

/************* UN-TWEAKABLES **************/

float4x4 WorldIT : WorldInverseTranspose < string UIWidget="None"; >;
float4x4 WorldViewProj : WorldViewProjection < string UIWidget="None"; >;
float4x4 World : World < string UIWidget="None"; >;

/************* TWEAKABLES *****************/

float4 LightPos : POSITION
<
	string Object = "PointLight";
    string UIName =  "Lamp Position";
	string Space = "World";
> = {1403.0f, 1441.0f, 1690.0f, 0.0f};

float4 AmbiColor : AMBIENT <
	string UIWidget = "Color";
    string UIName =  "Ambient";
>  = {0.1f, 0.1f, 0.1f, 1.0f};

float4 DiffColor : DIFFUSE <
	string UIWidget = "Color";
    string UIName =  "Surface Diffuse";
>  = {0.9f, 1.0f, 0.9f, 1.0f};

float4 SubColor <
	string UIWidget = "Color";
    string UIName =  "Subsurface \"Bleed-thru\" Color";
>  = {1.0f, 0.2f, 0.2f, 1.0f};

float RollOff
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 0.99;
    float UIStep = 0.01;
    string UIName =  "Subsurface Rolloff Range";
> = 0.2;

/////////////// texture /////////////////

texture ColorTexture : DIFFUSE <
	string ResourceName = "default_color.dds";
    string UIName =  "Color Texture (if used)";
	string ResourceType = "2D";
>;

sampler2D ColorSampler = sampler_state
{
	Texture = <ColorTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = WRAP;
	AddressV = WRAP;
};
/************* DATA STRUCTS **************/

/* data from application vertex buffer */
struct appdata {
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;
    float4 Normal	: NORMAL;
};

/* data passed from vertex shader to pixel shader */
struct shadedVertexOutput {
    float4 HPosition	: POSITION;
    float4 TexCoord	: TEXCOORD0;
    float4 diffCol	: COLOR0;
};

/* data passed from vertex shader to pixel shader */
struct vertexOutput {
    half4 HPosition	: POSITION;
    half4 TexCoord		: TEXCOORD0;
    half3 LightVec		: TEXCOORD1;
    half3 WorldNormal	: TEXCOORD2;
};

///////////////////////////////////////////////
// Shared "lambskin" diffuse function /////////
///////////////////////////////////////////////

//
// vectors are assumed to be normalized as needed
//
void lambskin(float3 N,
			  float3 L,
			  out float4 Diffuse,
			  out float4 Subsurface
) {
    float ldn = dot(L,N);
    float diffComp = max(0,ldn);
    Diffuse = float4((diffComp * DiffColor).xyz,1);
    float subLamb = smoothstep(-RollOff,1.0,ldn) - smoothstep(0.0,1.0,ldn);
    subLamb = max(0.0,subLamb);
    Subsurface = subLamb * SubColor;
}

/*********** vertex shader ******/

shadedVertexOutput lambVS(appdata IN) {
    shadedVertexOutput OUT;
    float3 Nn = normalize(mul(IN.Normal, WorldIT).xyz);
    float4 Po = float4(IN.Position.xyz,1);
    float3 Pw = mul(Po, World).xyz;
    float3 Ln = normalize(LightPos - Pw);
    float4 diffContrib;
    float4 subContrib;
	lambskin(Nn,Ln,diffContrib,subContrib);
    OUT.diffCol = diffContrib + AmbiColor + subContrib;
    OUT.TexCoord = IN.UV;
    OUT.HPosition = mul(Po, WorldViewProj);
    return OUT;
}

vertexOutput simpleVS(appdata IN) {
    vertexOutput OUT;
    half4 normal = normalize(IN.Normal);
    OUT.WorldNormal = mul(normal, WorldIT).xyz;
    half4 Po = half4(IN.Position.xyz,1);
    half3 Pw = mul(Po, World).xyz;
    OUT.LightVec = normalize(LightPos - Pw);
    OUT.TexCoord = IN.UV;
    OUT.HPosition = mul(Po, WorldViewProj);
    return OUT;
}

/********* pixel shader ********/

void lamb_ps_shared(vertexOutput IN,
			out float4 DiffuseContrib,
			out float4 SubContrib)
{
    half3 Ln = normalize(IN.LightVec);
    half3 Nn = normalize(IN.WorldNormal);
	lambskin(Nn,Ln,DiffuseContrib,SubContrib);
}

float4 lambPS(vertexOutput IN) : COLOR {
	float4 diffContrib;
	float4 subContrib;
	lamb_ps_shared(IN,diffContrib,subContrib);
	float4 litC = diffContrib + AmbiColor + subContrib;
	return litC;
}

float4 lambPS_t(vertexOutput IN) : COLOR {
	float4 diffContrib;
	float4 subContrib;
	lamb_ps_shared(IN,diffContrib,subContrib);
	float4 litC = diffContrib + AmbiColor + subContrib;
	return (litC*tex2D(ColorSampler,IN.TexCoord.xy));
}

/*************/

technique UntexturedVS <
	string Script = "Pass=p0;";
> {
	pass p0 <
	string Script = "Draw=geometry;";
> {		
		VertexShader = compile vs_1_1 lambVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		// no pixel shader needed
		SpecularEnable = false;
		ColorArg1[ 0 ] = Diffuse;
		ColorOp[ 0 ]   = SelectArg1;
		ColorArg2[ 0 ] = Specular;
		AlphaArg1[ 0 ] = Diffuse;
		AlphaOp[ 0 ]   = SelectArg1;
	}
}

technique TexturedVS <
	string Script = "Pass=p0;";
> {
	pass p0 <
	string Script = "Draw=geometry;";
> {		
		VertexShader = compile vs_1_1 lambVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		// no pixel shader needed
		SpecularEnable = false;
	    Texture[0] = <ColorTexture>;
	    MinFilter[0] = Linear;
	    MagFilter[0] = Linear;
	    MipFilter[0] = None;
        ColorArg1[ 0 ] = Texture;
        ColorOp[ 0 ] = Modulate;
        ColorArg2[ 0 ] = Diffuse;
	}
}

technique UntexturedPS <
	string Script = "Pass=p0;";
> {
	pass p0 <
	string Script = "Draw=geometry;";
> {		
		VertexShader = compile vs_1_1 simpleVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_2_0 lambPS();
	}
}

technique TexturedPS <
	string Script = "Pass=p0;";
> {
	pass p0 <
	string Script = "Draw=geometry;";
> {		
		VertexShader = compile vs_1_1 simpleVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_2_0 lambPS_t();
	}
}

/***************************** eof ***/
