/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/post_orb.fx#1 $

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
	Wave the mouse over the rendered image.....

******************************************************************************/

#include "Quad.fxh"

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	string Script = "Technique=Technique?soft:hard;";
> = 0.8; // version #

float4 ClearColor : DIFFUSE = {0.3,0.3,0.3,1.0};
float ClearDepth
<
	string UIWidget = "none";
> = 1.0;

float4 MouseL : LEFTMOUSEDOWN < string UIWidget="None"; >; // unused, hack to turn on "always draw"
// float4 MouseR : RIGHTMOUSEDOWN < string UIWidget="None"; >;
float3 MousePos : MOUSEPOSITION < string UIWidget="None"; >;
// float Time : TIME < string UIWidget = "None"; >;

float Radius <
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 1.0;
	float UIStep = 0.01;
> = 0.6f;

float EffectScale <
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 1.0;
	float UIStep = 0.01;
> = 0.75f;

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Data //////
///////////////////////////////////////////////////////////

DECLARE_QUAD_TEX(SceneTexture,SceneSampler,"A8R8G8B8")
DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer, "D24S8")

//////////////////////////////////////////
/// Procedural Bulb Textures /////////////
//////////////////////////////////////////

#define ORB_TEX_SIZE 256

float4 lens_hard(float2 Pos : POSITION, float2 ps : PSIZE) : COLOR
{
	float4 result;
	float2 dx = Pos - float2(0.5,0.5);
	float l = length(dx);
	dx /= l;
	float c = l/(0.5-ps.x);
	if (c<=1.0) {
		result = float4(float2(0.5,0.5)+(0.5*c*dx),0.5,1.0);
	} else {
		result = float4(0.5,0.5,0.5,1.0);
	}
    return result;
}

float4 lens_soft(float2 Pos : POSITION, float2 ps : PSIZE) : COLOR
{
	float4 result;
	float2 dx = Pos - float2(0.5,0.5);
	float l = length(dx);
	dx /= l;
	float c = l/(0.5-ps.x);
	if (c<=1.0) {
		result = float4(float2(0.5,0.5)+(1-c)*(0.5*c*dx),0.5,1.0);
	} else {
		result = float4(0.5,0.5,0.5,1.0);
	}
    return result;
}

////// textures that call the above functions

texture HardLensTex  <
    string TextureType = "2D";
    string function = "lens_hard";
    string UIWidget = "None";
    string format = "a16b16g16r16f";
    int width = ORB_TEX_SIZE,
		height = ORB_TEX_SIZE;
>;

texture SoftLensTex  <
    string TextureType = "2D";
    string function = "lens_soft";
    string UIWidget = "None";
    string format = "a16b16g16r16f";
    int width = ORB_TEX_SIZE,
		height = ORB_TEX_SIZE;
>;

// samplers
sampler HardLensSamp = sampler_state 
{
    texture = <HardLensTex>;
    AddressU  = CLAMP;        
    AddressV  = CLAMP;
    MIPFILTER = LINEAR;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};

// samplers
sampler SoftLensSamp = sampler_state 
{
    texture = <SoftLensTex>;
    AddressU  = CLAMP;        
    AddressV  = CLAMP;
    MIPFILTER = LINEAR;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};

/****************************************/
/*** Shaders ****************************/
/****************************************/

float4 mouseDisplacePS(QuadVertexOutput IN,
	uniform sampler DisplacmentSamp) : COLOR
{
    float2 delta = IN.UV.xy-MousePos.xy;
	delta /= Radius;
	delta += float2(0.5,0.5);
	float2 nuv = tex2D(DisplacmentSamp,delta).xy - float2(0.5,0.5);
	nuv *= EffectScale;
	nuv += IN.UV.xy;
	float4 bg = tex2D(SceneSampler,nuv);
	return bg;
}

/****************************************/
/*** Technique **************************/
/****************************************/

technique hard <
	string Script =
			"RenderColorTarget0=SceneTexture;"
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
		VertexShader = compile vs_1_1 ScreenQuadVS();
		ZEnable = false;
		ZWriteEnable = false;
		CullMode = None;
		AlphaBlendEnable = false;
		PixelShader  = compile ps_2_0 mouseDisplacePS(HardLensSamp);
	}
}

technique soft <
	string Script =
			"RenderColorTarget0=SceneTexture;"
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
		VertexShader = compile vs_1_1 ScreenQuadVS();
		ZEnable = false;
		ZWriteEnable = false;
		CullMode = None;
		AlphaBlendEnable = false;
		PixelShader  = compile ps_2_0 mouseDisplacePS(SoftLensSamp);
	}
}

/****************************************/
/******************************* eof ****/
/****************************************/
