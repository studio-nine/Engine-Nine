/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/blur59.fxh#1 $

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
    Utility declarations for doing 2D blur effects in FXComposer.

    To use:
	Choose a render target size. The default is 256x256. You can
		change this by defining RTT_SIZE before including this header.
	Choose filter size and filter weights. 9x9 and 5x5 filter routines
		are supplied in this file. They use filter weights defined
		as WT9_0 through WT9_4 and WT5_0 through WT5_2. Again, if you
		want something other than the default, specify these weights
		via #define before #including this header file.
	Declare render targets. Use the SQUARE_TARGET macro to declare both the
		texturetarget and a sampler to read it. For two-pass convolutions
		you'll need at least two such targets.
	Use the BLUR9_PASSES macro to declare blur operations. It will declare both required
		passes and render to the target texture you specify.

	Extra handy stuff for RTT:
		ScreenAlignedQuadData structure (vertex buffer for FXComposer screen quad)

	// Example: ////////////////////

	// ..in your .fx file declarations....

	#define RTT_SIZE 512	// if we want to use a size other than the default
	#include "blur59.h"
	// now we'll declare two RTT maps
	SQUARE_TARGET(RTTMap1,RTTSampler1)
	SQUARE_TARGET(RTTMap2,RTTSampler2)

	// ...in your technique...

	technique aBlurryTechnique
	{
	    // we start by rendering something into the first RTT map
	    pass initial 
		<
		    string RENDERCOLORTARGET = "RTTMap1"; 
		    float cleardepth = 1.0f;
		    dword clearcolor = 0x0;
		>
	    {		
		VertexShader = compile vs_2_0 typicalVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_2_0 typicalPS();
	    }
	    // So now our plain image is accessable via "RTTSampler1"
	    BLUR9_PASSES(RTTSampler1,"RTTMap2",RTTSampler2,"RTTMap1")
	    //
	    // The blurred result has now been rendered and is
	    //	agains available via "RTTSampler1" -- the intermediate
	    //	(horizontal blur only) pass was rendered to "RTTSampler2"
	    //	and then blurred vertically back into "RTTSampler1"

	    // ... additional passes....
	}

	If you had wanted to re-use the initial unblurred image, just declare
		a third texture target, and specify it as the last parameter
		of the "BLUR9_PASSES" macro instead of "RTTSampler1"


******************************************************************************/

#ifndef _BLUR59_H_
#define _BLUR59_H_

#include "Quad.fxh"

// Default rendertargets are this size.
//   If you want a different size, define RTT_SIZE before
// 	including this file.
#ifndef RTT_SIZE
#define RTT_SIZE 256
#endif /* RTT_SIZE */

#define RTT_TEXEL_SIZE (1.0f/RTT_SIZE)

#ifdef ONLY_FIXED_SIZE_SQUARE_TEXTURES
QUAD_REAL RTTTexelIncrement <
    string UIName =  "RTT Texel Size";
> = RTT_TEXEL_SIZE;
#endif /* ONLY_FIXED_SIZE_SQUARE_TEXTURES */

// Relative filter weights for each texel. The default here is for symmetric distribution.
// To assign your own filter weights, just define WT9_0 through WT9_4 *before* including
//	this file.....

// weights for 9x9 filtering

#ifndef WT9_0
// Relative filter weights indexed by distance (in texels) from "home" texel
//	(WT9_0 is the "home" or center of the filter, WT9_4 is four texels away)
#define WT9_0 1.0
#define WT9_1 0.8
#define WT9_2 0.6
#define WT9_3 0.4
#define WT9_4 0.2
#endif /* WT9_0 */

// weights for 5x5 filtering

#ifndef WT5_0
// Relative filter weights indexed by distance (in texels) from "home" texel
//	(WT5_0 is the "home" or center of the filter, WT5_2 is two texels away)
#define WT5_0 1.0
#define WT5_1 0.8
#define WT5_2 0.2
#endif /* WT5_0 */

// RTT Textures

// call SQUARE_TARGET(tex,sampler) to create the declarations for a rendertarget
//	texture and its associated sampler. You will get a square 8-bit texture
//	of RTT_SIZE texels on each side

#define SQUARE_TARGET(texName,samplerName) texture texName : RENDERCOLORTARGET < \
    int width = RTT_SIZE; \
    int height = RTT_SIZE; \
    int MIPLEVELS = 1; \
    string format = "X8R8G8B8"; \
    string UIWidget = "None"; \
