/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/MoBlur.fx#1 $

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
	Accumulation Buffer Motion Blur
	Store the previous-frame's transform in FP32 texture!
	To use this, drag the camera around -- use the shift or control keys.

******************************************************************************/

float Script : STANDARDSGLOBAL
<
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";	
	// We just call a script in the main technique.
	string Script = "Technique=Blur;";
> = 0.8; // version number

#define QUAD_REAL float
#define QUAD_REAL2 float2
#define QUAD_REAL3 float3
#define QUAD_REAL4 float4
#define QUAD_REAL4x4 float4x4
#define QUAD_REAL3x3 float3x3

#include "F:\\devrel\\SDK\\MEDIA\\HLSL\\Quad.fxh"

// #define USE_TIMER

///////////////////////////////////////////////////////////////
/// UNTWEAKABLES //////////////////////////////////////////////
///////////////////////////////////////////////////////////////

float4x4 WorldITXf : WorldInverseTranspose <string UIWidget="None";>;
float4x4 WvpXf : WorldViewProjection  <string UIWidget="None";>;
float4x4 WorldXf : World <string UIWidget="None";>;
float4x4 ViewIXf : ViewInverse <string UIWidget="None";>;
float4x4 WorldViewITXf : WorldViewInverseTranspose <string UIWidget="None";>;
float4x4 WorldViewXf : WorldView <string UIWidget="None";>;
float4x4 ViewXf : View <string UIWidget="None";>;
float4x4 ViewITXf : ViewInverseTranspose <string UIWidget="None";>;

float passnumber <string UIWidget = "none";>; // loop counter, hidden

///////////////////////////////////////////////////////////////
/// TWEAKABLES ////////////////////////////////////////////////
///////////////////////////////////////////////////////////////

float npasses <
	float UIStep = 1.0;
	string UIName = "Blur passes";
> = 16.0f;

float Accel
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.01;
    string UIName = "Blur biasing";
> = 1.0;

float Bright
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 8.0;
    float UIStep = 0.01;
    string UIName = "Brighten";
> = 1.0;

////////////////////////////////////////////// spot light

float3 LightDir : DIRECTION <
	string UIName = "Light Direction";
	string Object = "DirectionalLight";
	string Space = "World";
> = {-0.707f, 0.707f, 0.0f};

////////////////////////////////////////////// ambient light

float4 AmbiLightColor : Ambient
<
    string UIName = "Ambient Light";
	string UIWidget = "Color";
> = {0.07f, 0.07f, 0.07f, 1};

////////////////////////////////////////////// surface

float4 SurfColor : Diffuse
<
    string UIName = "Surface";
	string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f, 1};

float Ks
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.5;
    float UIStep = 0.01;
    string UIName = "Specular";
> = 1.0;


float SpecExpon : SpecularPower
<
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 128.0;
    float UIStep = 1.0;
    string UIName = "Specular power";
> = 52.0;

float Shutter <
	string UIName = "Shutter 0-1";
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 1.0;
	float UIStep = 0.01;
> = 0.5f;

////////////////////////////////////////////////////////
/// TEXTURES ///////////////////////////////////////////
////////////////////////////////////////////////////////

texture DiffTexture : Diffuse <
	string ResourceName = "default_color.dds";
	string ResourceType = "2D";
>;

sampler2D DiffSampler = sampler_state
{
	Texture = <DiffTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = WRAP;
	AddressV = WRAP;
};

//////////////////

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Data //////
///////////////////////////////////////////////////////////

float4 ClearColor : DIFFUSE <
	string UIName="Background";
	string UIWidget = "color";
> = {0,0,0,1.0};

float ClearDepth
<
	string UIWidget = "none";
> = 1.0;

bool bReset : FXCOMPOSER_RESETPULSE
<
	string UIName="Reset";
>;

DECLARE_QUAD_TEX(EachMap,EachSampler,"A8R8G8B8")
DECLARE_QUAD_TEX(AccumBuffer,AccumSampler,"A16B16G16R16F")

DECLARE_QUAD_DEPTH_BUFFER(SceneDepth,"D24S8")

DECLARE_SQUARE_QUAD_TEX(WvpTex,WvpSamp,"R32F",4)

/// identity matrix as a convenient initializer


// function used to fill the volume noise texture
float4 identity_xf(float2 Pos : POSITION) : COLOR
{
    float xf = 0;
    if (Pos.x == Pos.y) {
    	xf = 1;
    }
    return xf.xxxx;
}

