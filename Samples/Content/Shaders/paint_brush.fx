/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/paint_brush.fx#1 $

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
	An .FX Paint Program. Scene geometry is ignored.
	Draw with the left mouse button.
	To clear screen, just make the brush big and paint everything.
	Resizing the window will mess up your drawing.
	Brush strokes will change in size and opacity over time, set "FadeTime"
		to a high value for more even (though less expressive) strokes.

******************************************************************************/

#include "Quad.fxh"

float Script : STANDARDSGLOBAL
<
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";
	// We just call a script in the main technique.
	string Script = "Technique=paint;";
> = 0.8; // version #

bool bReset : FXCOMPOSER_RESETPULSE
<
	string UIName="Clear Canvas";
>;

bool bRevert
<
	string UIName="Use BgPic instead of BgColor?";
> = true;

float4 BgColor <
	string UIWidget = "color";
	string UIName = "Background";
> = {1.0,0.95,0.92,1.0};

float ClearDepth <string UIWidget = "none";> = 1.0;

///////////// untweakables //////////////////////////

float4 MouseL : LEFTMOUSEDOWN < string UIWidget="None"; >;
float3 MousePos : MOUSEPOSITION < string UIWidget="None"; >;
float Timer : TIME < string UIWidget = "None"; >;

////////////// tweakables /////////////////////////

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
> = 0.07f;

float BrushSizeEnd <
	string UIWidget = "slider";
	float UIMin = 0.001;
	float UIMax = 0.15;
	float UIStep = 0.001;
> = 0.01f;

float FadeTime <
	string UIWidget = "slider";
	float UIMin = 0.1;
	float UIMax = 10.0;
	float UIStep = 0.1;
> = 2.00f;

float3 PaintColor <
	string UIWidget = "Color";
> = {0.4f, 0.3f, 1.0f};

// VM functions (executed by CPU) ////////////

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

FILE_TEXTURE_2D_MODAL(BgPic,BgSampler,"default_color.dds",CLAMP)

/***************************************************/
/*** This shader performs the clear/revert *********/
/***************************************************/

float4 revertPS(QuadVertexOutput IN,uniform float UseTex) : COLOR
{
//	float4 result = BgColor;
//	float4 texc = tex2D(BgSampler,IN.UV);
//	if (bRevert) {
//		result = texc;
//	}
	// return result;
    return lerp(BgColor,tex2D(BgSampler,IN.UV),UseTex);
}

/***************************************************/
/*** The dead-simple paintbrush shader *************/
/***************************************************/

float4 strokePS(QuadVertexOutput IN,
	uniform float Fadeout,
	uniform float BrushSize) : COLOR
{
    float2 delta = IN.UV.xy-MousePos.xy;
    float dd = MouseL.z*(1-min(length(delta)/BrushSize,1));
    dd *= Opacity * Fadeout;
    return float4(PaintColor.xyz,dd);
}

////////////////// Technique ////////

technique paint <
	string Script =
	        "RenderColorTarget0=;"
	        "RenderDepthStencilTarget=;"
    		"LoopByCount=bReset;"
    			// "ClearSetColor=BgColor;"
    			// "Clear=Color0;"
				"Pass=revert;"
    		"LoopEnd=;"
        	"Pass=splat;";
> {
	pass revert <
		string Script = "Draw=Buffer;";
	> {
		VertexShader = compile vs_1_1 ScreenQuadVS();
		//PixelShader  = compile ps_2_0 revertPS((bRevert!=false)?1.0:0.0);
		PixelShader  = compile ps_2_0 revertPS(true);
		AlphaBlendEnable = false;
		ZEnable = false;
	}
	pass splat <
		string Script = "Draw=Buffer;";
	> {
		VertexShader = compile vs_1_1 ScreenQuadVS();
		PixelShader  = compile ps_2_0 strokePS(fadeout(),lerpsize());
		AlphaBlendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		ZEnable = false;
	}
}

/////////////////////////////////////////////
///////////////////////////////////// eof ///
/////////////////////////////////////////////
