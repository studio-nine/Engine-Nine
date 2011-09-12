/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/paint_TexBrush.fx#1 $

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
	Brush pattern is in the named texture. The "R" channel is used as a grayscale.
	Draw with the left mouse button.
	To clear screen, select "Clear Canvas" or resize the window.
	Brush strokes will change in size and opacity over time, set "FadeTime"
		to a high value for more even (though less expressive) strokes.
	Rotation jitter is supplied by the vertex program.

******************************************************************************/

#include "Quad.fxh"
#include "vertex_noise.fxh"

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

float4 ClearColor <
	string UIWidget = "color";
	string UIName = "Background";
> = {0.0,0.0,0.0,1.0};

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
> = 0.127f;

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

float RotJit <
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 180.0;
	float UIStep = 0.1;
	string UIName = "Rotation Jitter";
> = 95.00f;

float3 PaintColor <
	string UIWidget = "Color";
> = {1.0f, 0.7f, 0.2f};


float Speed <
    string UIWidget = "slider";
    float UIMin = -1.0f;
    float UIMax = 100.0f;
    float UIStep = 0.01f;
> = 21.f;

float2 TimeDelta = {1,.2};

// float Timer : TIME < string UIWidget="None";>;

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

/****************************************/
/*** Texture for Brush Pattern **********/
/****************************************/

FILE_TEXTURE_2D_MODAL(BrushTex,BrushSamp,"StarBrush.dds",CLAMP)

/////////////////////////////////////////////////////////////////////////////////////
// Structure Declaration ////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////

struct PaintVertexOutput {
   	QUAD_REAL4 Position	: POSITION;
    QUAD_REAL2 UV		: TEXCOORD0;
    QUAD_REAL2 CS		: TEXCOORD2;
};

/****************************************/
/*** Vertex shader **********************/
/****************************************/

PaintVertexOutput strokeVS(
		QUAD_REAL3 Position : POSITION, 
		QUAD_REAL3 TexCoord : TEXCOORD0
) {
    PaintVertexOutput OUT;
    OUT.Position = QUAD_REAL4(Position, 1);
	QUAD_REAL2 off = QUAD_REAL2(QuadTexOffset/(QuadScreenSize.x),QuadTexOffset/(QuadScreenSize.y));
    float2 dn = Speed*Timer*TimeDelta;
	OUT.UV = QUAD_REAL2(TexCoord.xy+off);
	float2 noisePos = (float2)(0.5)+off+dn;
	float i = radians(RotJit)*vertex_noise(noisePos, NTab);
	OUT.CS = QUAD_REAL2(cos(i),sin(i));
    return OUT;
}

/****************************************/
/*** Pixel shader ***********************/
/****************************************/

float4 strokePS(PaintVertexOutput IN,
	uniform float Fadeout,
	uniform float BrushSize) : COLOR
{
    float2 delta = IN.UV.xy-MousePos.xy;
    // float dd = MouseL.z*(1-min(length(delta)/BrushSize,1));
	delta = delta / BrushSize;
	delta = float2(
		(delta.x * IN.CS.x - delta.y * IN.CS.y),
		(delta.y * IN.CS.x + delta.x * IN.CS.y));
	float4 bt = tex2D(BrushSamp,delta.xy+float2(0.5,0.5));
    // dd *= Opacity * Fadeout;
    float dd = (MouseL.z * bt.r * Fadeout);
    return float4(PaintColor.xyz,dd);
}

////////////////// Technique ////////

technique paint <
	string Script =
        	"Pass=splat;";
> {
	pass splat <
		string Script = 
	        "RenderColorTarget0=;"
	        "RenderDepthStencilTarget=;"
    		"LoopByCount=bReset;"
    			"ClearSetColor=ClearColor;"
    			"Clear=Color0;"
    		"LoopEnd=;"
	        "Draw=Buffer;";
	> {
		VertexShader = compile vs_1_1 strokeVS();
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
