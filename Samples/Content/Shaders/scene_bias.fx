/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/scene_bias.fx#1 $

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
	Simple texture biasing demo/experiment

******************************************************************************/

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";
	string Script = "Technique=drawMap;";
> = 0.8; // version #

float Bias <
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 8.0;
	float UIStep = 0.1;
> = 5.0f;

float Gradient <
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 1.0;
	float UIStep = 0.01;
> = 0.5f;

////////////////////////////////////////////////

texture ColorTex <
    string ResourceName = "default_color.dds";
    string ResourceType = "2D";
>;

sampler2D ColorSampler = sampler_state {
    Texture = <ColorTex>;
    AddressU = CLAMP;
    AddressV = CLAMP;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
};

/****************************************/
/*** The dead-simple shaders: ***********/
/****************************************/

struct VS_OUTPUT {
    float4 Position : POSITION;
    float4 TexCoord0	: TEXCOORD0;
};

VS_OUTPUT VS_Quad(float3 Position : POSITION, 
				float3 TexCoord : TEXCOORD0)
{
    VS_OUTPUT OUT = (VS_OUTPUT)0;
    OUT.Position = float4(Position, 1);
	float g = Bias * lerp(1.0,TexCoord.x,Gradient);
    OUT.TexCoord0 = float4(TexCoord, g); 
    return OUT;
}

float4 drawMapPS(VS_OUTPUT IN) : COLOR
{
	float4 c = tex2Dbias(ColorSampler,IN.TexCoord0);
    return c;
}

////////////////// Technique ////////

// fill fb with previous state of paint texture
technique drawMap <
	string ScriptClass = "scene";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";
	string Script = "Pass=p0;";
> {
    pass p0 <
    	string Script = "RenderColorTarget0=;"
								"Draw=Buffer;";
	> {
		VertexShader = compile vs_1_1 VS_Quad();
		PixelShader  = compile ps_2_0 drawMapPS();
		AlphaBlendEnable = false;
		ZEnable = false;
	}
}

/////////////////////////////////////////////
///////////////////////////////////// eof ///
/////////////////////////////////////////////
