/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/seespaces.fx#1 $

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
    This .fx file uses 3d checks and vectors-as-colors
    to illustrate a number of important coordinate systems
    and shading vectors.
	Works fine with EffectEdit
    $Date: 2004/09/24 $

******************************************************************************/

string XFile = "tiger.x";

/************* UN-TWEAKABLES **************/

float4x4 WorldITXf : WorldInverseTranspose < string UIWidget="None"; >; // = {1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1};
float4x4 WvpXf : WorldViewProjection < string UIWidget="None"; >;
float4x4 WorldXf : World < string UIWidget="None"; >; // = {1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1};
float4x4 ViewITXf : ViewInverseTranspose < string UIWidget="None"; >;
float4x4 ViewIXf : ViewInverse < string UIWidget="None"; >;
float4x4 WorldViewXf : WorldView < string UIWidget="None"; >; // = {1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1};

/************************************************************/
/*** TWEAKABLES *********************************************/
/************************************************************/

float4 LightPos : Position <
    string Object = "PointLight";
    string Space = "World";
> = {100.0f, 100.0f, 100.0f, 1.0f};

float4 LightColor <
    string UIName = "Light Color";
    string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f, 1.0f};

float4 AmbiColor : Ambient
<
    string UIName = "Ambient Color";
> = {0.17f, 0.17f, 0.17f, 1.0f};

float4 SurfColor1 <
    string UIName = "Surface Color 1";
    string UIWidget = "Color";
> = {1.0f, 0.4f, 0.0f, 1.0f};

float4 SurfColor2 <
    string UIName = "Surface Color 2";
    string UIWidget = "Color";
> = {0.0f, 0.2f, 1.0f, 1.0f};

float Ks <
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 2.0;
    float uistep = 0.01;
    string UIName = "Specular";
> = 0.5;

float SpecExpon : SpecularPower <
    string UIWidget = "slider";
    float uimin = 1.0;
    float uimax = 128.0;
    float uistep = 1.0;
    string UIName = "Spec Exponent";
> = 25.0;

float Scale : UNITSSCALE
<
    string units = "inches";
    string UIWidget = "slider";
    float uimin = 0.001;
    float uimax = 40.0;
    float uistep = 0.01;
    string UIName = "Pattern Scale";
> = 2.0;

float Balance <
    string UIWidget = "slider";
    float uimin = 0.01;
    float uimax = 0.99;
    float uistep = 0.01;
    string UIName = "Balance";
> = 0.5;

// pass the transform from world coords to any user-defined coordinate system
// float4x4 UserXf = {1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1};
float4x4 UserXf = {0.5,-0.146447,1.707107,0, 0.5,0.853553,-0.292893,0, -0.707107,0.5,1,0, 0,0,0,1};

/////////////// prodecural texture /////////////

#define TEX_SIZE 128

texture stripeTex <
    string function = "MakeStripe";
    string UIWidget = "None";
    float2 Dimensions = { TEX_SIZE, TEX_SIZE };
>;

sampler2D StripeSampler = sampler_state
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
    float4 Tangent    : TANGENT0;
    float4 Binormal    : BINORMAL0;
};

/* data passed from vertex shader to pixel shader */
struct vertexOutput {
    float4 HPosition    : POSITION;
    float2 TexCoord    : TEXCOORD0;
    float3 LightVec    : TEXCOORD1;
    float3 WorldNormal    : TEXCOORD2;
    float3 WorldEyeVec    : TEXCOORD3;
    float3 WorldTangent    : TEXCOORD4;
    float3 WorldBinorm    : TEXCOORD5;
    float4 CheckCoords    : TEXCOORD6;
};

/*********** vertex shader ******/

