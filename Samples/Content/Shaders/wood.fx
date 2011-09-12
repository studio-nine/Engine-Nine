/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/wood.fx#1 $

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
	Similar to "RenderMan Companion" wood shader

******************************************************************************/

#define PROC_NOISE

/************* UN-TWEAKABLES **************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=wood;";
> = 0.8;

float4x4 WorldIT : WorldInverseTranspose;
float4x4 WorldViewProj : WorldViewProjection;
float4x4 World : World;
float4x4 ViewI : ViewInverse;

/************************************************************/
/*** tweakables *********************************************/
/************************************************************/

float3 LightPos : Position
<
    string Object = "PointLight";
    string Space = "World";
> = {-10.0f, 10.0f, -10.0f};

float3 LightColor
<
    string UIName =  "Lamp Color";
    string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f};

////

float3 AmbiColor : Ambient
<
    string UIName =  "Ambient Light Color";
    string UIWidget = "Color";
> = {0.17f, 0.17f, 0.17f};

////

float3 WoodColor1 <
    string UIName =  "Surface Color";
    string UIWidget = "Color";
> = {0.85f, 0.55f, 0.01f};

float3 WoodColor2 <
    string UIName =  "Surface Color";
    string UIWidget = "Color";
> = {0.60f, 0.41f, 0.0f};

float Ks <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 2.0;
    float UIStep = 0.01;
    string UIName =  "specular";
> = 0.6;

float SpecExpon : SpecularPower <
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 128.0;
    float UIStep = 1.0;
    string UIName =  "specular power";
> = 12.0;

float RingScale <
    string units = "inch";
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 10.0;
    float UIStep = 0.01;
    string UIName =  "Ring Scale";
> = 0.46;

float AmpScale <
    string UIWidget = "slider";
    float UIMin = 0.01;
    float UIMax = 2.0;
    float UIStep = 0.01;
    string UIName = "Wobbliness";
> = 0.7;

float PScale <
    string UIWidget = "slider";
    float UIMin = 0.01;
    float UIMax = 10.0;
    float UIStep = 0.01;
    string UIName =  "Log Scale";
> = 8;

float3 POffset <
    string UIName =  "Log Offset";
> = {-10.0f, -11.0f, 7.0f};

/////////////

#ifdef PROC_NOISE
#define NOISE_SCALE 40.0
#include "noise_3d.fxh"
#else /* !PROC_NOISE */

texture NoiseTex
<
    string ResourceName = "noiseL8_32x32x32.dds";
    string ResourceType = "3D";
>;

sampler3D NoiseSamp = sampler_state
{
	Texture = <NoiseTex>;
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = None;
};
#endif /* !PROC_NOISE */

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
    float3 WoodPos	: TEXCOORD3;
    float3 WorldView	: TEXCOORD4;
    float3 WorldTangent	: TEXCOORD5;
    float3 WorldBinorm	: TEXCOORD6;
    float4 ObjPos	: TEXCOORD7;
};

/*********** vertex shader ******/

vertexOutput mainVS(appdata IN)
{
    vertexOutput OUT;
    OUT.WorldNormal = mul(IN.Normal, WorldIT).xyz;
    OUT.WorldTangent = mul(IN.Tangent, WorldIT).xyz;
    OUT.WorldBinorm = mul(IN.Binormal, WorldIT).xyz;
    float4 Po = float4(IN.Position.xyz,1);
    float3 Pw = mul(Po, World).xyz;
    OUT.WoodPos = (PScale*Po) + POffset;
    OUT.LightVec = LightPos - Pw;
    OUT.TexCoord = IN.UV;
    OUT.WorldView = (ViewI[3].xyz - Pw);
    float4 hpos = mul(Po, WorldViewProj);
    OUT.ObjPos = Po;
    OUT.HPosition = hpos;
    return OUT;
}

/********* pixel shader ********/

float4 woodPS(vertexOutput IN) : COLOR
{
    float3 Ln = normalize(IN.LightVec);
    float3 Nn = normalize(IN.WorldNormal);
    float3 Pwood = IN.WoodPos + (AmpScale * tex3D(NoiseSamp,IN.WoodPos.xyz/32.0).xyz);
    float r = RingScale * sqrt(dot(Pwood.yz,Pwood.yz));
    r = r + tex3D(NoiseSamp,r.xxx/32.0).x;
    r = r - floor(r);
    r = smoothstep(0.0, 0.8, r) - smoothstep(0.83,1.0,r);
    float3 dColor = lerp(WoodColor1,WoodColor2,r);
    float3 Vn = normalize(IN.WorldView);
    float3 Hn = normalize(Vn + Ln);
    float hdn = dot(Hn,Nn);
    float ldn = dot(Ln,Nn);
    float4 litV = lit(ldn,hdn,SpecExpon);
    float3 diffContrib = dColor * ((litV.y*LightColor) + AmbiColor);
    float3 specContrib = Ks * litV.z * LightColor;
    float3 result = diffContrib + specContrib;
    return float4(result,1);
}

/*************/

technique wood <
	string Script = "Pass=p0;";
> {
	pass p0 <
	string Script = "Draw=geometry;";
> {		
		VertexShader = compile vs_2_0 mainVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_2_0 woodPS();
	}
}

/***************************** eof ***/
