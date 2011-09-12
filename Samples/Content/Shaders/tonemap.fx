/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/tonemap.fx#1 $

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
	Simple tone mapping shader
	with exposure and gamma controls
******************************************************************************/

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";
	string Script = "Technique=Main;";
> = 0.8; // version #

texture Image <
    string ResourceName = "nave.hdr";
    string ResourceType = "2D";
	string format="A16B16G16R16f";
>;

sampler2D ImageSampler = sampler_state
{
	Texture = <Image>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = None;
};

float exposure
<
    string UIWidget = "slider";
    string UIName = "Exposure";
    float UIMin = -10.0; float UIMax = 10.0; float UIStep = 0.1;
> = 0.0;

float defog
<
    string UIWidget = "slider";
    string UIName = "De-fog";
    float UIMin = 0.0; float UIMax = 0.1; float UIStep = 0.001;
> = 0.0;


float gamma
<
    string UIWidget = "slider";
    string UIName = "Gamma";
    float UIMin = 0.0; float UIMax = 1.0; float UIStep = 0.01;
> = 1.0 / 2.2;

float3 fogColor = { 1.0, 1.0, 1.0 };

//////////////////////////////

struct a2v {
    float4 position  : POSITION;
    float4 texcoord  : TEXCOORD0;
};

struct v2f {
    float4 position  : POSITION;
    float4 texcoord  : TEXCOORD0;
    // calculated values:
    float exposure   : TEXCOORD1;
};

/////////////////////////////////

v2f TonemapVS(a2v IN)
{
	v2f OUT;
	OUT.position = IN.position;
	OUT.texcoord = IN.texcoord;
	OUT.exposure = pow(2.0, exposure);
    return OUT;
}

half4 TonemapPS(v2f IN,
				uniform half3 defog,
				uniform half gamma) : COLOR
{
	half3 c = tex2D(ImageSampler, IN.texcoord);
  	c = max(0, c - defog);
    c *= IN.exposure;
    // gamma correction - could use texture lookups for this
    c = pow(c, gamma);
    return half4(c.rgb, 1.0);
}

//////////////////////////////////////////

technique Main <
	string ScriptClass = "scene";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";
	string Script = "Pass=p0;";
> {
    pass p0 <
    	string Script = "RenderColorTarget0=;"
								"Draw=Buffer;";
    > {
		cullmode = none;
		ZEnable = false;
		AlphaBlendEnable = false;
		VertexShader = compile vs_1_1 TonemapVS();
		PixelShader = compile ps_2_0 TonemapPS(defog*fogColor, gamma);
    }
}
