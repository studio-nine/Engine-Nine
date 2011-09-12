/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.0/SDK/MEDIA/HLSL/EasyBake.fx#8 $

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
    Simple pixel-shaded point-lit plastic without falloff -- but with
    	lighting baked into a texture. See "bakeliteTimes9" for example
		of application to instantiations.
		
Culling currently whacked.....

******************************************************************************/

// string XFile = "tiger.x";

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "object";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";
	string Script = "Technique=Main;";
> = 0.8; // version #

float4 ClearColor <
	string UIWidget = "color";
	string UIName = "background";
> = {0,0,0,0};

float ClearDepth <string UIWidget = "none";> = 1.0;

/************* UN-TWEAKABLES **************/

float4x4 WorldITXf : WorldInverseTranspose < string UIWidget="None"; > = {1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1};
float4x4 WvpXf : WorldViewProjection < string UIWidget="None"; >;
float4x4 WorldXf : World < string UIWidget="None"; >;
float4x4 ViewIXf : ViewInverse < string UIWidget="None"; >;

/*********** Tweakables **********************/

float3 LightPos : Position
<
    string Object = "PointLight";
    string Space = "World";
> = {-1.0f, 1.0f, -0.4f};

float3 LightColor
<
    string UIName =  "Lamp";
    string UIWidget = "Color";
    string Object = "PointLight";
> = {1.0f, 1.0f, 1.0f};

//////

float ThisInstance <string UIWidget = "none";>; // loop counter, hidden

float NInstances <
	float UIStep = 1.0;
	string UIName = "Baked Instances";
> = 3.0f;

float spacing
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 5.0;
    float UIStep = 0.01;
    string UIName =  "Spacing for instances";
> = 2.1;

/////////////

float3 SurfColor : DIFFUSE
<
    string UIName =  "Surface";
    string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f};

float3 AmbiColor : Ambient
<
    string UIName =  "Ambient";
    string UIWidget = "Color";
> = {0.1f, 0.1f, 0.1f};

float Ks
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.01;
    string UIName =  "specular intensity";
> = 0.5;

float SpecExpon : SpecularPower
<
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 128.0;
    float UIStep = 1.0;
    string UIName =  "specular power";
> = 30.0;

/// texture ///////////////////////////////////

texture colorTexture : DIFFUSE
<
    string ResourceName = "tiger.bmp";
    string ResourceType = "2D";
>;

sampler2D ColorSampler = sampler_state
{
    Texture = <colorTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};

///////////

#define BAKE_SIZE 256

texture BakeTex : RENDERCOLORTARGET < 
    float2 Dimensions = { BAKE_SIZE, BAKE_SIZE};
    int MIPLEVELS = 1;
    string format = "X8R8G8B8";
    string UIWidget = "None";
>;

texture DepthBuffer : RENDERDEPTHSTENCILTARGET <
	float2 Dimensions = { BAKE_SIZE, BAKE_SIZE};
	string format = "D24S8";
>;

sampler BakeSampler = sampler_state 
{
    texture = <BakeTex>;
    AddressU  = CLAMP;        
    AddressV  = CLAMP;
    MIPFILTER = LINEAR;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};

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
    float2 UV        : TEXCOORD0;
    float3 LightVec    : TEXCOORD1;
    float3 WorldNormal    : TEXCOORD2;
    float3 WorldEyeVec    : TEXCOORD3;
};

/* data passed from vertex shader to pixel shader */
struct bakedVertexOutput {
    float4 HPosition    : POSITION;
    float2 UV        : TEXCOORD0;
};

/*********** vertex shader ******/

// use this to create baked lighting textures
vertexOutput bakeVS(appdata IN) {
    vertexOutput OUT;
    OUT.WorldNormal = mul(IN.Normal, WorldITXf).xyz;
    float4 Po = float4(IN.Position.xyz,1.0);
    float3 Pw = mul(Po, WorldXf).xyz;
    OUT.LightVec = (LightPos - Pw);
    OUT.WorldEyeVec = (ViewIXf[3].xyz - Pw);
    // OUT.HPosition = mul(Po, WvpXf);
    float2 nuPos = float2(IN.UV.x,1-IN.UV.y);
    nuPos = 2.0*(nuPos-0.5);
    OUT.HPosition = float4(nuPos,1.0,1.0);
    OUT.UV = IN.UV.xy;
    return OUT;
}

// use this to apply pre-baked lighting textures
bakedVertexOutput minimalVS(appdata IN,uniform float3 offset) {
    bakedVertexOutput OUT;
    float4 Po = float4((IN.Position.xyz+offset),1.0);
    OUT.HPosition = mul(Po, WvpXf);
    OUT.UV = IN.UV.xy;
    return OUT;
}

/********* pixel shader ********/

// simple plastic shading (DiffColor could be textured or not)
float3 sharedPS(vertexOutput IN,float3 DiffColor) :COLOR {
    float3 Ln = normalize(IN.LightVec);
    float3 Nn = normalize(IN.WorldNormal);
    float3 Vn = normalize(IN.WorldEyeVec);
    float3 Hn = normalize(Vn + Ln);
    float4 lv = lit(dot(Ln,Nn),dot(Hn,Nn),SpecExpon);
    float3 diffContrib = DiffColor * (lv.y*LightColor + AmbiColor);
    float3 specContrib = Ks * lv.z * LightColor;
    return diffContrib + specContrib;
}

// simple plastic shading with flat color
float4 untexturedPS(vertexOutput IN) :COLOR {
    return float4(sharedPS(IN,SurfColor),1);
}

// simple plastic shading with texture
float4 texturedPS(vertexOutput IN) :COLOR {
    return float4(sharedPS(IN,(SurfColor*tex2D(ColorSampler,IN.UV).xyz)),1);
}

// just use pre-baked texture for all color/lighting
float4 prebakedPS(bakedVertexOutput IN) :COLOR {
    return float4(tex2D(BakeSampler,IN.UV).xyz,1);
}


/////// VM function for instancing ///////////

float3 instance_offset()
{
	float x = (ThisInstance - ((NInstances-1)/2.0)) * spacing;
	return (float3(x,0,0));
}

/*************/

technique Main <
	string ScriptClass = "object";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";
	string Script =
        	"Pass=bake;"
			"RenderColorTarget0=;"
	        "RenderDepthStencilTarget=;"
			"ClearSetColor=ClearColor;"
	        "ClearSetDepth=ClearDepth;"
   			"Clear=Color;"
			"Clear=Depth;"
        	"LoopByCount=NInstances;"
				"LoopGetIndex=ThisInstance;"
				"Pass=useBakedLighting;"
	        "LoopEnd;";
> {
    pass bake <
		string Script =
			"RenderColorTarget0=BakeTex;"
	        "RenderDepthStencilTarget=DepthBuffer;"
	        	"ClearSetColor=ClearColor;"
	        	"ClearSetDepth=ClearDepth;"
   				"Clear=Color;"
				"Clear=Depth;"
	        	"Draw=geometry;";
	> {        
        VertexShader = compile vs_2_0 bakeVS();
        ZEnable = true;
        ZWriteEnable = true;
        CullMode = None;
        PixelShader = compile ps_2_0 texturedPS();
    }
    pass useBakedLighting <
		string Script = 
				"Draw=geometry;";
	> {        
        VertexShader = compile vs_1_1 minimalVS(instance_offset());
        ZEnable = true;
        ZWriteEnable = true;
        ZFunc = LessEqual;
        CullMode = None;
        PixelShader = compile ps_1_1 prebakedPS();
    }
}

/***************************** eof ***/
