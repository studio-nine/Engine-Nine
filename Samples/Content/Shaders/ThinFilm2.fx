/*
File: ThinFilm.fx

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

//ThinFilm2.fx

//////////////////////////////////////////////////////////////////////////
// untweakables //////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=ThinFilm;";
> = 0.8;

float4x4 WorldITXf : WorldInverseTranspose < string UIWidget="None"; > = {1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1};
float4x4 WvpXf : WorldViewProjection < string UIWidget="None"; >;
float4x4 WorldXf : World < string UIWidget="None"; >;
float4x4 ViewIXf : ViewInverse < string UIWidget="None"; >;

//////////////////////////////////////////////////////////////////////////
// tweakables ////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////

// Point Light 1 ////

float3 LightPosP1 : POSITION <
	string UIName = "Light Pos 1";
	string Object = "PointLight";
	string Space = "World";
> = {-10.0f, 10.0f, -10.0f};

// float4   EyeVector = {0,0,1,0};

float3 SurfColor : DIFFUSE <
	string UIName = "Color Name";
	string UIWidget = "Color";
> = {0.3f, 0.3f, 0.5f};

float SpecExpon : SpecularPower <
	string UIName = "Spec Power";
	string UIWidget = "slider";
	float UIMin = 1.0;
	float UIMax = 128.0;
	float UIStep = 1.0;
> = 12.0f;

#include "ThinFilmTex.fxh"

//////////////////////////////////////////////////////////////////////////
// structs ///////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////

struct appData
{
    float4 Position : POSITION;
    float4 Normal   : NORMAL;
};

struct vertexOutput
{
    float4 HPOS      : POSITION;
    float4 diffCol   : COLOR0;
    float4 specCol   : COLOR1;
    float2 filmDepth : TEXCOORD0;
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
    float4 litV = lit(dot(Ln,Nn),dot(Hn,Nn),SpecExpon);
    OUT.diffCol = (float4)litV.y;
    OUT.specCol = (float4)pow(dot(Hn,Nn),SpecExpon); // (float4)litV.z;
    // OUT.specCol = litV.zzzz;   // bug in lit() ?
    // compute the view depth for the thin film
    // float viewdepth = (1.0 / dot(Nn,Vn)) * FilmDepth.x;
    float viewdepth = calc_view_depth(dot(Nn,Vn),FilmDepth.x);
    OUT.filmDepth = viewdepth.xx;
    return OUT;
}

///////// /////////////////////////////////////////////////////////////////
// PIXEL SHADER //////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////

float4 ThinFilmPS(vertexOutput IN) : COLOR
{
    // lookup fringe value based on view depth
    float3 fringeCol = (float3)tex2D(FringeMapSampler, IN.filmDepth);
    // modulate specular lighting by fringe color, combine with regular lighting
    float3 rgb = fringeCol*IN.specCol + IN.diffCol*SurfColor;
    return float4(rgb,1);
}

//////////////////////////////////////////////////////////////////////////
// TECHNIQUE /////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////

technique ThinFilm <
	string Script = "Pass = P0";
> {
    pass P0 < string Script = "Draw=geometry;"; > {
        VertexShader = compile vs_2_0 ThinFilmVS(LightPosP1);
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
        PixelShader  = compile ps_2_0 ThinFilmPS();
    }
}

/////////////////////////////////// eof ///
