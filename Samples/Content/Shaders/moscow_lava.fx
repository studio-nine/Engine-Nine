/******************************************************************************

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

Comments:
	Lava Effect by Sergey A. Makovkin (sergeymak@pisem.net)

******************************************************************************/


float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Lava;";
> = 0.8;


#define MARBLE_SIZE 64

#define MARBLE_COL_PALE float3(0.25, 0.25, 0.35)
#define MARBLE_COL_MED float3(0.40, 0.30, 0.30)
#define MARBLE_COL_DARK float3(0.05, 0.05, 0.26)
#define MARBLE_COL_DARKER float3(0.03, 0.03, 0.20)

float TexelIncrement < string UIWidget = "none"; > = 1.0f / 128.0f;

#define MARBLE_KNOTS 13
#define CR00	-0.5
#define CR01	 1.5
#define CR02	-1.5
#define CR03	 0.5
#define CR10	 1.0
#define CR11	-2.5
#define CR12	 2.0
#define CR13	-0.5
#define CR20	-0.5
#define CR21	 0.0
#define CR22	 0.5
#define CR23     0.0
#define CR30     0.0
#define CR31	 1.0
#define CR32     0.0
#define CR33     0.0

float3 InterpSplineCatmullRom(float u, float3 knot0, float3 knot1, float3 knot2, float3 knot3 )
{
	float3 c0;
	float3 c1;
	float3 c2;
	float3 c3;

	c3 = (CR00 * knot0) + (CR01 * knot1) + (CR02 * knot2) + (CR03 * knot3);
	c2 = (CR10 * knot0) + (CR11 * knot1) + (CR12 * knot2) + (CR13 * knot3);
	c1 = (CR20 * knot0) + (CR21 * knot1) + (CR22 * knot2) + (CR23 * knot3);
	c0 = (CR30 * knot0) + (CR31 * knot1) + (CR32 * knot2) + (CR33 * knot3);

	return ((c3*u + c2)*u + c1)*u + c0;
}

static const float3 marble_knots[MARBLE_KNOTS+4] = 
{
  MARBLE_COL_PALE,  // dummy
  MARBLE_COL_PALE,  // dummy
  MARBLE_COL_PALE,      // 0
  MARBLE_COL_PALE,
  MARBLE_COL_MED, 
  MARBLE_COL_MED, 
  MARBLE_COL_MED,
  MARBLE_COL_PALE, 
  MARBLE_COL_PALE,
  MARBLE_COL_DARK, 
  MARBLE_COL_DARK,
  MARBLE_COL_DARKER, 
  MARBLE_COL_DARKER,
  MARBLE_COL_PALE, 
  MARBLE_COL_DARKER,     // 1
  MARBLE_COL_DARKER, // dummy
  MARBLE_COL_DARKER, // dummy
};

// put map in texture 4, 64 entries cubic interpolation over the surface
float3 make_marble_map(float x)
{
    float3 knot0, knot1, knot2, knot3;

    // need to get 0-1 -> 1-13 for knot values
    float f = x * 15.0 + 1.0;

    float k = floor(f); // get lower integer for knots

    knot0 = marble_knots[k - 1];
    knot1 = marble_knots[k];
    knot2 = marble_knots[k + 1];
    knot3 = marble_knots[k + 2];
    
    return InterpSplineCatmullRom(f - k, knot0, knot1, knot2, knot3);            
}

// Matrices
float4x4 WorldInverseTranspose : WorldInverseTranspose < string UIWidget = "none"; >;
float4x4 WorldViewProjection : WorldViewProjection < string UIWidget = "none"; >;
float4x4 World : World < string UIWidget = "none"; >;
float4x4 ViewInverse : ViewInverse < string UIWidget = "none"; >;

float Time : TIME < string UIWidget = "none"; >;

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
    float3 WorldNormal	: TEXCOORD2;
    float3 WorldPos	: TEXCOORD3;
    float3 WorldEyePos	: TEXCOORD4;
};

/* Output pixel values */
struct pixelOutput {
  float4 col : COLOR;
};

// Volume texture
texture VolumeMap 
< 
    string ResourceType = "VOLUME"; 
    string function = "GenerateVolumeMap"; 
    float3 Dimensions = { 64.0f, 64.0f, 64.0f};
>;

// Volume texture
texture MarbleColorTexture
< 
    string ResourceType = "2D"; 
    string function = "MarbleColor"; 
    float2 Dimensions = { MARBLE_SIZE, MARBLE_SIZE };
>;

sampler3D VolumeTexture =
sampler_state
{
    texture = (VolumeMap);
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    
    AddressU = WRAP;
    AddressV = WRAP;
    AddressW = WRAP;
};


sampler2D MarbleColorSampler =
sampler_state
{
    texture = (MarbleColorTexture);
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    
    AddressU = WRAP;
    AddressV = WRAP;
};

