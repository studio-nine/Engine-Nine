/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/post_wipe.fx#1 $

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

Wipe an overlay over the scene -- it's Obi-wan's favorite major scene transition!

******************************************************************************/

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	string Script = "Technique=wipe;";
> = 0.8; // version #

#include "Quad.fxh"

float4 ClearColor : DIFFUSE < string UIWidget="color"; > = {0.3,0.3,0.3,1.0};

float ClearDepth < string UIWidget = "none"; > = 1.0;

float Wipe <
	string UIName = "Wipe Center";
	string UIWidget = "slider";
	float UIMin = -0.5;
	float UIMax = 1.5;
	float UIStep = 0.001;
> = 0.0f;

float WipeSoft <
	string UIName = "Softness";
	string UIWidget = "slider";
	float UIMin = 0.04;
	float UIMax = 0.5;
	float UIStep = 0.001;
> = 0.07f;

float Angle <
	string UIName = "Rotate";
	string UIWidget = "slider";
	float UIMin = -90;
	float UIMax = 180;
	float UIStep = .1;
> = 0.0f;

float Slant <
	string UIName = "Slant";
	string UIWidget = "slider";
	float UIMin = -.5;
	float UIMax = 0.5;
	float UIStep = .01;
> = 0.18f;

///////////////////////////////////////////////////////////
// Procedural Soft-Edge ///////////////////////////////////
///////////////////////////////////////////////////////////

float4 soft_trans(float2 Pos : POSITION) : COLOR { return float4((1-Pos.x).xxx,1); }

texture SoftEdgeTex  <
    string TextureType = "2D";
    string function = "soft_trans";
    string UIWidget = "None";
    int width = 256, height = 1;
>;

sampler SoftEdgeSamp = sampler_state 
{
    texture = <SoftEdgeTex>;
    AddressU  = CLAMP;        
    AddressV  = CLAMP;
    MIPFILTER = LINEAR;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Data //////
///////////////////////////////////////////////////////////

DECLARE_QUAD_TEX(SceneTexture,SceneSampler,"A8R8G8B8")
DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer, "D24S8")

/////////////////////////////////////////////////////
//// Textures for Input Images //////////////////////
/////////////////////////////////////////////////////

FILE_TEXTURE_2D(Overlay,OverlaySampler,"Veggie.dds")

/////////////////////////////////////////////////////
//// Vertex Connector ///////////////////////////////
/////////////////////////////////////////////////////

struct WipeVertexOutput {
   	float4 Position	: POSITION;
    float2 UV		: TEXCOORD0;
    float2 Wipe		: TEXCOORD1;
};

////////////////////////////////////////////////////////////
/////////////////////////////////////// Shader /////////////
////////////////////////////////////////////////////////////

WipeVertexOutput WipeQuadVS(
		float3 Position : POSITION, 
		float3 TexCoord : TEXCOORD0,
		uniform float ca,
		uniform float sa
) {
    WipeVertexOutput OUT;
    OUT.Position = float4(Position, 1);
	float2 off = float2(QuadTexOffset/(QuadScreenSize.x),QuadTexOffset/(QuadScreenSize.y));
	float2 baseUV = float2(TexCoord.xy+off); 
    OUT.UV = baseUV;
    baseUV -= float2(0.5,0.5);
    baseUV = float2(ca*baseUV.x - sa*baseUV.y,
    				sa*baseUV.x + ca*baseUV.y);
    baseUV.x += baseUV.y*Slant;
    baseUV += float2(0.5,0.5);
    baseUV = float2(0.5,0)+(baseUV-float2(Wipe,0))/WipeSoft;
    OUT.Wipe = baseUV;
    return OUT;
}

float4 wipePS(WipeVertexOutput IN) : COLOR
{   
	float4 scnCol = tex2D(SceneSampler, IN.UV);
	float4 ovrCol = tex2D(OverlaySampler, IN.UV);
	float wiper = tex2D(SoftEdgeSamp,IN.Wipe).x;
	float4 res = lerp(scnCol,ovrCol,wiper);
	//res = float4(IN.Wipe.xy,0,1);
	return res;
}  

////////////////////////////////////////////////////////////
/////////////////////////////////////// techniques /////////
////////////////////////////////////////////////////////////

technique wipe <
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
	        			"RenderDepthStencilTarget=;"
						"Draw=Buffer;";
    > {
		cullmode = none;
		ZEnable = false;
		ZWriteEnable = false;
		VertexShader = compile vs_2_0 WipeQuadVS(cos(radians(Angle)),
												 sin(radians(Angle)));
		PixelShader = compile ps_2_0 wipePS();
    }
}

////////////// eof ///
