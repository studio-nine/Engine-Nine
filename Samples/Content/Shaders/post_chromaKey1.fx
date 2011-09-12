/*********************************************************************NVMH3****
File:  $Id: //sw/devrel/SDK/MEDIA/HLSL/post_keyer.fx#4 $

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

Key based on the RGB-space distance from a specified color

******************************************************************************/

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	string Script = "Technique=keyer;";
> = 0.8; // version #

#include "Quad.fxh"
#include "rgb-hsv.fxh"

float4 ClearColor : DIFFUSE = {0.3,0.3,0.3,1.0};
float ClearDepth
<
	string UIWidget = "none";
> = 1.0;

///////////////////////

float3 KeyColor <
	string UIWidget = "color";
> = {0,1,0};

static QUAD_REAL3 KeyColorHSV = rgb_to_hsv(KeyColor);

float MinKey : PARAM0 <
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 0.99;
	float minimum = 0.0;
	float maximum = 1.0;
	float UIStep = 0.001;
> = 0.2;

float Edge : PARAM1 <
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 0.99;
	float minimum = 0.0;
	float maximum = 1.0;
	float UIStep = 0.001;
> = 0.6;

static float MaxKey = min(MinKey+Edge,1.0);

float SatScale : PARAM2 <
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 1.0;
	float minimum = 0.0;
	float maximum = 1.0;
	float UIStep = 0.001;
> = 1.0;

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Data //////
///////////////////////////////////////////////////////////

DECLARE_QUAD_TEX(Tex0,Sampler0,"A8R8G8B8")
DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer, "D24S8")

////////////////////////////////////////////////////////////
/////////////////////////////////////// Shader /////////////
////////////////////////////////////////////////////////////

QUAD_REAL4 keyerPS(QuadVertexOutput IN) : COLOR
{   
	QUAD_REAL4 texCol = tex2D(Sampler0, IN.UV);
	QUAD_REAL3 hsv = rgb_to_hsv(texCol.xyz);
	QUAD_REAL2 delta = hsv.xy - KeyColorHSV.xy;
	delta.y *= SatScale;
	QUAD_REAL d2 = dot(delta,delta);
	d2 = smoothstep(MinKey,MaxKey,d2);
	return QUAD_REAL4(d2.xxx,d2*texCol.w);
}  

////////////////////////////////////////////////////////////
/////////////////////////////////////// techniques /////////
////////////////////////////////////////////////////////////

technique keyer <
	string Script =
			"RenderColorTarget0=Tex0;"
	        "RenderDepthStencilTarget=DepthBuffer;"
	        "ClearSetColor=ClearColor;"
	        "ClearSetDepth=ClearDepth;"
   			"Clear=Color;"
			"Clear=Depth;"
			"ScriptExternal=color;"
        	"Pass=p0;";
> {
    pass p0 <
    	string Script = "RenderColorTarget0=;"
	        			"RenderDepthStencilTarget=;"
						"Draw=Buffer;";
    > {
		cullmode = none;
		ZEnable = false;
		ZWriteEnable = false;
		VertexShader = compile vs_2_0 ScreenQuadVS();
		PixelShader = compile ps_2_a keyerPS();
    }
}

////////////// eof ///
