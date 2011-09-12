/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/AimShadows.fxh#1 $

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

Some useful macros and functions for building shadows.

The shadow is projected from any point to what I call an "aim sphere" -- the
	aim sphere is defined by a 3D center and a radius.

The projected shadow encloses the sphere -- that is, the cone of the shadow projection will
	exactly enclose that sphere. Two "Adjust" values let you tweak the near/far
	clipping planes of the shadow (which would normally be at the boundaries
	of the aim sphere).

Advantages of this method:
	* Shadow is always aiming at the right thing no matter where the lamp is
	* Point/cone/distant/whatever lamps can be used without worrying about shadow projections
	* Shadow map contains only the info you want
	* Regardless of location or distance of light, shadow map always contains
		about the same # of overall texels -- so shadow-quality crawling, a
		concern for animated objects or lamps, ever presents a problem.

CONTENTS OF THIS FILE:

	* User-overridable Macros to declare defaults for shadow size, texture format,
		and a variety of global tweakable range parameters
	* Macros for easy declaration of shadow rendertargets and depth targets
	* A macro for easy declaration of standard aim sphere global tweakables
	* A global tweakable "Up" vector used to create shadow transform matrices
	* Hidden global constants for clearing the color and depth of shadow maps
	* Hidden global constants for automatic tracking of World and View coordinates
	* Structure declarations for application and vertex-pixel connections for shadowing
	* A VM function for creating aim-sphere projection transforms
	* Vertex and Pixel shaders for generating shadows, along with an example
		template showing their use in a typical endeing pass
	* a float function, aim_shadow(), to use in pixel shaders that use these shadow maps
	* a sample vertex shader template for typical shaders that use these shadows
	* a sample pixel shader template for typical shaders that use these shadows

See "PointShadowAim.fx" for an example complete shader that uses this file.

$Date: 2004/09/24 $
$Author: jallen $

******************************************************************************/

#ifndef _H_AIMSHADOWS
#define _H_AIMSHADOWS

#include "nvMatrix.fxh"
#include "Quad.fxh"

//////////////////////////////////////////////////
// TEXTURE-DECLARATION MACROS FOR SHADOWS ////////
//////////////////////////////////////////////////

#ifndef AIM_SHADOW_SIZE
#define AIM_SHADOW_SIZE 1024
#endif /* AIM_SHADOW_SIZE */

#ifndef AIM_SHADOW_FMT
#define AIM_SHADOW_FMT  "a16b16g16r16f"
#endif /* AIM_SHADOW_FMT */

#define AIM_SHAD_TEX(Tex,Samp) texture Tex : RENDERCOLORTARGET < \
	float2 Dimensions = { AIM_SHADOW_SIZE, AIM_SHADOW_SIZE }; \
    string Format = (AIM_SHADOW_FMT) ; \
	string UIWidget = "None"; \
>; \
sampler Samp = sampler_state { \
    texture = <Tex>; \
    AddressU  = CLAMP; \
    AddressV = CLAMP; \
    MipFilter = NONE; \
    MinFilter = LINEAR; \
    MagFilter = LINEAR; \
};

#define AIM_DEPTH_TEX(Tex) texture Tex : RENDERDEPTHSTENCILTARGET < \
	int Width = AIM_SHADOW_SIZE;\
	int Height = AIM_SHADOW_SIZE;\
    string format = "D24S8";\
    string UIWidget = "None";\
>;

///////////////////////////////////////////////////////////////////////////
/// MACROS TO DECLARE STANDARD SUITE OF AIM-SPHERE TWEAKBALE PARAMETERS ///
///////////////////////////////////////////////////////////////////////////

// These macros describe scale factors for the parameter macro below.
// You can change them or not be pre-defining them before reading this file

#ifndef AIM_MAX_RADIUS
#define AIM_MAX_RADIUS 15.0
#endif /* AIM_MAX_RADIUS */
#ifndef AIM_DEFAULT_RADIUS
#define AIM_DEFAULT_RADIUS 5
#endif /* AIM_DEFAULT_RADIUS */
#ifndef AIM_MIN_NEAR_ADJUST
#define AIM_MIN_NEAR_ADJUST 0.0
#endif /* AIM_MIN_NEAR_ADJUST */
#ifndef AIM_MAX_NEAR_ADJUST
#define AIM_MAX_NEAR_ADJUST 4.0
#endif /* AIM_MAX_NEAR_ADJUST */
#ifndef AIM_MIN_FAR_ADJUST
#define AIM_MIN_FAR_ADJUST 0.0
#endif /* AIM_MIN_FAR_ADJUST */
#ifndef AIM_MAX_FAR_ADJUST
#define AIM_MAX_FAR_ADJUST 4.0
#endif /* AIM_MAX_FAR_ADJUST */
#ifndef AIM_MAX_BIAS
#define AIM_MAX_BIAS 0.03
#endif /* AIM_MAX_BIAS */
#ifndef AIM_DEFAULT_BIAS
#define AIM_DEFAULT_BIAS 0.01
#endif /* AIM_DEFAULT_BIAS */

