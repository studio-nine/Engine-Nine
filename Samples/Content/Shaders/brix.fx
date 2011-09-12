/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/brix.fx#1 $

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
    Brick pattern with controls using texture-based patterning.
    $Date: 2004/09/24 $

******************************************************************************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=brix;";
> = 0.8;

/*** TWEAKABLES *********************************************/

float3 LightDir : DIRECTION <
	string UIName = "Light Direction";
	string Object = "DirectionalLight";
	string Space = "World";
> = {0.577, -0.577, 0.577};

float4 AmbiColor : AMBIENT <
    string UIName = "Ambient Light Color";
> = {0.17f, 0.17f, 0.17f, 1.0f};

float4 SurfColor1 <
    string UIName = "Brick 1";
	string UIWidget = "Color";
> = {0.9, 0.5, 0.0, 1.0f};

float4 SurfColor2 <
    string UIName = "Brick 2";
	string UIWidget = "Color";
> = {0.8, 0.48, 0.15, 1.0f};

float4 GroutColor <
    string UIName = "Grout";
	string UIWidget = "Color";
> = {0.8f, 0.75f, 0.75f, 1.0f};

float BrickWidth : UNITSSCALE <
    string UNITS = "inches";
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 0.15;
    float UIStep = 0.001;
    string UIName = "Width";
> = 0.134;

float BrickHeight : UNITSSCALE <
    string UNITS = "inches";
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 0.15;
    float UIStep = 0.001;
    string UIName = "Height";
> = 0.04;

float GBalance <
    string UIWidget = "slider";
    float UIMin = 0.01;
    float UIMax = 0.25;
    float UIStep = 0.01;
    string UIName = "grouting";
> = 0.1;

///////////////////////////////////////////
// Procedural Texture /////////////////////
///////////////////////////////////////////

#define TEX_SIZE 256

texture stripeTex <
    string function = "MakeStripe";
    float2 Dimensions = { TEX_SIZE, TEX_SIZE };
    string UIWidget = "None";
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

float4 MakeStripe(float2 Pos : POSITION,float ps : PSIZE) : COLOR
{
   float v = 0;
   float nx = Pos.x+ps;
   v = nx > Pos.y;
   return float4(v.xxx,1.0);
}

/************* UN-TWEAKABLES **************/

float4x4 WorldIT : WORLDINVERSETRANSPOSE <string UIWidget="None";>;
float4x4 WorldViewProj : WORLDVIEWPROJECTION <string UIWidget="None";>;
float4x4 World : WORLD <string UIWidget="None";>;
float4x4 ViewInv : VIEWINVERSE <string UIWidget="None";>;

/************* DATA STRUCTS **************/

/* data from application vertex buffer */
struct appdata {
    half3 Position    : POSITION;
    half4 UV        : TEXCOORD0;
    half4 Normal    : NORMAL;
};

/* data passed from vertex shader to pixel shader */
struct vertexOutput {
    half4 HPosition    : POSITION;
    half4 TexCoord    : TEXCOORD0;
    half3 WorldNormal    : TEXCOORD1;
    half3 WorldEyeVec    : TEXCOORD2;
    half4 ObjPos    : TEXCOORD3;
    float4 DCol : COLOR0;
};

/*********** vertex shader ******/

vertexOutput mainVS(appdata IN) {
    vertexOutput OUT;
    float3 Nw = normalize(mul(IN.Normal, WorldIT).xyz);
    OUT.WorldNormal = Nw;
    OUT.DCol = max(0,dot(Nw,-LightDir)) + AmbiColor;
    half4 Po = half4(IN.Position.xyz,1);
    half3 Pw = mul(Po, World).xyz;
    OUT.TexCoord = IN.UV;
    OUT.WorldEyeVec = normalize(ViewInv[3].xyz - Pw);
    half4 hpos = mul(Po, WorldViewProj);
    //OUT.ObjPos = half4(Po.x/BrickWidth,Po.y/BrickHeight,Po.zw);
    OUT.ObjPos = half4(IN.UV.y/BrickWidth,IN.UV.x/BrickHeight,Po.zw);
    OUT.HPosition = hpos;
    return OUT;
}

/******************** pixel shader *********************/

half4 brixPS(vertexOutput IN) : COLOR {
    half v = ((half4)tex2D(stripeSampler,half2(IN.ObjPos.x,0.5))).x;
    half4 dColor1 = lerp(SurfColor1,SurfColor2,v);
    v = ((half4)tex2D(stripeSampler,half2(IN.ObjPos.x*2,GBalance))).x;
    dColor1 = lerp(GroutColor,dColor1,v);
    v = ((half4)tex2D(stripeSampler,half2(IN.ObjPos.x+0.25,0.5))).x;
    half4 dColor2 = lerp(SurfColor1,SurfColor2,v);
    v = ((half4)tex2D(stripeSampler,half2((IN.ObjPos.x+0.25)*2,GBalance))).x;
    dColor2 = lerp(GroutColor,dColor2,v);
    v = ((half4)tex2D(stripeSampler,half2(IN.ObjPos.y,0.5))).x;
    half4 brix = lerp(dColor1,dColor2,v);
    v = ((half4)tex2D(stripeSampler,half2(IN.ObjPos.y*2,GBalance))).x;
    brix = lerp(GroutColor,brix,v);
	return IN.DCol * brix;
}

/*************/

technique brix <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
> {        
        VertexShader = compile vs_2_0 mainVS();
        ZEnable = true;
        ZWriteEnable = true;
        CullMode = None;
        PixelShader = compile ps_2_0 brixPS();
    }
}

/***************************** eof ***/
