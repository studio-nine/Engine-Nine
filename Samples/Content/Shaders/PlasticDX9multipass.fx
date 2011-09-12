/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/PlasticDX9multipass.fx#1 $

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
    This example shows how to apply one-pass-per-light
		shading to a plastic surface. The shading is done in DX9
		ps2 - pixel shaded.
    In this example, there are two point sources, one without falloff and one
        with "real" quadratic falloff, an infinitely-distant source,
    	ambient light, and a fresnel reflection.
    EffectEdit compatible, but it won't load all the transforms.

******************************************************************************/

string XFile = "tiger.x";

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=plastic_multipass;";
> = 0.8;

/************* UN-TWEAKABLES **************/

half4x4 WorldITXf : WorldInverseTranspose < string UIWidget="None"; > = {1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1};
half4x4 WvpXf : WorldViewProjection < string UIWidget="None"; >;
half4x4 WorldXf : World < string UIWidget="None"; >;
half4x4 ViewIXf : ViewInverse < string UIWidget="None"; >;

/*********** Tweakables **********************/

// first lamp - no falloff

half3 LightPosP : Position
<
    string Object = "PointLight";
    string Space = "World";
> = {100.0f, 100.0f, -100.0f};

half3 LightColorP : Specular
<
    string UIName =  "Point Light Color";
    string UIWidget = "Color";
> = {1.0f, 0.8f, 0.5f};

half LightIntensityP
<
    string UIName =  "Point Light Strength";
> = 1;

///// second lamp - quadratic falloff

half3 LightPosQ : Position
<
    string Object = "PointLight";
    string Space = "World";
> = {-10.0f, 15.0f, 3.0f};

half3 LightColorQ : Specular
<
    string UIName =  "Quadratic Light Color";
    string UIWidget = "Color";
> = {0.2f, 0.3f, 1.0f};

half LightIntensityQ
<
    string UIName =  "Quadratic Light Strength";
> = 1000;

///// third lamp -- distant source

half3 LightDirD : Direction
<
    string UIName = "Light Direction"; 
    string Object = "DirectionalLight";
    string Space = "World";
> = {-10.0f, 15.0f, 30.0f};

half3 LightColorD : Specular
<
    string UIName =  "Distant Light Color";
    string UIWidget = "Color";
> = {0.2f, 0.3f, 1.0f};

half LightIntensityD
<
    string UIName =  "Distant Light Strength";
> = 0.5;

///// ambient light

half3 AmbiColor : Ambient
<
    string UIName =  "Ambient Color";
    string UIWidget = "Color";
> = {0.1f, 0.1f, 0.1f};

///// Surface Parameters ///////////////////////

half4 SurfColor : DIFFUSE
<
    string UIName =  "Surface Color";
    string UIWidget = "Color";
> = {0.8f, 0.8f, 1.0f, 1.0f};

half Ks
<
    string UIWidget = "slider";
    half UIMin = 0.0;
    half UIMax = 1.0;
    half UIStep = 0.01;
    string UIName = "specular intensity";
> = 0.5;

half SpecExpon : SpecularPower
<
    string UIWidget = "slider";
    half UIMin = 1.0;
    half UIMax = 128.0;
    half UIStep = 1.0;
    string UIName =  "specular power";
> = 30.0;

// reflection parameters
half Kr
<
    string UIWidget = "slider";
    half UIMin = 0.0;
    half UIMax = 1.0;
    half UIStep = 0.01;
    string UIName =  "max reflection strength, along edges";
> = 1.0;

half KrMin
<
    string UIWidget = "slider";
    half UIMin = 0.0;
    half UIMax = 1.0;
    half UIStep = 0.01;
    string UIName =  "min reflection strength, facing you";
> = 0.05;

// reducing this gives a more "thick clearcoat" appearance
half FresnelExpon : SpecularPower
<
    string UIWidget = "slider";
    half UIMin = 1.0;
    half UIMax = 5.0;
    half UIStep = 0.1;
    string UIName =  "Schlick Approximation Exponent";
> = 5.0;

/// textures ///////////////////////////////////

texture ColorTexture : DIFFUSE
<
    string ResourceName = "tiger.bmp";
    string ResourceType = "2D";
>;

sampler2D ColorSamp = sampler_state
{
	Texture = <ColorTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

/// environment map ///

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
};

/************* DATA STRUCTS **************/

/* data from application vertex buffer */
struct appdata {
    half3 Position	: POSITION;
    half4 UV		: TEXCOORD0;
    half4 Normal	: NORMAL;
};

// vertex outputs - from vert shader to pixel shader //

// used for ambient/refl pass
struct ambiReflVOut {
    half4 HPosition	: POSITION;
    half2 UV		: TEXCOORD0;
    half3 WorldNormal	: TEXCOORD1;
    half3 WorldEyeVec	: TEXCOORD2;
};

// used for all other passes
struct vertexOutput {
    half4 HPosition	: POSITION;
    half2 UV		: TEXCOORD0;
    half3 LightVec	: TEXCOORD1;
    half3 WorldNormal	: TEXCOORD2;
    half3 WorldEyeVec	: TEXCOORD3;
};

/*********** vertex shaders ******/