//
// For now, we are hacking the aim point with a "Pointlight" annotation, since there are no
//		"null objects" in FX Composer 1.5. The point could come from any object (such as the
//		centerpoint of a character or vehicle)
//
#define AIM_SHADOW_PARAMS(AimPt,Rad,NFud,FFud,Bias) float3 AimPt : POSITION < \
	string UIName = "Shadow is centered here"; \
	string Object = "PointLight"; \
	string Space = "World"; \
> = {0.0f, 0.0f, 0.0f}; \
float Rad < \
    string UIWidget = "slider"; \
    float UIMin = 0.01; \
    float UIMax = AIM_MAX_RADIUS; \
    float UIStep = 0.01; \
    string UIName = "Shadow Radius"; \
> = AIM_DEFAULT_RADIUS; \
float NFud < \
    string UIWidget = "slider"; \
    float UIMin = AIM_MIN_NEAR_ADJUST; \
    float UIMax = AIM_MAX_NEAR_ADJUST; \
    float UIStep = 0.01; \
    string UIName = "Shadow Near Fudge"; \
> = 0.0; \
float FFud < \
    string UIWidget = "slider"; \
    float UIMin = AIM_MIN_FAR_ADJUST; \
    float UIMax = AIM_MAX_FAR_ADJUST; \
    float UIStep = 0.01; \
    string UIName = "Shadow Far Fudge"; \
> = 0.0; \
float ShadBias < \
    string UIWidget = "slider"; \
    float UIMin = 0.0; \
    float UIMax = AIM_MAX_BIAS; \
    float UIStep = 0.0001; \
    string UIName = "Shadow Bias"; \
> = AIM_DEFAULT_BIAS;

///////////////////////////////////////////////////////////////
/// TWEAKABLES ////////////////////////////////////////////////
///////////////////////////////////////////////////////////////

float3 AimShadowUpVector <
	string UIName = "\"Up\" Vector";
	// bool Normalized = 1;
> = { 0.0f, 0.0f, 1.0f};

/************* "UN-TWEAKABLES" THAT SHOULD REMAIN CONSANT **************/

float4 AimShadowClearColor <
	string UIWidget = "color";
	string UIName = "none";
> = {1,1,1,0.0};

float AimClearDepth <string UIWidget = "none";> = 1.0;

/************* "UN-TWEAKABLES" TRACKED BY CPU APPLICATION **************/

float4x4 AimWorldITXf : WorldInverseTranspose <string UIWidget="None";>;
float4x4 AimWorldXf : World <string UIWidget="None";>;
float4x4 AimViewIXf : ViewInverse <string UIWidget="None";>;

// float4x4 AimWorldViewProjXf : WorldViewProjection <string UIWidget="None";>;
// float4x4 AimWorldViewITXf : WorldViewInverseTranspose <string UIWidget="None";>;
// float4x4 AimWorldViewXf : WorldView <string UIWidget="None";>;
// float4x4 AimViewXf : View <string UIWidget="None";>;
// float4x4 AimViewITXf : ViewInverseTranspose <string UIWidget="None";>;

/**********************************************************/
/************* SHARED DATA STRUCTS ************************/
/**********************************************************/

/* data from application vertex buffer */
struct AimShadowAppData {
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;
    float4 Normal	: NORMAL;
};

// Connector from vertex to pixel shader
struct AimShadowVertexOutput {
    float4 HPosition	: POSITION;
    float2 UV		: TEXCOORD0;
    float3 LightVec	: TEXCOORD1;
    float3 WNormal	: TEXCOORD2;
    float3 WView	: TEXCOORD3;
    float4 LP		: TEXCOORD4;	// current position in light-projection space
};

/////////////////////////////////////////////////////////
// VM func, executed by CPU /////////////////////////////
/////////////////////////////////////////////////////////

//
// returns the shadow projection matrix
//
float4x4 aimShadowProjXf(float3 LampPt,			// center of projection -- where the lamp is
						float3 ShadAimPt,	// the aim point
						float3 UpVect,			// an arbitrary user-defined "up" vector
						float ShadRad,			// radius of the aim sphere
						float NearAdjust,		// near-clipping fudge factor
						float FarAdjust)		// far-clipping fudge factor
{
	float3 aimVec = (ShadAimPt-LampPt);
	float d = length(aimVec);
	float shadCone = asin(ShadRad/length(aimVec));
	float3 aimN = normalize(aimVec);
	float4x4 spotXf = nv_spot_xf(LampPt,aimN,UpVect);
	float shadHither = max(0.01,(d-(ShadRad+NearAdjust)));
	float shadYon = d+ShadRad+FarAdjust;
	float4x4 projXf = nv_spot_proj_xf(spotXf,
									shadCone,
									shadHither,
									shadYon);
	return projXf;
}

////////////////////////////////////////////////////////////////////////////////
/// Vertex Shaders /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

