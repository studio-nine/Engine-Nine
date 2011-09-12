/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/post_edgeDetect.fx#1 $

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

******************************************************************************/

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	string Script = "Technique=Main;";
> = 0.8; // version #

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

float4 ClearColor <
	string UIWidget = "color";
	string UIName = "background";
> = {0,0,0,0.0};

float ClearDepth <string UIWidget = "none";> = 1.0;

#include "Quad.fxh"

float NPixels <
    string UIName = "Pixels Steps";
    string UIWidget = "slider";
    float UIMin = 1.0f;
    float UIMax = 5.0f;
    float UIStep = 0.5f;
> = 1.5f;

float Threshhold
<
    string UIWidget = "slider";
    float UIMin = 0.01f;
    float UIMax = 0.5f;
    float UIStep = 0.01f;
> = 0.2;

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Data //////
///////////////////////////////////////////////////////////

DECLARE_QUAD_TEX(SceneTexture,SceneSampler,"X8R8G8B8")
DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer, "D24S8")

// QUAD_REAL Time : TIME <string UIWidget="None";>;

//////////////////////////////////////////////////////
////////////////////////////////// pixel shader //////
//////////////////////////////////////////////////////

float getGray(float4 c)
{
    return(dot(c.rgb,((0.33333).xxx)));
}

float4 edgeDetectPS(QuadVertexOutput IN,
	uniform sampler2D ColorMap
) : COLOR {
	float2 ox = float2(NPixels/QuadScreenSize.x,0.0);
	float2 oy = float2(0.0,NPixels/QuadScreenSize.y);
	float2 PP = IN.UV.xy - oy;
	float4 CC = tex2D(ColorMap,PP-ox); float g00 = getGray(CC);
	CC = tex2D(ColorMap,PP);    float g01 = getGray(CC);
	CC = tex2D(ColorMap,PP+ox); float g02 = getGray(CC);
	PP = IN.UV.xy;
	CC = tex2D(ColorMap,PP-ox); float g10 = getGray(CC);
	CC = tex2D(ColorMap,PP);    float g11 = getGray(CC);
	CC = tex2D(ColorMap,PP+ox); float g12 = getGray(CC);
	PP = IN.UV.xy + oy;
	CC = tex2D(ColorMap,PP-ox); float g20 = getGray(CC);
	CC = tex2D(ColorMap,PP);    float g21 = getGray(CC);
	CC = tex2D(ColorMap,PP+ox); float g22 = getGray(CC);
	float K00 = -1;
	float K01 = -2;
	float K02 = -1;
	float K10 = 0;
	float K11 = 0;
	float K12 = 0;
	float K20 = 1;
	float K21 = 2;
	float K22 = 1;
	float sx = 0;
	float sy = 0;
	sx += g00 * K00;
	sx += g01 * K01;
	sx += g02 * K02;
	sx += g10 * K10;
	sx += g11 * K11;
	sx += g12 * K12;
	sx += g20 * K20;
	sx += g21 * K21;
	sx += g22 * K22; 
	sy += g00 * K00;
	sy += g01 * K10;
	sy += g02 * K20;
	sy += g10 * K01;
	sy += g11 * K11;
	sy += g12 * K21;
	sy += g20 * K02;
	sy += g21 * K12;
	sy += g22 * K22; 
	float dist = sqrt(sx*sx+sy*sy);
	float result = 1;
	if (dist>Threshhold) { result = 0; }
	return result.xxxx;
}

////////////////////////////////////////////////////////////
/////////////////////////////////////// techniques /////////
////////////////////////////////////////////////////////////

technique Main <
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	string Script =
			"RenderColorTarget0=SceneTexture;"
	        "RenderDepthStencilTarget=DepthBuffer;"
	        	"ClearSetColor=ClearColor;"
	        	"ClearSetDepth=ClearDepth;"
  				"Clear=Color;"
				"Clear=Depth;"
	        	"ScriptExternal=color;"
        	"Pass=ImageProc;";
> {
    pass ImageProc <
    	string Script = "RenderColorTarget0=;"
								"Draw=Buffer;";
    > {
		cullmode = none;
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		VertexShader = compile vs_1_1 ScreenQuadVS();
		PixelShader = compile ps_2_0 edgeDetectPS(SceneSampler);
    }
}

////////////// eof ///
