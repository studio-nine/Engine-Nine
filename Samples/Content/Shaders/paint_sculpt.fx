/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/paint_sculpt.fx#1 $

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
	An .FX Paint-Sculpt Program that needs scene geometry to use "showUV.fx"
	Uses ps_3_0 and vs_3_0 for this effect.
	Draw with the left mouse button.
	To clear screen, resize the window or set "Clear Canvas".
	Toggle "Sculpting Now?" to draw or not draw (for tumbling camera etc).
	For brush strokes will change in size and opacity over time, set USE_TIMER --
		assign "FadeTime" to a high value for more even (though
		less expressive) strokes.
	Textures are signed floating-point -- they may DISPLAY as
		black but may have lots of negative detail.
	Watch out for stray light source objects! Select the scene window's
		"Scene->Render Options->Show Lights" to turn them off

******************************************************************************/

#include "Quad.fxh"

///////////////////////////////////////////////////
/// UNTWEAKABLES //////////////////////////////////
///////////////////////////////////////////////////

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";
	string Script = "Technique=Main;";
> = 0.8; // version #

bool bReset : FXCOMPOSER_RESETPULSE
<
	string UIName="Clear Canvas";
>;

bool Sculpting
<
	string UIName="Sculpting Now?";
> = true;

float ClearDepth <string UIWidget = "none";> = 1.0;

float4x4 WorldViewProjXf : WorldViewProjection < string UIWidget="None"; >;
float4x4 WorldITXf : WorldInverseTranspose <string UIWidget="None";>;

// #define USE_TIMER

float4 MouseL : LEFTMOUSEDOWN < string UIWidget="None"; >;
float3 MousePos : MOUSEPOSITION < string UIWidget="None"; >;
#ifdef USE_TIMER
float Timer : TIME < string UIWidget = "None"; >;
#endif /* USE_TIMER */

///////////////////////////////////////////////////
/// TWEAKABLES ////////////////////////////////////
///////////////////////////////////////////////////

float4 ClearColor <
	string UIWidget = "color";
	string UIName = "Background";
> = {0,0,0,0.0};

float Opacity <
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 1.0;
	float UIStep = 0.01;
> = 0.45f;

#ifdef USE_TIMER
float BrushSizeStart <
	string UIWidget = "slider";
	float UIMin = 0.001;
	float UIMax = 0.15;
	float UIStep = 0.001;
	string UIName = "Start Brush Size";
> = 0.07f;

float BrushSizeEnd <
	string UIWidget = "slider";
	float UIMin = 0.001;
	float UIMax = 0.15;
	float UIStep = 0.001;
	string UIName = "End Brush Size";
> = 0.01f;

float FadeTime <
	string UIWidget = "slider";
	float UIMin = 0.1;
	float UIMax = 10.0;
	float UIStep = 0.1;
	string UIName = "Brush Fade Time";
> = 2.00f;

#else /* !USE_TIMER */
float BrushSize <
	string UIWidget = "slider";
	float UIMin = 0.001;
	float UIMax = 0.15;
	float UIStep = 0.001;
	string UIName = "Brush Size";
> = 0.07f;
#endif /* !USE_TIMER */

float PaintValue <
	string UIWidget = "slider";
	float UIMin = -1.0;
	float UIMax = 1.0;
	float UIStep = 0.01;
	string UIName = "Paint In/Out";
> = 0.0f;

float Bumpy <
	string UIName = "Overall Strength";
	string UIWidget = "slider";
	float UIMin = -1.0;
	float UIMax = 1.0;
	float UIStep = 0.02;
> = 1.0f;

float Coloring <
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 1.0;
	float UIStep = 0.01;
> = 0.25f;

float Texturing <
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 1.0;
	float UIStep = 0.01;
	string UIName = "Tex Saturate";
> = 1.0f;

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Data //////
///////////////////////////////////////////////////////////

texture SurfTex : Diffuse <
	string ResourceName = "default_color.dds";
	string TextureType = "2D";
	string UIName = "Surface Tex";
>;

