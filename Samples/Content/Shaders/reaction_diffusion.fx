/*********************************************************************NVMH3****
File:  $Id: //sw/devrel/SDK/MEDIA/HLSL/scene_ripplePaint.fx#1 $

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

******************************************************************************/

//#define TWEAKABLE_TEXEL_OFFSET 
#ifndef QUAD_REAL
#define QUAD_REAL float
#define QUAD_REAL2 float2
#define QUAD_REAL3 float3
#define QUAD_REAL4 float4
#define QUAD_REAL3x3 float3x3
#define QUAD_REAL4x3 float4x3
#define QUAD_REAL3x4 float3x4
#define QUAD_REAL4x4 float4x4
#endif /* QUAD_REAL */

#include "Quad.fxh"

float Script : STANDARDSGLOBAL
<
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";
	// We just call a script in the main technique.
	string Script = "Technique=reaction_diffusion;";
> = 0.8;

bool bReset : FXCOMPOSER_RESETPULSE
<
	string UIName="Clear Canvas";
>;

bool Painting 
<
	string UIName="Painting Now?";
> = true;

float iterationsPerFrame
<
	string UIName = "Iterations Per Frame";
	string UIWidget = "slider";
	float UIMin = 0;
	float UIMax = 100;
	float UIStep = 1;
> = 10;

float BrushSize <
	string UIWidget = "slider";
	float UIMin = 0.001;
	float UIMax = 0.15;
	float UIStep = 0.001;
	string UIName = "Brush Size";
> = 0.045f;

float4 ClearColor = {0.0,0.0,0.0,0.0};

float4 MouseL : LEFTMOUSEDOWN < string UIWidget="None"; >;
float3 MousePos : MOUSEPOSITION < string UIWidget="None"; >;
float Timer : TIME < string UIWidget = "None"; >;

/////////// R-D params
float rd_K 
<
	string UIWidget = "slider";
	string UIName = "K";
	float UIMin = 0.0;
	float UIMax = 1.0;
	float UIStep = 0.001;
> = 0.052f;

float rd_F
<
	string UIWidget = "slider";
	string UIName = "f";
	float UIMin = 0.0;
	float UIMax = 1.0;
	float UIStep = 0.001;
> = 0.012f;

float rd_Du
<
	string UIWidget = "slider";
	string UIName = "Du";
	float UIMin = 0.0;
	float UIMax = 1.0;
	float UIStep = 0.0001;
> = 0.0004f;

float rd_Dv
<
	string UIWidget = "slider";
	string UIName = "Dv";
	float UIMin = 0.0;
	float UIMax = 1.0;
	float UIStep = 0.0001;
> = 0.0002f;

////////////////////////////////////////////////////
/// Textures ///////////////////////////////////////
////////////////////////////////////////////////////

DECLARE_QUAD_TEX(PaintTex,PaintSamp,"A8B8G8R8")

texture RDTex0 : RENDERCOLORTARGET 
<
	float2 Dimensions = { 64, 64 }; 
    string Format = "A32B32G32R32F";
	string UIWidget = "None";
	int miplevels = 1;
>;

texture RDTex1 : RENDERCOLORTARGET 
<
	float2 Dimensions = { 64, 64 }; 
    string Format = "A32B32G32R32F";
	string UIWidget = "None";
	int miplevels = 1;
>;

sampler RDSamp0 = sampler_state 
{
    texture = <RDTex0>;
    AddressU  = CLAMP;
    AddressV  = CLAMP;
    MipFilter = POINT;
    MinFilter = POINT;
    MagFilter = POINT;
};

sampler RDSamp1 = sampler_state 
{
    texture = <RDTex1>;
    AddressU  = CLAMP;
    AddressV  = CLAMP;
    MipFilter = POINT;
    MinFilter = POINT;
    MagFilter = POINT;
};

sampler RDSamp0Wrap = sampler_state 
{
    texture = <RDTex0>;
    AddressU  = WRAP;
    AddressV  = WRAP;
    MipFilter = POINT;
    MinFilter = POINT;
    MagFilter = POINT;
};

