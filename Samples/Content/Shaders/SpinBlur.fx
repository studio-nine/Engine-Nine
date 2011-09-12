/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/SpinBlur.fx#1 $

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
	Accumulation Buffer Test

******************************************************************************/

float Script : STANDARDSGLOBAL
<
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";	
	// We just call a script in the main technique.
	string Script = "Technique=Blur;";
> = 0.8; // version number

#include "Quad.fxh"

// #define USE_TIMER

// Compile-time flags
// feature flags

// performance flags
//#define USE_NORMALIZATION_CUBEMAP
//#define USE_HALF

// float for pixel shaders
#ifdef USE_HALF
#define REAL half
#define REAL2 half2
#define REAL3 half3
#define REAL4 half4
#else /* !USE_HALF */
#define REAL float
#define REAL2 float2
#define REAL3 float3
#define REAL4 float4
#endif /* !USE_HALF */

///////////////////////////////////////////////////////////////
/// UNTWEAKABLES //////////////////////////////////////////////
///////////////////////////////////////////////////////////////

float4x4 WorldITXf : WorldInverseTranspose <string UIWidget="None";>;
float4x4 WvpXf : WorldViewProjection <string UIWidget="None";>;
float4x4 WorldXf : World <string UIWidget="None";>;
float4x4 ViewIXf : ViewInverse <string UIWidget="None";>;
float4x4 WorldViewITXf : WorldViewInverseTranspose <string UIWidget="None";>;
float4x4 WorldViewXf : WorldView <string UIWidget="None";>;
float4x4 ViewXf : View <string UIWidget="None";>;
float4x4 ViewITXf : ViewInverseTranspose <string UIWidget="None";>;

#ifdef USE_TIMER
float Timer : TIME <string UIWidget="None";>;
#else /* !USE_TIMER */

float Timer <
	string UIName = "Time At Frame Start";
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 10.0;
	float UIStep = 0.1;
> = 0.0f;
#endif /* !USE_TIMER */

float passnumber <string UIWidget = "none";>; // loop counter, hidden

///////////////////////////////////////////////////////////////
/// TWEAKABLES ////////////////////////////////////////////////
///////////////////////////////////////////////////////////////

float npasses <
	float UIStep = 1.0;
	string UIName = "Blur passes";
> = 16.0f;

float Accel
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.01;
    string UIName = "Blur biasing";
> = 1.0;

////////////////////////////////////////////// spot light

half3 LightDir : DIRECTION <
	string UIName = "Light Direction";
	string Object = "DirectionalLight";
	string Space = "World";
> = {-0.707f, 0.707f, 0.0f};

////////////////////////////////////////////// ambient light

half4 AmbiLightColor : Ambient
<
    string UIName = "Ambient Light";
	string UIWidget = "Color";
> = {0.07f, 0.07f, 0.07f, 1};

////////////////////////////////////////////// surface

half4 SurfColor : Diffuse
<
    string UIName = "Surface";
	string UIWidget = "Color";
> = {1.0f, 0.7f, 0.3f, 1};

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
> = 52.0;

half Speed <
	string UIName = "Radians Per Sec";
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 3.14159265358979;
	float UIStep = 0.01;
> = 0.2f;

half Shutter <
	string UIName = "Shutter 0-1";
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 1.0;
	float UIStep = 0.01;
> = 0.5f;

float FPS <
	string UIName = "Frames Per Second";
	string UIWidget = "slider";
	float UIMin = 2.0;
	float UIMax = 60.0;
	float UIStep = 0.03;
> = 29.97f;

////////////////////////////////////////////////////////
/// TEXTURES ///////////////////////////////////////////
////////////////////////////////////////////////////////

texture DiffTexture : Diffuse <
	string ResourceName = "default_color.dds";
	string ResourceType = "2D";
>;

sampler2D DiffSampler = sampler_state
{
	Texture = <DiffTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = WRAP;
	AddressV = WRAP;
};

//////////////////

#ifdef USE_NORMALIZATION_CUBEMAP
#include "normalize.fxh"
// only for pixel shaders....

/*
half3 my_normalize(half3 v)
{
	half3 v2 = texCUBE(NormalizeSampler,v);
	return (2*(v2-0.5));
}
half4 my_normalize(half4 v)
{
	half3 v2 = texCUBE(NormalizeSampler,v.xyz);
	return half4((2*(v2-0.5)),1);
}
*/

#define NORM my_normalize
#else /* !USE_NORMALIZATION_CUBEMAP */
#define NORM normalize
#endif /* !USE_NORMALIZATION_CUBEMAP */

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Data //////
///////////////////////////////////////////////////////////

float4 ClearColor : DIFFUSE = {0,0,0,1.0};
float ClearDepth
<
	string UIWidget = "none";
> = 1.0;

DECLARE_QUAD_TEX(EachMap,EachSampler,"A8R8G8B8")
DECLARE_QUAD_TEX(AccumBuffer,AccumSampler,"A16B16G16R16F")

DECLARE_QUAD_DEPTH_BUFFER(SceneDepth,"D24S8")

