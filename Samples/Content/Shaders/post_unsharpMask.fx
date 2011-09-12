/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/post_unsharpMask.fx#1 $

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
    Classic Unsharp Mask Sharpening

******************************************************************************/

#include "Quad.fxh"

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	string Script = "Technique=Main;";
> = 0.8; // version #

float4 ClearColor <
	string UIWidget = "color";
	string UIName = "background";
> = {0,0,0,0};

float ClearDepth <string UIWidget = "none";> = 1.0;

/*********** Tweakables **********************/

QUAD_REAL Sharp <
	string UIWidget = "slider";
	QUAD_REAL UIMin = 0.0;
	QUAD_REAL UIMax = 5.0;
	QUAD_REAL UIStep = 0.01;
> = 1.0f;

DECLARE_QUAD_TEX(OriginalMap,OriginalSampler,"X8R8G8B8")
DECLARE_QUAD_TEX(HorizBlurMap,HorizBlurSampler,"X8R8G8B8")
DECLARE_QUAD_TEX(BlurredMap,BlurredSampler,"X8R8G8B8")
DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer,"D24S8")

/************* DATA STRUCTS **************/

/* data from application vertex buffer for screen-aligned quad*/
struct quaddata {
    QUAD_REAL3 Position	: POSITION;
    QUAD_REAL3 UV		: TEXCOORD0;
};

struct blurVOut {
    QUAD_REAL4 Position   : POSITION;
    QUAD_REAL4 Diffuse    : COLOR0;
    QUAD_REAL4 TexCoord0   : TEXCOORD0;
    QUAD_REAL4 TexCoord1   : TEXCOORD1;
    QUAD_REAL4 TexCoord2   : TEXCOORD2;
    QUAD_REAL4 TexCoord3   : TEXCOORD3;
    QUAD_REAL4 TexCoord4   : TEXCOORD4;
    QUAD_REAL4 TexCoord5   : TEXCOORD5;
    QUAD_REAL4 TexCoord6   : TEXCOORD6;
    QUAD_REAL4 TexCoord7   : TEXCOORD7;
    QUAD_REAL4 TexCoord8   : COLOR1;   
};

/*********** vertex shaders ******/

// vertex shaders for screen-aligned quad data

blurVOut vertBlurVS(quaddata IN)
{
    blurVOut OUT = (blurVOut)0;
    OUT.Position = QUAD_REAL4(IN.Position, 1);
	QUAD_REAL TexelIncrement = 1.0/QuadScreenSize.y;
	QUAD_REAL2 off = QUAD_REAL2(QuadTexOffset/(QuadScreenSize.x),QuadTexOffset/(QuadScreenSize.y));
    QUAD_REAL3 Coord = QUAD_REAL3(IN.UV.x + off.x, IN.UV.y + off.y, 1);
    OUT.TexCoord0 = QUAD_REAL4(Coord.x, Coord.y + TexelIncrement, IN.UV.z, 1);
    OUT.TexCoord1 = QUAD_REAL4(Coord.x, Coord.y + TexelIncrement * 2, IN.UV.z, 1);
    OUT.TexCoord2 = QUAD_REAL4(Coord.x, Coord.y + TexelIncrement * 3, IN.UV.z, 1);
    OUT.TexCoord3 = QUAD_REAL4(Coord.x, Coord.y + TexelIncrement * 4, IN.UV.z, 1);
    OUT.TexCoord4 = QUAD_REAL4(Coord.x, Coord.y, IN.UV.z, 1);
    OUT.TexCoord5 = QUAD_REAL4(Coord.x, Coord.y - TexelIncrement, IN.UV.z, 1);
    OUT.TexCoord6 = QUAD_REAL4(Coord.x, Coord.y - TexelIncrement * 2, IN.UV.z, 1);
    OUT.TexCoord7 = QUAD_REAL4(Coord.x, Coord.y - TexelIncrement * 3, IN.UV.z, 1);
    OUT.TexCoord8 = QUAD_REAL4(Coord.x, Coord.y - TexelIncrement * 4, IN.UV.z, 1);
    return OUT;
}

