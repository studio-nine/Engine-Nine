/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/post_mouseDrop.fx#1 $

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
	Click on the screen, move the mouse around (or not).
	While mouse is pressed, a ripple effect will appear.

******************************************************************************/

#include "Quad.fxh"

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";
	string Script = "Technique=mouseDrop;";
> = 0.8; // version #

float4 ClearColor <
	string UIWidget = "color";
	string UIName = "Scene Background";
> = {0.0,0.0,0.0,0.0};

float ClearDepth <string UIWidget = "none";> = 1.0;

///////////////////////////////////////////////////////////
//////////////////////////////// Texture Render Targets ///
///////////////////////////////////////////////////////////

// DECLARE_QUAD_TEX(SceneMap,SceneSampler,"A16B16G16R16F")
DECLARE_QUAD_TEX(SceneMap,SceneSampler,"A8B8G8R8")

DECLARE_QUAD_DEPTH_BUFFER(DepthMap,"D24S8")

/////////////// untweakables ///////////

float4 MouseL : LEFTMOUSEDOWN < string UIWidget="None"; >;
// float4 MouseR : RIGHTMOUSEDOWN < string UIWidget="None"; >;
float3 MousePos : MOUSEPOSITION < string UIWidget="None"; >;
float Time : TIME < string UIWidget = "None"; >;

/////////////// tweakables ///////////

float TimeScale <
	string UIWidget = "slider";
	float UIMin = 0.01;
	float UIMax = 4.0;
	float UIStep = 0.01;
> = 0.6f;

float Strength <
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 1.0;
	float UIStep = 0.001;
> = 0.025f;

float Waves <
	string UIWidget = "slider";
	float UIMin = 0.01;
	float UIMax = 18.0;
	float UIStep = 0.001;
> = 15.0f;

/****************************************/
/*** Shaders ****************************/
/****************************************/

float4 dropPS(QuadVertexOutput IN) : COLOR
{
    //float2 delta = IN.UV.xy-MouseL.xy;
    float2 delta = IN.UV.xy-MousePos.xy;
    float distance = length(delta);
    float elapsed = (Time - MouseL.w);
    float radiate = elapsed*TimeScale;
    float wave = sin(Waves*(radiate-distance));
    float dr = min(1,max(0,1-abs(distance-radiate)));
	dr *= MouseL.z; // if this line is commented, effect only happens when mouse is pressed
	float2 off = wave * Strength * dr * delta / distance;
	float2 nuv = IN.UV.xy + off;
	return tex2D(SceneSampler,nuv);
	//return float4(wave.xxxx);
//    float2 mDelta = 0.5+(MousePos.xy - MouseL.xy);
//    return float4(dr*mDelta,dr,1);
}

/****************************************/
/*** Technique **************************/
/****************************************/

technique mouseDrop <
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	string Script = "ClearSetDepth=ClearDepth;"
	      	"RenderColorTarget=SceneMap;"
	        "RenderDepthStencilTarget=DepthMap;"
	        	"ClearSetColor=ClearColor;"
	        	"ClearSetDepth=ClearDepth;"
   				"Clear=Color;"
				"Clear=Depth;"
	        	"ScriptSignature=color;"
	        	"ScriptExternal=;"
	        "Pass=p0;";
> {
    pass p0 <
    	string Script = "RenderColorTarget0=;"
								"Draw=Buffer;";
    > {
		VertexShader = compile vs_1_1 ScreenQuadVS();
		ZEnable = false;
		ZWriteEnable = false;
		CullMode = None;
		AlphaBlendEnable = false;
		PixelShader  = compile ps_2_0 dropPS();
	}
}

/****************************************/
/******************************* eof ****/
/****************************************/
