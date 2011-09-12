/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/pre_gradient.fx#1 $

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

******************************************************************************/

#include "Quad.fxh"

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "preprocess";
	string ScriptOutput = "color";
	string Script = "Technique=grad;";
> = 0.8; // version #

QUAD_REAL3 CLR <
	string UIName = "Lower Right";
	string UIWidget = "Color";
> = {1.0f, 1.0f, 0.6f};

QUAD_REAL3 CLL <
	string UIName = "Lower Left";
	string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f};

QUAD_REAL3 CUR <
	string UIName = "Upper Right";
	string UIWidget = "Color";
> = {0.0f, 0.0f, 0.2f};

QUAD_REAL3 CUL <
	string UIName = "Upper Left";
	string UIWidget = "Color";
> = {0.0f, 0.0f, 0.0f};

QUAD_REAL4 gradPS(QuadVertexOutput IN) : COLOR
{   
	QUAD_REAL3 lo = lerp(CLL,CLR,IN.UV.x);
	QUAD_REAL3 hi = lerp(CUL,CUR,IN.UV.x);
	QUAD_REAL3 gr = lerp(hi,lo,IN.UV.y);
	return QUAD_REAL4(gr,0);	// leave alpha alone
}  

float ClearDepth <string UIWidget = "none";> = 1.0;

//////////////////////

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
		PixelShader  = compile ps_2_0 gradPS();
	}
}

/***************************** eof ***/
