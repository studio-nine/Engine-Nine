/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/cage.fx#1 $

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
    3D meshcage effect, created by procedural texturing.
    Texture is pre-calculated by HLSL.
    Wires are aligned to world coordinates in this sample.
    Works fine in effectedit
    $Date: 2004/09/24 $

******************************************************************************/

// properties for EffectEdit
string XFile = "bigship1.x"; 
int    BCLR  = 0xff806010;

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=wires;";
> = 0.8;

/************* UN-TWEAKABLES **************/

float4x4 WvpXf : WorldViewProjection < string UIWidget="None"; >;
float4x4 WorldXf : World < string UIWidget="None"; >;

/******** TWEAKABLES ****************************************/

float4 BrightColor : Diffuse
<
    string UIName = "Wire Color";
> = {1.0f, 0.8f, 0.0f, 1.0f};

float4 EmptyColor
<
    string UIName = "Color of 'Air'";
    string UIWidget = "Color";
> = {0.0f, 0.0f, 0.0f, 0.0f};

float Balance
<
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 1.0;
    float uistep = 0.01;
    string UIName = "Relative Width of Wire";
> = 0.1;

float Scale : UNITSSCALE
<
    string units = "inches";
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 20.0;
    float uistep = 0.01;
    string UIName = "size of pattern";
> = 5.1;

/////////////// prodecural texture /////////////

#define TEX_SIZE 64

texture stripeTex <
    string function = "MakeStripe";
    string UIWidget = "None";
    float2 Dimensions = { TEX_SIZE, TEX_SIZE };
>;

sampler2D stripeSampler = sampler_state
{
    Texture = <stripeTex>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = WRAP;
    AddressV = CLAMP;
};

/*********** texture shader ******/

float4 MakeStripe(float2 Pos : POSITION,float ps : PSIZE) : COLOR
{
   float v = 0;
   float nx = Pos.x+ps; // keep the last column full-on, always
   v = nx > Pos.y;
   return float4(v.xxxx);
}

/************* DATA STRUCTS **************/

/* data from application vertex buffer */
struct appdata {
    float3 Position    : POSITION;
    float4 UV        : TEXCOORD0;
    float4 Normal    : NORMAL;
};

/* data passed from vertex shader to pixel shader */
struct vertexOutput {
    float4 HPosition    : POSITION;
    float4 TexCoord    : TEXCOORD0;//
};

/*********** vertex shader ******/

vertexOutput mainVS(appdata IN) {
    vertexOutput OUT;
    float4 Po = float4(IN.Position.x,IN.Position.y,IN.Position.z,1.0);    // object space
    float4 hpos  = mul(Po, WvpXf);        // position (projected)
    OUT.HPosition  = hpos;
    float4 Pw = mul(Po, WorldXf); // world coords
    OUT.TexCoord = Pw * Scale;
    return OUT;
}

/******************** pixel shader *********************/

float4 strokeTexPS(vertexOutput IN) : COLOR {
    float stripex = tex2D(stripeSampler,float2(IN.TexCoord.x,Balance)).x;
    float stripey = tex2D(stripeSampler,float2(IN.TexCoord.y,Balance)).x;
    float stripez = tex2D(stripeSampler,float2(IN.TexCoord.z,Balance)).x;
    float check = stripex * stripey * stripez;
    float4 dColor = lerp(BrightColor,EmptyColor,check);
    return dColor;
}

/*************/

technique wires <
    string Script = "Pass=p0; Pass=p1;";
> {
    pass p0  <
        string Script = "Draw=geometry;";
    > {        
        VertexShader = compile vs_2_0 mainVS();
        PixelShader = compile ps_2_0 strokeTexPS();
    }
}

/***************************** eof ***/