void sharedVS(appdata IN,
    out float3 Normal,
    out float3 Tangent,
    out float3 Binormal,
    out float3 LightVec,
    out float2 TexCoord,
    out float3 EyeVec,
    out float4 HPosition,
    out float4 Po,
    out float4 Pw,
    out float4 Pu)
{
    Normal = mul(IN.Normal,WorldITXf).xyz;
    Tangent = mul(IN.Tangent,WorldITXf).xyz;
    Binormal = mul(IN.Binormal,WorldITXf).xyz;
    Po = float4(IN.Position.xyz,1.0);
    Pw = mul(Po,WorldXf);
    Pu = mul(Pw,UserXf);	// P in "user coords"
    LightVec = (LightPos - Pw.xyz);
    TexCoord = IN.UV.xy;
    EyeVec = normalize(ViewITXf[3].xyz - Pw.xyz);
    HPosition = mul(Po,WvpXf);
}

vertexOutput userVS(appdata IN) {
    vertexOutput OUT;
    float4 Po, Pw, Pu;
    sharedVS(IN,OUT.WorldNormal,OUT.WorldTangent,OUT.WorldBinorm,
        OUT.LightVec,OUT.TexCoord,OUT.WorldEyeVec,OUT.HPosition,Po,Pw,Pu);
    OUT.CheckCoords = float4((Scale*Pu).xyz,Balance);
    return OUT;
}

vertexOutput worldVS(appdata IN) {
    vertexOutput OUT;
    float4 Po, Pw, Pu;
    sharedVS(IN,OUT.WorldNormal,OUT.WorldTangent,OUT.WorldBinorm,
        OUT.LightVec,OUT.TexCoord,OUT.WorldEyeVec,OUT.HPosition,Po,Pw,Pu);
    OUT.CheckCoords = float4((Scale*Pw).xyz,Balance);
    return OUT;
}

/////////

vertexOutput eyeVS(appdata IN) {
    vertexOutput OUT;
    float4 Po, Pw, Pu;
    sharedVS(IN,OUT.WorldNormal,OUT.WorldTangent,OUT.WorldBinorm,
        OUT.LightVec,OUT.TexCoord,OUT.WorldEyeVec,OUT.HPosition,Po,Pw,Pu);
    OUT.CheckCoords = float4((Scale*OUT.HPosition).xyz,Balance);
    return OUT;
}

vertexOutput screenVS(appdata IN) {
    vertexOutput OUT;
    float4 Po, Pw, Pu;
    sharedVS(IN,OUT.WorldNormal,OUT.WorldTangent,OUT.WorldBinorm,
        OUT.LightVec,OUT.TexCoord,OUT.WorldEyeVec,OUT.HPosition,Po,Pw,Pu);
    OUT.CheckCoords = float4((Scale*OUT.HPosition).xyz,OUT.HPosition.w);
    return OUT;
}

vertexOutput worldViewVS(appdata IN) {
    vertexOutput OUT;
    float4 Po, Pw, Pu;
    sharedVS(IN,OUT.WorldNormal,OUT.WorldTangent,OUT.WorldBinorm,
        OUT.LightVec,OUT.TexCoord,OUT.WorldEyeVec,OUT.HPosition,Po,Pw,Pu);
    OUT.CheckCoords = float4((Scale*mul(Po,WorldViewXf)).xyz,Balance);
    return OUT;
}

/////////////

vertexOutput objectVS(appdata IN) {
    vertexOutput OUT;
    float4 Po, Pw, Pu;
    sharedVS(IN,OUT.WorldNormal,OUT.WorldTangent,OUT.WorldBinorm,
        OUT.LightVec,OUT.TexCoord,OUT.WorldEyeVec,OUT.HPosition,Po,Pw,Pu);
    OUT.CheckCoords = float4(Scale*Po.xyz,Balance);
    return OUT;
}

vertexOutput uvVS(appdata IN) {
    vertexOutput OUT;
    float4 Po, Pw, Pu;
    sharedVS(IN,OUT.WorldNormal,OUT.WorldTangent,OUT.WorldBinorm,
        OUT.LightVec,OUT.TexCoord,OUT.WorldEyeVec,OUT.HPosition,Po,Pw,Pu);
    OUT.CheckCoords = float4(Scale*IN.UV.xy,Balance,Balance);
    return OUT;
}

