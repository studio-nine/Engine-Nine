/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/post_blendOverlay.fx#1 $

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
    Typical set of blend modes -- overlay a file texture.
	See "scene_blendModes" for a similar example.

******************************************************************************/

#include "Quad.fxh"

// Turn this on to get the "dissolve" technique -- a bit slow just now
// #define DISSOLVE

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "scene";
    string ScriptOrder = "postprocess";
    string ScriptOutput = "color";
	string Script = "Technique=Technique?over:base:blend"
#ifdef DISSOLVE
					":dissolve"
#endif /* DISSOLVE */
    // NO TECHNIQUES FOUND

					":average:darken:lighten:multiply:add:subtract:difference:inverseDifference"
					":exclusion:screen:colorBurn:colorDodge:overlay:softLight:hardLight";
> = 0.8;

float4 ClearColor : DIFFUSE <
	string UIName = "Background";
> = {0.3,0.3,0.3,1.0};
float ClearDepth
<
	string UIWidget = "none";
> = 1.0;

// Top-Level Compilation Switches /////////////////////////////////////

// Turn this on to provide "advanced" PS-style blend ranges
//	#define BLEND_RANGE
// Turn this on to use grayscale values for the above. Ignored if (!BLEND_RANGE)
//	#define BLEND_GRAY
// If (!BLEND_GRAY) use this channel for BLEND_RANGE calculations
//	#define BLEND_CHANNEL r
// Define this if you want to build static lookup textures for the blend-range functions
//	#define BLEND_TEX

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Data //////
///////////////////////////////////////////////////////////

DECLARE_QUAD_TEX(SceneTexture,SceneSampler,"A8R8G8B8")
DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer, "D24S8")

/////////////////////////////////////////////////////
//// Textures for Input Images //////////////////////
/////////////////////////////////////////////////////

FILE_TEXTURE_2D(BlendImg,BlendImgSampler,"Blended.dds")

/////////////////////////////////////////////////////
//// Tweakables /////////////////////////////////////
/////////////////////////////////////////////////////

float Opacity <
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 1.0;
    float uistep = 0.01;
    string UIName = "Blend Opacity";
> = 1.0;

// Blend Ranges for "Advanced Blend"
//	(these could be float4's but are individual sliders for FX Composer)

#ifdef BLEND_RANGE

#define LNORM(minv,maxv,p) ((p)-(minv))/((maxv)-(minv))

// #define CLAMP(p) min(1.0,(max(0.0,(p))))
#define CLAMP(p) (((p)<=0.0)?0:(((p)>=1)?1:(p)))

// #define CLAMPRANGE(minv,maxv,p) min(1.0,(max(0.0,LNORM(minv,maxv,p))))
#define CLAMPRANGE(minv,maxv,p) CLAMP((LNORM((minv),(maxv),(p))))

#ifdef BLEND_TEX

///////////////////////////////////////////////////////////
////// procedural textures used for blending //////////////
///////////////////////////////////////////////////////////

#define BASE_LO_MIN 0.0
#define BASE_LO_MAX 0.7
#define BASE_HI_MIN 0.9
#define BASE_HI_MAX 1.0

#define BLEND_LO_MIN 0.0
#define BLEND_LO_MAX 0.3
#define BLEND_HI_MIN 0.8
#define BLEND_HI_MAX 1.0

#define BLEND_TEX_SIZE 256