texture IdentityTex  <
    string TextureType = "2D";
    string function = "identity_xf";
    string UIWidget = "None";
    int width = 4, height = 4;
    string Format = "r32f";
>;

// samplers
sampler IdentitySamp = sampler_state 
{
    texture = <IdentityTex>;
    AddressU  = CLAMP;        
    AddressV  = CLAMP;
    AddressW  = CLAMP;
    MIPFILTER = POINT;
    MINFILTER = Point;
    MAGFILTER = Point;
};

////////////////////////////////////////////////////////////////////////////
/// SHADER CODE BEGINS /////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////

/* data from application vertex buffer */
struct appdata {
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;
    float4 Normal	: NORMAL;
};

// used for all other passes
struct vertexOutput {
    float4 HPosition	: POSITION;
    float2 UV		: TEXCOORD0;
    float3 WNormal	: TEXCOORD1;
    float3 WView		: TEXCOORD2;
	float4 DiffCol : COLOR0;
};

/*********************************************************/
/*********** vertex shader *******************************/
/*********************************************************/

vertexOutput blurVS(appdata IN,uniform float delta)
{
    vertexOutput OUT;
    float4x4 PrevWvpXf;
    PrevWvpXf[0][0] = tex2Dlod(WvpSamp,float4(0,0,0,0)).x;
    PrevWvpXf[1][0] = tex2Dlod(WvpSamp,float4(0,.33,0,0)).x;
    PrevWvpXf[2][0] = tex2Dlod(WvpSamp,float4(0,.66,0,0)).x;
    PrevWvpXf[3][0] = tex2Dlod(WvpSamp,float4(0,1,0,0)).x;
    PrevWvpXf[0][1] = tex2Dlod(WvpSamp,float4(.33,0,0,0)).x;
    PrevWvpXf[1][1] = tex2Dlod(WvpSamp,float4(.33,.33,0,0)).x;
    PrevWvpXf[2][1] = tex2Dlod(WvpSamp,float4(.33,.66,0,0)).x;
    PrevWvpXf[3][1] = tex2Dlod(WvpSamp,float4(.33,1,0,0)).x;
    PrevWvpXf[0][2] = tex2Dlod(WvpSamp,float4(.66,0,0,0)).x;
    PrevWvpXf[1][2] = tex2Dlod(WvpSamp,float4(.66,.33,0,0)).x;
    PrevWvpXf[2][2] = tex2Dlod(WvpSamp,float4(.66,.66,0,0)).x;
    PrevWvpXf[3][2] = tex2Dlod(WvpSamp,float4(.66,1,0,0)).x;
    PrevWvpXf[0][3] = tex2Dlod(WvpSamp,float4(1,0,0,0)).x;
    PrevWvpXf[1][3] = tex2Dlod(WvpSamp,float4(1,.33,0,0)).x;
    PrevWvpXf[2][3] = tex2Dlod(WvpSamp,float4(1,.66,0,0)).x;
    PrevWvpXf[3][3] = tex2Dlod(WvpSamp,float4(1,1,0,0)).x;
    float4x4 NewWvpXf = lerp(PrevWvpXf,WvpXf,delta);
    OUT.UV = IN.UV.xy;
    float3 Nn = normalize(mul(IN.Normal, WorldITXf).xyz); // world Coords
    float4 Po = float4(IN.Position.xyz,1);	// obj coords
    OUT.WView = normalize(ViewIXf[3].xyz - Po.xyz);	// obj coords
    OUT.WNormal = Nn; // mul(Po,WorldViewProjXf);	// screen clipspace coords
	float d = dot(-LightDir,Nn);
	OUT.DiffCol = float4(max(0,d).xxx,1.0);
    float4 Ph = mul(Po, NewWvpXf);
    OUT.HPosition = Ph;
    return OUT;
}

/*********************************************************/
/*********** pixel shader ********************************/
/*********************************************************/

float4 nPS(vertexOutput IN, uniform float delta) : COLOR
{
    float3 Nn = normalize(IN.WNormal);
    float3 Vn = normalize(IN.WView);
    float3 Ln = -LightDir;
    float3 Hn = normalize(Vn + Ln);
    float hdn = Ks * pow(dot(Hn,Nn),SpecExpon);
    float4 sc = float4(hdn.xxx,0);
	float4 tc = tex2D(DiffSampler,IN.UV);
	float4 dc = tc * SurfColor * (IN.DiffCol + AmbiLightColor);
	return delta*(dc+sc);
}

