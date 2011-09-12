/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/paint_3D.fx#1 $

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
	An .FX 3D Paint Program!
	Draw with the left mouse button.
	To clear screen, resize the window or set "Clear Canvas".
	Toggle "Paint Now?" to draw or not draw (for tumbling camera etc).
	Watch out for stray light source objects! Select the scene window's
		"Scene->Render Options->Show Lights" to turn them off

******************************************************************************/

#include "Quad.fxh"

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

bool Painting <
	string UIName="Paint Now?";
> = true;

float4 ClearColor <
	string UIWidget = "color";
	string UIName = "Scene Background";
> = {0.0,0.0,0.0,0.0};

float4 PaintClearColor <
	string UIWidget = "color";
	string UIName = "Texture Background";
> = {0.1,0.1,0.1,0.0};

float ClearDepth <string UIWidget = "none";> = 1.0;

// #define USE_TIMER

half4 MouseL : LEFTMOUSEDOWN < string UIWidget="None"; >;
half3 MousePos : MOUSEPOSITION < string UIWidget="None"; >;
#ifdef USE_TIMER
half Timer : TIME < string UIWidget = "None"; >;
#endif /* USE_TIMER */

float4x4 WorldViewProjXf : WorldViewProjection < string UIWidget="None"; >;
float4x4 WorldIT : WorldInverseTranspose  < string UIWidget="None"; >;

/////////////////

half Opacity <
	string UIWidget = "slider";
	half UIMin = 0.0;
	half UIMax = 1.0;
	half UIStep = 0.01;
> = 0.45f;

#ifdef USE_TIMER
half BrushSizeStart <
	string UIWidget = "slider";
	half UIMin = 0.001;
	half UIMax = 0.15;
	half UIStep = 0.001;
> = 0.07f;

half BrushSizeEnd <
	string UIWidget = "slider";
	half UIMin = 0.001;
	half UIMax = 0.15;
	half UIStep = 0.001;
> = 0.01f;

half FadeTime <
	string UIWidget = "slider";
	half UIMin = 0.1;
	half UIMax = 10.0;
	half UIStep = 0.1;
> = 2.00f;

#else /* !USE_TIMER */
half BrushSize <
	string UIWidget = "slider";
	half UIMin = 0.001;
	half UIMax = 0.15;
	half UIStep = 0.001;
	string UIName = "Brush Size";
> = 0.07f;
#endif /* !USE_TIMER */

half3 PaintColor <
	string UIWidget = "Color";
	string UIName = "Brush";
> = {0.4f, 0.3f, 1.0f};

half Shading <
	string UIWidget = "slider";
	half UIMin = 0.0;
	half UIMax = 1.0;
	half UIStep = 0.01;
	string UIName = "Flat/Shaded";
> = 0.5;

float4 LightDir : Direction
<
	string Object = "DirectionalLight";
    string Space = "World";
    // string UIWidget="None";
> = {1.0f, -1.0f, 1.0f, 0.0f};

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Data //////
///////////////////////////////////////////////////////////

DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer,"D24S8")

DECLARE_QUAD_TEX(UVMap,UVSampler,"A16B16G16R16F")
// DECLARE_QUAD_TEX(UVMap,UVSampler,"A8R8G8B8")
DECLARE_QUAD_TEX(PaintMap,PaintSampler,"A8R8G8B8")
DECLARE_QUAD_TEX(BufMap,BufSampler,"A8R8G8B8")

/*********************************************************/
/************* 3D Structs and Shaders ********************/
/*********************************************************/

/* data from application vertex buffer */
struct appdata {
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;
    float3 Normal	: NORMAL0;
};

struct vertexOutput {
    float4 HPosition	: POSITION;
    float2 UV			: TEXCOORD0;
	float4 Diff		: COLOR0;
};

vertexOutput minVS(appdata IN) {
	vertexOutput OUT;
    float4 Po = float4(IN.Position.xyz,1.0);	// object coordinates
    OUT.HPosition = mul(Po,WorldViewProjXf);	// screen clipspace coords
    OUT.UV = IN.UV.xy;
    float3 Nw = mul(float4(IN.Normal,0),WorldIT);
    float ldn = 0.5+dot(LightDir,Nw)*0.5;
    OUT.Diff = float4(ldn.xxx,1);
    return OUT;
}

float4 uvPS(vertexOutput IN) : COLOR { 
	return float4(IN.UV.xy,IN.Diff.x,1); }

float4 paintPS(vertexOutput IN) : COLOR { 
    return IN.Diff * tex2D(PaintSampler,IN.UV.xy);
}

/*******************************************/
/*** The dead-simple 2D shaders: ***********/
/*******************************************/

half4 restorePaintPS(QuadVertexOutput IN) : COLOR
{
	half4 c = tex2D(PaintSampler,IN.UV);
    return c;
}

half4 brushPS(QuadVertexOutput IN) : COLOR
{
	half4 uvmap = tex2D(UVSampler,MousePos.xy);
    half2 delta = IN.UV.xy-uvmap.xy;
	half dd = uvmap.w * Opacity * Painting;
#ifdef USE_TIMER
	half fadeout = 1 - min(1,max(0,(Timer - MouseL.w))/FadeTime);
	half ds = lerp(BrushSizeEnd,BrushSizeStart,fadeout);
    dd *= fadeout;
    dd *= MouseL.z*(1-min(length(delta)/ds,1));
#else /* USE_TIMER */
    dd *= MouseL.z*(1-min(length(delta)/BrushSize,1));
#endif /* USE_TIMER */
    return dd*half4(PaintColor,1);
}

half4 remapPS(QuadVertexOutput IN) : COLOR
{
	half4 uvmap = tex2D(UVSampler,IN.UV);
	half3 c = uvmap.w * tex2D(PaintSampler,uvmap.xy).xyz;
	half sh = lerp(1,uvmap.z,Shading);
	c = c * sh;
    return half4(c,uvmap.w);
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
	        "Pass=showTex;";
> {
	pass ShowUV <
    	string Script = "RenderColorTarget0=PaintMap;"
    					"LoopByCount=bReset;"
    						"ClearSetColor=PaintClearColor;"
    						"Clear=Color0;"
    						"RenderColorTarget0=BufMap;"
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
		VertexShader = compile vs_2_0 minVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_2_0 uvPS();
	}
	// fill fb with previous state of paint texture
	pass restorePaint <
    	string Script ="RenderColorTarget0=BufMap;"
    							"Draw=Buffer;";
	> {
		VertexShader = compile vs_1_1 ScreenQuadVS();
		PixelShader  = compile ps_2_0 restorePaintPS();
		AlphaBlendEnable = false;
		ZEnable = false;
	}
	// paint into paint texture
	pass brushStroke <
    	string Script ="RenderColorTarget0=PaintMap;"
    							"Draw=Buffer;";
	> {
		VertexShader = compile vs_1_1 ScreenQuadVS();
		PixelShader  = compile ps_2_0 brushPS();
		AlphaBlendEnable = true;
		SrcBlend = One;
		DestBlend = InvSrcAlpha;
		ZEnable = false;
	}
	// draw revised texture onto surface
	pass showTex <
    	string Script ="RenderColorTarget0=;"
    							"Draw=Buffer;";
	> {
		VertexShader = compile vs_1_1 ScreenQuadVS();
		PixelShader  = compile ps_2_0 remapPS();
		AlphaBlendEnable = false;
		ZEnable = false;
	}
}

/////////////////////////////////////////////
///////////////////////////////////// eof ///
/////////////////////////////////////////////
