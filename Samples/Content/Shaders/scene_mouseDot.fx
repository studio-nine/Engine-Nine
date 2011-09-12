/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/scene_mouseDot.fx#1 $

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
	Fun with mice.
	Click on the screen, move the mouse around.
	Scene geometry is ignored.

******************************************************************************/

#include "Quad.fxh"

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";
	string Script = "Technique=mouseBloom;";
> = 0.8; // version #

float4 MouseL : LEFTMOUSEDOWN < string UIWidget="None"; >;
// float4 MouseR : RIGHTMOUSEDOWN < string UIWidget="None"; >;
float3 MousePos : MOUSEPOSITION < string UIWidget="None"; >;
float Time : TIME < string UIWidget = "None"; >;

float TimeScale <
	string UIWidget = "slider";
	float UIMin = 0.01;
	float UIMax = 4.0;
	float UIStep = 0.01;
> = 0.6f;

/****************************************/
/*** Shaders ****************************/
/****************************************/

float4 PS(QuadVertexOutput IN) : COLOR
{
    float2 delta = IN.UV.xy-MouseL.xy;
    float dd = length(delta);
    float dt = (Time - MouseL.w)*TimeScale;
    float dr = min(1,max(0,1-abs(dd-dt)));
    // dr = dr * MouseL.z; // if this line is commented, effect only happens when mouse is pressed
    float2 mDelta = 0.5+(MousePos.xy - MouseL.xy);
    return float4(dr*mDelta,dr,1);
}

/****************************************/
/*** Technique **************************/
/****************************************/

technique mouseBloom <
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	string Script = "Pass=p0;";
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
		PixelShader  = compile ps_2_0 PS();
	}
}

/****************************************/
/******************************* eof ****/
/****************************************/
