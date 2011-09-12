/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/Gungrip.fx#1 $

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
	Bumpy, fresnel-shiny, dielectric, bumptextured, two
	    quadratic point sources + ambient.

******************************************************************************/

// Compile-time flags
// feature flags
// #define DO_COLORTEX
#define DO_BUMP
// #define DO_GLOSSMAP
#define DO_REFLECTION
// #define DO_QUADRATIC

// performance flags
//#define USE_NORMALIZATION_CUBEMAP

/************* "UN-TWEAKABLES," TRACKED BY CPU APPLICATION **************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Multipass;";
> = 0.8;

half4x4 WorldITXf : WorldInverseTranspose < string UIWidget="None"; > = {1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1};
half4x4 WvpXf : WorldViewProjection < string UIWidget="None"; >;
half4x4 WorldXf : World < string UIWidget="None"; >;
half4x4 ViewIXf : ViewInverse < string UIWidget="None"; >;

///////////////////////////////////////////////////////////////
/// TWEAKABLES ////////////////////////////////////////////////
///////////////////////////////////////////////////////////////

////////////////////////////////////////////// point light

// Point Light1 ////

half4 LightPosP1 : POSITION <
	string UIName = "Light Position 1";
	string Object = "PointLight";
	string Space = "World";
> = {-1.0f, 1.0f, -1.0f, 1.0f};

half4 LightColorP1 : SPECULAR <
	string UIName = "Light Color 1";
	string UIWidget = "Color";
	string Object = "PointLight";
> = {1.0f, 1.0f, 0.9f, 1.0f};

half LightIntensityP1 <
	string UIName = "Light Strength 1";
	string UIWidget = "slider";
	half UIMin = 0.0;
	half UIMax = 2.0;
	half UIStep = 0.1;
> = 1.0f;

// Point Light 2 ////

half4 LightPosP2 : POSITION <
	string UIName = "Light Position 2";
	string Object = "PointLight";
	string Space = "World";
> = {1.0f, 0.3f, 0.3f, 1.0f};

half4 LightColorP2 : SPECULAR <
	string UIName = "Light Color 2";
	string UIWidget = "Color";
	string Object = "PointLight";
> = {0.4f, 0.3f, 1.0f, 1.0f};

half LightIntensityP2 <
	string UIName = "Light Strength 2";
	string UIWidget = "slider";
	half UIMin = 0.0;
	half UIMax = 2.0;
	half UIStep = 0.1;
> = 0.8f;

////////////////////////////////////////////// ambient light

half4 AmbiLightColor : Ambient <
    string UIName = "Ambient Light Color";
	string UIWidget = "Color";
> = {0.07f, 0.07f, 0.07f, 1.0f};

////////////////////////////////////////////// surface

half4 SurfColor : DIFFUSE
<
    string UIName = "Surface Color";
	string UIWidget = "Color";
> = {0.8f, 0.8f, 0.85f, 1.0f};

half Kd
<
    string UIWidget = "slider";
    half UIMin = 0.0;
    half UIMax = 1.5;
    half UIStep = 0.01;
    string UIName = "Diffuse";
> = 1.0;

half Ks
<
    string UIWidget = "slider";
    half UIMin = 0.0;
    half UIMax = 1.5;
    half UIStep = 0.01;
    string UIName = "Specular";
> = 1.0;


half SpecExpon : SpecularPower
<
    string UIWidget = "slider";
    half UIMin = 1.0;
    half UIMax = 128.0;
    half UIStep = 1.0;
    string UIName = "Specular power";
> = 12.0;

half Metalness
<
    string UIWidget = "slider";
    half UIMin = 0.0;
    half UIMax = 1.0;
    half UIStep = 0.1;
    string UIName = "Metalness";
> = 0.2;

#ifdef DO_REFLECTION
half Kr
<
    string UIWidget = "slider";
    half UIMin = 0.0;
    half UIMax = 1.5;
    half UIStep = 0.01;
    string UIName = "Reflection Max";
> = 1.0;

half KrMin
<
    string UIWidget = "slider";
    half UIMin = 0.0;
    half UIMax = 0.2;
    half UIStep = 0.001;
    string UIName = "Reflection Min";
> = 0.002;

half FresExp : SpecularPower
<
    string UIWidget = "slider";
    half UIMin = 1.0;
    half UIMax = 7.0;
    half UIStep = 0.1;
    string UIName = "Edging of fresnel effect";
> = 5.0;
#endif /* DO_REFLECTION */

#ifdef DO_BUMP
half Bumpy
<
    string UIWidget = "slider";
    half UIMin = 0.0;
    half UIMax = 5.0;
    half UIStep = 0.1;
    string UIName = "Bump Height";
> = 1.0;
#endif /* DO_BUMP */

////////////////////////////////////////////////////////
/// TEXTURES ///////////////////////////////////////////
////////////////////////////////////////////////////////

