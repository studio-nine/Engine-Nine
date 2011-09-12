/*********************************************************************NVMH3****
Path:  NVSDK\Common\media\cgfx
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/PerfTutor.fx#1 $

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

Mini-Style Guide:
	Shader parameter names start with Caps.
	Connector member names start with Caps.
	Local shader variable names generally start with lower case letters.
	L, P, N, V are typical Light vector, Point, Normal, View.
	Vectors that are normalized have names that end in "n" (except "Nb").
	When possible use float3 calculations to free-up w terms for compiler.

******************************************************************************/

// Compile-time flags
// feature flags
//#define DO_COLORTEX
//#define DO_BUMP
//#define DO_GLOSSMAP
//#define DO_QUADRATIC
//#define DO_REFLECTION

// performance flags
#define USE_NORMALIZATION_CUBEMAP

/************* "UN-TWEAKABLES," TRACKED BY CPU APPLICATION **************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?OnePass:Multipass:AmbiOnly:DirOnly:PtOnly;";
> = 0.8;

float4x4 WorldITXf : WorldInverseTranspose < string UIWidget="None"; > = {1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1};
float4x4 WvpXf : WorldViewProjection < string UIWidget="None"; >;
float4x4 WorldXf : World < string UIWidget="None"; >;
float4x4 ViewIXf : ViewInverse < string UIWidget="None"; >;

///////////////////////////////////////////////////////////////
/// TWEAKABLES ////////////////////////////////////////////////
///////////////////////////////////////////////////////////////

////////////////////////////////////////////// point light

float3 PointLightPos : POSITION <
	string UIName = "Point Pos";
	string Object = "PointLight";
	string Space = "World";
> = {10.0f, 10.0f, 10.0f};

float3 PointLightColor : SPECULAR <
	string UIName = "Point Color";
	string UIWidget = "Color";
> = {0.8f, 1.0f, 0.4f};

#ifdef DO_QUADRATIC
#define MAX_INTENSITY 25.0
#define DEFAULT_INTENSITY 2.0
#else /* ! DO_QUADRATIC */
#define MAX_INTENSITY 1.2
#define DEFAULT_INTENSITY 0.8
#endif /* ! DO_QUADRATIC */

float PointLightIntensity <
	string UIName = "Point Intensity";
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = MAX_INTENSITY;
	float UIStep = 0.1;
> = DEFAULT_INTENSITY;

////////////////////////////////////////////// directional light

float3 DirLight : DIRECTION <
	string UIName = "Dir Light";
	string Object = "DirectionalLight";
	string Space = "World";
> = {0.707f, 0.707f, -0.707f};

float3 DirLightColor : SPECULAR <
	string UIName = "Dir Light Color";
	string UIWidget = "Color";
> = {0.3f, 0.3f, 0.8f};

////////////////////////////////////////////// ambient light

float3 AmbiLightColor : Ambient
<
    string UIName = "Ambient Light Color";
> = {0.07f, 0.07f, 0.07f};

////////////////////////////////////////////// surface

float3 SurfColor : DIFFUSE
<
    string UIName = "Surface Color";
	string UIWidget = "Color";
> = {1.0f, 0.7f, 0.3f};

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
> = 1.0;


float SpecExpon : SpecularPower
<
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 128.0;
    float UIStep = 1.0;
    string UIName = "Specular power";
> = 12.0;

float Metalness
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.1;
    string UIName = "Metalness";
> = 0.2;

#ifdef DO_REFLECTION
float Kr
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.5;
    float UIStep = 0.01;
    string UIName = "Reflection Max";
> = 1.0;

float KrMin
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 0.2;
    float UIStep = 0.001;
    string UIName = "Reflection Min";
> = 0.002;

float FresExp : SpecularPower
<
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 7.0;
    float UIStep = 0.1;
    string UIName = "Edging of fresnel effect";
> = 5.0;
#endif /* DO_REFLECTION */

#ifdef DO_BUMP
float Bumpy
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 10.0;
    float UIStep = 0.1;
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
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;
    float4 Normal	: NORMAL;
