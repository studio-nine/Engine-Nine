/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/post_frost.fx#1 $

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

******************************************************************************/

#include "Quad.fxh"

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	string Script = "Technique=frosted;";
> = 0.8; // version #

float4 ClearColor <
	string UIWidget = "color";
	string UIName = "background";
> = {0,0,0,0};

float ClearDepth <string UIWidget = "none";> = 1.0;


DECLARE_QUAD_TEX(SceneMap,SceneSampler,"X8R8G8B8")

DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer,"D24S8")

/************* TWEAKABLES **************/

float DeltaX <
	string UIName = "X Delta";
	string UIWidget = "slider";
	float UIMin = 0.001;
	float UIMax = 0.03;
	float UIStep = 0.0001;
> = 0.0073f;

float DeltaY <
	string UIName = "X Delta";
	string UIWidget = "slider";
	float UIMin = 0.001;
	float UIMax = 0.03;
	float UIStep = 0.0001;
> = 0.0108f;

float Freq <
	string UIName = "Frequency";
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 0.2;
	float UIStep = 0.001;
> = 0.115f;

#define NOISE_SHEET_SIZE 128
#include "noise_2d.fxh"

//////////////////// RTT
/********* pixel shader ********/

float4 spline(float x, float4 c1, float4 c2, float4 c3, float4 c4, float4 c5, float4 c6, float4 c7, float4 c8, float4 c9) {
    float w1, w2, w3, w4, w5, w6, w7, w8, w9;
    w1 = 0;
    w2 = 0;
    w3 = 0;
    w4 = 0;
    w5 = 0;
    w6 = 0;
    w7 = 0;
    w8 = 0;
    w9 = 0;
    float tmp = x * 8.0;
    if (tmp<=1.0) {
      w1 = 1.0 - tmp;
      w2 = tmp;
    }
    else if (tmp<=2.0) {
      tmp = tmp - 1.0;
      w2 = 1.0 - tmp;
      w3 = tmp;
    }
    else if (tmp<=3.0) {
      tmp = tmp - 2.0;
      w3 = 1.0-tmp;
      w4 = tmp;
    }
    else if (tmp<=4.0) {
      tmp = tmp - 3.0;
      w4 = 1.0-tmp;
      w5 = tmp;
    }
    else if (tmp<=5.0) {
      tmp = tmp - 4.0;
      w5 = 1.0-tmp;
      w6 = tmp;
    }
    else if (tmp<=6.0) {
      tmp = tmp - 5.0;
      w6 = 1.0-tmp;
      w7 = tmp;
    }
    else if (tmp<=7.0) {
      tmp = tmp - 6.0;
      w7 = 1.0 - tmp;
      w8 = tmp;
    }
    else {
      tmp = saturate(tmp - 7.0);
      w8 = 1.0-tmp;
      w9 = tmp;
    }
    return w1*c1 + w2*c2 + w3*c3 + w4*c4 + w5*c5 + w6*c6 + w7*c7 + w8*c8 + w9*c9;
}

float4 frostedPS(QuadVertexOutput IN) : COLOR {
    float2 ox = float2(DeltaX,0.0);
    float2 oy = float2(0.0,DeltaY);
    float2 PP = IN.UV - oy;
    float4 C00 = tex2D(SceneSampler,PP - ox);
    float4 C01 = tex2D(SceneSampler,PP);
    float4 C02 = tex2D(SceneSampler,PP + ox);
	   PP = IN.UV;
    float4 C10 = tex2D(SceneSampler,PP - ox);
    float4 C11 = tex2D(SceneSampler,PP);
    float4 C12 = tex2D(SceneSampler,PP + ox);
	   PP = IN.UV + oy;
    float4 C20 = tex2D(SceneSampler,PP - ox);
    float4 C21 = tex2D(SceneSampler,PP);
    float4 C22 = tex2D(SceneSampler,PP + ox);

    float n = NOISE2D(Freq*IN.UV).x;
    n = fmod(n, 0.111111)/0.111111;
    float4 result = spline(n,C00,C01,C02,C10,C11,C12,C20,C21,C22);
    // this also looks pretty cool....
    // float4 result = float4(n,n,n,1.0);
    // float4 result = lerp(C00,C22,n);
    return result;
}

/*************/

technique frosted <
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
    	string Script ="RenderColorTarget0=;"
    							"Draw=Buffer;";
    > {		
		VertexShader = compile vs_2_0 ScreenQuadVS();
		ZEnable = false;
		ZWriteEnable = false;
		CullMode = None;
		PixelShader = compile ps_2_b frostedPS();
    }
}

/***************************** eof ***/