// texture functions used to fill the volume noise texture
float4 blend_1D(float2 Pos : POSITION) : COLOR
{
	float lo, hi;
	if (BLEND_LO_MIN == BLEND_LO_MAX) {
	    lo = (Pos.x >= BLEND_LO_MIN) ? 1 : 0;
	} else {
		lo = CLAMPRANGE(BLEND_LO_MIN,BLEND_LO_MAX,Pos.x);
	}
	if (BLEND_HI_MIN == BLEND_HI_MAX) {
	    hi = (Pos.x <= BLEND_HI_MIN) ? 1 : 0;
	} else {
		hi = 1 - CLAMPRANGE(BLEND_HI_MIN,BLEND_HI_MAX,Pos.x);
	}
	float4 blends = 0;
	blends.x = lo * hi;
	if (BASE_LO_MIN == BASE_LO_MAX) {
	    lo = (Pos.y >= BASE_LO_MIN) ? 1 : 0;
	} else {
		lo = CLAMPRANGE(BASE_LO_MIN,BASE_LO_MAX,Pos.y);
	}
	if (BASE_HI_MIN == BASE_HI_MAX) {
	    hi = (Pos.y <= BASE_HI_MIN) ? 1 : 0;
	} else {
		hi = 1 - CLAMPRANGE(BLEND_HI_MIN,BLEND_HI_MAX,Pos.y);
	}
 	blends.y = lo * hi;
	return blends;
}

texture BlendTex  <
    string ResourceType = "2D";
    string function = "blend_1D";
    string UIWidget = "None";
	string Format = "A16B16G16R16F";
    float2 Dimensions = { BLEND_TEX_SIZE, BLEND_TEX_SIZE };
>;

sampler BlendSampler = sampler_state 
{
    texture = <BlendTex>;
    AddressU  = CLAMP;        
    AddressV  = CLAMP;
    MIPFILTER = LINEAR;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};

#else /* ! BLEND_TEX */

float baseLoMin
<
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 1.0;
    float uistep = 0.01;
    string UIName = "Blend Opacity";
> = 0.0;

float baseLoMax
<
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 1.0;
    float uistep = 0.01;
    string UIName = "Blend Opacity";
> = 0.1;

float baseHiMin
<
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 1.0;
    float uistep = 0.01;
    string UIName = "Blend Opacity";
> = 0.9;

float baseHiMax
<
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 1.0;
    float uistep = 0.01;
    string UIName = "Blend Opacity";
> = 1.0;


float blendLoMin
<
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 1.0;
    float uistep = 0.01;
    string UIName = "Blend Opacity";
> = 0.0;

float blendLoMax
<
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 1.0;
    float uistep = 0.01;
    string UIName = "Blend Opacity";
> = 0.1;

float blendHiMin
<
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 1.0;
    float uistep = 0.01;
    string UIName = "Blend Opacity";
> = 0.9;

float blendHiMax
<
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 1.0;
    float uistep = 0.01;
    string UIName = "Blend Opacity";
> = 1.0;

#endif /* ! BLEND_TEX */
#endif /* BLEND_RANGE */

// Dissolve-Related tweakable and functions //////

#ifdef DISSOLVE
float Dissolver
<
    string UIWidget = "slider";
    float uimin = 0.0;
    float uimax = 1.0;
    float uistep = 0.01;
    string UIName = "For Dissolve";
> = 0.5;

////// procedural texture used for dissolve

#define NOISE_SIZE 256
#define NOISE_SCALE 110

// function used to fill the volume noise texture
float4 noise_2d(float2 Pos : POSITION) : COLOR
{
    float4 Noise = (float4)0;
    for (int i = 1; i < 256; i += i) {
        Noise.r += abs(noise(Pos * NOISE_SCALE * i)) / i;
        //Noise.g += abs(noise((Pos + 1.2)* NOISE_SCALE * i)) / i;
        //Noise.b += abs(noise((Pos + 2.3) * NOISE_SCALE * i)) / i;
        //Noise.a += abs(noise((Pos + 3.1) * NOISE_SCALE * i)) / i;
    }
    return Noise.rrrr;
}

texture NoiseTex  <
    string ResourceType = "2D";
    string function = "noise_2d";
    string UIWidget = "None";
    
    float2 Dimensions = { NOISE_SIZE, NOISE_SIZE };
>;

sampler NoiseSampler = sampler_state 
{
    texture = <NoiseTex>;
    AddressU  = WRAP;        
    AddressV  = WRAP;
    MIPFILTER = LINEAR;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};

#endif /* DISSOLVE */

