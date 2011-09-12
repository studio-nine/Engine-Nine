/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/EnvPassThrough.fx#1 $

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
	For use in backgrounds
	$Date: 2004/09/24 $
	$Revision: #1 $

******************************************************************************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=passthru;";
> = 0.8;

/************* TWEAKABLES **************/

float4 AmbiColor : Ambient
<
    string UIName =  "Ambient Light Color";
> = {1.0f, 1.0f, 1.0f, 1.0f};

/////////

texture cubeMap : ENVIRONMENT
<
    string ResourceName = "nvlobby_cube_mipmap.dds";
    string ResourceType = "Cube";
>;

samplerCUBE cubeSampler = sampler_state
{
    Texture = <cubeMap>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = clamp;
    AddressV = clamp;
    AddressW = clamp;
};

/***********************************************/
/*** automatically-tracked "tweakables" ********/
/***********************************************/

float4x4 WorldIT : WorldInverseTranspose < string UIWidget="None"; >;
float4x4 WorldViewProj : WorldViewProjection < string UIWidget="None"; >;
float4x4 World : World < string UIWidget="None"; >;
float4x4 ViewI : ViewInverse < string UIWidget="None"; >;

/**************************************/
/***** SHARED STRUCT ******************/
/**** Data from app vertex buffer *****/
/****     for all passes          *****/
/**************************************/

struct appdata {
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;
    // float4 Tangent	: TANGENT;
    // float4 Binormal	: BINORMAL;
    float4 Normal	: NORMAL;
};

/****************************************/
/*** REFLECTIONS, AMBIENT, and Z PASS ***/
/****************************************/

// vertex->fragment registers used for this pass only
struct passThruVertexData {
    float4 HPosition	: POSITION;
    float3 ViewVec	: TEXCOORD0;
    float3 wNormal	: TEXCOORD1;
};

passThruVertexData passThruVS(appdata IN)
{
    passThruVertexData OUT;
    float3 Nw = mul(IN.Normal, WorldIT).xyz;
    // OUT.wNormal = normalize(Nw);
    float4 objPos = float4(IN.Position.x,IN.Position.y,IN.Position.z,1.0);
    float4 hpos = mul(objPos, WorldViewProj);
    float4 wpos = mul(objPos, World);
    OUT.ViewVec = wpos.xyz - ViewI[3].xyz;
    OUT.wNormal = hpos.xyz;
    OUT.HPosition = hpos;
    return OUT;
}

float4 passThruPS(passThruVertexData IN)	: COLOR
{
    float4 env = texCUBE(cubeSampler,IN.ViewVec);
    float4 pixelColor = AmbiColor * env;
    return pixelColor;
}

/****************************************************/
/********** TECHNIQUES ******************************/
/****************************************************/

technique passthru <
	string Script = "Pass=envPass;";
> {
    // shared lighting: ambient, environment, and Z
    pass envPass  <
		string Script = "Draw=geometry;";
    > {		
		VertexShader = compile vs_1_1 passThruVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_1_1 passThruPS();
    }
}

/***************************** eof ***/
