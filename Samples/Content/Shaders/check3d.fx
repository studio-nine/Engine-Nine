/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/check3d.fx#1 $

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
	3D Checker showing anti-aliasing using ddx/ddy
		-- this result is PURELY numeric, so slower than texture-based AA
	$Date: 2004/09/24 $

******************************************************************************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=ps2_numeric;";
> = 0.8;

/************* TWEAKABLES **************/

float4x4 WorldIT : WorldInverseTranspose < string UIWidget="None"; >;
float4x4 WorldViewProj : WorldViewProjection < string UIWidget="None"; >;
float4x4 World : World < string UIWidget="None"; >;
float4x4 ViewInv : ViewInverse < string UIWidget="None"; >;

/************************************************************/

float4 LightPos : Position
<
    string Object = "PointLight";
    string Space = "World";
> = {100.0f, 100.0f, 100.0f, 0.0f};

float4 LightColor
 <
    string UIName =  "Light Color";
    string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f, 1.0f};

float4 AmbiColor : Ambient
<
    string UIName =  "Ambient Light Color";
> = {0.17f, 0.17f, 0.17f, 1.0f};

float4 SurfColor1 : Diffuse
<
    string UIName =  "Surface Color";
    string UIWidget = "Color";
> = {1.0f, 0.4f, 0.0f, 1.0f};

float4 SurfColor2 : Diffuse
<
    string UIName =  "Surface Color";
    string UIWidget = "Color";
> = {0.0f, 0.2f, 1.0f, 1.0f};

float Ks
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 2.0;
    float UIStep = 0.01;
    string UIName =  "specular";
> = 0.5;

float SpecExpon : SpecularPower
<
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 128.0;
    float UIStep = 1.0;
    string UIName =  "specular power";
> = 25.0;

float SWidth
<
    string UIWidget = "slider";
    float UIMin = 0.001;
    float UIMax = 10.0;
    float UIStep = 0.001;
    string UIName =  "filter width";
> = 1.0;

float Scale
<
    string units = "inches";
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 5.0;
    float UIStep = 0.01;
    string UIName =  "scale of pattern";
> = 0.5;

float Balance
<
    string UIWidget = "slider";
    float UIMin = 0.01;
    float UIMax = 0.99;
    float UIStep = 0.01;
    string UIName =  "clip";
> = 0.5;

/************* DATA STRUCTS **************/

/* data from application vertex buffer */
struct appdata {
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;
    float4 Normal	: NORMAL;
    float4 Tangent	: TANGENT0;
    float4 Binormal	: BINORMAL0;
};

/* data passed from vertex shader to pixel shader */
struct vertexOutput {
    float4 HPosition	: POSITION;
    float4 TexCoord	: TEXCOORD0;
    float3 LightVec	: TEXCOORD1;
    float3 WorldNormal	: TEXCOORD2;
    float3 WorldEyeVec	: TEXCOORD3;
    float3 WorldTangent	: TEXCOORD4;
    float3 WorldBinorm	: TEXCOORD5;
    float4 ObjPos	: TEXCOORD6;
};

/*********** vertex shader ******/

vertexOutput mainVS(appdata IN) {
    vertexOutput OUT;
    OUT.WorldNormal = mul(IN.Normal, WorldIT).xyz;
    OUT.WorldTangent = mul(IN.Tangent, WorldIT).xyz;
    OUT.WorldBinorm = mul(IN.Binormal, WorldIT).xyz;
    float4 Po = float4(IN.Position.x,IN.Position.y,IN.Position.z,1.0);
    float3 Pw = mul(Po, World).xyz;
    OUT.LightVec = LightPos - Pw;
    OUT.TexCoord = IN.UV;
    OUT.WorldEyeVec = normalize(ViewInv[3].xyz - Pw);
    float4 hpos = mul(Po, WorldViewProj);
    OUT.ObjPos = Po;
    OUT.HPosition = hpos;
    return OUT;
}

/********* pixel shader ********/

// PS with box-filtered step function
float4 checkerPS(vertexOutput IN) : COLOR {
    float3 Ln = normalize(IN.LightVec);
    float3 Nn = normalize(IN.WorldNormal);
    float edge = Scale*Balance;
    float op = SWidth/Scale;

    // x stripes
    float width = abs(ddx(IN.ObjPos.x)) + abs(ddy(IN.ObjPos.x));
    float w = width*op;
    float x0 = IN.ObjPos.x/Scale - (w/2.0);
    float x1 = x0 + w;
    float nedge = edge/Scale;
    float i0 = (1.0-nedge)*floor(x0) + max(0.0, frac(x0)-nedge);
    float i1 = (1.0-nedge)*floor(x1) + max(0.0, frac(x1)-nedge);
    float check = (i1 - i0)/w;
    check = min(1.0,max(0.0,check));

    // y stripes
    width = abs(ddx(IN.ObjPos.y)) + abs(ddy(IN.ObjPos.y));
    w = width*op;
    x0 = IN.ObjPos.y/Scale - (w/2.0);
    x1 = x0 + w;
    nedge = edge/Scale;
    i0 = (1.0-nedge)*floor(x0) + max(0.0, frac(x0)-nedge);
    i1 = (1.0-nedge)*floor(x1) + max(0.0, frac(x1)-nedge);
    float s = (i1 - i0)/w;
    check = abs(check - min(1.0,max(0.0,s)));

    // z stripes
    width = abs(ddx(IN.ObjPos.z)) + abs(ddy(IN.ObjPos.z));
    w = width*op;
    x0 = IN.ObjPos.z/Scale - (w/2.0);
    x1 = x0 + w;
    nedge = edge/Scale;
    i0 = (1.0-nedge)*floor(x0) + max(0.0, frac(x0)-nedge);
    i1 = (1.0-nedge)*floor(x1) + max(0.0, frac(x1)-nedge);
    s = (i1 - i0)/w;
    check = abs(check - min(1.0,max(0.0,s)));

    float4 dColor = lerp(SurfColor1,SurfColor2,check);
    float3 Vn = normalize(IN.WorldEyeVec);
    float3 Hn = normalize(Vn + Ln);
    float4 lighting = lit(dot(Ln,Nn),dot(Hn,Nn),SpecExpon);
    float hdn = lighting.z; // Specular coefficient
    float ldn = lighting.y; // Diffuse coefficient
    float4 diffContrib = dColor * (ldn*LightColor + AmbiColor);
    float4 specContrib = hdn * LightColor;
    float4 result = diffContrib + (Ks * specContrib);
    return result;
}

/*************/

technique ps2_numeric <
	string Script = "Pass=p0;";
> {
	pass p0  <
		string Script = "Draw=geometry;";
	> {		
		VertexShader = compile vs_2_0 mainVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_3_0 checkerPS();
	}
}

/***************************** eof ***/
