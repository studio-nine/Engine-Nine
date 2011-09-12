/*********************************************************************NVMH3****
Path:  
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/DiffTex.fx#1 $

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
	A simple diffuse example that shows texture positioning capabilities
	inherent in HLSL.
	$Date: 2004/09/24 $

******************************************************************************/

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

float4 SurfColor : DIFFUSE
<
    string UIName =  "Surface Color";
    string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f, 1.0f};

float RepeatS
<
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 32.0;
    float UIStep = 0.1;
> = 1.0;

float RepeatT
<
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 32.0;
    float UIStep = 0.1;
> = 1.0;

float Angle
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 360.0;
    float UIStep = 1.0;
    string UIName =  "Degrees";
> = 0.0;

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

float RotCenterS
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.01;
> = 0.5;

float RotCenterT
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.01;
> = 0.5;

/************** model textures **************/

texture colorTexture : DIFFUSE
<
    string ResourceName = "default_color.dds";
    string ResourceType = "2D";
>;

sampler2D colorTextureSampler = sampler_state
{
	Texture = <colorTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

/************** light info **************/

float4 LightPos : Position
<
    string Object = "PointLight";
    string Space = "World";
> = {100.0f, 100.0f, 100.0f, 0.0f};

float4 LightColor : Specular <
    string UIName =  "Light Color";
> = {1.0f, 1.0f, 1.0f, 1.0f};

/***********************************************/
/*** automatically-tracked "tweakables" ********/
/***********************************************/

float4x4 WorldIT : WorldInverseTranspose < string UIWidget="None"; >;
float4x4 WorldViewProj : WorldViewProjection < string UIWidget="None"; >;
float4x4 World : World < string UIWidget="None"; >;
float4x4 WorldI : WorldInverse < string UIWidget="None"; >;

/**************************************/
/***** SHARED STRUCT ******************/
/**** Data from app vertex buffer *****/
/****     for all passes          *****/
/**************************************/

struct appdata {
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;
    float4 Tangent	: TANGENT0;
    float4 Binormal	: BINORMAL0;
    float4 Normal	: NORMAL;
};

/****************************************/
/*** LIGHTING PASS - DIFFUSE ************/
/****************************************/

struct vertexOutput {
    float4 HPosition	: POSITION;
    float2 UV	: TEXCOORD0;
    float4 diffCol	: COLOR0;
};

vertexOutput DiffTexVS(appdata IN) {
    vertexOutput OUT;
    float4 objPos = float4(IN.Position.x,IN.Position.y,IN.Position.z,1.0);
    float4 worldSpacePos = mul(objPos, World);
    float4 LightVec = normalize(LightPos - worldSpacePos);
    float4 objSpaceLightVec = normalize(mul(LightVec, WorldI));
    float4 Nn = normalize(IN.Normal);
    float ldn = dot(objSpaceLightVec,Nn);
    OUT.diffCol = max(0,ldn).xxxx * SurfColor + AmbiColor;
    OUT.diffCol.w = 1.0;
    OUT.HPosition = mul(objPos, WorldViewProj);
    float a = radians(Angle);
    float ca = cos(a);
    float sa = sin(a);
    float2 off = float2(RotCenterS,RotCenterT);
    float2 nuv = IN.UV.xy - off;
    float2 ruv = float2(nuv.x*ca-nuv.y*sa,nuv.x*sa+nuv.y*ca);
    nuv = ruv + off;
    OUT.UV = float2(max(0.001,RepeatS) * nuv.x + OffsetS,
		    max(0.001,RepeatT) * nuv.y + OffsetT);
    return OUT;
}

float4 DiffTexPS(vertexOutput IN) : COLOR
{
    float4 map = tex2D(colorTextureSampler,IN.UV);
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
		VertexShader = compile vs_1_1 DiffTexVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_1_1 DiffTexPS();
    }
}

/***************************** eof ***/
