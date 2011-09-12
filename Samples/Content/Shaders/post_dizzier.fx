/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/post_dizzier.fx#1 $

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

******************************************************************************/

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	string Script = "Technique=Main;";
> = 0.8; // version #

float4 ClearColor <
	string UIWidget = "color";
	string UIName = "background";
> = {0,0,0,0.0};

float ClearDepth <string UIWidget = "none";> = 1.0;

#include "Quad.fxh"
#include "vertex_noise.fxh"

DECLARE_QUAD_TEX(SceneTexture,SceneSampler,"X8R8G8B8")
DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer, "D24S8")

QUAD_REAL Time : TIME <string UIWidget="None";>;

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

float Speed <
    string UIWidget = "slider";
    float UIMin = -1.0f;
    float UIMax = 20.0f;
    float UIStep = 0.01f;
> = 1.3f;

float TimeSpacing <
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 0.2f;
    float UIStep = 0.01f;
> = 0.04f;

float Shake <
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 0.1f;
    float UIStep = 0.001f;
> = 0.01f;

float Sharpness <
    string UIWidget = "slider";
    float UIMin = 0.1f;
    float UIMax = 3.0f;
    float UIStep = 0.1f;
> = 0.4f;

float2 TimeDelta = {1,.2};

//////////////////////////////////////////////////////
////////////////////////////////// vert connect //////
//////////////////////////////////////////////////////

struct QuadVertexOutput2
{
   	QUAD_REAL4 Position	: POSITION;
    QUAD_REAL4 UV		: TEXCOORD0;
    QUAD_REAL4 UV2		: TEXCOORD1;
};

//////////////////////////////////////////////////////
////////////////////////////////// pixel shaders /////
//////////////////////////////////////////////////////

QuadVertexOutput2 ShakerVS(
		float3 Position : POSITION, 
		float2 TexCoord : TEXCOORD0,
		uniform float TimeOff
) {
    QuadVertexOutput2 OUT;
    OUT.Position = float4(Position, 1);
	float4 off = float2(QuadTexOffset/(QuadScreenSize.x),QuadTexOffset/(QuadScreenSize.y)).xyxy;
    float4 dn = Speed*float4(TimeDelta,TimeDelta)*
    				   float4((Time+TimeOff*TimeSpacing).xx,
           				      (Time+(TimeOff+1)*TimeSpacing).xx);
    float4 noisePos = (float4)(0.5)+off+dn;
    float4 i = Shake*float4(vertex_noise(noisePos.xy, NTab),
							vertex_noise(noisePos.yx, NTab),
							vertex_noise(noisePos.zw, NTab),
							vertex_noise(noisePos.wz, NTab));
    i = sign(i) * pow(i,Sharpness);
    OUT.UV = TexCoord.xyxy +i;
    dn = Speed*float4(TimeDelta,TimeDelta)*
    				   float4((Time+(TimeOff+2)*TimeSpacing).xx,
           				      (Time+(TimeOff+3)*TimeSpacing).xx);
    noisePos = (float4)(0.5)+off+dn;
    i = Shake*float4(vertex_noise(noisePos.xy, NTab),
							vertex_noise(noisePos.yx, NTab),
							vertex_noise(noisePos.zw, NTab),
							vertex_noise(noisePos.wz, NTab));
    i = sign(i) * pow(i,Sharpness);
    OUT.UV2 = TexCoord.xyxy +i;
    return OUT;
}

#define C0 1.0
#define C1 2.0
#define C2 4.0
#define C3 8.0

#define CS (C0+C1+C2+C3)

#define W0 (C0/CS)
#define W1 (C1/CS)
#define W2 (C2/CS)
#define W3 (C3/CS)

QUAD_REAL4 TexQuadPS2(QuadVertexOutput2 IN) : COLOR {   
	QUAD_REAL4 texCol0 = tex2D(SceneSampler, IN.UV.xy)*W0;
	QUAD_REAL4 texCol1 = tex2D(SceneSampler, IN.UV.zw)*W1;
	QUAD_REAL4 texCol2 = tex2D(SceneSampler, IN.UV2.xy)*W2;
	QUAD_REAL4 texCol3 = tex2D(SceneSampler, IN.UV2.zw)*W3;
	return (texCol0+texCol1+texCol2+texCol3);
}  
////////////////////////////////////////////////////////////
/////////////////////////////////////// techniques /////////
////////////////////////////////////////////////////////////

technique Main <
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
		VertexShader = compile vs_2_0 ShakerVS(0);
		cullmode = none;
		ZEnable = false;
		PixelShader  = compile ps_2_0 TexQuadPS2();
    }
}

//// eof
