/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/scene_Mandelbrot.fx#1 $

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
    Mandelbrot set browser.
	Scene geometry is ignored.
	For more speed, reduce MAXIMUM_ITERATIONS
	For more detail, increase MAXIMUM_ITERATIONS

******************************************************************************/

#define MAXIMUM_ITERATIONS 25

#include "Quad.fxh"

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";
	string Script = "Technique=mandy;";
> = 0.8; // version #

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

float Scale <
	string UIWidget = "slider";
	float UIMin = 0.01;
	float UIMax = 4.0;
	float UIStep = 0.01;
> = 0.11f;

// float2 Center = {0.5,0.1};
float CenterX <
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 1.0;
	float UIStep = 0.001;
> = 0.709f;

float CenterY <
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 1.0;
	float UIStep = 0.001;
> = 0.350f;

float Range <
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 0.05;
	float UIStep = 0.001;
	string UIName = "Outer Color Gradation";
> = 0.05f;

float3 InColor <
	string UIWidget = "Color";
	string UIName = "Inside Region";
> = {0,0,0};

float3 OutColorA <
	string UIWidget = "Color";
	string UIName = "Outer Region";
> = {1,0,.3};

float3 OutColorB <
	string UIWidget = "Color";
	string UIName = "Edge Region";
> = {.2,1,0};

/////////////////////////////////////////////////////
////////////////////////////////// pixel shader /////
/////////////////////////////////////////////////////

float4 mandyPS(QuadVertexOutput IN) : COLOR {
    //float3 c = mandelbrot_color(IN.TexCoord0);
    float2 pos = frac(IN.UV.xy);
    float real = ((pos.x - 0.5)*Scale)-CenterX;
    float imag = ((pos.y - 0.5)*Scale)-CenterY;
    float Creal = real;
    float Cimag = imag;
    float r2 = 0;
    float i;
    for (i=0; (i<MAXIMUM_ITERATIONS) && (r2<4.0); i++) {
		float tempreal = real;
		real = (tempreal*tempreal) - (imag*imag) + Creal;
		imag = 2*tempreal*imag + Cimag;
		r2 = (real*real) + (imag*imag);
    }
    float3 finalColor;
    if (r2 < 4) {
       finalColor = InColor;
    } else {
    	finalColor = lerp(OutColorA,OutColorB,frac(i * Range));
    }
    return float4(finalColor,1);
}

///////////////////////////////////////////////////////////
/////////////////////////////////////// technique /////////
///////////////////////////////////////////////////////////

technique mandy <
	string ScriptClass = "scene";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";
	string Script = "Pass=p0;";
> {
    pass p0 <
    	string Script = "RenderColorTarget0=;"
								"Draw=Buffer;";
    > {
		VertexShader = compile vs_1_1 ScreenQuadVS();
		cullmode = none;
		ZEnable = false;
		ZWriteEnable = false;
		AlphaBlendEnable = false;
		PixelShader = compile ps_2_b mandyPS();
    }
}

//////////////////////////////////////////////////////////
////////////////////////////////////////////////// eof ///
//////////////////////////////////////////////////////////
