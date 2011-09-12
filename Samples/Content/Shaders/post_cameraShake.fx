/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/post_cameraShake.fx#1 $

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
	string Script = "Technique=Shake;";
> = 0.8; // version #

float4 ClearColor <
	string UIWidget = "color";
	string UIName = "background";
> = {0,0,0,1.0};
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
    float UIMax = 100.0f;
    float UIStep = 0.01f;
> = 21.f;

float Shake <
    string UIWidget = "slider";
    float UIMin = 0.0f;
    float UIMax = 1.0f;
    float UIStep = 0.01f;
> = 0.25f;

float Sharpness <
    string UIWidget = "slider";
    float UIMin = 0.1f;
    float UIMax = 3.0f;
    float UIStep = 0.1f;
> = 2.2f;

float2 TimeDelta = {1,.2};

//////////////////////////////////////////////////////
////////////////////////////////// pixel shaders /////
//////////////////////////////////////////////////////

QuadVertexOutput ShakerVS(
		float3 Position : POSITION, 
		float2 TexCoord : TEXCOORD0
) {
    QuadVertexOutput OUT;
    OUT.Position = float4(Position, 1);
    float2 dn = Speed*Time*TimeDelta;
	float2 off = float2(QuadTexOffset/(QuadScreenSize.x),QuadTexOffset/(QuadScreenSize.y));
    //float2 noisePos = TexCoord+off+dn;
    float2 noisePos = (float2)(0.5)+off+dn;
    float2 i = Shake*float2(vertex_noise(noisePos, NTab),
							vertex_noise(noisePos.yx, NTab));
    i = sign(i) * pow(i,Sharpness);
    OUT.UV = TexCoord.xy+i;
    return OUT;
}

////////////////////////////////////////////////////////////
/////////////////////////////////////// techniques /////////
////////////////////////////////////////////////////////////

technique Shake <
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
		VertexShader = compile vs_2_0 ShakerVS();
		cullmode = none;
		ZEnable = false;
		AlphaBlendEnable = false;
		PixelShader  = compile ps_2_0 TexQuadPS(SceneSampler);
    }
}
