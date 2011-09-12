/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/PlasticDX9.fx#1 $

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
    Simple pixel-shaded point-lit plastic without falloff
    EffectEdit okay.

******************************************************************************/

string XFile = "tiger.x";

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?plastic_textured:plastic_untextured:plastic_fresnel_untextured:plastic_fresnel_textured:vertex_shaded_textured:vertex_shaded_untextured:vertex_shaded_fresnel_textured:vertex_shaded_fresnel_untextured;";
> = 0.8;

/************* UN-TWEAKABLES **************/

half4x4 WorldITXf : WorldInverseTranspose < string UIWidget="None"; > = {1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1};
half4x4 WvpXf : WorldViewProjection < string UIWidget="None"; >;
half4x4 WorldXf : World < string UIWidget="None"; >;
half4x4 ViewIXf : ViewInverse < string UIWidget="None"; >;

/*********** Tweakables **********************/

half4 LightPos : Position
<
    string Object = "PointLight";
    string Space = "World";
> = {10.0f, 10.0f, -10.0f, 0.0f};

half4 LightColor : DIFFUSE
<
    string UIName =  "Light Color";
    string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f, 1.0f};

/////

half4 AmbiColor : Ambient
<
    string UIName =  "Surface Color";
    string UIWidget = "Color";
> = {0.1f, 0.1f, 0.1f, 1.0f};

/////

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
    string UIName =  "specular intensity";
> = 0.5;

half SpecExpon : SpecularPower
<
    string UIWidget = "slider";
    half UIMin = 1.0;
    half UIMax = 128.0;
    half UIStep = 1.0;
    string UIName =  "specular power";
> = 30.0;

half Kr
<
    string UIWidget = "slider";
    half UIMin = 0.0;
    half UIMax = 1.0;
    half UIStep = 0.01;
    string UIName =  "max reflection strength";
> = 1.0;

half KrMin
<
    string UIWidget = "slider";
    half UIMin = 0.0;
    half UIMax = 1.0;
    half UIStep = 0.01;
    string UIName =  "min reflection strength";
> = 0.05;

// reducing this gives a more "thick clearcoat" appearance
half FresnelExpon : SpecularPower
<
    string UIWidget = "slider";
    half UIMin = 1.0;
    half UIMax = 5.0;
    half UIStep = 0.1;
    string UIName =  "Expon used in Schlick Fresnel Func";
> = 5.0;

/// texture ///////////////////////////////////

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

/// environment map

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

/* data passed from vertex shader to pixel shader */
struct vertexOutput {
    half4 HPosition	: POSITION;
    half2 UV		: TEXCOORD0;
    half3 LightVec	: TEXCOORD1;
    half3 WorldNormal	: TEXCOORD2;
    half3 WorldEyeVec	: TEXCOORD3;
};

/* data passed from vertex shader to pixel shader */
struct vertexShadedOutput {
    half4 HPosition	: POSITION;
    half2 UV		: TEXCOORD0;
    half4 DiffColor	: COLOR0;
    half4 SpecColor	: COLOR1;
};

/* data passed from vertex shader to pixel shader */
struct vertexShadedFOutput {
    half4 HPosition	: POSITION;
    half2 UV		: TEXCOORD0;
    half4 Refl		: TEXCOORD1;
    half4 DiffColor	: COLOR0;
    half4 SpecColor	: COLOR1;
};

/*********** utility lighting function, shared by pixel and vertex shading ******/

void phong_lighting(half3 LightVec,half3 ViewVec,half3 Normal,half4 SurfaceColor,
		    out half3 Vn,
		    out half3 Nn,
		    out half4 DiffResult, out half4 SpecResult) {
    half3 Ln = normalize(LightVec);
    Vn = normalize(ViewVec);
    Nn = normalize(Normal);
    half3 Hn = normalize(Vn + Ln);
    half4 lv = lit(dot(Ln,Nn),dot(Hn,Nn),SpecExpon);
    DiffResult = SurfaceColor * (lv.y*LightColor + AmbiColor);
    SpecResult = Ks * lv.z * LightColor;
}

half4 fresnelVec(half3 Vn,half3 Nn)
{
    half vdn=1-abs(dot(Vn,Nn));
    half fres = KrMin + (Kr-KrMin) * pow(vdn,FresnelExpon);
    return half4(reflect(Vn,Nn),fres);
}

/*********** vertex shaders ******/

vertexOutput pixelShadedVS(appdata IN) {
    vertexOutput OUT;
    OUT.WorldNormal = mul(IN.Normal, WorldITXf).xyz;
    half4 Po = half4(IN.Position.xyz,1);
    half3 Pw = mul(Po, WorldXf).xyz;
    OUT.LightVec = (LightPos - Pw);
    OUT.WorldEyeVec = (ViewIXf[3].xyz - Pw);
    OUT.HPosition = mul(Po, WvpXf);
    OUT.UV = IN.UV.xy;
    return OUT;
}

vertexShadedOutput vertexShadedVS(appdata IN) {
    vertexShadedOutput OUT;
    half3 Vn, Nn;
    half4 Po = half4(IN.Position.xyz,1);
    half3 Pw = mul(Po, WorldXf).xyz;
    phong_lighting((LightPos-Pw), (ViewIXf[3].xyz - Pw), mul(IN.Normal, WorldITXf).xyz, SurfColor,
		    Vn, Nn, OUT.DiffColor, OUT.SpecColor);
    OUT.HPosition = mul(Po, WvpXf);
    OUT.UV = IN.UV.xy;
    return OUT;
}

