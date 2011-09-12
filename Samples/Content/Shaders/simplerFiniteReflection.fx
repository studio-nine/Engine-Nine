/*********************************************************************NVMH3****
File:  $Header: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/simplerFiniteReflection.fx#1 $

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
	$Date: 2004/09/24 $

Similar to the finite reflection code from GPU GEMS, but
simplifed for the FX Composer UI. Instead of the app (FX Composer)
sending full transform and inverse transforms for the reflection
coordinate system w.r.t. world space, this version lets you select
a center and a scale -- the vertex shader will then construct and
use the appropriate matrices.

For simple interaction, you can attach the reflection center to
an object like a pointlight (though this light will emit no
illumination).

******************************************************************************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=refl;";
> = 0.8;

/************* TWEAKABLES **************/

half4x4 WorldIT : WORLDINVERSETRANSPOSE < string UIWidget="None"; >;
half4x4 WorldViewProj : WORLDVIEWPROJECTION < string UIWidget="None"; >;
half4x4 World : WORLD < string UIWidget="None"; >;
half4x4 ViewIT : VIEWINVERSE < string UIWidget="None"; >;

/************************************************************/

half3 SurfColor : DIFFUSE <
    string UIName = "Surface Color";
> = {1,1,1};

half Kr <
    string UIWidget = "slider";
    half UIMin = 0.0;
    half UIMax = 2.0;
    half UIStep = 0.01;
    string UIName = "Max Reflection";
> = 0.95;

half KrMin <
    string UIWidget = "slider";
    half UIMin = 0.0;
    half UIMax = 0.9;
    half UIStep = 0.01;
    string UIName = "Min Reflection";
> = 0.25;

half FresExp : SpecularPower <
    string UIWidget = "slider";
    half UIMin = 1.0;
    half UIMax = 5.0;
    half UIStep = 0.1;
    string UIName = "Fresnel Exponent";
> = 4.0;

half Bumpiness <
    string UIWidget = "slider";
    half UIMin = 0.0;
    half UIMax = 1.2;
    half UIStep = 0.01;
    string UIName = "bumpy";
> = 0.5;

half3 ReflCenter : POSITION <
    string UIName = "Center of Reflection Environment";
    string Object = "Pointlight";
> = {0,0,0};

half ReflScale : UNITSSCALE <
    string UIWidget = "slider";
    half UIMin = 2.0;
    half UIMax = 20.0;
    half UIStep = 0.1;
    string UIName = "Reflection Scale";
> = 5.0;

//////////////////////
// Textures //////////
//////////////////////

texture NormalTexture : DIFFUSE <
    string ResourceName = "default_color.dds";
    string ResourceType = "2D";
>;

sampler2D NormalSampler = sampler_state
{
	Texture = <NormalTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

//////////

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

/************* DATA STRUCTS **************/

/* data from application vertex buffer */
struct appdata {
    half3 Position	: POSITION;
    half4 UV		: TEXCOORD0;
    half4 Normal	: NORMAL;
    half4 Tangent	: TANGENT0;
    half4 Binormal	: BINORMAL0;
};

/* data passed from vertex shader to pixel shader */
struct vertexOutput {
    half4 HPosition	: POSITION;
    half4 TexCoord	: TEXCOORD0;
    half3 UserNormal	: TEXCOORD1;
    half3 WorldEyeVec	: TEXCOORD2;
    half3 UserTangent	: TEXCOORD3;
    half3 UserBinorm	: TEXCOORD4;
    half3 UserEyeVec	: TEXCOORD5;
    half3 UserPos	: TEXCOORD6;
};

/*********** vertex shader ******/

vertexOutput mainVS(appdata IN)
{
    vertexOutput OUT;
    half4x4 userXf;
    half ir = 1.0/ReflScale;
    userXf[0] = half4(ir,0,0,ReflCenter.x);
    userXf[1] = half4(0,ir,0,ReflCenter.y);
    userXf[2] = half4(0,0,ir,ReflCenter.z);
    userXf[3] = half4(0,0,0,1);
    half4x4 userITXf;
    userITXf[0] = half4(ReflScale,0,0,0);
    userITXf[1] = half4(0,ReflScale,0,0);
    userITXf[2] = half4(0,0,ReflScale,0);
    userITXf[3] = half4(-ReflCenter.xyz,1);
    half4 Po = half4(IN.Position.xyz,1.0);
    half4 Pw = mul(Po, World);
    OUT.TexCoord = IN.UV;
    half3 WorldEyePos = ViewIT[3].xyz;
    half4 UserEyePos = mul(half4(WorldEyePos,1.0),userXf);
    half4 hpos = mul(Po, WorldViewProj);
    half4 Pu = mul(Pw,userXf);
    half4 Nw = mul(IN.Normal, WorldIT);
    half4 Tw = mul(IN.Tangent, WorldIT);
    half4 Bw = mul(IN.Binormal, WorldIT);
    OUT.UserEyeVec = (UserEyePos - Pu).xyz;
    OUT.WorldEyeVec = normalize(WorldEyePos - Pw.xyz);
    OUT.UserNormal = mul(Nw,userITXf).xyz;
    OUT.UserTangent = mul(Tw,userITXf).xyz;
    OUT.UserBinorm = mul(Bw,userITXf).xyz;
    OUT.UserPos = mul(Pw,userXf).xyz;
    OUT.HPosition = hpos;
    return OUT;
}

/********* pixel shader ********/

// PS with box-filtered step function
half4 mainPS(vertexOutput IN) : COLOR
{
	half3 Vu = normalize(IN.UserEyeVec);
	half3 Nu = normalize(IN.UserNormal);
	half3 Tu = normalize(IN.UserTangent);
	half3 Bu = normalize(IN.UserBinorm);
	half3 bumps = Bumpiness * (tex2D(NormalSampler,IN.TexCoord.xy).xyz-(0.5).xxx);
	half3 Nb = Nu + (bumps.x * Tu + bumps.y * Bu);
	Nb = normalize(Nb);	// expressed in user-coord space
	half vdn = dot(Vu,Nu);
	half fres =  KrMin + (Kr-KrMin) * pow((1.0-abs(vdn)),FresExp);
	half3 reflVect = -normalize(reflect(Vu,Nb));	// yes, normalize in this instance!
	// we are using the coord sys of the reflection, so to simplify things we assume (radius == 1)
	half b = -2.0 * dot(reflVect,IN.UserPos);
	half c = dot(IN.UserPos,IN.UserPos) - 1.0;
	half discrim = b*b - 4.0*c;
	bool hasIntersects = false;
	half nearT = 0;
	half3 reflColor = half3(1,0,0);
	if (discrim > 0) {
		discrim = sqrt(discrim);
		nearT = -(discrim-b)/2.0;
		hasIntersects=true;
		if (nearT <= 0.0001) {
			nearT = (discrim - b)/2.0;
			hasIntersects = (nearT>0.0001);
		}
	}
	if (hasIntersects) {
		reflVect = -IN.UserPos + nearT*reflVect;
		reflColor = fres * texCUBE(EnvSampler,reflVect).xyz;
	}
	half3 result = SurfColor * reflColor.xyz;
	return half4(result,1);
}

/*************/

technique refl <
	string Script = "Pass=p0;";
> {
    pass p0  <
		string Script = "Draw=geometry;";
    > {		
		VertexShader = compile vs_2_0 mainVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_2_0 mainPS();
    }
}

/***************************** eof ***/
