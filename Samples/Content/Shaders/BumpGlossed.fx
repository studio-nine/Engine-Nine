/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/BumpGlossed.fx#1 $

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
    Bumpy, fresnel-shiny, dielectric, textured, with two quadratic-falloff lights

******************************************************************************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=DirectX9;";
> = 0.8;

/************* TWEAKABLES **************/

half4x4 WorldIT : WorldInverseTranspose < string UIWidget = "None"; >;
half4x4 WorldViewProj : WorldViewProjection < string UIWidget = "None"; >;
half4x4 World : World < string UIWidget = "None"; >;
half4x4 ViewInv : ViewInverse < string UIWidget = "None"; >;

////////////////////////////////////////////// lamp 1

half4 LightPos1 : Position <
    string Object = "Pointlight";
    string Space = "World";
> = {1.0f, 1.0f, -1.0f, 0.0f};

half4 LightColor1 : Specular <
    string UIName =  "Light1 Color";
    string Object = "Pointlight";
    string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f, 1.0f};

half LightIntensity1
<
    string UIWidget = "slider";
    half UIMin = 1.0;
    half UIMax = 50.0;
    half UIStep = 0.1;
    string UIName =  "lamp1 power";
> = 2.0;

////////////////////////////////////////////// lamp 2

half4 LightPos2 : Position
<
    string Object = "Pointlight";
    string Space = "World";
> = {-1.0f, 0.0f, 1.0f, 0.0f};

half4 LightColor2 : Specular <
    string UIName =  "Light2 Color";
    string Object = "Pointlight";
    string UIWidget = "Color";
> = {0.5f, 0.5f, 1.0f, 1.0f};


half LightIntensity2
<
    string UIWidget = "slider";
    half UIMin = 1.0;
    half UIMax = 50.0;
    half UIStep = 0.1;
    string UIName =  "lamp2 power";
> = 0.5;

////////////////////////////////////////////// surface

half4 AmbiColor : Ambient <
    string UIName =  "Ambient Light Color";
    string UIWidget = "Color";
> = {0.07f, 0.07f, 0.07f, 1.0f};

half4 SurfColor : Diffuse
<
    string UIName =  "Surface Color";
    string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f, 1.0f};

half Kd <
    string UIWidget = "slider";
    half UIMin = 0.0;
    half UIMax = 1.5;
    half UIStep = 0.01;
    string UIName =  "diffuse";
> = 1.0;

half Ks <
    string UIWidget = "slider";
    half UIMin = 0.0;
    half UIMax = 1.5;
    half UIStep = 0.01;
    string UIName =  "specular";
> = 1.0;


half SpecExpon : SpecularPower <
    string UIWidget = "slider";
    half UIMin = 1.0;
    half UIMax = 128.0;
    half UIStep = 1.0;
    string UIName =  "specular power";
> = 12.0;

half Bumpy <
    string UIWidget = "slider";
    half UIMin = 0.0;
    half UIMax = 10.0;
    half UIStep = 0.1;
    string UIName =  "bumpiness";
> = 1.0;

half Kr <
    string UIWidget = "slider";
    half UIMin = 0.0;
    half UIMax = 1.5;
    half UIStep = 0.01;
    string UIName =  "Reflection Max";
> = 1.0;


half KrMin <
    string UIWidget = "slider";
    half UIMin = 0.0;
    half UIMax = 0.2;
    half UIStep = 0.001;
    string UIName =  "Reflection Min";
> = 0.002;

half FresExp : SpecularPower <
    string UIWidget = "slider";
    half UIMin = 1.0;
    half UIMax = 7.0;
    half UIStep = 0.1;
    string UIName =  "edging";
> = 5.0;

/////////////////////////////////

texture colorTexture : Diffuse <
    string ResourceName = "default_color.dds";
    string ResourceType = "2D";
>;

texture normalTexture : NORMAL <
    string ResourceName = "default_bump_normal.dds";
    string ResourceType = "2D";
>;

texture glossTexture : SPECULAR <
    string ResourceName = "default_gloss.dds";
    string ResourceType = "2D";
>;

texture envTexture : ENVIRONMENT <
    string ResourceName = "default_reflection.dds";
    string ResourceType = "Cube";
>;

////////