// all Vertex shaders in this FX file will need to determine
//   clipspace position, worldspace normals, an eye vector & texture coords.
//   "Pw" is used to determine point-lighting vectors
void sharedVS (appdata IN,
    out half4 Ph,
    out half3 N,
    out half3 V,
    out half2 UV,
    out half3 Pw)
{
    N = mul(IN.Normal, WorldITXf).xyz;
    half4 Po = half4(IN.Position.xyz,1);
    Pw = mul(Po, WorldXf).xyz;
    V = (ViewIXf[3].xyz - Pw);
    Ph = mul(Po, WvpXf);
    UV = IN.UV.xy;
}

// vertex shader for ambient/reflection pass
ambiReflVOut ambiReflVS(appdata IN) {
    ambiReflVOut OUT; half3 Pw;
    sharedVS(IN,OUT.HPosition,OUT.WorldNormal,OUT.WorldEyeVec,OUT.UV,Pw);
    return OUT;
}

// vertex shader for point lights
vertexOutput pointlightVS(appdata IN,
    uniform half3 LightPos
) {
    vertexOutput OUT; half3 Pw;
    sharedVS(IN,OUT.HPosition,OUT.WorldNormal,OUT.WorldEyeVec,OUT.UV,Pw);
    OUT.LightVec = (LightPos - Pw);
    return OUT;
}

// vertex shader for distant (directional) lights
vertexOutput distantlightVS(appdata IN,
    uniform half3 LightDir
) {
    vertexOutput OUT; half3 Pw;
    sharedVS(IN,OUT.HPosition,OUT.WorldNormal,OUT.WorldEyeVec,OUT.UV,Pw);
    OUT.LightVec = -LightDir;
    return OUT;
}

//////////////////////////////////
/********* pixel shaders ********/
//////////////////////////////////

// ambient/refl pass
half4 ambiReflPS(ambiReflVOut IN) : COLOR {
    half4 diffContrib, specContrib;
    half3 Vn = normalize(IN.WorldEyeVec);
    half3 Nn = normalize(IN.WorldNormal);
    half4 ambiContrib = half4(AmbiColor,1)*SurfColor*tex2D(ColorSamp,IN.UV);
    half vdn=1-abs(dot(Vn,Nn));
    half fres = KrMin + (Kr-KrMin) * pow(vdn,FresnelExpon);
    half3 rv = reflect(Vn,Nn);
    half4 refl = fres * texCUBE(EnvSampler,rv);
    return ambiContrib + refl;
}

// this portion shared for all lamps
half4 sharedPS(vertexOutput IN,
    half4 DiffColor,
    half3 LightColor,
    half Intensity
) {
    half3 Ln = normalize(IN.LightVec);
    half3 Vn = normalize(IN.WorldEyeVec);
    half3 Nn = normalize(IN.WorldNormal);
    half3 Hn = normalize(Vn + Ln);
    half4 lv = lit(dot(Ln,Nn),dot(Hn,Nn),SpecExpon);
    half4 diffContrib = DiffColor * half4((Intensity*lv.y*LightColor + AmbiColor),1);
    half4 specContrib = Ks * lv.z * half4(Intensity*LightColor,0);
    return diffContrib + specContrib;
}

// lamps without falloff
half4 lampPS(vertexOutput IN,
    uniform half3 LightColor,
    uniform half Intensity
) : COLOR {
    return sharedPS(IN,(SurfColor*tex2D(ColorSamp,IN.UV)),LightColor,Intensity);
}

// quadratic lamp
half4 quadPS(vertexOutput IN,
    uniform half3 LightColor,
    uniform half Intensity
) : COLOR {
    half atten = length(IN.LightVec);
    return sharedPS(IN,(SurfColor*tex2D(ColorSamp,IN.UV)),LightColor,Intensity/(atten*atten));
}

/*************/

technique plastic_multipass <
	string Script = "Pass=ambiRefl; Pass=pointlight1; Pass=quadlight1; Pass=distantlight1;";
> {
    // first lay down ambient, reflection, and Z
    //    subsequent passes need not save Z, and they will
    //	be faster because of implicit Z occlusion
    pass ambiRefl <
	string Script = "Draw=geometry;";
> {		
		VertexShader = compile vs_2_0 ambiReflVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_2_0 ambiReflPS();
    }
    // now accumulate point light contribution
    pass pointlight1 <
	string Script = "Draw=geometry;";
> {		
		VertexShader = compile vs_2_0 pointlightVS(LightPosP);
		ZEnable = true;
		ZWriteEnable = false;
		ZFunc = LessEqual;
		CullMode = None;
		AlphaBlendEnable = true;
		SrcBlend = One;
		DestBlend = One;
		PixelShader = compile ps_2_0 lampPS(LightColorP,LightIntensityP);
    }
    // quadratic light contribution
    pass quadlight1 <
	string Script = "Draw=geometry;";
> {		
		VertexShader = compile vs_2_0 pointlightVS(LightPosQ);
		ZEnable = true;
		ZWriteEnable = false;
		ZFunc = LessEqual;
		CullMode = None;
		AlphaBlendEnable = true;
		SrcBlend = One;
		DestBlend = One;
		PixelShader = compile ps_2_0 quadPS(LightColorQ,LightIntensityQ);
    }
    // directional light contribution
    pass distantlight1 <
	string Script = "Draw=geometry;";
> {		
		VertexShader = compile vs_2_0 distantlightVS(LightDirD);
		ZEnable = true;
		ZWriteEnable = false;
		ZFunc = LessEqual;
		CullMode = None;
		AlphaBlendEnable = true;
		SrcBlend = One;
		DestBlend = One;
		PixelShader = compile ps_2_0 lampPS(LightColorD,LightIntensityD);
    }
}

/***************************** eof ***/
