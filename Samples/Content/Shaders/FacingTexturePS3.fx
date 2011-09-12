/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/FacingTexturePS3.fx#1 $

Copyright NVIDIA Corporation 2003
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
	Just Show Me The Texture, for BOTH SIDES
	$Date: 2004/09/24 $
	$Revision: #1 $

******************************************************************************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?texColor:texAlpha;";
> = 0.8;

float3 BaseColor
<
    string UIName = "Tint";
	string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f};

texture colorMap : DIFFUSE
<
    string ResourceName = "default_color.dds";
    string ResourceType = "2D";
>;

sampler2D colorSampler = sampler_state
{
    Texture = <colorMap>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
};

/*************************************************/
/*** automatically-tracked "untweakables" ********/
/*************************************************/

float4x4 WorldViewProj : WorldViewProjection < string UIWidget="None"; >;

/**************************************/
/***** STRUCTS ************************/
/**************************************/

struct appdata {
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;
};

struct vertData {
    float4 HPosition	: POSITION;
    float2 UV		: TEXCOORD0;
};

///////////// vertex shader ///

vertData pureTexVS(appdata IN)
{
    vertData OUT;// = (vertData)0;
    float4 objPos = float4(IN.Position.x,IN.Position.y,IN.Position.z,1.0);
    float4 hpos = mul(objPos, WorldViewProj);
    OUT.UV = IN.UV.xy;
    OUT.HPosition = hpos;
    return OUT;
}

////////////////// pixel shaders ////

float4 colorPS(vertData IN,
				float Vf : VFACE)	: COLOR
{
    float3 map;
	float2 uv = IN.UV;
	if (Vf>0) uv.x = 1.0 - uv.x;
    map = tex2D(colorSampler,uv).xyz;
    return float4((map*BaseColor),1);
}

float4 alphaPS(vertData IN,
				float Vf : VFACE)	: COLOR
{
	float2 uv = IN.UV;
	if (Vf>0) uv.x = 1.0 - uv.x;
    float map = tex2D(colorSampler,uv).w;
    return float4((map*BaseColor),1);
}

/****************************************************/
/********** TECHNIQUES ******************************/
/****************************************************/

technique texColor <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
> {		
		VertexShader = compile vs_3_0 pureTexVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_3_0 colorPS();
    }
}

technique texAlpha <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
> {		
		VertexShader = compile vs_3_0 pureTexVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_3_0 alphaPS();
    }
}

/***************************** eof ***/