#ifdef DO_COLORTEX
texture ColorTexture : DIFFUSE
<
    string ResourceName = "default_color.dds";
    string ResourceType = "2D";
>;

sampler2D ColorSampler = sampler_state
{
	Texture = <ColorTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = wrap;
	AddressV = wrap;
};
#endif /* !DO_COLORTEX */

#ifdef DO_BUMP
texture NormalTexture : NORMAL
<
    string ResourceName = "default_bump_normal.dds";
    string ResourceType = "2D";
>;

sampler2D NormalSampler = sampler_state
{
	Texture = <NormalTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = wrap;
	AddressV = wrap;
};
#endif /* DO_BUMP */

//////////////

#ifdef DO_GLOSSMAP
texture GlossTexture : SPECULAR
<
    string ResourceName = "default_gloss.dds";
    string ResourceType = "2D";
>;

sampler2D GlossSampler = sampler_state
{
	Texture = <GlossTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = wrap;
	AddressV = wrap;
};
#endif /* !DO_GLOSSMAP */

//////////////

#ifdef DO_REFLECTION
texture EnvTexture : ENVIRONMENT
<
    string ResourceName = "default_reflection.dds";
    string ResourceType = "Cube";
>;

samplerCUBE EnvSampler = sampler_state
{
	Texture = <EnvTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = clamp;
	AddressV = clamp;
	AddressW = clamp;
};
#endif /* DO_REFLECTION */

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
    half3 Position	: POSITION;
    half4 UV		: TEXCOORD0;
    half4 Normal	: NORMAL;
#ifdef DO_BUMP
    half4 Tangent	: TANGENT0;
    half4 Binormal	: BINORMAL0;
#endif /* DO_BUMP */
};

struct vertexOutput {
    half4 HPosition	: POSITION;
    half2 UV			: TEXCOORD0;
    half3 PtLightVec	: TEXCOORD1;
    half3 WorldNormal	: TEXCOORD2;
    half3 WorldView	: TEXCOORD3;
#ifdef DO_BUMP
    half3 WorldTangent	: TEXCOORD4;
    half3 WorldBinorm	: TEXCOORD5;
#endif /* DO_BUMP */
};

/*********************************************************/
/*********** vertex shader *******************************/
/*********************************************************/

vertexOutput basicVS(appdata IN,
    uniform half3 LightPos	// in world coordinates
) {
    vertexOutput OUT;
    OUT.WorldNormal = mul(IN.Normal,WorldITXf).xyz;
#ifdef DO_BUMP
    OUT.WorldTangent = mul(IN.Tangent,WorldITXf).xyz;
    OUT.WorldBinorm = mul(IN.Binormal,WorldITXf).xyz;
#endif /* DO_BUMP */
    half4 Po = half4(IN.Position.xyz,1.0);	// object coordinates
    half3 Pw = mul(Po,WorldXf).xyz;		// world coordinates
    OUT.PtLightVec = LightPos - Pw;
    OUT.UV = IN.UV.xy;
    OUT.WorldView = normalize(ViewIXf[3].xyz - Pw);	// obj coords
    OUT.HPosition = mul(Po,WvpXf);	// screen clipspace coords
    return OUT;
}

/*********************************************************/
/*********** pixel shader ********************************/
/*********************************************************/

#ifdef DO_REFLECTION
// extra function
half3 refl_color(half3 V, half3 N)
{
    // reflection
    half3 reflVect = reflect(V,N);
    half vdn = dot(V,N);
    half fres = KrMin + (Kr-KrMin) * pow((1.0-abs(vdn)),FresExp);
    half3 reflColor = lerp(fres,Kr,Metalness) * texCUBE(EnvSampler,reflVect).xyz;
	return reflColor;
}
#endif /* !DO_REFLECTION */

// split for multipass ///////////////////////////////

half4 ambiReflPS(vertexOutput IN) : COLOR {
#ifdef DO_COLORTEX
    half3 colorTex = SurfColor * tex2D(ColorSampler,IN.UV).xyz;
#define SURF_COLOR colorTex
#else /* !DO_COLORTEX */
#define SURF_COLOR SurfColor
#endif /* !DO_COLORTEX */
#ifdef DO_REFLECTION
    half3 Nn = NORM(IN.WorldNormal);
#ifdef DO_BUMP
    half3 Tn = NORM(IN.WorldTangent);
    half3 Bn = NORM(IN.WorldBinorm);
    half3 bumps = Bumpy * (tex2D(NormalSampler,IN.UV).xyz-(0.5).xxx);
    half3 Nb = Nn + (bumps.x * Tn + bumps.y * Bn);
    Nb = NORM(Nb);
#define SURFACE_NORMAL Nb
#else /* DO_BUMP */
#define SURFACE_NORMAL Nn
#endif /* DO_BUMP */
    half3 Vn = NORM(IN.WorldView);
    half3 reflColor = refl_color(Vn,SURFACE_NORMAL);
    half3 result = (SURF_COLOR*AmbiLightColor) +
	    lerp(((1.0).xxx),SURF_COLOR,Metalness)*reflColor;
#else /* !DO_REFLECTION */
    half3 result = SURF_COLOR*AmbiLightColor;
#endif /* !DO_REFLECTION */
    return half4(result.xyz,1.0);
}