#ifdef DO_BUMP
    float4 Tangent	: TANGENT0;
    float4 Binormal	: BINORMAL0;
#endif /* DO_BUMP */
};

struct vertexOutput {
    float4 HPosition	: POSITION;
    float2 UV			: TEXCOORD0;
    float3 LightVec		: TEXCOORD1;
    float3 WorldNormal	: TEXCOORD2;
    float3 WorldView	: TEXCOORD3;
#ifdef DO_BUMP
    float3 WorldTangent	: TEXCOORD4;
    float3 WorldBinorm	: TEXCOORD5;
#endif /* DO_BUMP */
};

/*********************************************************/
/*********** vertex shader *******************************/
/*********************************************************/

void sharedVS(appdata IN,
	out vertexOutput OUT,
	out float3 Pw
) {
    OUT = (vertexOutput)0;
    OUT.WorldNormal = mul(IN.Normal,WorldITXf).xyz;
#ifdef DO_BUMP
    OUT.WorldTangent = mul(IN.Tangent,WorldITXf).xyz;
    OUT.WorldBinorm = mul(IN.Binormal,WorldITXf).xyz;
#endif /* DO_BUMP */
    float4 Po = float4(IN.Position.xyz,1.0);	// object coordinates
    Pw = mul(Po,WorldXf).xyz;		// world coordinates
    OUT.WorldView = normalize(ViewIXf[3].xyz - Pw);	// obj coords
    OUT.HPosition = mul(Po,WvpXf);	// screen clipspace coords
    OUT.UV = IN.UV.xy;
}

vertexOutput ambiVS(appdata IN) {
    vertexOutput OUT;
	float3 Pw;
	sharedVS(IN,OUT,Pw);
    return OUT;
}

vertexOutput basicVS(appdata IN,
    uniform float3 LightPos	// in world coordinates
) {
    vertexOutput OUT;
	float3 Pw;
	sharedVS(IN,OUT,Pw);
    OUT.LightVec = LightPos - Pw;
    return OUT;
}

/*********************************************************/
/*********** functions for pixel shaders *****************/
/*********************************************************/

// some vectors and colors shared by all lit pixel shaders
void shared_ps_data(vertexOutput IN,
	out float3 Nn,
	out float3 Vn,
	out float Shiny,
	out float3 MatColor
) {
    Nn = NORM(IN.WorldNormal);
#ifdef DO_BUMP
    float3 Tn = NORM(IN.WorldTangent);
    float3 Bn = NORM(IN.WorldBinorm);
    float3 bumps = Bumpy * (tex2D(NormalSampler,IN.UV).xyz-(0.5).xxx);
    Nn = Nn + (bumps.x * Tn + bumps.y * Bn);
    Nn = NORM(Nn);
#endif /* DO_BUMP */
    Vn = NORM(IN.WorldView);
	Shiny = Ks;
#ifdef DO_GLOSSMAP
    Shiny *= tex2D(GlossSampler,IN.UV).x;
#endif /* DO_GLOSSMAP */
	MatColor = SurfColor;
#ifdef DO_COLORTEX
    MatColor *= tex2D(ColorSampler,IN.UV).xyz;
#endif /* DO_COLORTEX */
}

// point light contribution
void point_light_calc(float3 L,
    uniform float3 LightColor,
    uniform float Intensity,
	float3 Vn,float3 Nn,float Shiny,
	out float3 diffResult,
	out float3 specResult
) {
#ifdef DO_QUADRATIC
    float distSquared = dot(L,L);
#ifdef USE_NORMALIZATION_CUBEMAP
    float3 Ln = NORM(L);
#else /* !USE_NORMALIZATION_CUBEMAP */
    float3 Ln = L/ sqrt(distSquared);
#endif /* !USE_NORMALIZATION_CUBEMAP */
#else /* !DO_QUADRATIC */
    float3 Ln = NORM(L);
#endif /* !DO_QUADRATIC */
    float3 Hn = NORM(Vn + Ln);
    float hdn = dot(Hn,Nn);
    float ldn = dot(Ln,Nn);
    float4 litVec = lit(ldn,hdn,SpecExpon);
    ldn = litVec.y * Intensity;
#ifdef DO_QUADRATIC
    ldn /= distSquared; 
#endif /* DO_QUADRATIC */
    diffResult = (ldn * LightColor);
    specResult = ((ldn * litVec.z * Shiny) * LightColor);
}