////////////////////////////////////////////////////////////////////////////
/// SHADER CODE BEGINS /////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////

/* data from application vertex buffer */
struct appdata {
    half3 Position	: POSITION;
    half4 UV		: TEXCOORD0;
    half4 Normal	: NORMAL;
};

// used for all other passes
struct vertexOutput {
    half4 HPosition	: POSITION;
    half2 UV		: TEXCOORD0;
    float3 WNormal	: TEXCOORD1;
    float3 WView		: TEXCOORD2;
	half4 DiffCol : COLOR0;
};

/*********************************************************/
/*********** vertex shader *******************************/
/*********************************************************/

vertexOutput spinVS(appdata IN,uniform half delta)
{
    vertexOutput OUT;
    OUT.UV = IN.UV.xy;
    half3 Nn = normalize(mul(IN.Normal, WorldITXf).xyz);
    half4 Po = half4(IN.Position.xyz,1);	// obj coords
	half angNext = Speed * (Timer + (delta/FPS));
	half2 ci = cos(angNext);
	half2 si = sin(angNext);
	half3 Nrz = half3(Nn.x*ci.x-Nn.y*si.x,
						Nn.x*si.x+Nn.y*ci.x,
						Nn.z);
	half4 Prz = half4(Po.x*ci.x-Po.y*si.x,
						Po.x*si.x+Po.y*ci.x,
						Po.zw);
    OUT.WView = normalize(ViewIXf[3].xyz - Prz.xyz);	// obj coords
    OUT.WNormal = Nrz; // mul(Po,WorldViewProjXf);	// screen clipspace coords
	float d = dot(-LightDir,Nrz);
	OUT.DiffCol = half4(max(0,d).xxx,1.0);
    half4 Ph = mul(Prz, WvpXf);
    OUT.HPosition = Ph;
    return OUT;
}

/*********************************************************/
/*********** pixel shader ********************************/
/*********************************************************/

half4 nPS(vertexOutput IN, uniform float delta) : COLOR
{
    float3 Nn = NORM(IN.WNormal);
    float3 Vn = NORM(IN.WView);
    float3 Ln = -LightDir;
    float3 Hn = NORM(Vn + Ln);
    float hdn = Ks * pow(dot(Hn,Nn),SpecExpon);
    half4 sc = half4(hdn.xxx,0);
	half4 tc = tex2D(DiffSampler,IN.UV);
	half4 dc = tc * SurfColor * (IN.DiffCol + AmbiLightColor);
	return delta*(dc+sc);
}

///////////////////////////////////////////////////////////
/// Final Pass ////////////////////////////////////////////
///////////////////////////////////////////////////////////

QUAD_REAL4 accumPS(QuadVertexOutput IN) : COLOR
{   
	QUAD_REAL4 texCol = tex2D(EachSampler, IN.UV);
	return texCol;
}

QUAD_REAL4 finalPS(QuadVertexOutput IN) : COLOR
{   
	QUAD_REAL4 texCol = tex2D(AccumSampler, IN.UV) / npasses;
	return texCol;
}  

////////////////////////////////////////////////////////////////////
/// TECHNIQUES /////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////

technique Blur <
	string Script =
			// Clear Accum Buffer
			"RenderColorTarget0=AccumBuffer;"
			"ClearSetColor=ClearColor;"
			"Clear=Color;"
        	"LoopByCount=npasses;"
        	//"LoopBegin;"
				"LoopGetIndex=passnumber;"
				// Render Object(s)
				"Pass=drawObj;"
				// Blend Results into accum buffer
				"Pass=Accumulate;"
	        "LoopEnd;"
	        // draw accum buffer to framebuffer
	        "Pass=FinalPass;";
> {
	pass drawObj <
		string Script =
				"RenderColorTarget0=EachMap;"
				"RenderDepthStencilTarget=SceneDepth;"
	        	"ClearSetColor=ClearColor;"
	        	"ClearSetDepth=ClearDepth;"
   				"Clear=Color;"
				"Clear=Depth;"
				"Draw=geometry;";
	> {
		VertexShader = compile vs_2_0 spinVS((passnumber/(npasses-1.0)));
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		AlphaBlendEnable = false;
		PixelShader = compile ps_2_0 nPS((1-Accel)+Accel*(passnumber/(npasses-1.0)));
	}
	pass Accumulate <
		string Script = 
				"RenderColorTarget0=AccumBuffer;"
				"RenderDepthStencilTarget=;"
				"Draw=Buffer;";
	> {
		VertexShader = compile vs_2_0 ScreenQuadVS();
		ZEnable = false;
		AlphaBlendEnable = true;
		SrcBlend = ONE;
		DestBlend = ONE;
		PixelShader  = compile ps_2_0 accumPS();
	}
	pass FinalPass <
		string Script =
	        "RenderColorTarget0=;"
	        "RenderDepthStencilTarget=;"
			"Draw=Buffer;";
	> {
		VertexShader = compile vs_2_0 ScreenQuadVS();
		AlphaBlendEnable = false;
		ZEnable = false;
		PixelShader  = compile ps_2_0 finalPS();
	}
}

/***************************** eof ***/