>; \
sampler samplerName = sampler_state { \
    texture = <texName>; \
    AddressU  = CLAMP; \
    AddressV  = CLAMP; \
    AddressW  = CLAMP; \
    MIPFILTER = NONE; \
    MINFILTER = LINEAR; \
    MAGFILTER = LINEAR; \
};

/************* DATA STRUCTS **************/

/* data from FXComposer vertex buffer for built-in screen-aligned quad */
struct ScreenAlignedQuadData {
    QUAD_REAL3 Position	: POSITION;
    QUAD_REAL3 UV		: TEXCOORD0;
};

// nine texcoords, to sample (usually) nine in-line texels
struct ScreenAligned9TexelVOut
{
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

// five texcoords, to sample (usually) five in-line texels
struct ScreenAligned5TexelVOut
{
    QUAD_REAL4 Position   : POSITION;
    QUAD_REAL4 Diffuse    : COLOR0;
    QUAD_REAL4 TexCoord0   : TEXCOORD0;
    QUAD_REAL4 TexCoord1   : TEXCOORD1;
    QUAD_REAL4 TexCoord2   : TEXCOORD2;
    QUAD_REAL4 TexCoord3   : TEXCOORD3;
    QUAD_REAL4 TexCoord4   : TEXCOORD4;
};

/*********** vertex shaders ******/

// vertex shader to align blur samples vertically
ScreenAligned9TexelVOut vert9BlurVS(ScreenAlignedQuadData IN)
{
    ScreenAligned9TexelVOut OUT = (ScreenAligned9TexelVOut)0;
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

// vertex shader to align blur samples horizontally
ScreenAligned9TexelVOut horiz9BlurVS(ScreenAlignedQuadData IN)
{
    ScreenAligned9TexelVOut OUT = (ScreenAligned9TexelVOut)0;
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

// vertex shader to align blur samples vertically
ScreenAligned5TexelVOut vert5BlurVS(ScreenAlignedQuadData IN)
{
    ScreenAligned5TexelVOut OUT = (ScreenAligned5TexelVOut)0;
    OUT.Position = QUAD_REAL4(IN.Position, 1);
	QUAD_REAL TexelIncrement = 1.0/QuadScreenSize.y;
	QUAD_REAL2 off = QUAD_REAL2(QuadTexOffset/(QuadScreenSize.x),QuadTexOffset/(QuadScreenSize.y));
    QUAD_REAL3 Coord = QUAD_REAL3(IN.UV.x + off.x, IN.UV.y + off.y, 1);
    OUT.TexCoord0 = QUAD_REAL4(Coord.x, Coord.y + TexelIncrement, IN.UV.z, 1);
    OUT.TexCoord1 = QUAD_REAL4(Coord.x, Coord.y + TexelIncrement * 2, IN.UV.z, 1);
    OUT.TexCoord2 = QUAD_REAL4(Coord.x, Coord.y, IN.UV.z, 1);
    OUT.TexCoord3 = QUAD_REAL4(Coord.x, Coord.y - TexelIncrement, IN.UV.z, 1);
    OUT.TexCoord4 = QUAD_REAL4(Coord.x, Coord.y - TexelIncrement * 2, IN.UV.z, 1);
    return OUT;
}

// vertex shader to align blur samples horizontally
ScreenAligned5TexelVOut horiz5BlurVS(ScreenAlignedQuadData IN)
{
    ScreenAligned5TexelVOut OUT = (ScreenAligned5TexelVOut)0;
    OUT.Position = QUAD_REAL4(IN.Position, 1);
	QUAD_REAL TexelIncrement = 1.0/QuadScreenSize.x;
	QUAD_REAL2 off = QUAD_REAL2(QuadTexOffset/(QuadScreenSize.x),QuadTexOffset/(QuadScreenSize.y));
    QUAD_REAL3 Coord = QUAD_REAL3(IN.UV.x + off.x, IN.UV.y + off.y, 1);
    OUT.TexCoord0 = QUAD_REAL4(Coord.x + TexelIncrement, Coord.y, IN.UV.z, 1);
    OUT.TexCoord1 = QUAD_REAL4(Coord.x + TexelIncrement * 2, Coord.y, IN.UV.z, 1);
    OUT.TexCoord2 = QUAD_REAL4(Coord.x, Coord.y, IN.UV.z, 1);
    OUT.TexCoord3 = QUAD_REAL4(Coord.x - TexelIncrement, Coord.y, IN.UV.z, 1);
    OUT.TexCoord4 = QUAD_REAL4(Coord.x - TexelIncrement * 2, Coord.y, IN.UV.z, 1);
    return OUT;
}

//////////////////////////////////
/********* pixel shaders ********/
//////////////////////////////////

#define WT9_NORMALIZE (WT9_0+2.0*(WT9_1+WT9_2+WT9_3+WT9_4))

QUAD_REAL4 blur9PS(ScreenAligned9TexelVOut IN,uniform sampler2D SrcSamp) : COLOR
{   
    QUAD_REAL4 OutCol = tex2D(SrcSamp, IN.TexCoord0) * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord1) * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord2) * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord3) * (WT9_4/WT9_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord4) * (WT9_0/WT9_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord5) * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord6) * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord7) * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord8) * (WT9_3/WT9_NORMALIZE);
    return OutCol;
} 

