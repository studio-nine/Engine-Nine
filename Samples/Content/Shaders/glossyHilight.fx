/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/glossyHilight.fx#1 $

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
    A glossy textured surface. The highlight is done in VERTEX shading --
	not as a texture. A version with pixel hilighting, requiring a
	special gloss texture, is forthcoming.
    Glossiness is controlled not only by the usual power function, but also
        by applying a set of gloss controls that cause a sharp falloff across
	a specified range.
    The falloff will occur in the highlight range [glossBot-glossTop] and the
        amount of falloff is specified by "glossDrop." Setting "glossDrop"
	to 1.0 nullifies the effect.

******************************************************************************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?Untextured:Textured:UntexturedPS:TexturedPS;";
> = 0.8;

/************* TWEAKABLES **************/

float4x4 WorldIT : WorldInverseTranspose < string UIWidget="None"; >;
float4x4 WorldViewProj : WorldViewProjection < string UIWidget="None"; >;
float4x4 World : World < string UIWidget="None"; >;
float4x4 ViewI : ViewInverse < string UIWidget="None"; >;

//////

float4 LightPos : Position
<
    string Object = "PointLight";
    string Space = "World";
    string UIName =  "Light Position";
> = {100.0f, 100.0f, 100.0f, 0.0f};

float4 AmbiColor : Ambient
<
	string UIWidget = "Color";
    string UIName =  "Ambient";
> = {0.1f, 0.1f, 0.1f, 1.0f};

float4 DiffColor : DIFFUSE
<
	string UIWidget = "Color";
    string UIName =  "Diffuse";
> = {0.9f, 1.0f, 0.9f, 1.0f};

float4 SpecColor : Specular
<
	string UIWidget = "Color";
    string UIName =  "Specular";
> = {0.7f, 0.7f, 1.0f, 1.0f};

float SpecExpon : SpecularPower
<
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 128.0;
    float UIStep = 1.0;
    string UIName =  "Specular power";
> = 8.0;

float GlossTop
<
    string UIWidget = "slider";
    float UIMin = 0.2;
    float UIMax = 1.0;
    float UIStep = 0.05;
    string UIName = "Bright Glossy Edge";
> = 0.7;

float GlossBot
<
    string UIWidget = "slider";
    float UIMin = 0.05;
    float UIMax = 0.95;
    float UIStep = 0.05;
    string UIName = "Dim Glossy Edge";
> = 0.5;

float GlossDrop
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.05;
    string UIName = "Glossy Brightness Drop";
> = 0.2;

/////////////

texture ColorTexture : DIFFUSE
<
    string ResourceName = "default_color.dds";
    string ResourceType = "2D";
    string UIName =  "Color Texture (if used)";
>;

sampler2D ColorSampler = sampler_state
{
	Texture = <ColorTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = None;
};

/************* DATA STRUCTS **************/

/* data from application vertex buffer */
struct appdata {
    float3 Position	: POSITION;
    float3 UV		: TEXCOORD0;
    float4 Normal	: NORMAL;
};

/* data passed from shaded vertex shader to pixel shader */
struct shadedVertexOutput {
    float4 HPosition	: POSITION;
    float4 TexCoord0	: TEXCOORD0;
    float4 diffCol	: COLOR0;
    float4 specCol	: COLOR1;
};

/* data passed from simpler vertex shader to pixel shader */
struct vertexOutput {
    half4 HPosition	: POSITION;
    half4 TexCoord		: TEXCOORD0;
    half3 LightVec		: TEXCOORD1;
    half3 WorldNormal	: TEXCOORD2;
    half3 WorldView	: TEXCOORD5;
};

///////////////////////// gloss-hilight function ///////////

//
// function can be used anywhere -- not just vertex shaders
//
float glossy_drop(float v,
		    uniform float top,
		    uniform float bot,
		    uniform float drop)
{
    return (drop+smoothstep(bot,top,v)*(1.0-drop));
}

/*********** vertex shader ******/