half4 pointlitPS(vertexOutput IN,
    uniform half3 PtLightColor,
    uniform half PtIntensity
) : COLOR {
    half3 Nn = NORM(IN.WorldNormal);
#ifdef DO_BUMP
    half3 Tn = NORM(IN.WorldTangent);
    half3 Bn = NORM(IN.WorldBinorm);
    half3 bumps = Bumpy * (tex2D(NormalSampler,IN.UV).xyz-(0.5).xxx);
    half3 Nb = Nn + (bumps.x * Tn + bumps.y * Bn);
    Nb = NORM(Nb);
#define SURFACE_NORMAL Nb
#else /* DO_BUMP */
#define SURFACE_NORMAL Nn
#endif /* DO_BUMP */
    half3 Vn = NORM(IN.WorldView);
    // point light
#ifdef DO_QUADRATIC
    half ld = 1.0 / length(IN.PtLightVec);
    half3 Ln = ld * IN.PtLightVec;	// normalizes
#else /* !DO_QUADRATIC */
    half3 Ln = NORM(IN.PtLightVec);
#endif /* !DO_QUADRATIC */
    half3 Hn = NORM(Vn + Ln);
    half hdn = dot(Hn,SURFACE_NORMAL);
    half ldn = dot(Ln,SURFACE_NORMAL);
    half4 litVec = lit(ldn,hdn,SpecExpon);
#ifdef DO_QUADRATIC
    ldn = (ld*litVec.y) * PtIntensity; 				// 1/length
    half3 diffContrib = ld*(ldn * PtLightColor);	// 1/length^2
#else /* !DO_QUADRATIC */
    ldn = litVec.y * PtIntensity;
    half3 diffContrib = ldn * PtLightColor;
#endif /* !DO_QUADRATIC */
#ifdef DO_GLOSSMAP
    half gloss = Ks * tex2D(GlossSampler,IN.UV).x;
#define SHINY gloss
#else /* !DO_GLOSSMAP */
#define SHINY Ks
#endif /* !DO_GLOSSMAP */
#ifdef DO_QUADRATIC
    half3 specContrib = ld*((ldn * litVec.z * SHINY) * PtLightColor);	// 1/length^2
#else /* !DO_QUADRATIC */
    half3 specContrib = (ldn * litVec.z * SHINY) * PtLightColor;
#endif /* !DO_QUADRATIC */
#ifdef DO_COLORTEX
    half3 colorTex = SurfColor * tex2D(ColorSampler,IN.UV).xyz;
#define SURF_COLOR colorTex
#else /* !DO_COLORTEX */
#define SURF_COLOR SurfColor
#endif /* !DO_COLORTEX */
    half3 result = (SURF_COLOR*(Kd*diffContrib)) +
	    lerp(((1.0).xxx),SURF_COLOR,Metalness)*specContrib;
    return half4(result.xyz,1.0);
}

////////////////////////////////////////////////////////////////////
/// TECHNIQUES /////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////

// splt into two passes. For more lights, repeat "pointLight1"
technique Multipass <
	string Script = "Pass=ambiZ; Pass=pointLight1; Pass=pointLight2;";
> {
	pass ambiZ <
	string Script = "Draw=geometry;";
> {
		VertexShader = compile vs_2_0 basicVS(LightPosP1); // ignore light pos for this pass
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_2_0 ambiReflPS();
	}
	pass pointLight1 <
	string Script = "Draw=geometry;";
> {
		VertexShader = compile vs_2_0 basicVS(LightPosP1);
		ZEnable = true;
		ZWriteEnable = false;
		ZFunc = LessEqual;
		CullMode = None;
		AlphaBlendEnable = true;
		SrcBlend = One;
		DestBlend = One;
		PixelShader = compile ps_2_0 pointlitPS(LightColorP1,LightIntensityP1);
	}
	pass pointLight2 <
	string Script = "Draw=geometry;";
> {
		VertexShader = compile vs_2_0 basicVS(LightPosP2);
		ZEnable = true;
		ZWriteEnable = false;
		ZFunc = LessEqual;
		CullMode = None;
		AlphaBlendEnable = true;
		SrcBlend = One;
		DestBlend = One;
		PixelShader = compile ps_2_0 pointlitPS(LightColorP2,LightIntensityP2);
	}
}

/***************************** eof ***/