//
// Render from the lamp's POV --
//		This one goes with "aimMakeShadowVS()"
//
AimShadowVertexOutput aimMakeShadowVS(AimShadowAppData IN,
					uniform float4x4 ShadowXf,	// typically created from aimShadowProjXf()
					uniform float3 LampPos		// location of lamp in world coords
) {
    AimShadowVertexOutput OUT = (AimShadowVertexOutput)0;
    OUT.WNormal = mul(IN.Normal,AimWorldITXf).xyz;
    float4 Po = float4(IN.Position.xyz,(float)1.0);	// object coordinates
    float4 Pw = mul(Po,AimWorldXf);			// world coordinates
    float4 Pl = mul(Pw,ShadowXf);  // "P" in light coords
    OUT.LP = Pl;		// view coords (also lightspace projection coords in this case)
    OUT.WView = normalize(AimViewIXf[3].xyz - Pw.xyz);	// obj coords
    OUT.HPosition = Pl; // screen clipspace coords
    OUT.UV = IN.UV.xy;	// just pass-through
    OUT.LightVec = LampPos - Pw.xyz; // in world coords
    return OUT;
}

/*
//
// Typical VS for "normal" pass -- note that OUT.LP must STILL be calculated, it's
//		used by the pixel shader to determine in/out of shadowing
//
AimShadowVertexOutput mainCamVS(AimShadowAppData IN,
					uniform float4x4 ShadowXf)
{
    AimShadowVertexOutput OUT = (AimShadowVertexOutput)0;
    OUT.WNormal = mul(IN.Normal,AimWorldITXf).xyz;
    float4 Po = float4(IN.Position.xyz,(float)1.0);	// object coordinates
    float4 Pw = mul(Po,AimWorldXf);			// world coordinates
    float4 Pl = mul(Pw,ShadowXf);  // "P" in light coords
    OUT.LP = Pl;		// F:\devrel\SDK\MEDIA\HLSL\view coords
    OUT.WView = normalize(AimViewIXf[3].xyz - Pw.xyz);	// obj coords
    OUT.HPosition = mul(Po,WorldViewProjXf);	// screen clipspace coords
    OUT.UV = IN.UV.xy;
    OUT.LightVec = PointLightPos - Pw.xyz; // in world coords
    return OUT;
}
*/

/*********************************************************/
/*********** pixel shader ********************************/
/*********************************************************/

//
// This one goes with "aimMakeShadowVS()"
//
float4 aimMakeShadowPS(AimShadowVertexOutput IN) : COLOR
{
    return float4(IN.LP.zzz/IN.LP.w,1);
}

//
// Utility function for pixel shaders to use this shadow map
//
float aim_shadow(float4 LP,	// current shaded point in light-projected coordinates
	uniform sampler ShadowMapSampler,	// obvious
	uniform float ShadowBiasing
) {
    float2 nuv = float2(.5,-.5)*LP.xy/LP.w + float2(.5,.5);
    float shadMapDepth = tex2D(ShadowMapSampler,nuv).x;
    float depth = (LP.z/LP.w) - ShadowBiasing;
    float shad = 1-(shadMapDepth<depth);
	return (shad);
}

/*
//
// Example pixel shader that uses aim_shadow()
//
float4 shadowedPlasticPS(AimShadowVertexOutput IN) : COLOR
{
    float3 Nn = NORM(IN.WNormal);
    float3 Vn = NORM(IN.WView);
    float3 Ln = normalize(IN.LightVec);
    float3 Hn = NORM(Vn + Ln);
    float hdn = dot(Hn,Nn);
    float ldn = dot(Ln,Nn);
    float4 litVec = lit(ldn,hdn,SpecExpon);
    ldn = litVec.y * PointLightIntensity;
    float3 ambiContrib = SurfColor * AmbiLightColor;
    float3 diffContrib = SurfColor*(Kd*ldn * PointLightColor);
    float3 specContrib = ((ldn * litVec.z * Ks) * PointLightColor);
    float3 result = diffContrib + specContrib;
    float shadowed = aim_shadow(IN.LP,ShadSampler,ShadBias);
    return float4((shadowed*result)+ambiContrib,1);
}
*/

/////////////////////////////////////////////////////////////
// Techniques ///////////////////////////////////////////////
/////////////////////////////////////////////////////////////

// Typical pass to create a shadow map
//	pass MakeShadow <
//		string Script = "RenderColorTarget0=ShadMap;"
//						"RenderDepthStencilTarget=ShadDepthTarget;"
//						"ClearSetColor=AimShadowClearColor;"
//						"ClearSetDepth=AimClearDepth;"
//						"Clear=Color;"
//						"Clear=Depth;"
//						"Draw=geometry;";
//	> {
//		VertexShader = compile vs_2_0 aimMakeShadowVS(shadowProjXf(),PointLightPos);
//		ZEnable = true;
//		ZWriteEnable = true;
//		CullMode = None;
//		PixelShader = compile ps_2_a aimMakeShadowPS();
//	}

#endif /* _H_AIMSHADOWS */

/***************************** eof ***/