shadedVertexOutput glossVS(appdata IN) {
    shadedVertexOutput OUT;
    float3 Nn = normalize(mul(IN.Normal, WorldIT).xyz);
    float4 Po = float4(IN.Position.xyz,1);
    //compute worldspace position
    float3 Pw = mul(Po, World).xyz;
    float3 Ln = normalize(LightPos - Pw);
    float3 Vn = normalize(ViewI[3].xyz - Pw);
    float3 Hn = normalize(Vn + Ln);
    float4 litV = lit(dot(Ln,Nn),dot(Hn,Nn),SpecExpon);
    float4 diffContrib = litV.y * DiffColor;
    diffContrib.w = 1.0;
    OUT.diffCol = diffContrib + AmbiColor;
    OUT.TexCoord0 = float4(IN.UV, 1);
    float spec = litV.y * litV.z;
    spec *= glossy_drop(spec,GlossTop,GlossBot,GlossDrop);
    OUT.specCol = spec * SpecColor;
    // transform into homogeneous-clip space
    OUT.HPosition = mul(Po, WorldViewProj);
    return OUT;
}

vertexOutput simpleVS(appdata IN) {
    vertexOutput OUT;
    half4 normal = normalize(IN.Normal);
    OUT.WorldNormal = mul(normal, WorldIT).xyz;
    half4 Po = half4(IN.Position.xyz,1);
    half3 Pw = mul(Po, World).xyz;
    OUT.LightVec = normalize(LightPos - Pw);
    OUT.TexCoord = IN.UV.xyxx;
    OUT.WorldView = normalize(ViewI[3].xyz - Pw);
    OUT.HPosition = mul(Po, WorldViewProj);
    return OUT;
}

/********* pixel shader ********/

void gloss_shared(vertexOutput IN,
			out half3 DiffuseContrib,
			out half3 SpecularContrib)
{
    half3 Ln = normalize(IN.LightVec);
    half3 Nn = normalize(IN.WorldNormal);
    half3 Vn = normalize(IN.WorldView);
    half3 Hn = normalize(Vn + Ln);
    half4 litV = lit(dot(Ln,Nn),dot(Hn,Nn),SpecExpon);
    DiffuseContrib = litV.y * DiffColor + AmbiColor;
    float spec = litV.y * litV.z;
    spec *= glossy_drop(spec,GlossTop,GlossBot,GlossDrop);
    SpecularContrib = spec * SpecColor;
}

half4 glossPS(vertexOutput IN) : COLOR {
    half3 diffContrib;
    half3 specContrib;
	gloss_shared(IN,diffContrib,specContrib);
    half3 result = diffContrib + specContrib;
    return half4(result,1);
}

half4 glossPS_t(vertexOutput IN) : COLOR {
    half3 diffContrib;
    half3 specContrib;
	gloss_shared(IN,diffContrib,specContrib);
    half3 map = tex2D(ColorSampler,IN.TexCoord.xy).xyz;
    half3 result = specContrib + (map * diffContrib);
    return half4(result,1);
}

/*************/

technique Untextured <
	string Script = "Pass=p0;";
> {
    pass p0  <
				string Script = "Draw=geometry;";
    > {		
	    VertexShader = compile vs_1_1 glossVS();
	    ZEnable = true;
	    ZWriteEnable = true;
	    CullMode = None;
	    // no pixel shader needed
	    SpecularEnable = true;
	    ColorArg1[ 0 ] = Diffuse;
	    ColorOp[ 0 ]   = SelectArg1;
	    ColorArg2[ 0 ] = Specular;
	    AlphaArg1[ 0 ] = Diffuse;
	    AlphaOp[ 0 ]   = SelectArg1;
    }
}

technique Textured <
	string Script = "Pass=p0;";
> {
    pass p0  <
		string Script = "Draw=geometry;";
    > {		
		VertexShader = compile vs_1_1 glossVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		// no pixel shader needed
		SpecularEnable = false;
	    Texture[0] = <ColorTexture>;
	    MinFilter[0] = Linear;
	    MagFilter[0] = Linear;
	    MipFilter[0] = None;
        ColorArg1[ 0 ] = Texture;
        ColorOp[ 0 ] = Modulate;
        ColorArg2[ 0 ] = Diffuse;
    }
}

technique UntexturedPS <
	string Script = "Pass=p0;";
> {
    pass p0  <
				string Script = "Draw=geometry;";
    > {		
	    VertexShader = compile vs_1_1 simpleVS();
	    ZEnable = true;
	    ZWriteEnable = true;
	    CullMode = None;
	    PixelShader = compile ps_2_0 glossPS();
    }
}

technique TexturedPS <
	string Script = "Pass=p0;";
> {
    pass p0  <
				string Script = "Draw=geometry;";
    > {		
	    VertexShader = compile vs_1_1 simpleVS();
	    ZEnable = true;
	    ZWriteEnable = true;
	    CullMode = None;
	    PixelShader = compile ps_2_0 glossPS_t();
    }
}

/***************************** eof ***/
