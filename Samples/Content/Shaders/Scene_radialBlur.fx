/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/Scene_radialBlur.fx#1 $

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
	Radial blur effect
	Fast version calculates texcoords in vertex shader, but only supports 8 samples

TODO:
	See if converting to polar coordinates, blurring horizontally, then converting
	back is faster...

******************************************************************************/

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	string Script = "Technique=RadialBlur;";
> = 0.8; // version #

float4 ClearColor <
	string UIWidget = "color";
	string UIName = "background";
> = {0.25,0.25,0.25,0.0};

float ClearDepth <string UIWidget = "none";> = 1.0;

#include "Quad.fxh"

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

float2 Center = { 0.5, 0.5 };

float BlurStart <
    string UIName = "Blur start";
    string UIWidget = "slider";
    float UIMin = 0.0f; float UIMax = 1.0f; float UIStep = 0.01f;
> = 1.0f;

float BlurWidth <
    string UIName = "Blur width";
    string UIWidget = "slider";
    float UIMin = -1.0f; float UIMax = 1.0f; float UIStep = 0.01f;
> = -0.2f;

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Data //////
///////////////////////////////////////////////////////////

DECLARE_QUAD_TEX(SceneMap,SceneSampler,"X8R8G8B8")
DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer, "D24S8")

///////////////////////////////////////////////////////////
/////////////////////////////////// data structures ///////
///////////////////////////////////////////////////////////

struct VS_OUTPUT_FAST
{
   	float4 Position    : POSITION;
    float2 TexCoord[8] : TEXCOORD0;
};

////////////////////////////////////////////////////////////
////////////////////////////////// vertex shaders //////////
////////////////////////////////////////////////////////////

QuadVertexOutput VS_RadialBlur(float4 Position : POSITION, 
				  		float2 TexCoord : TEXCOORD0)
{
    QuadVertexOutput OUT;
    OUT.Position = Position;
 	float2 texelSize = 1.0 / QuadScreenSize;
    // don't want bilinear filtering on original scene:
    OUT.UV = TexCoord + texelSize*0.5 - Center;
    return OUT;
}

VS_OUTPUT_FAST VS_RadialBlurFast(float4 Position : POSITION, 
				  				 float2 TexCoord : TEXCOORD0,
				  				 uniform int nsamples)
{
    VS_OUTPUT_FAST OUT;
    OUT.Position = Position;
    // generate texcoords for radial blur (scale around center)
	float2 texelSize = 1.0 / QuadScreenSize;
	float2 s = TexCoord + texelSize*0.5;
    for(int i=0; i<nsamples; i++) {
    	float scale = BlurStart + BlurWidth*(i/(float) (nsamples-1));	// this will be precalculated (i hope)
    	OUT.TexCoord[i] = (s - Center)*scale + Center;
   	}
    return OUT;
}

//////////////////////////////////////////////////////
////////////////////////////////// pixel shaders /////
//////////////////////////////////////////////////////

half4 PS_RadialBlur(QuadVertexOutput IN,
			   		uniform sampler2D tex,
			   		uniform int nsamples
			   		) : COLOR
{
    half4 c = 0;
    // this loop will be unrolled by compiler and the constants precalculated:
    for(int i=0; i<nsamples; i++) {
    	float scale = BlurStart + BlurWidth*(i/(float) (nsamples-1));
    	c += tex2D(tex, IN.UV.xy*scale + Center );
   	}
   	c /= nsamples;
    return c;
} 

half4 PS_RadialBlurFast(VS_OUTPUT_FAST IN,
			   			uniform sampler2D tex,
			   			uniform int nsamples
			   			) : COLOR
{
    half4 c = 0;
    for(int i=0; i<nsamples; i++) {
    	c += tex2D(tex, IN.TexCoord[i]);
   	}
   	c /= nsamples;
    return c;
}

////////////////////////////////////////////////////////////
/////////////////////////////////////// techniques /////////
////////////////////////////////////////////////////////////

technique RadialBlur <
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	string Script =
			"RenderColorTarget0=SceneMap;"
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
								"Draw=Buffer;";
    > {
		cullmode = none;
		ZEnable = false;
		VertexShader = compile vs_2_0 VS_RadialBlur();
		PixelShader  = compile ps_2_0 PS_RadialBlur(SceneSampler, 16);
    }
}

technique RadialBlurFast <
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	string Script =
			"RenderColorTarget0=SceneMap;"
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
								"Draw=Buffer;";
    > {
		cullmode = none;
		ZEnable = false;
		VertexShader = compile vs_2_0 VS_RadialBlurFast(8);
		PixelShader  = compile ps_2_0 PS_RadialBlurFast(SceneSampler, 8);
    }
}

////////////////////// eof ///