// directional light contribution
void dir_light_calc(float3 L,
    uniform float3 LightColor,
	float3 Vn,
	float3 Nn,
	float Shiny,
	out float3 diffResult,
	out float3 specResult
) {
    float3 Hn = NORM(Vn + L);
    float hdn = dot(Hn,Nn);
    float ldn = dot(L,Nn);
    float4 litVec = lit(ldn,hdn,SpecExpon);
    diffResult = (litVec.y * LightColor);
    specResult = ((litVec.y * litVec.z * Shiny) * LightColor);
}

#ifdef DO_REFLECTION
float3 refl_color(float3 V, float3 N)
{
    // reflection
    float3 reflVect = reflect(V,N);
    float vdn = dot(V,N);
    float fres = KrMin + (Kr-KrMin) * pow((1.0-abs(vdn)),FresExp);
    float3 reflColor = lerp(fres,Kr,Metalness) * texCUBE(EnvSampler,reflVect).xyz;
	return reflColor;
}
#endif /* !DO_REFLECTION */

/*********************************************************/
/*********** pixel shader ********************************/
/*********************************************************/

float4 everythingPS(vertexOutput IN,
    uniform float3 PtColor,
    uniform float PtIntensity,
    uniform float3 DirLightVec,	// PRENORMALIZED, WE HOPE
    uniform float DirColor
) : COLOR {
	float3 Nn, Vn;
	float3 materialColor;
	float shininess;
	shared_ps_data(IN,Nn,Vn,shininess,materialColor);
	// first we do point light illumination
	float3 diffContrib, specContrib;
	point_light_calc(IN.LightVec,PtColor,PtIntensity,Vn,Nn,shininess,diffContrib,specContrib);
	// now add-in directional light
    float3 diff, spec;
	dir_light_calc(DirLightVec,DirColor,Vn,Nn,shininess,diff,spec);
	diffContrib += diff;
	specContrib += spec;
    // add all, incorporating ambient light term
#ifdef DO_REFLECTION
    specContrib += refl_color(Vn,Nn);
#endif /* DO_REFLECTION */
    float3 result = (materialColor*(Kd*diffContrib+AmbiLightColor)) +
	    lerp(((1.0).xxx),materialColor,Metalness)*specContrib;
    return float4(result,1.0);
}

// split for multipass ///////////////////////////////

float4 ambiReflPS(vertexOutput IN) : COLOR {
#ifdef DO_COLORTEX
    float3 colorTex = SurfColor * tex2D(ColorSampler,IN.UV).xyz;
#define SURF_COLOR colorTex
#else /* !DO_COLORTEX */
#define SURF_COLOR SurfColor
#endif /* !DO_COLORTEX */
#ifdef DO_REFLECTION
    float3 Nn = NORM(IN.WorldNormal);
#ifdef DO_BUMP
    float3 Tn = NORM(IN.WorldTangent);
    float3 Bn = NORM(IN.WorldBinorm);
    float3 bumps = Bumpy * (tex2D(NormalSampler,IN.UV).xyz-(0.5).xxx);
    float3 Nb = Nn + (bumps.x * Tn + bumps.y * Bn);
    Nb = NORM(Nb);
#define SURFACE_NORMAL Nb
#else /* DO_BUMP */
#define SURFACE_NORMAL Nn
#endif /* DO_BUMP */
    float3 Vn = NORM(IN.WorldView);
    float3 reflColor = refl_color(Vn,SURFACE_NORMAL);
    float3 result = (SURF_COLOR*AmbiLightColor) +
	    lerp(((1.0).xxx),SURF_COLOR,Metalness)*reflColor;
#else /* !DO_REFLECTION */
    float3 result = SURF_COLOR*AmbiLightColor;
#endif /* !DO_REFLECTION */
    return float4(result.xyz,1.0);
}