sampler2D SurfSampler = sampler_state
{
	Texture = <SurfTex>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = WRAP;
	AddressV = WRAP;
};
// first render model UVs to a texture
texture UVMap : RENDERCOLORTARGET < 
    float2 ViewPortDimensions = {1.0,1.0};
    int MIPLEVELS = 1;
    string Format = "a16b16g16r16f";
    string UIWidget = "None";
>;
texture DepthBuffer : RENDERDEPTHSTENCILTARGET
<
	float2 ViewportRatio = { 1.0f, 1.0f};
    string format = "D24S8";
    string UIWidget = "None";
>;
sampler UVSampler = sampler_state {
    texture = <UVMap>;
    AddressU  = CLAMP;        
    AddressV  = CLAMP;
    AddressW  = CLAMP;
    MIPFILTER = NONE;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};

// depth target dummy texture 
texture DepthTarget : RENDERDEPTHSTENCILTARGET < 
    float2 ViewPortDimensions = {1.0,1.0};
    string format = "D24S8";
    string UIWidget = "None";
>;

///////////////////// render final


texture PaintStrokeMap : RENDERCOLORTARGET < 
    float2 ViewPortDimensions = {1.0,1.0};
    int MIPLEVELS = 1;
    string Format = "a16b16g16r16f";
    string UIWidget = "None";
>;

sampler PaintStrokeSampler = sampler_state {
    texture = <PaintStrokeMap>;
    AddressU  = CLAMP;        
    AddressV  = CLAMP;
    AddressW  = CLAMP;
    MIPFILTER = NONE;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};

texture PaintBufferMap : RENDERCOLORTARGET < 
    float2 ViewPortDimensions = {1.0,1.0};
    int MIPLEVELS = 1;
    string Format = "a16b16g16r16f";
    string UIWidget = "None";
>;

sampler PaintBufferSampler = sampler_state {
    texture = <PaintBufferMap>;
    AddressU  = CLAMP;        
    AddressV  = CLAMP;
    AddressW  = CLAMP;
    MIPFILTER = NONE;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};

texture DisplaceMap : RENDERCOLORTARGET < 
    float2 ViewPortDimensions = {1.0,1.0};
    int MIPLEVELS = 1;
    string Format = "a32b32g32r32f";
    string UIWidget = "None";
>;

sampler DisplaceSampler = sampler_state {
    texture = <DisplaceMap>;
    AddressU  = CLAMP;        
    AddressV  = CLAMP;
    AddressW  = CLAMP;
    MIPFILTER = NONE;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};

/****************************************/
/*** The 3D shaders: ********************/
/****************************************/

/* data from application vertex buffer */
struct appdata {
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;
    float4 Normal	: NORMAL;
};

////////////// vertex shader with displacement

QuadVertexOutput basic3DVS(appdata IN) {
	QuadVertexOutput OUT;
	float2 uv = IN.UV.xy;
	float4 t = tex2Dlod(DisplaceSampler,float4(uv,0,0));
	float d = t.x * Bumpy;
    OUT.UV = uv;
    float4 No = normalize(IN.Normal);
    float3 Nn = normalize(mul(No, WorldITXf).xyz);
    float4 Po = float4(IN.Position.xyz,1.0);	// object coordinates
	Po = Po + d*IN.Normal;
    OUT.Position = mul(Po,WorldViewProjXf);	// screen clipspace coords
    return OUT;
}

// uv-drawing pixel shader

float4 uvPS(QuadVertexOutput IN) : COLOR {
	return float4(IN.UV,0,1);
}

/****************************************/
/*** The dead-simple shaders: ***********/
/****************************************/

