/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/pre_gradientTexture.fx#1 $

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
    Put a texture behind the current scene
	Geometry Ignored

******************************************************************************/

#include "Quad.fxh"

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "preprocess";
	string ScriptOutput = "color";
	string Script = "Technique=grad;";
> = 0.8; // version #

float ClearDepth <string UIWidget = "none";> = 1.0;

#define CLR float3(1.0f, 1.0f, 0.6f)
#define CLL float3(1.0f, 1.0f, 1.0f)
#define CUR float3(0.0f, 0.0f, 0.2f)
#define CUL float3(0.0f, 0.0f, 0.0f)
#define GAMMAV 3.2
#define GAMMAH 1.0

float4 gradient_tex(float2 Pos : POSITION) : COLOR
{   
	float xx = pow(Pos.x,GAMMAH);
	float3 lo = lerp(CLL,CLR,xx);
	float3 hi = lerp(CUL,CUR,xx);
	float3 gr = lerp(hi,lo,pow(Pos.y,GAMMAV));
	return float4(gr,0);	// leave alpha alone
}

texture GradTex  <
    string ResourceType = "2D";
    string function = "gradient_tex";
    float2 ViewportRatio = {1.0,1.0};
    string UIWidget = "None";
>;

// samplers
sampler GradSampler = sampler_state 
{
    texture = <GradTex>;
    AddressU  = WRAP;        
    AddressV  = WRAP;
    AddressW  = WRAP;
    MIPFILTER = LINEAR;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};

///////////////////////////////

technique grad <
	string ScriptClass = "scene";
	string ScriptOrder = "preprocess";
	string ScriptOutput = "color";
	string Script =
				"ClearSetDepth=ClearDepth;"
				"Clear=Depth;"
				"Pass=Bg;"
				"ScriptExternal=Scene;";
> {
	pass Bg <
		string Script = "Draw=Buffer;";
	> {
		VertexShader = compile vs_2_0 ScreenQuadVS();
		AlphaBlendEnable = false;
		ZEnable = false;
		PixelShader  = compile ps_2_0 TexQuadPS(GradSampler);
	}
}

/***************************** eof ***/