sampler RDSamp1Wrap = sampler_state 
{
    texture = <RDTex1>;
    AddressU  = WRAP;
    AddressV  = WRAP;
    MipFilter = POINT;
    MinFilter = POINT;
    MagFilter = POINT;
};


texture SeedTexture <
	string ResourceName = "rd_seed.png";
	string TextureType = "2D";
	string UIName = "Original Image";
>;

sampler2D SeedSampler = sampler_state
{
	Texture = <SeedTexture>;
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Point;
	AddressU = CLAMP;
	AddressV = CLAMP;
};

/****************************************/
/*** The shaders: ***********************/
/****************************************/

// VM functions

QUAD_REAL2 sumNeighborsWrap(sampler2D tex, QUAD_REAL2 coords)
{
	QUAD_REAL2 pixelSize = 1.0f / QuadScreenSize.xy;

  	QUAD_REAL2 sum;
  	sum  = tex2D(tex, coords + QUAD_REAL2(pixelSize.x, 0)).xy;
  	sum += tex2D(tex, coords + QUAD_REAL2(-pixelSize.x, 0)).xy;
    sum += tex2D(tex, coords + QUAD_REAL2(0, pixelSize.y)).xy;
  	sum += tex2D(tex, coords + QUAD_REAL2(0, -pixelSize.y)).xy;
  	
  	return sum;
}

QUAD_REAL2 sumNeighborsNoWrap(sampler2D tex, QUAD_REAL2 coords)
{
  	QUAD_REAL2 pixelSize = 1.0 / QuadScreenSize.xy;
  	QUAD_REAL2 uv = coords;
	
  	QUAD_REAL2 sum;
  	
  	uv.x = (coords.x > 1.0 - pixelSize.x) ? 0.0  : coords.x + pixelSize.x;
  	sum = tex2D(tex, uv).xy;
  	uv.x = (coords.x <= pixelSize.x) ? 1.0 - pixelSize.x : coords.x - pixelSize.x;
  	sum += tex2D(tex, uv).xy;
  	uv = coords;
  	uv.y = (coords.y <= pixelSize.y) ? 1.0 - pixelSize.y : coords.y - pixelSize.y;
  	sum += tex2D(tex, uv);
  	uv.y = (coords.y > 1.0 - pixelSize.y) ? 0.0 : coords.y + pixelSize.y;
  	sum += tex2D(tex, uv);
  	
  	return sum;
}

QUAD_REAL4 reaction_diffusionPS(QuadVertexOutput IN,
  	 				    uniform sampler2D concentration) : COLOR
{
  	// Setup the texture coordinate pairs and sample the center and its 4 nearest
  	// neighbors.  Note that since the texture rectangles do not support texture 
  	// repeat wrapping, we have to detect the edge fragments and do wrapping on 
  	// our own.  The conditional expressions below take care of this.  The 
  	// neighbor samples are simply averaged to compute (part of) the Laplacian, 
  	// and then divided scaled by the diffusion coefficient
 
	float2 dudv = 655.36f * float2(rd_Du, rd_Dv);    
  	
  	QUAD_REAL2 centerSample = tex2D(concentration, IN.UV.xy).xy;	
  	QUAD_REAL2 diffusion;  

//#define WRAP
#ifdef WRAP
	diffusion = sumNeighborsWrap(concentration, IN.UV.xy);
#else
	diffusion = sumNeighborsNoWrap(concentration, IN.UV.xy);
#endif
  
  // Scale the sum by 1/4 to get the average, 
  // then multiply by the diffusion coeffs.
  diffusion *= (0.25f * dudv);
    
  // The reaction operates only on the center sample.  Different computations 
  // are performed on the x and y channels because they represent different 
  // chemical concentrations, and are governed by two different PDEs.
  float2 reaction = centerSample.xx * centerSample.yy * centerSample.yy;
  reaction.x *= -1.0f;
  
  reaction.x += (1.0f - dudv.x) * centerSample.x + 
                rd_F * (1.0f - centerSample.x);
  reaction.y += centerSample.y * (-rd_K - rd_F + (1 - dudv.y));

  // Now add the diffusion to the reaction to get the result.
  return float4(diffusion + reaction, 0, 0);
}

