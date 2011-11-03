/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/goochy_HLSL.fx#1 $

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
	Gooch shading w/glossy hilight in HLSL ps_2 pixel shader.
	Textured and non-textued versions are supplied.

******************************************************************************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?Untextured:Textured;";
> = 0.8;

/************* TWEAKABLES **************/

float4x4 WorldIT : WorldInverseTranspose < string UIWidget="None"; >;
float4x4 WorldViewProj : WorldViewProjection < string UIWidget="None"; >;
float4x4 World : World < string UIWidget="None"; >;
float4x4 ViewInv : ViewInverse < string UIWidget="None"; >;

float4 LightPos : Position
<
	string Object = "PointLight";
	string Space = "World";
> = {100.0f, 100.0f, 100.0f, 0.0f};

float3 LiteColor <
    string UIName =  "Bright DrawableSurface Color";
    string UIWidget = "Color";
> = {0.8f, 0.5f, 0.1f};

float3 DarkColor <
    string UIName =  "Dark DrawableSurface Color";
    string UIWidget = "Color";
> = {0.0f, 0.0f, 0.0f};

float3 WarmColor <
    string UIName =  "Gooch warm tone";
    string UIWidget = "Color";
> = {0.5f, 0.4f, 0.05f};

float3 CoolColor <
    string UIName =  "Gooch cool tone";
    string UIWidget = "Color";
> = {0.05f, 0.05f, 0.6f};

float3 SpecColor : Specular <
    string UIName =  "Hilight color";
    string UIWidget = "Color";
> = {0.7f, 0.7f, 1.0f};

float SpecExpon : SpecularPower <
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 128.0;
    float UIStep = 1.0;
    string UIName =  "specular power";
> = 40.0;

float GlossTop <
    string UIWidget = "slider";
    float UIMin = 0.2;
    float UIMax = 1.0;
    float UIStep = 0.05;
    string UIName =  "Maximum for Gloss Dropoff";
> = 0.7;

float GlossBot
<
    string UIWidget = "slider";
    float UIMin = 0.05;
    float UIMax = 0.95;
    float UIStep = 0.05;
    string UIName =  "Minimum for Gloss Dropoff";
> = 0.5;

float GlossDrop
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.05;
    string UIName =  "Strength of Glossy Dropoff";
> = 0.2;

texture ColorMap : DIFFUSE <
	string ResourceName = "default_color.dds";
	string ResourceType = "2D";
>;

sampler2D ColorSampler = sampler_state
{
	Texture = <ColorMap>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = WRAP;
	AddressV = WRAP;
};

//////////////////////////

/* data from application vertex buffer */
struct appdata {
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;
    float4 Normal	: NORMAL;
};

/* data passed from vertex shader to pixel shader */
struct vertexOutput {
    float4 HPosition	: POSITION;
    float4 TexCoord	: TEXCOORD0;
    float3 LightVec	: TEXCOORD1;
    float3 WorldNormal	: TEXCOORD2;
    float3 WorldPos	: TEXCOORD3;
    float3 WorldEyePos	: TEXCOORD4;
};

/*********** vertex shader ******/

vertexOutput mainVS(appdata IN)
{
    vertexOutput OUT;
    OUT.WorldNormal = mul(IN.Normal, WorldIT).xyz;
    float4 Po = float4(IN.Position.xyz,1);
    float3 Pw = mul(Po, World).xyz;
    OUT.WorldPos = Pw;
    OUT.LightVec = LightPos - Pw;
    OUT.TexCoord = IN.UV;
    OUT.WorldEyePos = ViewInv[3].xyz;
    OUT.HPosition = mul(Po, WorldViewProj);
    return OUT;
}

/*********** pixel shader ******/

void gooch_shared(vertexOutput IN,
		out float4 DiffuseContrib,
		out float4 SpecularContrib)
{
    float3 Ln = normalize(IN.LightVec);
    float3 Nn = normalize(IN.WorldNormal);
    float3 Vn = normalize(IN.WorldEyePos - IN.WorldPos);
    float3 Hn = normalize(Vn + Ln);
    float hdn = pow(max(1E-10,dot(Hn,Nn)),SpecExpon);
    hdn = hdn * (GlossDrop+smoothstep(GlossBot,GlossTop,hdn)*(1.0-GlossDrop));
    SpecularContrib = float4((hdn * SpecColor),1);
    float ldn = dot(Ln,Nn);
    float mixer = 0.5 * (ldn + 1.0);
    float diffComp = max(0,ldn);
    float3 surfColor = lerp(DarkColor,LiteColor,mixer);
    float3 toneColor = lerp(CoolColor,WarmColor,mixer);
    DiffuseContrib = float4((surfColor + toneColor),1);
}

float4 gooch_PS(vertexOutput IN) :COLOR
{
	float4 diffContrib;
	float4 specContrib;
	gooch_shared(IN,diffContrib,specContrib);
    float4 result = diffContrib + specContrib;
    return result;
}

float4 goochT_PS(vertexOutput IN) :COLOR
{
	float4 diffContrib;
	float4 specContrib;
	gooch_shared(IN,diffContrib,specContrib);
    float4 result = tex2D(ColorSampler,IN.TexCoord.xy)*diffContrib + specContrib;
    return result;
}

/*************/

technique Untextured <
	string Script = "Pass=p0;";
> {
	pass p0 <
	string Script = "Draw=geometry;";
> {		
        VertexShader = compile vs_2_0 mainVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
        PixelShader = compile ps_2_0 gooch_PS();
	}
}

technique Textured <
	string Script = "Pass=p0;";
> {
	pass p0 <
	string Script = "Draw=geometry;";
> {		
        VertexShader = compile vs_2_0 mainVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
        PixelShader = compile ps_2_0 goochT_PS();
	}
}

/***************************** eof ***/
