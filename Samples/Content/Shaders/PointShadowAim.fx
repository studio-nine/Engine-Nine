/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/PointShadowAim.fx#1 $

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

Simple shadow map example using FP16 textures.

The lightsource is a point lamp centered at "PointLightPos"

The shadow is projected to what I call an "aim sphere"

The shadow is projected from there to aim at the location "ShadowCenter" and
	is guaranteed to enclosed a sphere of "ShadowRadius" centered there (imagine
	a sphere centered at "ShadowRadius" -- the cone of the shadow projection will
	exactly enclose that sphere). The "Fudge" values let you tweak the near/far
	clipping planes of the shadow (which would normally be at the boundaries
	of the aim sphere).
	
Advantages of this method:
	* Shadow is always aiming at the right thing
	* Point/cone/whatever lamps can be used without worrying about shadow projections
	* Shadow map contains only the info you want
	* Regardless of location or distance of light, shadow map always contains
		about the same # of overall texels -- so shadow-quality crawling, a
		concern for animated objects or lamps, ever presents a problem.

$Date: 2004/09/24 $
$Author: jallen $

******************************************************************************/

#include "AimShadows.fxh"

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	string Script = "Technique=Main;";
> = 0.8; // version #

float4 ClearColor <
	string UIWidget = "color";
	string UIName = "background";
> = {0,0,0,0.0};

float ClearDepth <string UIWidget = "none";> = 1.0;


///////////////////////////////////////////////////////////////
/// TWEAKABLES ////////////////////////////////////////////////
///////////////////////////////////////////////////////////////

////////////////////////////////////////////// spot light

float3 PointLightPos : POSITION <
	string UIName = "Light Pos";
	string Object = "PointLight";
	string Space = "World";
> = {-1.0f, 1.0f, 0.0f};

float3 PointLightColor : Specular <
	string UIName = "Light Color";
	string Object = "PointLight";
	string UIWidget = "Color";
> = {0.8f, 1.0f, 0.4f};

float PointLightIntensity <
	string UIName = "Light Intensity";
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 2;
	float UIStep = 0.1;
> = 1;

////////////////////////////////////////////// ambient light

float3 AmbiLightColor : Ambient
<
    string UIName = "Ambient Light Color";
> = {0.07f, 0.07f, 0.07f};

/////////////// Shadow and Aiming Parameters

AIM_SHADOW_PARAMS(ShadowCenter,ShadRadius,ShadNear,ShadFar,ShadBias)


////////////////////////////////////////////// surface

float3 SurfColor : Diffuse
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

////////////////////////////////////////////////////////
/// TEXTURES ///////////////////////////////////////////
////////////////////////////////////////////////////////

AIM_SHAD_TEX(ShadMap,ShadSampler)
AIM_DEPTH_TEX(ShadDepthTarget)

/////////////////////////////////////////////////////////////

/************* "UN-TWEAKABLES," TRACKED BY CPU APPLICATION **************/

float4x4 WorldITXf : WorldInverseTranspose <string UIWidget="None";>;
float4x4 WorldViewProjXf : WorldViewProjection <string UIWidget="None";>;
float4x4 WorldXf : World <string UIWidget="None";>;
float4x4 ViewIXf : ViewInverse <string UIWidget="None";>;
float4x4 WorldViewITXf : WorldViewInverseTranspose <string UIWidget="None";>;
float4x4 WorldViewXf : WorldView <string UIWidget="None";>;
float4x4 ViewXf : View <string UIWidget="None";>;
float4x4 ViewITXf : ViewInverseTranspose <string UIWidget="None";>;

////////////////////////////////////////////////////////////////////////////
/// SHADER CODE BEGINS /////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////

/*********************************************************/
/*********** VM funcs ************************************/
/*********************************************************/

//
// returns the shadow projection matrix - just a wrapper for
//	aimShadowProjXf() using this FX file's global variables
//
float4x4 shadowProjXf()
{
	return aimShadowProjXf(PointLightPos,ShadowCenter,AimShadowUpVector,
					ShadRadius,ShadNear,ShadFar);
}

////////////////////////////////////////////////////////////////////////////////
/// Vertex Shaders /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// from scene camera POV
AimShadowVertexOutput mainCamVS(AimShadowAppData IN,
					uniform float4x4 ShadowXf)
{
    AimShadowVertexOutput OUT = (AimShadowVertexOutput)0;
    OUT.WNormal = mul(IN.Normal,WorldITXf).xyz; // world coords
    float4 Po = float4(IN.Position.xyz,(float)1.0);	// "P" in object coordinates
    float4 Pw = mul(Po,WorldXf);			// "P" in world coordinates
    float4 Pl = mul(Pw,ShadowXf);			// "P" in light coords...
    OUT.LP = Pl;							// ...for pixel-shader shadow calcs
    OUT.WView = normalize(ViewIXf[3].xyz - Pw.xyz);	// world coords
    OUT.HPosition = mul(Po,WorldViewProjXf);	// screen clipspace coords
    OUT.UV = IN.UV.xy;							// pass-thru
    OUT.LightVec = PointLightPos - Pw.xyz;		// world coords
    return OUT;
}

/*********************************************************/
/*********** pixel shader ********************************/
/*********************************************************/

float4 useShadowPS(AimShadowVertexOutput IN) : COLOR
{
    float3 Nn = normalize(IN.WNormal);
    float3 Vn = normalize(IN.WView);
    float3 Ln = normalize(IN.LightVec);
    float3 Hn = normalize(Vn + Ln);
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

////////////////////////////////////////////////////////////////////
/// TECHNIQUES /////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////

technique Main <
	string Script = "Pass=MakeShadow;"
					"Pass=UseShadow;";
> {
	pass MakeShadow <
		string Script = "RenderColorTarget0=ShadMap;"
						"RenderDepthStencilTarget=ShadDepthTarget;"
						"ClearSetColor=AimShadowClearColor;"
						"ClearSetDepth=AimClearDepth;"
						"Clear=Color;"
						"Clear=Depth;"
						"Draw=geometry;";
	> {
		VertexShader = compile vs_2_0 aimMakeShadowVS(shadowProjXf(),PointLightPos);
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_2_0 aimMakeShadowPS();
	}
	pass UseShadow <
		string Script = "RenderColorTarget0=;"
						"RenderDepthStencilTarget=;"
						"ClearSetColor=ClearColor;"
						"ClearSetDepth=ClearDepth;"
						"Clear=Color;"
						"Clear=Depth;"
						"Draw=geometry;";
	> {
		VertexShader = compile vs_2_0 mainCamVS(shadowProjXf());
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_2_0 useShadowPS();
	}
}

/***************************** eof ***/