/**************************************************************/
/********* utility pixel-shader functions and macros **********/
/**************************************************************/

#define GET_PIX float4 base = tex2D(SceneSampler, IN.UV); \
				float4 blend = tex2D(BlendImgSampler, IN.UV)

#ifdef BLEND_GRAY
float grayValue(float3 Color)
{
	return dot(Color,float3(0.25,0.65,0.1)); // just made this chroma target up - KB
}
#endif /* BLEND_GRAY */

// assume NON-pre-multiplied RGB...
float4 final_mix(float4 NewColor,float4 Base,float4 Blend)
{
    float A2 = Opacity * Blend.a;
#ifdef BLEND_RANGE
#ifdef BLEND_GRAY
	float blendV = grayValue(Blend.rgb);
	float baseV = grayValue(Base.rgb);
#else /* ! BLEND_GRAY */
	float blendV = Blend.BLEND_CHANNEL;
	float baseV = Base.BLEND_CHANNEL;
#endif /* !BLEND_GRAY */
#ifdef BLEND_TEX
	float2 bv = tex2D(BlendSampler,float2(blendV,baseV)).xy;
	// baseV = tex2D(BlendSampler,float2(baseV,0)).y;
	A2 *= (bv.x * bv.y);
#else /* !BLEND_TEX */
	blendV = 
		CLAMPRANGE(blendLoMin,blendLoMax,blendV)*
		(1-CLAMPRANGE(blendHiMin,blendHiMax,blendV));
	baseV = 
		CLAMPRANGE(baseLoMin,baseLoMax,baseV)*
		(1-CLAMPRANGE(baseHiMin,baseHiMax,baseV));
	A2 *= (blendV * baseV);
#endif /* !BLEND_TEX */
#endif /*BLEND_RANGE */
    float3 mixRGB = A2 * NewColor.rgb;
    mixRGB += ((1.0-A2) * Base.rgb);
	// float mixA = max(Base.a, A2);
	// float mixA = 1 - (1-Base.w)*(1-A2);
	return float4(mixRGB,Blend.a);	// wrong grrrrr
}

/**************************************/
/********* blend pixel shaders ********/
/**************************************/

float4 overPS(QuadVertexOutput IN) : COLOR {
    GET_PIX;
    return final_mix(blend,base,blend);
}

float4 basePS(QuadVertexOutput IN) : COLOR {
    GET_PIX;
    return base;
}

float4 blendPS(QuadVertexOutput IN) : COLOR {
    GET_PIX;
    return blend;
}

#ifdef DISSOLVE
float4 dissolvePS(QuadVertexOutput IN) : COLOR {
    GET_PIX;
    float diss = tex2D(NoiseSampler, IN.UV1).x;
    float4 newC;
    if (diss < Dissolver) {
		newC = blend;
    } else {
		newC = base;
    }
    return final_mix(newC,base,blend);
}
#endif /* DISSOLVE */

float4 averagePS(QuadVertexOutput IN) : COLOR {
    GET_PIX;
    return final_mix((blend+base)/2,base,blend);
}

float4 darkenPS(QuadVertexOutput IN) : COLOR {
    GET_PIX;
    return final_mix(min(base,blend),base,blend);
}

float4 lightenPS(QuadVertexOutput IN) : COLOR {
    GET_PIX;
    return final_mix(max(base,blend),base,blend);
}

float4 multiplyPS(QuadVertexOutput IN) : COLOR {
    GET_PIX;
    return final_mix((base*blend),base,blend);
}

float4 addPS(QuadVertexOutput IN) : COLOR {
    GET_PIX;
    return final_mix((base+blend),base,blend);
 }

float4 subtractPS(QuadVertexOutput IN) : COLOR {
    GET_PIX;
    return final_mix((base*blend),base,blend);
}

float4 differencePS(QuadVertexOutput IN) : COLOR {
    GET_PIX;
    return final_mix(abs(base*blend),base,blend);
}