float4 GenerateVolumeMap(float3 Pos : POSITION) : COLOR
{
	return (noise(Pos * 50.5) * .5) + .5f;
}

float3 MarbleColor(float3 Pos : POSITION) : COLOR
{
	return make_marble_map(Pos.x);
}

texture bark : Diffuse
<
	string ResourceName = "bark.dds";
    string ResourceType = "2D";
>;

sampler2D BarkSampler = sampler_state
{
    Texture = <bark>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};

/*********** vertex shader ******/
vertexOutput mainVS(appdata IN,
    uniform float4x4 WorldViewProjection,
    uniform float4x4 WorldInverseTranspose,
    uniform float4x4 World,
    uniform float4x4 ViewInverse
) {
    vertexOutput OUT;
    OUT.WorldNormal = mul(IN.Normal, WorldInverseTranspose).xyz;
    float3 WorldSpacePos = mul(float4(IN.Position, 1), World).xyz;
    OUT.WorldPos = WorldSpacePos;
    OUT.TexCoord = IN.UV;
    OUT.WorldEyePos = ViewInverse[3].xyz;
    OUT.HPosition = mul(float4(IN.Position, 1), WorldViewProjection);
    return OUT;
}

/*********** pixel shader ******/

pixelOutput mainPS(vertexOutput IN,
    uniform float4 LavaColor1,
    uniform float4 LavaColor2,
    uniform float LavaFactor
) {
    pixelOutput OUT; 
         
	// Sample 4 octaves of noise
	
	float rnd = 0.0f;
	float f = 1;
	float3 Coord = IN.WorldPos + (Time * .03f);
	for (int i = 0; i < 4; i++)
	{
		half4 fnoise = tex3D(VolumeTexture, Coord * .2f * f);
		fnoise -=.5f;
		fnoise *= 4.0f;
		rnd += ( fnoise) / f;
		f *= 4.17;	
	}
	    
	float3 coord = IN.WorldPos;
	coord.x += rnd*LavaFactor;
	coord.y += rnd*LavaFactor;
	float4 tex = tex2D(BarkSampler, coord);
	
	// Add the terms
	OUT.col = tex * LavaColor1 * (rnd + 0.1) * 10 + LavaColor2;
    return OUT;
}

pixelOutput mainGlowPS(vertexOutput IN,
    uniform float4 LavaColor1,
    uniform float4 LavaColor2,
    uniform float LavaFactor
) {
    pixelOutput OUT;     
    
	// Sample 4 octaves of noise
	
	float rnd = 0.0f;
	float f = 1;
	float3 Coord = IN.WorldPos + (Time * .03f);
	for (int i = 0; i < 4; i++)
	{
		half4 fnoise = tex3D(VolumeTexture, Coord * .2f * f);
		fnoise -=.5f;
		fnoise *= 4.0f;
		rnd += ( fnoise) / f;
		f *= 4.17;	
	}
	    
	float3 coord = IN.WorldPos;
	coord.x += rnd*LavaFactor;
	coord.y += rnd*LavaFactor;
	float4 tex = tex2D(BarkSampler, coord);
	
	// Add the terms
	OUT.col = tex * LavaColor1 * (rnd + 0.1) * 0.5;    
    return OUT;
}


float4 LavaColor1
<
    string UIName = "Lava Color 1";
    string UIWidget = "color";
> = {0.8f, 0.8f, 0.4f, 1.0f};

float4 LavaColor2
<
    string UIName = "Lava Color 2";
    string UIWidget = "color";
> = {0.5f, 0.0f, 0.0f, 0.0f};

float LavaFactor
<
    string uiwidget = "slider";
    float uimin = 0.01;
    float uimax = 1.0;
    float uistep = 0.01;
    string uiname = "Lava factor";
> = 0.1;

struct VS_OUTPUT
{
   	float4 Position   : POSITION;
    float4 Diffuse    : COLOR0;
    float4 TexCoord0   : TEXCOORD0;
};


/*************/

technique Lava <
	string Script = "Pass=GlowBuffer; Pass=p0;";
> {
	pass GlowBuffer <
		string Script = "Draw=geometry;";
	> {
		cullmode = none;
		ZEnable = true;
		VertexShader = compile vs_2_0 mainVS(WorldViewProjection, WorldInverseTranspose,World,ViewInverse);
        PixelShader  = compile ps_2_0 mainGlowPS(LavaColor1, LavaColor2, LavaFactor);
	}
	
	pass p0 <
		string Script = "Draw=geometry;";
	> {		
        VertexShader = compile vs_2_0 mainVS(WorldViewProjection, WorldInverseTranspose,World,ViewInverse);

		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;

        PixelShader = compile ps_2_0 mainPS(LavaColor1, LavaColor2, LavaFactor);
	}

}
