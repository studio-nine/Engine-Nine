/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/paint_liquid.fx#1 $

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
	An .FX "Liquid Image" Paint Program. Scene geometry is ignored.
	Draw with the left mouse button -- the direction of stroke is important.
	Works best on NV40/Shader Model 3.0 -- uses 16-bit floating-point render targets
		and blending.
	Resizing the window will clear your drawing.
	Brush strokes will change in size and opacity over time, set "FadeTime"
		to a high value for more even (though less expressive) strokes.

******************************************************************************/

//#define TWEAKABLE_TEXEL_OFFSET 
#include "Quad.fxh"

// we will draw into buffer "paint"
// then displace pic into buffer "pool"
// then display

float Script : STANDARDSGLOBAL
<
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";
	// We just call a script in the main technique.
	string Script = "Technique=liquid;";
> = 0.8;

bool bReset : FXCOMPOSER_RESETPULSE
<
	string UIName="Clear Canvas";
>;

bool Painting
<
	string UIName="Paint with Mouse?";
> = true;

float4 ClearColor <
	string UIWidget = "None";
	string UIName = "background";
> = {0.5,0.5,0.5,0.0};

float ClearDepth <string UIWidget = "none";> = 1.0;

float4 MouseL : LEFTMOUSEDOWN < string UIWidget="None"; >;
float3 MousePos : MOUSEPOSITION < string UIWidget="None"; >;
float Timer : TIME < string UIWidget = "None"; >;

float Opacity <
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 1.0;
	float UIStep = 0.01;
> = 0.45f;

float BrushSizeStart <
	string UIWidget = "slider";
	float UIMin = 0.001;
	float UIMax = 0.15;
	float UIStep = 0.001;
	string UIName = "Brush Start Size";
> = 0.045f;

float BrushSizeEnd <
	string UIWidget = "slider";
	float UIMin = 0.001;
	float UIMax = 0.15;
	float UIStep = 0.001;
	string UIName = "Brush End Size";
> = 0.01f;

float FadeTime <
	string UIWidget = "slider";
	float UIMin = 0.1;
	float UIMax = 10.0;
	float UIStep = 0.1;
	string UIName = "Brush Decay Time";
> = 1.10f;

float FadeInTime <
	string UIWidget = "slider";
	float UIMin = 0.001;
	float UIMax = 0.3;
	float UIStep = 0.001;
	string UIName = "Brush Attack Time";
> = 0.15f;

float Push <
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 1.0;
	float UIStep = 0.001;
	string UIName = "Brush Pressure";
> = 0.1f;

float Effect <
	string UIWidget = "slider";
	string UIName = "Effect Strength";
	float UIMin = 0.0;
	float UIMax = 1.0;
	float UIStep = 0.001;
> = 1.0f;

////////////////////////////////////////////////////
/// Textures ///////////////////////////////////////
////////////////////////////////////////////////////

//DECLARE_QUAD_TEX(PaintTex,PaintSamp,"A8R8G8B8")
DECLARE_QUAD_TEX(PaintTex,PaintSamp,"A16B16G16R16F")

texture SourceTexture <
	string ResourceName = "default_color.dds";
	string TextureType = "2D";
	string UIName = "Original Image";
>;

sampler2D SourceSampler = sampler_state
{
	Texture = <SourceTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

/****************************************/
/*** The dead-simple shaders: ***********/
/****************************************/

// VM functions

QUAD_REAL2 dir_color()
{
	QUAD_REAL2 dirVec = MousePos.xy - MouseL.xy;
	QUAD_REAL l = length(dirVec);
	if (l > 0.0) {
		dirVec = normalize(dirVec);
		dirVec = Push*dirVec;
	} else {
		dirVec = 0;
	}
	return dirVec;
}

float fadeout()
{
	float f = 1 - min(1,max(0,(Timer - MouseL.w))/FadeTime);
	return f;
}

float lerpsize()
{
	float f = fadeout();
	float ds = lerp(BrushSizeEnd,BrushSizeStart,f);
	return ds;
}

float fadein()
{
	float f = min(1,max(0,(Timer - MouseL.w))/FadeInTime);
	return f;
}

///

QUAD_REAL4 strokePS(QuadVertexOutput IN,
		uniform QUAD_REAL2 BrushColor,
		uniform QUAD_REAL Fadeout,
		uniform QUAD_REAL Fadein,
		uniform QUAD_REAL BrushSize) : COLOR
{
    QUAD_REAL2 delta = IN.UV.xy-MousePos.xy;
    QUAD_REAL dl = dot(delta,delta);
    QUAD_REAL dd = MouseL.z*(1-min(dl/BrushSize,1));
    dd *= Painting * Opacity * Fadeout * Fadein;
    return QUAD_REAL4(0.5+(BrushColor),0.5,dd);
    //return QUAD_REAL4(q.xxx,dd);
}

QUAD_REAL4 liquidPS(QuadVertexOutput IN) : COLOR
{   
	QUAD_REAL2 disp = Effect*2.0*(tex2D(PaintSamp, IN.UV).xy - QUAD_REAL2(0.5,0.5));
	QUAD_REAL4 texCol = tex2D(SourceSampler, IN.UV-disp);
	return texCol;
}  

////////////////// Technique ////////

technique liquid <
	string Script =
        	"Pass=paint;"
        	"Pass=display;";
> {
	pass paint <
		string Script = 
	        "RenderColorTarget0=PaintTex;"
	        "RenderDepthStencilTarget=;"
    					"LoopByCount=bReset;"
    					"ClearSetColor=ClearColor;"
    					"Clear=Color0;"
     					"LoopEnd=;"
	        "Draw=Buffer;";
	> {
		VertexShader = compile vs_1_1 ScreenQuadVS();
		PixelShader  = compile ps_2_0 strokePS(dir_color(),
												fadeout(),
												fadein(),
												lerpsize());
		AlphaBlendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		ZEnable = false;
	}
	pass display <
		string Script = 
	        "RenderColorTarget0=;"
	        "RenderDepthStencilTarget=;"
	        "Draw=Buffer;";
	> {
		VertexShader = compile vs_1_1 ScreenQuadVS();
		PixelShader  = compile ps_2_0 liquidPS();
		 //PixelShader  = compile ps_2_0 TexQuadPS(PaintSamp);
		AlphaBlendEnable = false;
		ZEnable = false;
	}
}

/////////////////////////////////////////////
///////////////////////////////////// eof ///
/////////////////////////////////////////////