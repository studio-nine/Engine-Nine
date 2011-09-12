/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/durer.fx#1 $

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
    Works fine in effectedit
    $Date: 2004/09/24 $

******************************************************************************/

string XFile = "bigship1.x"; 
//string XFile = "teapot.x";
int    BCLR  = 0xfff0c010;

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=durerDX;";
> = 0.8;

/************* UN-TWEAKABLES **************/

half4x3 WorldView  : WORLDVIEW < string UIWidget="None"; >;
half4x4 Projection : PROJECTION < string UIWidget="None"; >;

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
    half4 TexCoord    : TEXCOORD0;//
};

/******** TWEAKABLES ****************************************/

half3 LightDir : DIRECTION < 
    string Object = "DirectionalLight";
> = {0.577, -0.577, 0.577};

half4 BrightColor <
    string UIName = "Bright";
	string UIWidget = "Color";
> = {1.0f, 0.8f, 0.0f, 1.0f};

half4 DarkColor <
    string UIName = "Dark";
	string UIWidget = "Color";
> = {0.0f, 0.2f, 0.4f, 1.0f};

half4 LampColor <
    string UIName = "Lamp";
	string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f, 1.0f};

half Ks <
    string UIWidget = "slider";
    half UIMin = 0.0;
    half UIMax = 2.0;
    half UIStep = 0.01;
    string UIName = "specular";
> = 0.95;

half SpecExpon : SpecularPower <
    string UIWidget = "slider";
    half UIMin = 1.0;
    half UIMax = 128.0;
    half UIStep = 1.0;
    string UIName = "specular power";
> = 15.0;

//

half StripeScale : UNITSSCALE <
    string UIWidget = "slider";
    half UIMin = 0.001;
    half UIMax = 1.0;
    half UIStep = 0.001;
    string UIName = "Diff Stripes";
    string units = "inches";
> = 0.05;

half SpecScale : UNITSSCALE <
    string units = "inches";
    string UIWidget = "slider";
    half UIMin = 0.001;
    half UIMax = 1.0;
    half UIStep = 0.001;
    string UIName = "Spec Stripes";
> = 0.035;

/////////////// prodecural texture /////////////

#define TEX_SIZE 64

half4 stripe_function(half2 Pos : POSITION,half ps : PSIZE) : COLOR
{
   half v = 0;
   half nx = Pos.x+ps; // keep the last column full-on, always
   v = nx > Pos.y;
   return half4(v.xxxx);
}

texture stripeTex <
    string function = "stripe_function";
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

/*********** vertex shader ******/

vertexOutput mainVS(appdata IN) {
    vertexOutput OUT;
    half4 Po = half4(IN.Position.x,IN.Position.y,IN.Position.z,1.0);    // object space
                            // or (practically for EffectEdit) world space
    half3 Pv = mul(Po, (half4x3)WorldView);            // position (view space)
    half3 Nv = normalize(mul(IN.Normal, (half3x3)WorldView));    // normal (view space)
    half3 Lv = -LightDir;                    // pre-normalized, in view space
    half3 Rv = normalize(2 * dot(Nv,Lv)*Nv - Lv);        // reflection vector (view space)
    half3 V = -normalize(Pv);                    // view direction (view space)
    half4 hpos  = mul(half4(Pv, 1), Projection);        // position (projected)
    OUT.HPosition  = hpos;
    half d = max(0, dot(Nv, Lv)); // diffuse (ambient?)
    half s = Ks * pow(max(0, dot(Rv, V)), SpecExpon);   // specular
    OUT.TexCoord = half4(hpos.x/(StripeScale*hpos.w),d,
						hpos.y/(SpecScale*hpos.w),s);
    return OUT;
}

/******************** pixel shader *********************/

half4 strokeTexPS(vertexOutput IN) : COLOR {
    half stripes = tex2D(stripeSampler,IN.TexCoord.xy).x;
    half4 dColor = lerp(BrightColor,DarkColor,stripes);
    stripes = tex2D(stripeSampler,IN.TexCoord.zw).x;
    dColor = lerp(LampColor,dColor,stripes);
    return dColor;
}

/*************/

technique durerDX <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
> {        
        VertexShader = compile vs_2_0 mainVS();
        ZEnable = true;
        ZWriteEnable = true;
        CullMode = None;
        PixelShader = compile ps_2_0 strokeTexPS();
    }
}

/***************************** eof ***/
