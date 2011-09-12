/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/screentest.fx#1 $

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
    Remapping textures into screen space for single-image 2D image processing.
	Grayscale and color techniques, pretty simple but illustrative.
	Note this uses a special .x file, rather than a built-in quad.

******************************************************************************/

string XFile = "rect.x";

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?basePic:grayScale:colorize;";
> = 0.8;

/************* TWEAKABLES **************/

float3 ColorMixer
<
    string UIName =  "Weights for Grayscale Conversion";
> = { 0.3,0.59,0.11};

float3 BrightColor
<
    string UIName =  "Bkgd Tint Color";
    string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f};

float3 DarkColor
<
    string UIName =  "Colorization Black";
    string UIWidget = "Color";
> = {0.0f, 0.3f, 0.0f};

/////////////////////

texture colorTexture : DIFFUSE
<
    string ResourceName = "tiger.bmp"; // "default_color.dds";
    string ResourceType = "2D";
>;

sampler2D colorTextureSampler = sampler_state
{
    Texture = <colorTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = None;
};

/************* DATA STRUCTS **************/

/* data from application vertex buffer */
struct appdata {
    float3 Position    : POSITION;
    float4 UV        : TEXCOORD0;
};

/* data passed from vertex shader to pixel shader */
struct vertexOutput {
    float4 HPosition    : POSITION;
    float2 UV    : TEXCOORD0;
};

/*********** vertex shader ******/

vertexOutput screentestVS(appdata IN) {
    vertexOutput OUT;
    float2 nuPos = 2*(IN.UV.xyz-0.5);
    OUT.HPosition = float4(nuPos,1.0,1.0);
    OUT.UV = IN.UV.xy;
    return OUT;
}

/********* pixel shader ********/

float4  screentestPS(vertexOutput IN) : COLOR {
    return float4(BrightColor * tex2D(colorTextureSampler, IN.UV).xyz,1);
}

float4 screentestGrayPS(vertexOutput IN) : COLOR {
    float3 map = tex2D(colorTextureSampler, IN.UV).xyz;
    float  gray = dot(map,ColorMixer);
    return float4(BrightColor * gray,1);
}

float4 screentestColorizePS(vertexOutput IN) : COLOR {
    float3 map = tex2D(colorTextureSampler, IN.UV).xyz;
    float  gray = dot(map,ColorMixer);
    return float4(lerp(DarkColor,BrightColor,gray),1);
}

/*************/

technique basePic <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
> {        
		VertexShader = compile vs_1_1 screentestVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_1_1 screentestPS();
    }
}

technique grayScale <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
> {        
		VertexShader = compile vs_1_1 screentestVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_1_1 screentestGrayPS();
    }
}

technique colorize <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
> {        
		VertexShader = compile vs_1_1 screentestVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_1_1 screentestColorizePS();
    }
}

/***************************** eof ***/
