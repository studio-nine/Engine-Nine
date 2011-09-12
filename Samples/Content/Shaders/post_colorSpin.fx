/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/post_colorSpin.fx#1 $

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
    Ignore selection geometry, but use its orientation
    to rotate the colors of a texture mapped to a full-screen
    quad.
      In FX Composer, assign this to any node and then spin the node to tweak colors
******************************************************************************/

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	string Script = "Technique=Spin;";
> = 0.8; // version #

/*********** Tweakables **********************/

texture ImageTex <
	string ResourceName = "Veggie.dds";
	string ResourceType = "2D";
>;

sampler2D ImageSampler = sampler_state
{
	Texture = <ImageTex>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = WRAP;
	AddressV = WRAP;
};

/************* UN-TWEAKABLES **************/

half4x4 WorldView : WORLDVIEW <string UIWidget="None";>;

#include "Quad.fxh"

//////////////////////////////////
/********* pixel shader *********/
//////////////////////////////////

half4 spinPS(QuadVertexOutput IN) : COLOR
{
	half3 texCol = ((half4)tex2D(ImageSampler, IN.UV)).xyz - (half3)0.5;
	half3x3 wv;
	wv[0] = WorldView[0].xyz;
	wv[1] = WorldView[1].xyz;
	wv[2] = WorldView[2].xyz;
	texCol = mul(texCol,wv) + (half3)0.5;
	return half4(texCol,1);
}  

/*************/

technique Spin <
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	string Script = "Pass=p0;";
> {
    pass p0 <
    	string Script = "RenderColorTarget0=;"
								"Draw=Buffer;";
    > {
		VertexShader = compile vs_2_0 ScreenQuadVS();
		ZEnable = false;
		ZWriteEnable = false;
		CullMode = None;
		AlphaBlendEnable = false;
		PixelShader = compile ps_2_0 spinPS();
    }
}


/***************************** eof ***/