vertexOutput normalVS(appdata IN) {
    vertexOutput OUT;
    float4 Po, Pw, Pu;
    sharedVS(IN,OUT.WorldNormal,OUT.WorldTangent,OUT.WorldBinorm,
        OUT.LightVec,OUT.TexCoord,OUT.WorldEyeVec,OUT.HPosition,Po,Pw,Pu);
    OUT.CheckCoords = float4(Scale*normalize(OUT.WorldNormal),Balance);
    return OUT;
}

/********* pixel shader ********/

// 3d checker
float4 mainPS(vertexOutput IN) :COLOR {
    float stripex = tex2D(StripeSampler,IN.CheckCoords.xw).x;
    float stripey = tex2D(StripeSampler,IN.CheckCoords.yw).x;
    float stripez = tex2D(StripeSampler,IN.CheckCoords.zw).x;
    float check = abs(abs(stripex - stripey) - stripez);
    return lerp(SurfColor1,SurfColor2,check);
}

// 3d checker
float4 screenPS(vertexOutput IN) :COLOR {
    float2 Px = float2(IN.CheckCoords.x/IN.CheckCoords.w,Balance);
    float2 Py = float2(IN.CheckCoords.y/IN.CheckCoords.w,Balance);
    float stripex = tex2D(StripeSampler,Px).x;
    float stripey = tex2D(StripeSampler,Py).x;
    float check = abs(stripex - stripey);
    return lerp(SurfColor1,SurfColor2,check);
}

float4 mainLitPS(vertexOutput IN) :COLOR {
    float3 Ln = normalize(IN.LightVec);
    float3 Nn = normalize(IN.WorldNormal);
    float stripex = tex2D(StripeSampler,IN.CheckCoords.xw).x;
    float stripey = tex2D(StripeSampler,IN.CheckCoords.yw).x;
    float stripez = tex2D(StripeSampler,IN.CheckCoords.zw).x;
    float check = abs(abs(stripex - stripey) - stripez);
    float4 dColor = lerp(SurfColor1,SurfColor2,check);
    float3 Vn = normalize(IN.WorldEyeVec);
    float3 Hn = normalize(Vn + Ln);
    float4 lighting = lit(dot(Ln,Nn),dot(Hn,Nn),SpecExpon);
    float hdn = lighting.z; // Specular coefficient
    float ldn = lighting.y; // Diffuse coefficient
    float4 diffContrib = dColor * (ldn*LightColor + AmbiColor);
    float4 specContrib = hdn * LightColor;
    float4 result = diffContrib + (Ks * specContrib);
    return dColor; //result;
}

// 2d checker
float4 mainUvPS(vertexOutput IN) :COLOR {
    float3 Ln = normalize(IN.LightVec);
    float3 Nn = normalize(IN.WorldNormal);
    float stripex = tex2D(StripeSampler,IN.CheckCoords.xw).x;
    float stripey = tex2D(StripeSampler,IN.CheckCoords.yw).x;
    float check = abs(stripex - stripey);
    return lerp(SurfColor1,SurfColor2,check);
}

/****************************************************************/
/****************************************************************/
/******* TECHNIQUES *********************************************/
/****************************************************************/
/****************************************************************/

#define TECH(name,VS,PS) technique name { pass p0 { \
		VertexShader = compile vs_2_0 VS (); \
		ZEnable = true; ZWriteEnable = true; CullMode = None; \
		PixelShader = compile ps_2_0 PS (); } }

TECH(worldSpace, worldVS, mainPS)
TECH(objectSpace, objectVS, mainPS)
TECH(eyeSpace, eyeVS, mainPS)
TECH(screenSpace, screenVS, screenPS)
TECH(worldViewSpace, worldViewVS, mainPS)
TECH(uvSpace, uvVS, mainUvPS)
TECH(userCoordSysSpace, userVS, mainPS)
TECH(normalSpace, normalVS, mainPS)

/***************************** eof ***/
