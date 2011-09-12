/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/Graph1DTextures.fx#1 $

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

A simple way to view 1D Textures as a 2D graph

In this sample, a 1D texture is generated -- but if CURVE_FROM_FILE is defined,
	then you can use "ColorCurveTex" instead.

All geometry is ignored

******************************************************************************/

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	string Script = "Technique=ColorCurve;";
> = 0.8; // version #

// #define CURVE_FROM_FILE

#include "Quad.fxh"

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

QUAD_REAL TopOfScale <
    string UIWidget = "slider";
    QUAD_REAL UIMin = 0.2f;
    QUAD_REAL UIMax = 4.0f;
    QUAD_REAL UIStep = 0.01f;
	string UIName = "Maximum Y";
> = 1.0f;

QUAD_REAL Left <
    string UIWidget = "slider";
    QUAD_REAL UIMin = 0.0f;
    QUAD_REAL UIMax = 0.99f;
    QUAD_REAL UIStep = 0.001f;
	string UIName = "Minimum X";
> = 0.0f;

QUAD_REAL Right <
    string UIWidget = "slider";
    QUAD_REAL UIMin = 0.01f;
    QUAD_REAL UIMax = 1.0f;
    QUAD_REAL UIStep = 0.001f;
	string UIName = "Maximum X";
> = 1.0f;


QUAD_REAL AlphaEffect <
    string UIWidget = "slider";
    QUAD_REAL UIMin = 0.0f;
    QUAD_REAL UIMax = 1.0f;
    QUAD_REAL UIStep = 0.1f;
    string UIName = "Alpha Effect (if any)";
> = 0.2f;

///////////////////////////////////////////////////////////
///////////////////////////// Curve Texture ///////////////
///////////////////////////////////////////////////////////

#ifdef CURVE_FROM_FILE

texture ColorCurveTex  <
	string name="some_user_curve_map.dds";
    string ResourceType = "2D";
>;

#else /* ! CURVE_FROM_FILE */

//
// This code just builds a sample curve for testing
//
//

// assume "t" ranges from 0 to 1 safely
// brute-force this, it's running on the CPU
QUAD_REAL3 c_bezier(QUAD_REAL3 c0, QUAD_REAL3 c1, QUAD_REAL3 c2, QUAD_REAL3 c3, QUAD_REAL t)
{
	QUAD_REAL t2 = t*t;
	QUAD_REAL t3 = t2*t;
	QUAD_REAL nt = 1.0 - t;
	QUAD_REAL nt2 = nt*nt;
	QUAD_REAL nt3 = nt2 * nt;
	QUAD_REAL3 b = nt3*c0 + (3.0*t*nt2)*c1 + (3.0*t2*nt)*c2 + t3*c3;
	return b;
}

// function used to fill the volume noise texture
QUAD_REAL4 color_curve(QUAD_REAL2 Pos : POSITION) : COLOR
{
    QUAD_REAL3 kolor0 = QUAD_REAL3(0.0,0.0,0.0);
    QUAD_REAL3 kolor1 = QUAD_REAL3(0.9,0.7,0.0);
    QUAD_REAL3 kolor2 = QUAD_REAL3(0.3,0.5,0.95);
    QUAD_REAL3 kolor3 = QUAD_REAL3(1.0,0.9,1.0);
	QUAD_REAL3 sp = c_bezier(kolor0,kolor1,kolor2,kolor3,Pos.x);
    return QUAD_REAL4(sp,Pos.x);
}

texture ColorCurveTex  <
    string ResourceType = "2D";
    string function = "color_curve";
    string UIWidget = "None";
	float2 Dimensions = { 256.0f, 4.0f}; // could be height=1, but I want it to be visible in the Texture View...
>;

#endif /* ! CURVE_FROM_FILE */

sampler ColorCurveSampler = sampler_state 
{
    texture = <ColorCurveTex>;
    AddressU  = CLAMP;        
    AddressV  = CLAMP;
    MIPFILTER = LINEAR;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Data //////
///////////////////////////////////////////////////////////

QUAD_REAL4 graphPS(QuadVertexOutput IN) : COLOR
{   
	QUAD_REAL nx = Left + (IN.UV.x*(Right-Left));
	QUAD_REAL4 tv = tex2D(ColorCurveSampler,QUAD_REAL2(nx,0));
	QUAD_REAL4 f = 0;
	f = (tv/TopOfScale > (1-IN.UV.y).xxxx);
	f.xyz *= lerp(f.xyz,f.www,AlphaEffect);
	return QUAD_REAL4(f);
}  

////////////////////////////////////////////////////////////
/////////////////////////////////////// techniques /////////
////////////////////////////////////////////////////////////

technique ColorCurve <
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	string Script = "Pass=p0;";
> {
    pass p0 <
    	string Script = "Draw=Buffer;";
	> {
		cullmode = none;
		ZEnable = false;
		AlphaBlendEnable = false;
		VertexShader = compile vs_2_0 ScreenQuadVS();
		PixelShader  = compile ps_2_0 graphPS();
    }
}
