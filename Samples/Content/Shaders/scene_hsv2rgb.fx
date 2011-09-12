/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/scene_hsv2rgb.fx#1 $

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
    Weird fun with HSV manipulation
	Geometry Ignored

******************************************************************************/

#include "rgb-hsv.fxh"	// will also include "Quad.fxh"

// #define NOISE_SCALE 500
// #define NOISE_VOLUME_SIZE 32

#include "noise_3d.fxh"

#define QSNOISE3D(p) (((QUAD_REAL)NOISE3D(p))-0.5)

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";
	string Script = "Technique=toRGB;";
> = 0.8; // version #

///////// Textures ///////////////

texture HsvImage <
    string ResourceName = "Veggie-HSV.dds";
    string ResourceType = "2D";
>;

sampler2D HsvImageSampler = sampler_state {
    Texture = <HsvImage>;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = None;
};

/// Numeric Inputs ////

QUAD_REAL Clock : TIME < string UIWidget = "None"; >;

QUAD_REAL HueVar <
	string UIName = "Hue Variance";
    string UIWidget = "slider";
    QUAD_REAL uimin = 0.0;
    QUAD_REAL uimax = 2.0;
    QUAD_REAL uistep = 0.01;
> = 0.36;

QUAD_REAL HueSpeed <
    string UIWidget = "slider";
    QUAD_REAL uimin = 0.0001;
    QUAD_REAL uimax = 0.25;
    QUAD_REAL uistep = 0.01;
> = 0.03;

QUAD_REAL HueBlobSize <
    string UIWidget = "slider";
    QUAD_REAL uimin = 0.5;
    QUAD_REAL uimax = 20.0;
    QUAD_REAL uistep = 0.1;
> = 5.2;

//

QUAD_REAL SatVar
<
	string UIName = "Saturation Variance";
    string UIWidget = "slider";
    QUAD_REAL uimin = 0.0;
    QUAD_REAL uimax = 2.0;
    QUAD_REAL uistep = 0.01;
> = 0.86;


QUAD_REAL SatSpeed
<
	string UIName = "Saturation Speed";
    string UIWidget = "slider";
    QUAD_REAL uimin = 0.0001;
    QUAD_REAL uimax = 0.25;
    QUAD_REAL uistep = 0.001;
> = 0.006;


QUAD_REAL SatBlobSize
<
    string UIWidget = "slider";
    QUAD_REAL uimin = 0.1;
    QUAD_REAL uimax = 20.0;
    QUAD_REAL uistep = 0.1;
> = 10.0;

/************* DATA STRUCTS **************/

/* data from application vertex buffer */
struct appdata {
    QUAD_REAL3 Position    : POSITION;
    QUAD_REAL4 UV        : TEXCOORD0;
};

/* data passed from vertex shader to pixel shader */
struct vertexOutput {
    QUAD_REAL4 HPosition    : POSITION;
    QUAD_REAL2 UV    : TEXCOORD0;
    QUAD_REAL3 UV1    : TEXCOORD1;
    QUAD_REAL3 UV2    : TEXCOORD2;
};

/*********** vertex shader ******/

vertexOutput quadVS(appdata IN)
{
    vertexOutput OUT = (vertexOutput)0;
    OUT.HPosition = QUAD_REAL4(IN.Position, 1);
    // OUT.UV = IN.UV.xy;
	QUAD_REAL2 off = QUAD_REAL2(QuadTexOffset/(QuadScreenSize.x),QuadTexOffset/(QuadScreenSize.y));
    OUT.UV = QUAD_REAL2(IN.UV.xy+off); 
    OUT.UV1 = QUAD_REAL3(IN.UV.xy/HueBlobSize,Clock*HueSpeed);
    OUT.UV2 = QUAD_REAL3(IN.UV.xy/SatBlobSize,Clock*SatSpeed);
    return OUT;
}

/**************************************/
/********* blend pixel shaders ********/
/**************************************/

QUAD_REAL4 toRgbPS(vertexOutput IN) : COLOR {
	QUAD_REAL4 hsv = (QUAD_REAL4)tex2D(HsvImageSampler, IN.UV);
	QUAD_REAL ns = HueVar * (QSNOISE3D(IN.UV1));
	hsv.x += ns;
	hsv.x = frac(hsv.x);
	ns = SatVar * (QSNOISE3D(IN.UV2));
	hsv.y += ns;
	hsv.y = min(max(0,hsv.y),1);
	QUAD_REAL3 rgb = hsv_to_rgb(hsv.xyz);
    return QUAD_REAL4(rgb.xyz,1);
}

/*******************************************************************/
/************* TECHNIQUES ******************************************/
/*******************************************************************/

technique toRGB <
	string ScriptClass = "scene";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";
	string Script = "Pass=p0;";
> {
    pass p0 <
    	string Script = "RenderColorTarget0=;"
								"Draw=Buffer;";
	> {
		VertexShader = compile vs_2_0 quadVS();
		ZEnable = false;
		ZWriteEnable = false;
		CullMode = None;
		PixelShader = compile ps_2_0 toRgbPS();
	}
}

/***************************** eof ***/
