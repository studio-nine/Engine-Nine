/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/post_colorMatrix.fx#1 $

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

Simple color correction controls using a color matrix, as seen in the "Toys" demo
http://www.sgi.com/grafica/matrix/

******************************************************************************/

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	string Script = "Technique=ColorControls;";
> = 0.8; // version #

float4 ClearColor <
	string UIWidget = "color";
	string UIName = "background";
> = {0.5,0.5,0.5,0.0};

float ClearDepth <string UIWidget = "none";> = 1.0;

#include "Quad.fxh"

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

float Brightness <
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 5.0f;
    float UIStep = 0.01f;
> = 1.0f;

float Contrast <
    string UIWidget = "slider";
    float UIMin = -5.0f;
    float UIMax = 5.0f;
    float UIStep = 0.01f;
> = 1.0f;

float Saturation <
    string UIWidget = "slider";
    float UIMin = -5.0f;
    float UIMax = 5.0f;
    float UIStep = 0.01f;
> = 1.0f;

float Hue <
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 360.0f;
    float UIStep = 1.0f;
> = 0.0f;

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Data //////
///////////////////////////////////////////////////////////

DECLARE_QUAD_TEX(SceneTexture,SceneSampler,"X8R8G8B8")
DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer, "D24S8")

///////////////////////////////////////////////////////////
/////////////////////////////////// data structures ///////
///////////////////////////////////////////////////////////

struct VS_OUTPUT
{
   	float4 Position      : POSITION;
    float2 TexCoord0     : TEXCOORD0;
    float4x3 colorMatrix : TEXCOORD1;
};

////////////////////////////////////////////////////////////
////////////////////////////////// vertex shaders //////////
////////////////////////////////////////////////////////////

float4x4 scaleMat(float s)
{
	return float4x4(
		s, 0, 0, 0,
		0, s, 0, 0,
		0, 0, s, 0,
		0, 0, 0, 1);
}

float4x4 translateMat(float3 t)
{
	return float4x4(
		1, 0, 0, 0,
		0, 1, 0, 0,
		0, 0, 1, 0,
		t, 1);
}

float4x4 rotateMat(float3 d, float ang)
{
	float s = sin(ang);
	float c = cos(ang);
	d = normalize(d);
	return float4x4(
		d.x*d.x*(1 - c) + c,		d.x*d.y*(1 - c) - d.z*s,	d.x*d.z*(1 - c) + d.y*s,	0,
		d.x*d.y*(1 - c) + d.z*s,	d.y*d.y*(1 - c) + c,		d.y*d.z*(1 - c) - d.x*s,	0, 
		d.x*d.z*(1 - c) - d.y*s,	d.y*d.z*(1 - c) + d.x*s,	d.z*d.z*(1 - c) + c,		0, 
		0, 0, 0, 1 );
}

VS_OUTPUT colorControlsVS(float4 Position : POSITION, 
				 		  float2 TexCoord : TEXCOORD0)
{
    VS_OUTPUT OUT = (VS_OUTPUT)0;
    OUT.Position = Position;
    float2 texelSize = 1.0 / QuadScreenSize;
    OUT.TexCoord0 = TexCoord + texelSize*0.5;	// match texels to pixels
    
    // construct color matrix
    // note - in a real application this would all be done on the CPU
    
    // brightness - scale around (0.0, 0.0, 0.0)
    float4x4 brightnessMatrix = scaleMat(Brightness);
	
 	// contrast - scale around (0.5, 0.5, 0.5)
    float4x4 contrastMatrix = translateMat(-0.5);
    contrastMatrix = mul(contrastMatrix, scaleMat(Contrast) );
    contrastMatrix = mul(contrastMatrix, translateMat(0.5) );
	
    // saturation
    // weights to convert linear RGB values to luminance
    const float rwgt = 0.3086;
    const float gwgt = 0.6094;
    const float bwgt = 0.0820;
    float s = Saturation;
    float4x4 saturationMatrix = float4x4(
		(1.0-s)*rwgt + s,	(1.0-s)*rwgt,   	(1.0-s)*rwgt,		0,
		(1.0-s)*gwgt, 		(1.0-s)*gwgt + s, 	(1.0-s)*gwgt,		0,
		(1.0-s)*bwgt,    	(1.0-s)*bwgt,  		(1.0-s)*bwgt + s,	0,
		0.0, 0.0, 0.0, 1.0);

	// hue - rotate around (1, 1, 1)
	float4x4 hueMatrix = rotateMat(float3(1, 1, 1), radians(Hue));
	
//	OUT.colorMatrix = brightnessMatrix;
//	OUT.colorMatrix = contrastMatrix;
//	OUT.colorMatrix = saturationMatrix;
//	OUT.colorMatrix = hueMatrix;

	// composite together matrices
	float4x4 m;
	m = brightnessMatrix;
	m = mul(m, contrastMatrix);
	m = mul(m, saturationMatrix);
	m = mul(m, hueMatrix);
	OUT.colorMatrix = m;
    return OUT;
}

//////////////////////////////////////////////////////
////////////////////////////////// pixel shaders /////
//////////////////////////////////////////////////////

half4 colorControlsPS(VS_OUTPUT IN) : COLOR
{   
	half4 scnColor = tex2D(SceneSampler, IN.TexCoord0);
	half4 c;
	// this compiles to 3 dot products:
	c.rgb = mul(half4(scnColor.rgb, 1), (half4x3) IN.colorMatrix);
	c.a = scnColor.a;
	return c;
}  

////////////////////////////////////////////////////////////
/////////////////////////////////////// techniques /////////
////////////////////////////////////////////////////////////

technique ColorControls <
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	string Script =
			"RenderColorTarget0=SceneTexture;"
	        "RenderDepthStencilTarget=DepthBuffer;"
	        	"ClearSetColor=ClearColor;"
	        	"ClearSetDepth=ClearDepth;"
				"Clear=Color;"
				"Clear=Depth;"
	        	"ScriptExternal=color;"
        	"Pass=p0;";
> {
    pass p0 <
    	string Script = "RenderColorTarget0=;"
								"Draw=Buffer;";
    > {
		cullmode = none;
		ZEnable = false;
		AlphaBlendEnable = false;
		VertexShader = compile vs_2_0 colorControlsVS();
		PixelShader  = compile ps_2_0 colorControlsPS();
    }
}

//////////////////////////////// eof ///