float4 inverseDifferencePS(QuadVertexOutput IN) : COLOR {
    GET_PIX;
    return final_mix(1-abs(1-base-blend),base,blend);
}

float4 exclusionPS(QuadVertexOutput IN) : COLOR {
    GET_PIX;
    return final_mix(base + blend - (2*base*blend),base,blend);
}

float4 screenPS(QuadVertexOutput IN) : COLOR {
    GET_PIX;
    return final_mix(1 - (1 - base) * (1 - blend),base,blend);
}

float4 colorBurnPS(QuadVertexOutput IN) : COLOR {
    GET_PIX;
    return final_mix(1-(1-base)/blend,base,blend);
}

float4 colorDodgePS(QuadVertexOutput IN) : COLOR {
    GET_PIX;
    return final_mix( base / (1-blend), base,blend);
}

float4 overlayPS(QuadVertexOutput IN) : COLOR {
    GET_PIX;
    float4 lumCoeff = float4(0.25,0.65,0.1,0);
    float L = min(1,max(0,10*(dot(lumCoeff,base)- 0.45)));
    float4 result1 = 2 * base * blend;
    float4 result2 = 1 - 2*(1-blend)*(1-base);
    return final_mix(lerp(result1,result2,L),base,blend);
}

float4 softLightPS(QuadVertexOutput IN) : COLOR {
    GET_PIX;
    return final_mix(2*base*blend + base*base - 2*base*base*blend,base,blend);
}

float4 hardLightPS(QuadVertexOutput IN) : COLOR {
    GET_PIX;
    float4 lumCoeff = float4(0.25,0.65,0.1,0);
    float L = min(1,max(0,10*(dot(lumCoeff,blend)- 0.45)));
    float4 result1 = 2 * base * blend;
    float4 result2 = 1 - 2*(1-blend)*(1-base);
    return final_mix(lerp(result1,result2,L),base,blend);
}

/*******************************************************************/
/************* TECHNIQUES ******************************************/
/*******************************************************************/

#define DEFSCRIPT "RenderColorTarget0=SceneTexture;RenderDepthStencilTarget=DepthBuffer;ClearSetColor=ClearColor;ClearSetDepth=ClearDepth;Clear=Color;Clear=Depth;ScriptExternal=color;Pass=p0;"


// simplest techniques work in ps_1_1
#define TECH(name,psName) technique name < string Script = (DEFSCRIPT); > {\
    pass p0 <string Script="RenderColorTarget0=;RenderDepthStencilTarget=;Draw=Buffer;";> { \
		VertexShader = compile vs_1_1 ScreenQuadVS(); \
		ZEnable = false; ZWriteEnable = false; CullMode = None; \
		PixelShader = compile ps_1_1 psName (); } }

// most techniques require ps_2 for blending
#define TECH2(name,psName) technique name < string Script = (DEFSCRIPT); > { \
    pass p0 <string Script="RenderColorTarget0=;RenderDepthStencilTarget=;Draw=Buffer;";> { \
		VertexShader = compile vs_2_0 ScreenQuadVS(); \
		ZEnable = false; ZWriteEnable = false; CullMode = None; \
		PixelShader = compile ps_2_0 psName (); } }

TECH2(over,overPS)
TECH(base,basePS)
TECH(blend,blendPS)
#ifdef DISSOLVE
TECH2(dissolve,dissolvePS)
#endif /* DISSOLVE */
TECH2(average,averagePS)
TECH2(darken,darkenPS)
TECH2(lighten,lightenPS)
TECH2(multiply,multiplyPS)
TECH2(add,addPS)
TECH2(subtract,subtractPS)
TECH2(difference,differencePS)
TECH2(inverseDifference,inverseDifferencePS)
TECH2(exclusion,exclusionPS)
TECH2(screen,screenPS)
TECH2(colorBurn,colorBurnPS)
TECH2(colorDodge,colorDodgePS)
TECH2(overlay,overlayPS)
TECH2(softLight,softLightPS)
TECH2(hardLight,hardLightPS)

/***************************** eof ***/