///////////////////////////////////////////////////////////
/// Final Pass ////////////////////////////////////////////
///////////////////////////////////////////////////////////

QUAD_REAL4 accumPS(QuadVertexOutput IN) : COLOR
{   
	QUAD_REAL4 texCol = tex2D(EachSampler, IN.UV);
	return texCol;
}

QUAD_REAL4 finalPS(QuadVertexOutput IN) : COLOR
{   
	QUAD_REAL4 texCol = Bright * tex2D(AccumSampler, IN.UV) / npasses;
	return texCol;
}  

QUAD_REAL4 saveXfPS(QuadVertexOutput IN) : COLOR
{   
	float rndx = floor(IN.UV.y*3.99);
	float4 row = WvpXf[0];
	if (rndx==1) row=WvpXf[1];
	if (rndx==2) row=WvpXf[2];
	if (rndx==3) row=WvpXf[3];
	float cndx = floor(IN.UV.x*3.99);
	float col = row.x;
	if (cndx==1) col=row.y;
	if (cndx==2) col=row.z;
	if (cndx==3) col=row.w;
	return col.xxxx;
}

////////////////////////////////////////////////////////////////////
/// TECHNIQUES /////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////

technique Blur <
	string Script =
			"LoopByCount=bReset;"
				"Pass=resetXf;"
			"LoopEnd=;"
			// Clear Accum Buffer
			"RenderColorTarget0=AccumBuffer;"
			"ClearSetColor=ClearColor;"
			"Clear=Color;"
        	"LoopByCount=npasses;"
        	//"LoopBegin;"
				"LoopGetIndex=passnumber;"
				// Render Object(s)
				"Pass=drawObj;"
				// Blend Results into accum buffer
				"Pass=Accumulate;"
	        "LoopEnd;"
	        // draw accum buffer to framebuffer
	        "Pass=saveXf;"
	        "Pass=FinalPass;";
> {
	pass resetXf <
		string Script = "RenderColorTarget0=WvpTex;"
						"RenderDepthStencilTarget=;"
						"Draw=buffer;";
	> {
		VertexShader = compile vs_3_0 ScreenQuadVS();
		ZEnable = false;
		AlphaBlendEnable = false;
		PixelShader  = compile ps_3_0 TexQuadPS(IdentitySamp);
	}
	pass saveXf <
		string Script = "RenderColorTarget0=WvpTex;"
						"RenderDepthStencilTarget=;"
						"Draw=buffer;";
	> {
		VertexShader = compile vs_3_0 ScreenQuadVS();
		ZEnable = false;
		AlphaBlendEnable = false;
		PixelShader  = compile ps_3_0 saveXfPS();
	}
	pass drawObj <
		string Script =
				"RenderColorTarget0=EachMap;"
				"RenderDepthStencilTarget=SceneDepth;"
	        	"ClearSetColor=ClearColor;"
	        	"ClearSetDepth=ClearDepth;"
   				"Clear=Color;"
				"Clear=Depth;"
				"Draw=geometry;";
	> {
		VertexShader = compile vs_3_0 blurVS((passnumber/(npasses-1.0)));
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		AlphaBlendEnable = false;
		PixelShader = compile ps_3_0 nPS((1-Accel)+Accel*(passnumber/(npasses-1.0)));
	}
	pass Accumulate <
		string Script = 
				"RenderColorTarget0=AccumBuffer;"
				"RenderDepthStencilTarget=;"
				"Draw=Buffer;";
	> {
		VertexShader = compile vs_3_0 ScreenQuadVS();
		ZEnable = false;
		AlphaBlendEnable = true;
		SrcBlend = ONE;
		DestBlend = ONE;
		PixelShader  = compile ps_3_0 accumPS();
	}
	pass FinalPass <
		string Script =
	        "RenderColorTarget0=;"
	        "RenderDepthStencilTarget=;"
			"Draw=Buffer;";
	> {
		VertexShader = compile vs_3_0 ScreenQuadVS();
		AlphaBlendEnable = false;
		ZEnable = false;
		PixelShader  = compile ps_3_0 finalPS();
	}
}

/***************************** eof ***/