vertexShadedFOutput vertexShadedFVS(appdata IN) {
    vertexShadedFOutput OUT;
    half3 Vn, Nn;
    half4 Po = half4(IN.Position.xyz,1);
    half3 Pw = mul(Po, WorldXf).xyz;
    phong_lighting((LightPos-Pw), (ViewIXf[3].xyz - Pw), mul(IN.Normal, WorldITXf).xyz, SurfColor,
		    Vn, Nn, OUT.DiffColor, OUT.SpecColor);
    OUT.Refl = fresnelVec(Vn,Nn);
    OUT.HPosition = mul(Po, WvpXf);
    OUT.UV = IN.UV.xy;
    return OUT;
}

/********* pixel shader ********/

half4 sharedPS(vertexOutput IN,half4 DiffColor) :COLOR {
    half4 diffContrib, specContrib;
    half3 Vn, Nn;
    phong_lighting(IN.LightVec, IN.WorldEyeVec, IN.WorldNormal, DiffColor,
		    Vn, Nn, diffContrib,specContrib);
    return diffContrib + specContrib;
}

half4 sharedFPS(vertexOutput IN,half4 DiffColor) :COLOR {
    half4 diffContrib, specContrib;
    half3 Vn, Nn;
    phong_lighting(IN.LightVec, IN.WorldEyeVec, IN.WorldNormal, DiffColor,
		    Vn, Nn, diffContrib,specContrib);
    half4 rv = fresnelVec(Vn,Nn);
    half4 refl = rv.w * texCUBE(EnvSampler,rv.xyz);
    return diffContrib + specContrib + refl;
}

half4 untexturedPS(vertexOutput IN):COLOR {return sharedPS(IN,SurfColor);}
half4 texturedPS(vertexOutput IN):COLOR {return sharedPS(IN,(SurfColor*tex2D(ColorSamp,IN.UV)));}
half4 untexturedFPS(vertexOutput IN):COLOR {return sharedFPS(IN,SurfColor);}
half4 texturedFPS(vertexOutput IN):COLOR {return sharedFPS(IN,SurfColor*tex2D(ColorSamp,IN.UV));}

half4 vertexShadedPS(vertexShadedOutput IN) : COLOR {
    return IN.DiffColor * tex2D(ColorSamp,IN.UV.xy) + IN.SpecColor;
}

half4 vertexShadedFPS(vertexShadedFOutput IN) : COLOR {
    half4 refl = IN.Refl.w * texCUBE(EnvSampler,IN.Refl.xyz);
    return IN.DiffColor * tex2D(ColorSamp,IN.UV.xy) + IN.SpecColor + refl;
}

half4 vertexShadedF_ntPS(vertexShadedFOutput IN) : COLOR {
    half4 refl = IN.Refl.w * texCUBE(EnvSampler,IN.Refl.xyz);
    return IN.DiffColor + IN.SpecColor + refl;
}

/*************/

technique plastic_textured <
	string Script = "Pass=p0;";
> {
	pass p0  <
		string Script = "Draw=geometry;";
	> {		
	    VertexShader = compile vs_2_0 pixelShadedVS();
	    ZEnable = true;
	    ZWriteEnable = true;
	    CullMode = None;
	    PixelShader = compile ps_2_0 texturedPS();
	}
}

technique plastic_untextured <
	string Script = "Pass=p0;";
> {
	pass p0  <
		string Script = "Draw=geometry;";
	> {		
	    VertexShader = compile vs_2_0 pixelShadedVS();
	    ZEnable = true;
	    ZWriteEnable = true;
	    CullMode = None;
	    PixelShader = compile ps_2_0 untexturedPS();
	}
}

technique plastic_fresnel_untextured <
	string Script = "Pass=p0;";
> {
	pass p0  <
		string Script = "Draw=geometry;";
	> {		
	    VertexShader = compile vs_2_0 pixelShadedVS();
	    ZEnable = true;
	    ZWriteEnable = true;
	    CullMode = None;
	    PixelShader = compile ps_2_0 untexturedFPS();
	}
}

technique plastic_fresnel_textured <
	string Script = "Pass=p0;";
> {
	pass p0  <
		string Script = "Draw=geometry;";
	> {		
	    VertexShader = compile vs_2_0 pixelShadedVS();
	    ZEnable = true;
	    ZWriteEnable = true;
	    CullMode = None;
	    PixelShader = compile ps_2_0 texturedFPS();
	}
}

technique vertex_shaded_textured <
	string Script = "Pass=p0;";
> {
	pass p0  <
		string Script = "Draw=geometry;";
	> {		
	    VertexShader = compile vs_1_1 vertexShadedVS();
	    ZEnable = true;
	    ZWriteEnable = true;
	    CullMode = None;
	    PixelShader = compile ps_1_1 vertexShadedPS();
	}
}

technique vertex_shaded_untextured <
	string Script = "Pass=p0;";
> {
	pass p0  <
		string Script = "Draw=geometry;";
	> {		
	    VertexShader = compile vs_1_1 vertexShadedVS();
	    ZEnable = true;
	    ZWriteEnable = true;
	    CullMode = None;
	    // no pixel shader
	}
}

technique vertex_shaded_fresnel_textured <
	string Script = "Pass=p0;";
> {
	pass p0  <
		string Script = "Draw=geometry;";
	> {		
	    VertexShader = compile vs_1_1 vertexShadedFVS();
	    ZEnable = true;
	    ZWriteEnable = true;
	    CullMode = None;
	    PixelShader = compile ps_2_0 vertexShadedFPS();
	}
}

technique vertex_shaded_fresnel_untextured <
	string Script = "Pass=p0;";
> {
	pass p0  <
		string Script = "Draw=geometry;";
	> {		
	    VertexShader = compile vs_1_1 vertexShadedFVS();
	    ZEnable = true;
	    ZWriteEnable = true;
	    CullMode = None;
	    PixelShader = compile ps_2_0 vertexShadedF_ntPS();
	}
}

/***************************** eof ***/