sampler2D colorSampler = sampler_state {
	Texture = <colorTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

sampler2D normalSampler = sampler_state {
	Texture = <normalTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

sampler2D glossSampler = sampler_state {
	Texture = <glossTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

samplerCUBE envSampler = sampler_state {
	Texture = <envTexture>;
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
    half4 LightVec1	: TEXCOORD1;
    half3 WorldNormal	: TEXCOORD2;
    half3 WorldPos	: TEXCOORD3;
    half3 WorldEyePos	: TEXCOORD4;
    half3 WorldTangent	: TEXCOORD5;
    half3 WorldBinorm	: TEXCOORD6;
    half4 LightVec2	: TEXCOORD7;
};

/*********** vertex shader ******/

vertexOutput mainVS(appdata IN) {
    vertexOutput OUT;
    OUT.WorldNormal = mul(IN.Normal, WorldIT).xyz;
    OUT.WorldTangent = mul(IN.Tangent, WorldIT).xyz;
    OUT.WorldBinorm = mul(IN.Binormal, WorldIT).xyz;
    half4 tempPos = half4(IN.Position.x,IN.Position.y,IN.Position.z,1.0);
    half3 worldSpacePos = mul(tempPos, World).xyz;
    OUT.WorldPos = worldSpacePos;
    half3 L1 = LightPos1 - worldSpacePos;
    half3 L2 = LightPos2 - worldSpacePos;
    half Ld1 = 1.0/length(L1);
    half Ld2 = 1.0/length(L2);
    OUT.LightVec1 = half4(L1.x,L1.y,L1.z,Ld1);
    OUT.LightVec2 = half4(L2.x,L2.y,L2.z,Ld2);
    OUT.TexCoord = IN.UV;
    OUT.WorldEyePos = ViewInv[3].xyz;
    OUT.HPosition = mul(tempPos, WorldViewProj);
    return OUT;
}


/********* pixel shader ********/

half4 mainPS(vertexOutput IN) : COLOR {
    half4 map = tex2D(colorSampler,IN.TexCoord.xy);
    half3 bumps = Bumpy * (tex2D(normalSampler,IN.TexCoord.xy).xyz-(0.5).xxx);
    half gloss = Ks * tex2D(glossSampler,IN.TexCoord.xy).x;
    half3 Nn = normalize(IN.WorldNormal);
    half3 Tn = normalize(IN.WorldTangent);
    half3 Bn = normalize(IN.WorldBinorm);
    half3 Nb = Nn + (bumps.x * Tn + bumps.y * Bn);
    Nb = normalize(Nb);
    half3 Vn = normalize(IN.WorldEyePos - IN.WorldPos);

    half3 Ln = normalize(IN.LightVec1.xyz);
    half3 Hn = normalize(Vn + Ln);
    half hdn = pow(max(0,dot(Hn,Nb)),SpecExpon) * gloss;
    half ldn = dot(Ln,Nb);
    ldn = (IN.LightVec1.w* max(0.0,ldn)) * LightIntensity1;
    half4 diffContrib = IN.LightVec1.w*(ldn*LightColor1);
    half4 specContrib = IN.LightVec1.w*((ldn * hdn) * LightColor1);

    // second lamp
    Ln = normalize(IN.LightVec2.xyz);
    Hn = normalize(Vn + Ln);
    hdn = pow(max(0,dot(Hn,Nb)),SpecExpon) * gloss;
    ldn = dot(Ln,Nb);
    ldn = (IN.LightVec2.w* max(0.0,ldn)) * LightIntensity2;
    diffContrib = diffContrib + IN.LightVec2.w*(ldn*LightColor2);
    specContrib = specContrib + IN.LightVec2.w*((ldn * hdn) * LightColor2);

    half3 reflVect = reflect(Vn,Nb);
    half vdn = dot(Vn,Nb);
    half fres = KrMin + (Kr-KrMin) * pow(1-abs(vdn),FresExp);
    half4 reflColor = fres * texCUBE(envSampler,half4(reflVect.x, reflVect.y, reflVect.z, 1));

    half4 result = (SurfColor*map*(Kd*diffContrib+AmbiColor)) + specContrib + reflColor;
    return result;
}

/*************/

technique DirectX9 <
	string Script = "Pass=p0;";
> {
	pass p0 <
	string Script = "Draw=geometry;";
> {
		VertexShader = compile vs_2_0 mainVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_2_b mainPS();
	}
}

/***************************** eof ***/