// PS -- add brush stroke to quad (and target "PaintStrokeMap")
float4 brushPS(QuadVertexOutput IN) : COLOR
{
	float4 uvmap = tex2D(UVSampler,MousePos.xy);
	//float4 uvmap = tex2D(UVSampler,2.0*MousePos.xy);
    float2 delta = IN.UV-uvmap.xy;
	float dd = uvmap.w * Opacity * Sculpting;
#ifdef USE_TIMER
	float fadeout = 1 - min(1,max(0,(Timer - MouseL.w))/FadeTime);
	float ds = lerp(BrushSizeEnd,BrushSizeStart,fadeout);
    dd *= fadeout;
    dd *= MouseL.z*(1-min(length(delta)/ds,1));
#else /* USE_TIMER */
    dd *= MouseL.z*(1-min(length(delta)/BrushSize,1));
#endif /* USE_TIMER */
    return dd*float4(PaintValue.xxx,1);
}

// apply "PaintStrokeMap" based on the UVs stored in "UVMap"
float4 showTexPS(QuadVertexOutput IN) : COLOR
{
	float4 uvmap = tex2D(UVSampler,IN.UV);
	float3 c = uvmap.w * tex2D(DisplaceSampler,uvmap.xy).xyz;
	c = lerp(c,uvmap.xyz,Coloring);
	float3 st = tex2D(SurfSampler,uvmap.xy).xyz;
	c = lerp(c,st,Texturing);
	// c = c * st;
    return float4(c,uvmap.w);
    // return float4(uvmap.xyz,1); // temp hack to see something
}


////////////////// Technique ////////

technique Main <
	string ScriptClass = "scene";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";
	string Script =
        	"Pass=ShowUV;"
        	"Pass=restorePaint;"
        	"Pass=brushStroke;"
        	"Pass=boost;"
	        "Pass=showTex;";
> {
	// first draw UV values to screen
	pass ShowUV <
    	string Script = "LoopByCount=bReset;"
							"ClearSetColor=ClearColor;"
							"RenderColorTarget0=PaintBufferMap;"
							"Clear=Color0;"
							"RenderColorTarget0=PaintStrokeMap;"
							"Clear=Color0;"
    					"LoopEnd=;"
    					"RenderColorTarget0=UVMap;"
						"RenderDepthStencilTarget=DepthBuffer;"
						"ClearSetColor=ClearColor;"
						"ClearSetDepth=ClearDepth;"
						"Clear=Color;"
						"Clear=Depth;"
						"Draw=Scene;";
	> {
		VertexShader = compile vs_3_0 basic3DVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		AlphaBlendEnable = false;
		PixelShader = compile ps_3_0 uvPS();
	}
	// fill fb with previous state of "PaintStrokeMap"
	pass restorePaint <
    	string Script = "RenderColorTarget0=PaintBufferMap;"
						"Draw=Buffer;";
	> {
		VertexShader = compile vs_3_0 ScreenQuadVS();
		AlphaBlendEnable = false;
		ZEnable = false;
		PixelShader = compile ps_3_0 TexQuadPS(PaintStrokeSampler);
	}
	//  blend new paint into "PaintStrokeMap" texture
	pass brushStroke  <
    	string Script = "RenderColorTarget0=PaintStrokeMap;"
						"Draw=Buffer;";
	> {
		VertexShader = compile vs_3_0 ScreenQuadVS();
		AlphaBlendEnable = true;	// fp16 blend
		SrcBlend = One;
		DestBlend = InvSrcAlpha;
		ZEnable = false;
		PixelShader  = compile ps_3_0 brushPS();
	}
	// bump-up to 32 bit
	pass boost <
    	string Script = "RenderColorTarget0=DisplaceMap;"
						"Draw=Buffer;";
	> {
		VertexShader = compile vs_3_0 ScreenQuadVS();
		ZEnable = false;
		ZWriteEnable = false;
		CullMode = None;
		PixelShader = compile ps_3_0 TexQuadPS(PaintStrokeSampler);
	}
	// draw revised texture onto surface using stored UVs
	pass showTex <
    	string Script = "RenderColorTarget0=;"
						"Draw=Buffer;";
	> {
		VertexShader = compile vs_3_0 ScreenQuadVS();
		AlphaBlendEnable = false;
		ZEnable = false;
		PixelShader  = compile ps_3_0 showTexPS();
	}
}

/////////////////////////////////////////////
///////////////////////////////////// eof ///
/////////////////////////////////////////////