QUAD_REAL4 strokePS(QuadVertexOutput IN) : COLOR
{
    QUAD_REAL2 delta = IN.UV.xy-MousePos.xy;

	QUAD_REAL dl = length(delta);
    QUAD_REAL dd = MouseL.z*(dl < BrushSize);//max(((BrushSize-dl)/BrushSize),0);

    dd *= Painting;
    return QUAD_REAL4(0, dd, 0, 1);
}

QUAD_REAL4 seedPaint(QuadVertexOutput IN) : COLOR
{
	return tex2D(RDSamp1, IN.UV.xy) + tex2D(PaintSamp, IN.UV.xy);
}

///

////////////////// Technique ////////

technique reaction_diffusion 
<
	string Script =
		    "LoopByCount=bReset;"
		    	"Pass=reset;"
		    "LoopEnd=;"
		    "LoopByCount=iterationsPerFrame;"
        		"Pass=simulate01;"
        		"Pass=simulate10;"
        	"LoopEnd=;"
        	"Pass=paint;"
        	"Pass=seed;"
        	"Pass=clearSeed;"
        	"Pass=display;";
        	
> {
	pass simulate01 
	<
		string Script = 
	        "RenderColorTarget0=RDTex1;"
	        "RenderDepthStencilTarget=;"
	        "Draw=Buffer;";
	> {
		VertexShader = compile vs_1_1 ScreenQuadVS();
		PixelShader  = compile ps_2_a reaction_diffusionPS(RDSamp0);
		AlphaBlendEnable = false;
		ZEnable = false;
	}
	pass simulate10 
	<
		string Script = 
	        "RenderColorTarget0=RDTex0;"
	        "RenderDepthStencilTarget=;"
	        "Draw=Buffer;";
	> {
		VertexShader = compile vs_1_1 ScreenQuadVS();
		PixelShader  = compile ps_2_a reaction_diffusionPS(RDSamp1);
		AlphaBlendEnable = false;
		ZEnable = false;
	}
	pass display 
	<
		string Script = 
	        "RenderColorTarget0=;"
	        "RenderDepthStencilTarget=;"
	        "Draw=Buffer;";
	> {
		VertexShader = compile vs_1_1 ScreenQuadVS();
		PixelShader  = compile ps_2_a TexQuadPS(RDSamp0);
		AlphaBlendEnable = false;
		ZEnable = false;
	}
	pass reset 
	<
		string Script = 
	        "RenderColorTarget0=RDTex0;"
	        "RenderDepthStencilTarget=;"
	        "Draw=Buffer;";
	> {
		VertexShader = compile vs_1_1 ScreenQuadVS();
		PixelShader  = compile ps_2_a TexQuadPS(SeedSampler);
		AlphaBlendEnable = false;
		ZEnable = false;
	}
	pass paint <
		string Script = 
	        "RenderColorTarget0=PaintTex;"
	        "RenderDepthStencilTarget=;"
    					"LoopByCount=bReset;"
    					"ClearSetColor=ClearColor;"
    					"Clear=Color0;"
     					"LoopEnd=;"
	        "Draw=Buffer;";
	> {
		VertexShader = compile vs_1_1 ScreenQuadVS();
		PixelShader  = compile ps_2_0 strokePS();
		AlphaBlendEnable = true;
		SrcBlend = One;
		DestBlend = One;
		ZEnable = false;
	}
	pass seed <
		string Script = 
	        "RenderColorTarget0=RDTex0;"
	        "RenderDepthStencilTarget=;"
    					"LoopByCount=bReset;"
    					"ClearSetColor=ClearColor;"
    					"Clear=Color0;"
     					"LoopEnd=;"
	        "Draw=Buffer;";
	> {
		VertexShader = compile vs_1_1 ScreenQuadVS();
		PixelShader  = compile ps_2_0 seedPaint();
		AlphaBlendEnable = true;
		SrcBlend = One;
		DestBlend = One;
		ZEnable = false;
	}
	pass clearSeed <
		string Script =
		    "RenderColorTarget0=PaintTex;"
	        "RenderDepthStencilTarget=;"
    		"ClearSetColor=ClearColor;"
    		"Clear=Color0;";
	>
	{
	}

}

/////////////////////////////////////////////
///////////////////////////////////// eof ///
/////////////////////////////////////////////
