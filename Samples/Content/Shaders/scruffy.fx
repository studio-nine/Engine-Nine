/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/scruffy.fx#1 $

Copyright NVIDIA Corporation 2004
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
	Shader-driven "shells" fur with wavy scruffy strands

******************************************************************************/

float Script : STANDARDSGLOBAL
<
	string UIWidget = "none";
	string ScriptClass = "object";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";	
	// We just call a script in the main technique.
	string Script = "Technique=Fur;";
> = 0.8; // version number

////////////////////////////////////////
// UNTWEAKABLE /////////////////////////
////////////////////////////////////////

float4x4 worldViewProj : WorldViewProjection <string UIWidget = "none";>;

float shellnumber <string UIWidget = "none";>; // loop counter, hidden

// build 3d noise for "scruffiness"
#include "noise_3d.fxh"

//////////////////////////////////////
// TWEAKABLES ////////////////////////
//////////////////////////////////////

float shellcount <
	float UIStep = 1.0;
	string UIName = "# of Shells";
> = 12.0f;

float FurDistance
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 0.1;
    float UIStep = 0.00001;
    string UIName = "Fur Shell Distance";
> = .0065f;

float FurSpace
<
    string UIWidget = "slider";
    float UIMin = 0.5;
    float UIMax = 4.0;
    float UIStep = 0.1;
    string UIName = "Fur Spacing";
> = 1.0f;

float FurAlpha
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.01;
    string UIName = "Fur Per-Pass Alpha";
> = .3f;

float Scruff
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 0.08;
    float UIStep = 0.00001;
    string UIName = "Scruffiness";
> = .0065f;

float NoiseScale
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.2;
    float UIStep = 0.01;
    string UIName = "Curl Tightness";
> = 0.85f;

float3 furColor : Diffuse
<
    string UIName = "Fur Color";
> = {.1f, 0.7f, 0.2f};

//------------------------------------

texture NoiseMap 
< 
    string ResourceType = "2D"; 
    string ResourceName="furmap.dds";
    float2 Dimensions = { 256.0f, 256.0f };
>;

sampler TextureSampler = sampler_state 
{
    texture = <NoiseMap>;
    AddressU  = WRAP;        
    AddressV  = WRAP;
    AddressW  = WRAP;
    MIPFILTER = LINEAR;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};

//------------------------------------

struct vertexInput {
    float3 position				: POSITION;
    float3 normal				: NORMAL;
    float4 texCoordDiffuse		: TEXCOORD0;
};

struct vertexOutput {
    float4 HPOS		: POSITION;
    float4 UV	: TEXCOORD0;
    float3 Po	: TEXCOORD1;
};


//------------------------------------
vertexOutput VS_TransformAndTexture(vertexInput IN) 
{
    vertexOutput OUT;

	float3 P = IN.position.xyz + (IN.normal * (FurDistance * shellnumber));
	OUT.Po = NoiseScale * P;
	float grade = shellnumber/shellcount;
	OUT.UV = float4(FurSpace*(IN.texCoordDiffuse.xy),0,grade);
	OUT.HPOS = mul(float4(P, 1.0f), worldViewProj);
    
    return OUT;
}

vertexOutput VS_TransformAndTextureSetup(vertexInput IN) 
{
    vertexOutput OUT;

	float3 P = IN.position.xyz;
	OUT.Po = NoiseScale * P;
	
	OUT.UV = IN.texCoordDiffuse;
	OUT.HPOS = mul(float4(P, 1.0f), worldViewProj);
    
    return OUT;
}


//-----------------------------------
float4 PS_Textured( vertexOutput IN,
		uniform float3 furBlendColor): COLOR
{
  float4 n = IN.UV.w * Scruff * SNOISE3D(IN.Po);
  float2 addr = IN.UV.xy + n.xy;
  float4 diffuseTexture = tex2D( TextureSampler, addr );
  
  return (float4(furBlendColor.xyz, FurAlpha) * diffuseTexture);
  //return (float4(n.xyz, .3) * diffuseTexture);
}


//-----------------------------------
technique Fur
<
	string ScriptClass = "object";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";
	string Script =	"Pass=Setup;"
        	"LoopByCount=shellcount;"
        	"LoopGetIndex=shellnumber;"
	        "Pass=Shell;"
	        "LoopEnd;";
>	        
{
    pass Setup 
    <
    	string Script="Draw=Geometry;";
    >
    {		
		VertexShader = compile vs_1_1 VS_TransformAndTextureSetup();
		PixelShader  = compile ps_2_0 PS_Textured(furColor/shellcount);
		AlphaBlendEnable = true;
		SrcBlend = srcalpha;
		DestBlend = zero;

    }
    pass Shell 
    <
    	string Script="Draw=Geometry;";
    >
    {		
		VertexShader = compile vs_1_1 VS_TransformAndTexture();
		PixelShader  = compile ps_2_0 PS_Textured(furColor/shellcount);
		AlphaBlendEnable = true;
		SrcBlend = srcalpha;
		DestBlend = one;

    }
}