#define WT5_NORMALIZE (WT5_0+2.0*(WT5_1+WT5_2))

QUAD_REAL4 blur5PS(ScreenAligned5TexelVOut IN,uniform sampler2D SrcSamp) : COLOR
{   
    QUAD_REAL4 OutCol = tex2D(SrcSamp, IN.TexCoord0) * (WT5_1/WT5_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord1) * (WT5_2/WT5_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord2) * (WT5_0/WT5_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord3) * (WT5_1/WT5_NORMALIZE);
    OutCol += tex2D(SrcSamp, IN.TexCoord4) * (WT5_2/WT5_NORMALIZE);
    return OutCol;
} 

/*************/

// Passes for two-pass blur.
// To use, render (or read) the data you want into sampler/target "src1"
//	and then call BLUR9_PASSES()
// Rendering will go from texture to texture: src->intermediate->final
// If the src is a renderable target, it's okay to have src=final, it will
//	just re-use the resource.
// Note that "target" names need to be in QUOTES because they are strings.

#define BLUR9_PASSES_COMMAND(intermediateTarget, finalTarget)	\
<setpass name="BlurGlowBuffer_Horz"/>	\
<setrendercolortarget=intermediateTarget/>	\
<clear depth="1.0"/>	\
<clear color="0,0,0,0"/>	\
<drawquad/>	\
<setpass name="BlurGlowBuffer_Vert"/>	\
<setrendercolortarget=finalTarget/>	\
<drawquad/>	\

#define BLUR5_PASSES_COMMAND(intermediateTarget, finalTarget)	\
<setpass name="BlurGlowBuffer_Horz"/>	\
<setrendercolortarget=intermediateTarget/>	\
<clear depth="1.0"/>	\
<clear color="0,0,0,0"/>	\
<drawquad/>	\
<setpass name="BlurGlowBuffer_Vert"/>	\
<setrendercolortarget=finalTarget/>	\
<drawquad/>	\

#define BLUR9_PASSES(srcSampler,intermediateTarget,intermediateSampler,finalTarget) \
    pass BlurGlowBuffer_Horz < \
		string ScriptFunction = "RenderColorTarget=" intermediateTarget "; Draw=Buffer;"; \
	> { \
		VertexShader = compile vs_2_0 horiz9BlurVS(); \
		cullmode = none; ZEnable = false; AlphaBlendEnable = false; \
		PixelShader  = compile ps_2_a blur9PS(srcSampler); } \
    pass BlurGlowBuffer_Vert < \
		string ScriptFunction = "RenderColorTarget=" finalTarget "; Draw=Buffer;"; \
	> { \
		VertexShader = compile vs_2_0 vert9BlurVS(); \
		cullmode = none; ZEnable = false; \
	    PixelShader  = compile ps_2_a blur9PS(intermediateSampler); }

#define BLUR5_PASSES(srcSampler,intermediateTarget,intermediateSampler,finalTarget) \
    pass BlurGlowBuffer_Horz < \
		string ScriptFunction = "RenderColorTarget=" intermediateTarget "; Draw=Buffer;"; \
	> { \
		VertexShader = compile vs_2_0 horiz5BlurVS(); \
		cullmode = none; ZEnable = false; AlphaBlendEnable = false; \
		PixelShader  = compile ps_2_a blur5PS(srcSampler); } \
    pass BlurGlowBuffer_Vert < \
		string ScriptFunction = "RenderColorTarget=" finalTarget "; Draw=Buffer;"; \
	> { \
		VertexShader = compile vs_2_0 vert5BlurVS(); \
		cullmode = none; ZEnable = false; \
	    PixelShader  = compile ps_2_a blur5PS(intermediateSampler); }
	    

#endif /* _BLUR59_H_ */

/***************************** eof ***/