float4 pointlitPS(vertexOutput IN,
    uniform float3 PtColor,
    uniform float PtIntensity
) : COLOR {
	float3 Nn, Vn;
	float3 materialColor;
	float shininess;
	shared_ps_data(IN,Nn,Vn,shininess,materialColor);
	float3 diffContrib, specContrib;
	point_light_calc(IN.LightVec,PtColor,PtIntensity,Vn,Nn,shininess,diffContrib,specContrib);
    float3 result = (materialColor*(Kd*diffContrib)) +
	    lerp(((1.0).xxx),materialColor,Metalness)*specContrib;
    return float4(result,1.0);
}

float4 dirlitPS(vertexOutput IN,
    uniform float3 DirLightVec,	// PRENORMALIZED, WE HOPE
    uniform float DirColor
) : COLOR {
	float3 Nn, Vn;
	float3 materialColor;
	float shininess;
	shared_ps_data(IN,Nn,Vn,shininess,materialColor);
	float3 diffContrib, specContrib;
	dir_light_calc(DirLightVec,DirColor,Vn,Nn,shininess,diffContrib,specContrib);
    float3 result = (materialColor*(Kd*diffContrib)) +
	    lerp(((1.0).xxx),materialColor,Metalness)*specContrib;
    return float4(result,1.0);
}

////////////////////////////////////////////////////////////////////
/// TECHNIQUES /////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////

// do all shading in a single pass

technique OnePass <
	string Script = "Pass=p0;";
> {
	pass p0  <
		string Script = "Draw=geometry;";
	> {
		VertexShader = compile vs_2_0 basicVS(PointLightPos);
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_2_0 everythingPS(PointLightColor,PointLightIntensity,
													DirLight,DirLightColor);
	}
}

// split into individual lighting passes

technique Multipass <
	string Script = "Pass=ambiZ; Pass=pointLight1; Pass=dirLight1;";
> {
	pass ambiZ  <
		string Script = "Draw=geometry;";
	> {
		VertexShader = compile vs_2_0 ambiVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_2_0 ambiReflPS();
	}
	pass pointLight1  <
		string Script = "Draw=geometry;";
	> {
		VertexShader = compile vs_2_0 basicVS(PointLightPos);
		ZEnable = true;
		ZWriteEnable = false;
		ZFunc = LessEqual;
		CullMode = None;
		AlphaBlendEnable = true;
		SrcBlend = One;
		DestBlend = One;
		PixelShader = compile ps_2_0 pointlitPS(PointLightColor,PointLightIntensity);
	}
	pass dirLight1  <
		string Script = "Draw=geometry;";
	> {
		VertexShader = compile vs_2_0 ambiVS();
		ZEnable = true;
		ZWriteEnable = false;
		ZFunc = LessEqual;
		CullMode = None;
		AlphaBlendEnable = true;
		SrcBlend = One;
		DestBlend = One;
		PixelShader = compile ps_2_0 dirlitPS(DirLight,DirLightColor);
	}
}

// see individual components /////////

technique AmbiOnly <
	string Script = "Pass=ambiPass;";
> {
	pass ambiPass <
		string Script = "Draw=geometry;";
	> {
		VertexShader = compile vs_2_0 ambiVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_2_0 ambiReflPS();
	}
}

technique DirOnly <
	string Script = "Pass=dirPass;";
> {
	pass dirPass  <
		string Script = "Draw=geometry;";
	> {
		VertexShader = compile vs_2_0 ambiVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_2_0 dirlitPS(DirLight,DirLightColor);
	}
}

technique PtOnly <
	string Script = "Pass=pointPass;";
> {
	pass pointPass  <
		string Script = "Draw=geometry;";
	> {
		VertexShader = compile vs_2_0 basicVS(PointLightPos);
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_2_0 pointlitPS(PointLightColor,PointLightIntensity);
	}
}

/***************************** eof ***/
