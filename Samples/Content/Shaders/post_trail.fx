/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/post_trail.fx#1 $

Copyright NVIDIA Corporation 2004
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
	Fading trails
	$Header: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/post_trail.fx#1 $

******************************************************************************/

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	string Script = "Technique=Simple;";
> = 0.8; // version #

bool bReset : FXCOMPOSER_RESETPULSE
<
	string UIName="Reset";
	string UIWidget = "none";
>;

float4 ClearColor <
	string UIWidget = "color";
	string UIName = "background";
> = {0,0,0,0.0};

float ClearDepth <string UIWidget = "none";> = 1.0;

#include "Quad.fxh"

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

half TrailFade <
    string UIName = "Trail Brightness";
    string UIWidget = "slider";
    half UIMin = 0.0f;
    half UIMax = 1.0f;
    half UIStep = 0.001f;
> = 0.75f;

half DisplayBright <
    string UIName = "Image Brightness";
    string UIWidget = "slider";
    half UIMin = 0.0f;
    half UIMax = 1.0f;
    half UIStep = 0.001f;
> = 0.5;

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Data //////
///////////////////////////////////////////////////////////

DECLARE_QUAD_TEX(SceneTexture,SceneSampler,"A16B16G16R16F")
DECLARE_QUAD_TEX(CompositeMap,CompositeSampler,"A16B16G16R16F")
DECLARE_QUAD_TEX(BlendMap,BlendSampler,"A16B16G16R16F")

DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer, "D24S8")

///////////////////////////////////////////////////////////
/////////////////////////////////// data structures ///////
///////////////////////////////////////////////////////////

half4 trailPS(QuadVertexOutput IN) : COLOR
{   
	half4 prev = TrailFade * tex2D(BlendSampler, IN.UV.xy);
	half4 orig = tex2D(SceneSampler, IN.UV.xy);
	return (orig+prev);
}  

QUAD_REAL4 TexQuadDimmerPS(QuadVertexOutput IN,
					uniform sampler2D InputSampler,
					uniform float Dim) : COLOR
{   
	QUAD_REAL4 texCol = tex2D(InputSampler, IN.UV);
	return Dim * texCol;
}  

////////////////////////////////////////////////////////////
/////////////////////////////////////// techniques /////////
////////////////////////////////////////////////////////////

technique Simple <
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	string Script =
			"LoopByCount=bReset;"
				"RenderColorTarget0=BlendMap;"
				"RenderDepthStencilTarget=DepthBuffer;"
				"ClearSetColor=ClearColor;"
	        	"ClearSetDepth=ClearDepth;"
				"Clear=Color0;"
				"Clear=Depth;"
			"LoopEnd=;"
			"RenderColorTarget0=SceneTexture;"
	        "RenderDepthStencilTarget=DepthBuffer;"
	        	"ClearSetColor=ClearColor;"
	        	"ClearSetDepth=ClearDepth;"
   				"Clear=Color;"
				"Clear=Depth;"
	        	"ScriptExternal=color;"
        	"Pass=CompOverTrail;"
        	"Pass=StoreTrailBuffer;"
        	"Pass=Display;";
> {
    pass CompOverTrail <
    	string Script = "RenderColorTarget0=CompositeMap;"
						"RenderDepthStencilTarget=DepthBuffer;"
						"Draw=Buffer;";
    > {
		cullmode = none;
		ZEnable = false;
		AlphaBlendEnable = false;
		VertexShader = compile vs_2_0 ScreenQuadVS();
		PixelShader  = compile ps_2_a trailPS();
    }
	// stash to saved buffer
    pass StoreTrailBuffer <
    	string Script = "RenderColorTarget0=BlendMap;"
	        "RenderDepthStencilTarget=DepthBuffer;"
								"Draw=Buffer;";
    > {
		cullmode = none;
		ZEnable = false;
		AlphaBlendEnable = false;
		VertexShader = compile vs_2_0 ScreenQuadVS();
		PixelShader  = compile ps_2_a TexQuadPS(CompositeSampler);
    }
    pass Display <
    	string Script = "RenderColorTarget0=;"
				"RenderDepthStencilTarget=;"
							"Draw=Buffer;";
    > {
		cullmode = none;
		ZEnable = false;
		AlphaBlendEnable = false;
		VertexShader = compile vs_2_0 ScreenQuadVS();
		PixelShader  = compile ps_2_a TexQuadDimmerPS(CompositeSampler,DisplayBright);
    }
}

////////////// eof ///
