/*
$Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/DawnCostume.fx#1 $

Copyright NVIDIA Corporation 2003
TO THE MAXIMUM EXTENT PERMITTED BY APPLICABLE LAW, THIS SOFTWARE IS PROVIDED
*AS IS* AND NVIDIA AND ITS SUPPLIERS DISCLAIM ALL WARRANTIES, EITHER EXPRESS
OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, IMPLIED WARRANTIES OF MERCHANTABILITY
AND FITNESS FOR A PARTICULAR PURPOSE.  IN NO EVENT SHALL NVIDIA OR ITS SUPPLIERS
BE LIABLE FOR ANY SPECIAL, INCIDENTAL, INDIRECT, OR CONSEQUENTIAL DAMAGES
WHATSOEVER (INCLUDING, WITHOUT LIMITATION, DAMAGES FOR LOSS OF BUSINESS PROFITS,
BUSINESS INTERRUPTION, LOSS OF BUSINESS INFORMATION, OR ANY OTHER PECUNIARY LOSS)
ARISING OUT OF THE USE OF OR INABILITY TO USE THIS SOFTWARE, EVEN IF NVIDIA HAS
BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.
*/

//based on ThinFilm2.fx

//////////////////////////////////////////////////////////////////////////
// untweakables //////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Costume;";
> = 0.8;

float4x4 WorldITXf : WorldInverseTranspose < string UIWidget="None"; > = {1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1};
float4x4 WvpXf : WorldViewProjection < string UIWidget="None"; >;
float4x4 WorldXf : World < string UIWidget="None"; >;
float4x4 ViewIXf : ViewInverse < string UIWidget="None"; >;

//////////////////////////////////////////////////////////////////////////
// tweakables ////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////

// Point Light 1 ////

float4 LightPosP1 : POSITION <
	string UIName = "Light Pos 1";
	string Object = "PointLight";
	string Space = "World";
> = {10.0f, 10.0f, 10.0f, 1.0f};

float4   EyeVector = {0,0,1,0};

float3 SurfColor : DIFFUSE <
	string UIName = "Diffuse";
	string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f};

float3 SpecColor : SPECULAR <
	string UIName = "Specular";
	string UIWidget = "Color";
> = {0.4f, 0.5f, 0.5f};

float SpecExpon : SpecularPower <
	string UIName = "Spec Power";
	string UIWidget = "slider";
	float UIMin = 1.0;
	float UIMax = 128.0;
	float UIStep = 1.0;
> = 12.0f;

float OpacityMax <
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 1.0;
	float UIStep = 0.01;
> = 0.95f;

float OpacityMin <
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 1.0;
	float UIStep = 0.01;
> = 0.8f;

float TranspExp <
	string UIWidget = "slider";
	float UIMin = 1.0;
	float UIMax = 6.0;
	float UIStep = 0.01;
> = 4.0f;

// #include "c:\\sw\\devrel\\SDK\\MEDIA\\HLSL\\ThinFilmTex.fxh"
#include "ThinFilmTex.fxh"

//////////////////////////////////////////////////////////////////////////
// structs ///////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////

struct appData
{
    float4 Position : POSITION;
    float4 Normal   : NORMAL;
    float2 UV   : TEXCOORD0;
};

struct vertexOutput
{
    float4 HPOS      : POSITION;
    float4 diffCol   : COLOR0;
    float4 specCol   : COLOR1;
    float2 UV : TEXCOORD0;
    float4 filmDepth : TEXCOORD1;
};

//////

texture diffuseTexture : DIFFUSE <
	string ResourceName = "default_color.dds";
	string ResourceType = "2D";
>;

sampler2D DiffuseSampler = sampler_state
{
	Texture = <diffuseTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = WRAP;
	AddressV = WRAP;
};

//////////////////////////////////////////////////////////////////////////
// VERTEX SHADER /////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////

vertexOutput ThinFilmVS(appData IN,uniform float3 LightPos)
{
    vertexOutput OUT;
    float3 Nn = mul(IN.Normal,WorldITXf).xyz;
    float4 Po = float4(IN.Position.xyz,1.0);	// object coordinates
    float3 Pw = mul(Po,WorldXf).xyz;		// world coordinates
    OUT.HPOS = mul(Po,WvpXf);	// screen clipspace coords
    float3 Ln = normalize(LightPos - Pw);
    float3 Vn = normalize(ViewIXf[3].xyz - Pw);	// obj coords
    float3 Hn = normalize(Ln + Vn);
    float4 litV = lit(abs(dot(Ln,Nn)),dot(Hn,Nn),SpecExpon);
    OUT.diffCol = (float4)litV.y;
    OUT.specCol = (float4)pow(dot(Hn,Nn),SpecExpon); // (float4)litV.z;
    // OUT.specCol = litV.zzzz;   // bug in lit() ?
    // compute the view depth for the thin film
    // float viewdepth = (1.0 / dot(Nn,Vn)) * FilmDepth.x;
	float ndv = abs(dot(Nn,Vn));
    float viewdepth = calc_view_depth(ndv,FilmDepth.x);
    OUT.filmDepth = float4(viewdepth.xx,ndv.xx);
    OUT.UV = IN.UV.xy;
    return OUT;
}

///////// /////////////////////////////////////////////////////////////////
// PIXEL SHADER //////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////

float4 ThinFilmPS(vertexOutput IN) : COLOR
{
    // lookup fringe value based on view depth
    float3 fringeCol = (float3)tex2D(FringeMapSampler, IN.filmDepth.xy);
    // modulate specular lighting by fringe color, combine with regular lighting
    float3 rgb = SpecColor*fringeCol*IN.specCol +
				IN.diffCol*SurfColor * tex2D(DiffuseSampler,IN.UV).xyz;
	//float a = 1-(Transparency*pow(IN.filmDepth.z,TranspExp));
	float a = OpacityMin+pow((1-IN.filmDepth.z),TranspExp)*(OpacityMax-OpacityMin);
    return float4(rgb,a);
}

//////////////////////////////////////////////////////////////////////////
// TECHNIQUE /////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////

technique Costume <
	string Script = "Pass=P0; Pass=P1;";
> {
    pass P0 <
		string Script = "Draw=geometry;";
	> {
        VertexShader = compile vs_2_0 ThinFilmVS(LightPosP1);
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = CCW;
		AlphaBlendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
        PixelShader  = compile ps_2_0 ThinFilmPS();
    }
    pass P1 <
		string Script = "Draw=geometry;";
	> {
        VertexShader = compile vs_2_0 ThinFilmVS(LightPosP1);
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = CW;
		AlphaBlendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
        PixelShader  = compile ps_2_0 ThinFilmPS();
    }
}

/////////////////////////////////// eof ///
