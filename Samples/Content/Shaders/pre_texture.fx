/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/pre_texture.fx#1 $

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
	string Script = "Technique=TexBg;";
> = 0.8; // version #

float ClearDepth <string UIWidget = "none";> = 1.0;

///////// Textures ///////////////

texture BgTexture
<
    string ResourceName = "Veggie.dds";
    string ResourceType = "2D";
    string UIName = "Background Texture";
>;

sampler2D BgSampler = sampler_state
{
    Texture = <BgTexture>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = None;
};

////////////////////////

technique TexBg <
	string Script =
				"ClearSetDepth=ClearDepth;"
				"Clear=Depth;"
				"Pass=Bg;"
				"ScriptExternal=Scene;";
> {
	pass Bg <
		string Script = "Draw=Buffer;";
	> {
		VertexShader = compile vs_1_1 ScreenQuadVS();
		AlphaBlendEnable = false;
		ZEnable = false;
		PixelShader  = compile ps_1_1 TexQuadPS(BgSampler);
	}
}

/***************************** eof ***/
