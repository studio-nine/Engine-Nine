/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/DiffNoise.fx#1 $

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
	A way to look at noise textures.
	$Date: 2004/09/24 $

******************************************************************************/

#define PROCEDURAL_NOISE

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=ps11;";
> = 0.8;

/************* TWEAKABLES **************/

float4 AmbiColor : Ambient
<
    string UIName =  "Ambient Light Color";
    string UIWidget = "Color";
> = {0.2f, 0.2f, 0.2f, 1.0f};

float4 SurfColor  : Diffuse
<
    string UIName =  "Surface Color";
    string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f, 1.0f};

float RepeatS
<
    string UIWidget = "slider";
    float UIMin = 0.01;
    float UIMax = 32.0;
    float UIStep = 0.01;
> = 1.0;

float RepeatT
<
    string UIWidget = "slider";
    float UIMin = 0.01;
    float UIMax = 32.0;
    float UIStep = 0.01;
> = 1.0;

float RepeatR
<
    string UIWidget = "slider";
    float UIMin = 0.01;
    float UIMax = 32.0;
    float UIStep = 0.01;
> = 1.0;

float OffsetS
<
    string UIWidget = "slider";
    float UIMin = -10.0;
    float UIMax = 10.0;
    float UIStep = 0.01;
> = 0.0;

float OffsetT
<
    string UIWidget = "slider";
    float UIMin = -10.0;
    float UIMax = 10.0;
    float UIStep = 0.01;
> = 0.0;

float OffsetR
<
    string UIWidget = "slider";
    float UIMin = -10.0;
    float UIMax = 10.0;
    float UIStep = 0.01;
> = 0.0;

/************** light info **************/

float4 LightPos : Position
<
    string Object = "PointLight";
    string Space = "World";
> = {100.0f, 100.0f, 100.0f, 0.0f};

float4 LightColor <
    string UIName =  "Light Color";
    string UIWidget =  "Color";
> = {0.8f, 0.8f, 1.0f, 1.0f};

/************** model textures **************/

#ifdef PROCEDURAL_NOISE

#define NOISE_VOLUME_SIZE 16
#include "noise_3d.fxh"

#else /* !PROCEDURAL_NOISE */

texture NoiseTex
<
    string ResourceName = "noiseL8_32x32x32.dds";
    string ResourceType = "3D";
>;

sampler3D NoiseSamp = sampler_state
{
	Texture = <NoiseTex>;
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = None;
};

#endif /* !PROCEDURAL_NOISE */

/***********************************************/
/*** automatically-tracked "tweakables" ********/
/***********************************************/

float4x4 WorldIT : WorldInverseTranspose < string UIWidget="None"; >;
float4x4 WorldViewProj : WorldViewProjection < string UIWidget="None"; >;
float4x4 World : World < string UIWidget="None"; >;
float4x4 WorldI : WorldInverse < string UIWidget="None"; >;

/**************************************/
/**************************************/
/**************************************/

struct appdata {
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;
    float4 Tangent	: TANGENT0;
    float4 Binormal	: BINORMAL0;
    float4 Normal	: NORMAL;
};

struct vertexOutput {
    float4 HPosition	: POSITION;
    float3 STR	: TEXCOORD0;
    float4 diffCol	: COLOR0;
};

/****************************************/
/*** LIGHTING PASS - DIFFUSE ************/
/****************************************/

vertexOutput DiffNoiseVS(appdata IN)
{
    vertexOutput OUT;
    float4 objPos = float4(IN.Position.x,IN.Position.y,IN.Position.z,1.0);
    float4 worldSpacePos = mul(objPos, World);
    float4 LightVec = normalize(LightPos - worldSpacePos);
    float4 objSpaceLightVec = normalize(mul(LightVec, WorldI));
    float4 Nn = normalize(IN.Normal);
    float ldn = dot(objSpaceLightVec,Nn);
    OUT.diffCol = max(0,ldn).x * LightColor * SurfColor + AmbiColor;
    OUT.diffCol.w = 1.0;
    OUT.HPosition = mul(objPos, WorldViewProj);
    OUT.STR = float3(max(0.001,RepeatS) * worldSpacePos.x + OffsetS,
		    max(0.001,RepeatT) * worldSpacePos.y + OffsetT,
		    max(0.001,RepeatR) * worldSpacePos.z + OffsetR);
    return OUT;
}

float4 DiffNoisePS(vertexOutput IN) : COLOR
{
    float4 map = tex3D(NoiseSamp,IN.STR);
    float4 final = (IN.diffCol * map);
    return final;
}

/****************************************************/
/********** TECHNIQUES ******************************/
/****************************************************/

technique ps11 <
	string Script = "Pass=p1d;";
> {
    pass p1d <
		string Script = "Draw=geometry;";
    > {		
	VertexShader = compile vs_1_1 DiffNoiseVS();
	ZEnable = true;
	ZWriteEnable = true;
	CullMode = None;
	PixelShader = compile ps_1_1 DiffNoisePS();
    }
}

/***************************** eof ***/
