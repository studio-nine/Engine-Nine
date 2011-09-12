/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/Aniso2003.fx#1 $

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
    Revised version makes its own anisotropy map, compatible with
    	FX Composer and EffectEdit
	Only needs DX8 hardware

******************************************************************************/

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "object";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";
	string Script = "Technique=Aniso;";
> = 0.8; // version #

// string XFile = "bigship1.x";	// special for EffectEdit
// int    BCLR  = 0xff202080;    // special for EffectEdit

// UN-TWEAKABLES /////////////

float4x4 WorldITXf : WorldInverseTranspose <
    string UIWidget = "None";
> = {1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1};
float4x4 WvpXf : WorldViewProjection < string UIWidget = "None"; >;
float4x4 WorldXf : World < string UIWidget = "None"; >;
float4x4 ViewInvXf : ViewInverse < string UIWidget = "None"; >;

// TWEAKABLES /////////////

float4 LightPos : Position
<
    string Object = "Pointlight";
    string Space = "World";
> = {-20.0f, 20.0f, -10.0f, 0.0f};

//// 2D Texture used for Aniso pre-calculation ////

#define TEX_SIZE 256

texture anisoTexture <
    string function = "aniso_vals";
    string UIWidget = "None";
    float2 Dimensions = { TEX_SIZE, TEX_SIZE };
>;

sampler2D AnisoSampler = sampler_state
{
    Texture = <anisoTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = None;
    AddressU = Clamp;
    AddressV = Clamp;
};

// texture-generation shader -- try changing this for varying looks!


#define ANISO_EXPON 12.0
#define ANISO_STRENGTH 0.8

float4 aniso_vals(float2 Pos : POSITION,float ps : PSIZE) : COLOR
{
    // S axis: renormalized N.L
    // T axis: renormalized N.H
    float sv = ANISO_STRENGTH*pow(max(0,Pos.x),ANISO_EXPON);
    float tv = max(0,(Pos.y));
    // return float4(sv.xxxx);
    return float4(sv*lerp(float3(1,1,1),float3(1,.7,.5),tv),1);
}

/////////// structs //////////////

struct appdata {
    float3 Position : POSITION;
    float4 Normal : NORMAL;
};

struct vpconn {
    float4 HPosition : POSITION;
    float2 TexCoord0 : TEXCOORD0;
};

/////// vertex shader does all the work

vpconn anisoVS(appdata IN)
{
    vpconn OUT;
    float3 Nn = normalize(mul(IN.Normal, WorldITXf).xyz);
    float4 Po = float4(IN.Position.xyz,1.0);    // obj coords
    float3 Pw = mul(Po, WorldXf).xyz; // world coords
    float3 Vn = normalize(ViewInvXf[3].xyz - Pw);
    float3 Ln = normalize(LightPos - Pw);
    float3 Hn = normalize(Vn + Ln);
    OUT.TexCoord0 = float2(dot(Ln, Nn), dot(Hn, Nn));
    OUT.HPosition = mul(Po, WvpXf);
    return OUT;
}

/////////// technique (no pixel shader needed)

technique Aniso <
	string Script = "Pass=p0;";
> {
    pass p0 <
		string Script = "Draw=Geometry";
    > {
		VertexShader = compile vs_1_1 anisoVS();
		ZEnable = true;
		ZWriteEnable = true;
		AlphaBlendEnable = false;
		CullMode = None;
		Lighting = false;
		Texture[0] = <anisoTexture>;
		TexCoordIndex[0] = 0;
		MinFilter[0] = Linear;
		MagFilter[0] = Linear;
		MipFilter[0] = None;
		AddressU[0] = Clamp;
		AddressV[0] = Clamp;
		ColorOp[0] = Modulate4x;
		ColorArg1[0] = Texture;
		ColorArg2[0] = Texture | AlphaReplicate;
    }
}

// eof
