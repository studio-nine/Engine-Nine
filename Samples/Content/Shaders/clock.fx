/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/clock.fx#1 $

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
    Align as a quad to screen according to the original tex coords --
	as a sprite, best applied to simple quads with 0-1 tex coords.
	   Then apply "Numbers.dds" as a stream of number textures to create an apparent
   numeric ocunter, driven by the vertex shader.

******************************************************************************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=self_align;";
> = 0.8;

float Speed
<
    string UIWidget = "slider";
    float UIMin = 0.5;
    float UIMax = 20.0;
    float UIStep = 0.1;
> = 4.0;

///////// Textures ///////////////

texture ImageTexture
<
    string ResourceName = "Numbers.dds"; // "default_color.dds";
    string ResourceType = "2D";
>;

sampler2D ImageSampler = sampler_state
{
    Texture = <ImageTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = None;
};

float4x4 WVxf : WORLDVIEW <string UIWidget="None";>;
float4x4 Pxf : PROJECTION <string UIWidget="None";>;

float Timer : TIME <string UIWidget="None";>;

/************* DATA STRUCTS **************/

/* data from application vertex buffer */
struct appdata {
    float3 Position    : POSITION;
    float4 UV        : TEXCOORD0;
};

/* data passed from vertex shader to pixel shader */
struct vertexOutput {
    float4 HPosition    : POSITION;
    float2 UV    		: TEXCOORD0;
};

/*********** vertex shader ******/

vertexOutput alignClockVS(appdata IN)
{
    vertexOutput OUT = (vertexOutput)0;
	float4 origin = float4(0,0,0,1);
	float4 w0 = mul(origin,WVxf);
	float2 nuv = 2*(IN.UV.xy-(float2)0.5); // flip upside-down
	float4 w1 = w0 + float4(nuv,0,0);
    OUT.HPosition = mul(w1,Pxf);
	float sec = floor(fmod(Timer*Speed,10.0))/16.0;
    OUT.UV = float2(sec+(IN.UV.x/16.0),1-IN.UV.y); // re-flips tex space
    return OUT;
}

float4 pureTexturePS(vertexOutput IN) : COLOR
{
	return tex2D(ImageSampler,IN.UV);
}

/*******************************************************************/
/************* TECHNIQUES ******************************************/
/*******************************************************************/

technique self_align <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
> {
		VertexShader = compile vs_1_1 alignClockVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_1_1 pureTexturePS();
	}
}

/***************************** eof ***/