blurVOut horizBlurVS(quaddata IN)
{
    blurVOut OUT = (blurVOut)0;
    OUT.Position = QUAD_REAL4(IN.Position, 1);
	QUAD_REAL TexelIncrement = 1.0/QuadScreenSize.x;
	QUAD_REAL2 off = QUAD_REAL2(QuadTexOffset/(QuadScreenSize.x),QuadTexOffset/(QuadScreenSize.y));
    QUAD_REAL3 Coord = QUAD_REAL3(IN.UV.x + off.x, IN.UV.y + off.y, 1);
    OUT.TexCoord0 = QUAD_REAL4(Coord.x + TexelIncrement, Coord.y, IN.UV.z, 1);
    OUT.TexCoord1 = QUAD_REAL4(Coord.x + TexelIncrement * 2, Coord.y, IN.UV.z, 1);
    OUT.TexCoord2 = QUAD_REAL4(Coord.x + TexelIncrement * 3, Coord.y, IN.UV.z, 1);
    OUT.TexCoord3 = QUAD_REAL4(Coord.x + TexelIncrement * 4, Coord.y, IN.UV.z, 1);
    OUT.TexCoord4 = QUAD_REAL4(Coord.x, Coord.y, IN.UV.z, 1);
    OUT.TexCoord5 = QUAD_REAL4(Coord.x - TexelIncrement, Coord.y, IN.UV.z, 1);
    OUT.TexCoord6 = QUAD_REAL4(Coord.x - TexelIncrement * 2, Coord.y, IN.UV.z, 1);
    OUT.TexCoord7 = QUAD_REAL4(Coord.x - TexelIncrement * 3, Coord.y, IN.UV.z, 1);
    OUT.TexCoord8 = QUAD_REAL4(Coord.x - TexelIncrement * 4, Coord.y, IN.UV.z, 1);
    return OUT;
}

//////////////////////////////////
/********* pixel shaders ********/
//////////////////////////////////

QUAD_REAL4 maskPS(QuadVertexOutput IN) : COLOR
{   
    QUAD_REAL4 tex = tex2D(OriginalSampler, IN.UV);
    QUAD_REAL4 mask = tex2D(BlurredSampler, IN.UV);
    QUAD_REAL4 masked = tex+Sharp*(tex-mask);
    return masked;
}  

// relative filter weights indexed by distance from "home" texel
#define WT0 1.0
#define WT1 0.8
#define WT2 0.6
#define WT3 0.4
#define WT4 0.2

#define WT_NORMALIZE (WT0+2.0*(WT1+WT2+WT3+WT4))

QUAD_REAL4 blurPS(blurVOut IN,uniform sampler2D SrcSamp) : COLOR
{   
    QUAD_REAL4 OutCol = tex2D(SrcSamp, IN.TexCoord0) * (WT1/WT_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord1) * (WT2/WT_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord2) * (WT3/WT_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord3) * (WT4/WT_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord4) * (WT0/WT_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord5) * (WT1/WT_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord6) * (WT2/WT_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord7) * (WT3/WT_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord8) * (WT3/WT_NORMALIZE);
    return OutCol;
} 

/*************/

technique Main <
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	string Script =
			"RenderColorTarget0=OriginalMap;"
	        "RenderDepthStencilTarget=DepthBuffer;"
	        	"ClearSetColor=ClearColor;"
	        	"ClearSetDepth=ClearDepth;"
   				"Clear=Color;"
				"Clear=Depth;"
	        	"ScriptExternal=color;"
        	"Pass=Horiz;"
        	"Pass=Vert;"
	        "Pass=Mask;";
> {
    pass Horiz  <
    	string Script ="RenderColorTarget0=HorizBlurMap;"
    							"Draw=Buffer;";
    > {
		VertexShader = compile vs_2_0 horizBlurVS();
		cullmode = none;
		ZEnable = false;
		AlphaBlendEnable = false;
        PixelShader  = compile ps_2_0 blurPS(OriginalSampler);
    }
    pass Vert <
    	string Script ="RenderColorTarget0=BlurredMap;"
    							"Draw=Buffer;";
    > {
     	VertexShader = compile vs_2_0 vertBlurVS();
		cullmode = none;
		ZEnable = false;
        PixelShader  = compile ps_2_0 blurPS(HorizBlurSampler);
    }
    pass Mask <
    	string Script ="RenderColorTarget0=;"
    							"Draw=Buffer;";
    > {
		VertexShader = compile vs_1_1 ScreenQuadVS();
		cullmode = none;
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		PixelShader = compile ps_2_0 maskPS();	
    }
}

/***************************** eof ***/
