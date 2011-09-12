/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/post_halftone_r.fx#1 $

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
	Radial-dot B&W halftones

******************************************************************************/

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	string Script = "Technique=Toned;";
> = 0.8; // version #

#include "Quad.fxh"

#define DOTS_PER_BIT 8.0
#define IMG_DIVS 8.0

///////////////////////////////////////////////////////////////
/// TWEAKABLES ////////////////////////////////////////////////
///////////////////////////////////////////////////////////////

float4 ClearColor : DIFFUSE = {0,0,0,1.0};

float ClearDepth < string UIWidget = "none"; > = 1.0;

////////////////////////////////////////////////////////
/// TEXTURES ///////////////////////////////////////////
////////////////////////////////////////////////////////

float4 make_tones(float3 Pos : POSITION, float3 Size : PSIZE) : COLOR {
	float2 delta = Pos.xy - float2(0.5,0.5);
	float d = dot(delta,delta);
	float rSquared = (Pos.z*Pos.z)/2.0;
	float n2 = (d<rSquared) ? 1.0 : 0.0;
	return float4(n2,n2,n2,1.0);
	//return float4(Pos,1.0);
}

texture ToneTex <
    string function = "make_tones";
	string ResourceType = "VOLUME";
	float3 Dimensions = { 16.0f, 16.0f, 32.0f };
	string UIWidget = "None";
	//string format = "A8R8G8B8";
>;

sampler ToneSampler = sampler_state 
{
    texture = <ToneTex>;
    AddressU  = WRAP;        
    AddressV  = WRAP;
    AddressW  = CLAMP;
    MIPFILTER = NONE;
    MINFILTER = ANISOTROPIC;
    MAGFILTER = LINEAR;
};

////////////

#define DOWNSIZE (1.0/IMG_DIVS)

DECLARE_SIZED_QUAD_TEX(SceneMap,SceneSampler,"A8R8G8B8",DOWNSIZE)

texture SceneDepth : RENDERDEPTHSTENCILTARGET <
	float2 ViewportRatio = {DOWNSIZE,DOWNSIZE};
	string Format = "D24S8";
	string UIWidget = "None";
>;

/*********************************************************/
/*********** pixel shader ********************************/
/*********************************************************/

float4 tonePS(QuadVertexOutput IN) : COLOR {
	QUAD_REAL4 scnC = tex2D(SceneSampler,IN.UV);
	float lum = dot(float3(.2,.7,.1),scnC.xyz);
	float3 lx = float3((DOTS_PER_BIT*IMG_DIVS*IN.UV.xy),lum);
	QUAD_REAL4 dotC = tex3D(ToneSampler,lx);
    return float4(dotC.xyz,1.0);
}

////////////////////////////////////////////////////////////////////
/// TECHNIQUES /////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////

technique Toned <
	string Script =
			"ClearSetColor=ClearColor;"
			"ClearSetDepth=ClearDepth;"
			"Clear=Color;"
			"Clear=Depth;"
			"RenderColorTarget0=SceneMap;"
	        "RenderDepthStencilTarget=SceneDepth;"
	        	"ClearSetColor=ClearColor;"
	        	"ClearSetDepth=ClearDepth;"
   				"Clear=Color;"
				"Clear=Depth;"
	        	"ScriptExternal=color;"
        	"Pass=p0;";
>
{
	pass p0 <
		string Script = 
	        "RenderColorTarget0=;"
	        "RenderDepthStencilTarget=;"
	        "Draw=Buffer;";
	> {
		VertexShader = compile vs_2_0 ScreenQuadVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_2_0 tonePS();
	}
}

/***************************** eof ***/
