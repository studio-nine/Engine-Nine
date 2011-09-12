/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/Nightfall.fx#1 $

Copyright NVIDIA Corporation 2004
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
	Day/Night Earth Shader

******************************************************************************/

// Compile-time flags
// feature flags

// performance flags
//#define USE_NORMALIZATION_CUBEMAP

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Main;";
> = 0.8;

/************* "UN-TWEAKABLES," TRACKED BY CPU APPLICATION **************/

float4x4 WorldITXf : WorldInverseTranspose <string UIWidget="None";>;
float4x4 WvpXf : WorldViewProjection <string UIWidget="None";>;
float4x4 WorldXf : World <string UIWidget="None";>;
float4x4 ViewIXf : ViewInverse <string UIWidget="None";>;

float Timer : TIME <string UIWidget="None";>;

///////////////////////////////////////////////////////////////
/// TWEAKABLES ////////////////////////////////////////////////
///////////////////////////////////////////////////////////////

////////////////////////////////////////////// light

float3 LightDir : DIRECTION <
	string UIName = "Sun Direction";
	string Object = "DirectionalLight";
	string Space = "World";
> = {-1.0f, -1.0f, -0.2f};

float3 LightColor : SPECULAR <
	string UIName = "Sun";
	string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f};

////////////////////////////////////////////// ambient light

float3 AmbiLightColor : Ambient
<
    string UIName = "Ambient";
	string UIWidget = "Color";
> = {0.07f, 0.07f, 0.07f};

////////////////////////////////////////////// surface

float Speed
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1;
    float UIStep = 0.01;
    string UIName = "Rotation";
> = 0.2;

float Kd
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.5;
    float UIStep = 0.01;
    string UIName = "Diffuse";
> = 1.0;

float Ks
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.5;
    float UIStep = 0.01;
    string UIName = "Specular";
> = 0.85;


float SpecExpon : SpecularPower
<
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 128.0;
    float UIStep = 1.0;
    string UIName = "Specular power";
> = 32.0;

float Bumpy
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 10.0;
    float UIStep = 0.1;
    string UIName = "Bump Height";
> = 1.0;

////////////////////////////////////////////////////////
/// TEXTURES ///////////////////////////////////////////
////////////////////////////////////////////////////////

texture DayTexture : DIFFUSE
<
    string ResourceName = "EarthDay.dds";
    string ResourceType = "2D";
>;

sampler2D DaySampler = sampler_state
{
	Texture = <DayTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

texture NightTexture : DIFFUSE
<
    string ResourceName = "EarthMoonLit.dds";
    string ResourceType = "2D";
>;

sampler2D NightSampler = sampler_state
{
	Texture = <NightTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

texture NormalTexture : NORMAL
<
    string ResourceName = "earth_bump.dds";
    string ResourceType = "2D";
>;

sampler2D NormalSampler = sampler_state
{
	Texture = <NormalTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

//////////////

texture GlossTexture : SPECULAR
<
    string ResourceName = "EarthSpec.dds";
    string ResourceType = "2D";
>;

sampler2D GlossSampler = sampler_state
{
	Texture = <GlossTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

//////////////

#ifdef USE_NORMALIZATION_CUBEMAP
#include "normalize.fxh"
// only for pixel shaders....
#define NORM my_normalize
#else /* !USE_NORMALIZATION_CUBEMAP */
#define NORM normalize
#endif /* !USE_NORMALIZATION_CUBEMAP */

/*********************************************************/
/************* DATA STRUCTS ******************************/
/*********************************************************/

/* data from application vertex buffer */
struct appdata {
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;
    float4 Normal	: NORMAL;
    float4 Tangent	: TANGENT0;
    float4 Binormal	: BINORMAL0;
};

struct vertexOutput {
    float4 HPosition	: POSITION;
    float2 UV			: TEXCOORD0;
    float3 WorldNormal	: TEXCOORD1;
    float3 WorldView	: TEXCOORD2;
    float3 WorldTangent	: TEXCOORD3;
    float3 WorldBinorm	: TEXCOORD4;
};

/*********************************************************/
/*********** Virtual Machine *****************************/
/*********************************************************/

#include "nvMatrix.fxh"

float4x4 planetMat()
{
	return(nvYRotateXf(Timer*Speed));
}

float4x4 planetMatN()
{
	float4x4 R = (nvYRotateXf(Timer*Speed));
	return mul(R,WorldITXf);
}

/*********************************************************/
/*********** vertex shader *******************************/
/*********************************************************/

vertexOutput basicVS(appdata IN,uniform float4x4 rotMat,uniform float4x4 rotMatN) {
    vertexOutput OUT;
    OUT.WorldNormal = mul(IN.Normal,rotMatN).xyz;
    OUT.WorldTangent = mul(IN.Tangent,rotMatN).xyz;
    OUT.WorldBinorm = mul(IN.Binormal,rotMatN).xyz;
    float4 Po = float4(IN.Position.xyz,1.0);	// object coordinates
    Po = mul(Po,rotMat);
    float3 Pw = mul(Po,WorldXf).xyz;		// world coordinates
    OUT.UV = float2(IN.UV.x,1-IN.UV.y);
    OUT.WorldView = normalize(ViewIXf[3].xyz - Pw);	// obj coords
    OUT.HPosition = mul(Po,WvpXf);	// screen clipspace coords
    return OUT;
}

/*********************************************************/
/*********** pixel shader ********************************/
/*********************************************************/

float4 everythingPS(vertexOutput IN) : COLOR {
    float3 Nn = NORM(IN.WorldNormal);
    float3 Tn = NORM(IN.WorldTangent);
    float3 Bn = NORM(IN.WorldBinorm);
    float3 bumps = Bumpy * (tex2D(NormalSampler,IN.UV).xyz-(0.5).xxx);
    Nn += (bumps.x * Tn + bumps.y * Bn);
    Nn = NORM(Nn);
    float3 Vn = NORM(IN.WorldView);
    float3 Ln = NORM(-LightDir);	// NORM() required?
    float3 Hn = NORM(Vn + Ln);
    float hdn = dot(Hn,Nn);
    float ldn = dot(Ln,Nn);
    float4 litVec = lit(ldn,hdn,SpecExpon);
    float3 diffContrib = litVec.y * LightColor;
    float gloss = Ks * tex2D(GlossSampler,IN.UV).x;
    float3 specContrib = ((litVec.y * litVec.z * gloss) * LightColor);
    // add, incorporating ambient light term
    float3 dayTex = tex2D(DaySampler,IN.UV).xyz;
    float3 nitTex = tex2D(NightSampler,IN.UV).xyz;
    float3 result = dayTex*(Kd*diffContrib+AmbiLightColor) + specContrib;
    result += saturate(4*(-ldn-0.1))*nitTex;
    return float4(result.xyz,1.0);
}

////////////////////////////////////////////////////////////////////
/// TECHNIQUES /////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////

technique Main <
	string Script = "Pass=p0;";
> {
	pass p0  <
		string Script = "Draw=geometry;";
	> {
		VertexShader = compile vs_2_0 basicVS(planetMat(),planetMatN());
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_2_0 everythingPS();
	}
}

/***************************** eof ***/
