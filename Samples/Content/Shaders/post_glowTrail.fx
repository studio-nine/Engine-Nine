/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/post_glowTrail.fx#1 $

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
	Persistence of blurry vision.

******************************************************************************/

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	string Script = "Technique=Main;";
> = 0.8; // version #

bool bReset : FXCOMPOSER_RESETPULSE
<
	string UIName="Reset";
	string UIWidget = "none";
>;

float4 ClearColor <
	string UIWidget = "color";
	string UIName = "background";
> = {0,0,0,0.0};

float ClearDepth <string UIWidget = "none";> = 1.0;

#include "Quad.fxh"

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////
 
half Glowness <
    string UIName = "Glow Edge";
    string UIWidget = "slider";
    half UIMin = 0.0f;
    half UIMax = 3.0f;
    half UIStep = 0.1f;
> = 3.0f;

half GlowBright <
    string UIName = "Glow Overlay";
    string UIWidget = "slider";
    half UIMin = 0.0f;
    half UIMax = 2.0f;
    half UIStep = 0.1f;
> = 0.8f;

half SceneBright <
    string UIName = "Underlying Brightness";
    string UIWidget = "slider";
    half UIMin = 0.0f;
    half UIMax = 1.0f;
    half UIStep = 0.1f;
> = 0.25f;

half TrailDrop <
    string UIName = "Trail Brightness";
    string UIWidget = "slider";
    half UIMin = 0.0f;
    half UIMax = 1.0f;
    half UIStep = 0.01f;
> = 0.75f;

half Cutoff <
    string UIName = "Glow Cutoff";
    string UIWidget = "slider";
    half UIMin = 0.0f;
    half UIMax = 0.99f;
    half UIStep = 0.01f;
> = 0.8f;

///////////////////////////////////////////////////////////
///////////////////////////// Render-to-Texture Data //////
///////////////////////////////////////////////////////////

half NPixels <
    string UIName = "Pixels Steps";
    string UIWidget = "slider";
    half UIMin = 1.0f;
    half UIMax = 5.0f;
    half UIStep = 0.5f;
> = 1.0f;

DECLARE_QUAD_TEX(SceneTexture,SceneSampler,"A8R8G8B8")
DECLARE_QUAD_TEX(HBlurMap,HBlurSampler,"A8R8G8B8")
DECLARE_QUAD_TEX(FinalBlurMap,FinalBlurSampler,"A8R8G8B8")
DECLARE_QUAD_TEX(CompositeMap,CompositeSampler,"A8R8G8B8")
DECLARE_QUAD_TEX(BlendMap,BlendSampler,"A8R8G8B8")

DECLARE_QUAD_DEPTH_BUFFER(DepthBuffer, "D24S8")

///////////////////////////////////////////////////////////
/////////////////////////////////// data structures ///////
///////////////////////////////////////////////////////////

struct VS_OUTPUT_BLUR
{
    half4 Position   : POSITION;
    half4 TexCoord0   : TEXCOORD0;
    half4 TexCoord1   : TEXCOORD1;
    half4 TexCoord2   : TEXCOORD2;
    half4 TexCoord3   : TEXCOORD3;
    half4 TexCoord4   : TEXCOORD4;
    half4 TexCoord5   : TEXCOORD5;
    half4 TexCoord6   : TEXCOORD6;
    half4 TexCoord7   : TEXCOORD7;
    half4 TexCoord8   : COLOR1;   
};

////////////////////////////////////////////////////////////
////////////////////////////////// vertex shaders //////////
////////////////////////////////////////////////////////////

VS_OUTPUT_BLUR VS_Quad_Vertical_9tap(half3 Position : POSITION, 
			half3 TexCoord : TEXCOORD0)
{
    VS_OUTPUT_BLUR OUT = (VS_OUTPUT_BLUR)0;
    OUT.Position = half4(Position, 1);
	half texelIncrement = NPixels/QuadScreenSize.y;
    half3 Coord = half3(TexCoord.xy, 1);
    OUT.TexCoord0 = half4(Coord.x, Coord.y + texelIncrement, TexCoord.z, 1);
    OUT.TexCoord1 = half4(Coord.x, Coord.y + texelIncrement * 2, TexCoord.z, 1);
    OUT.TexCoord2 = half4(Coord.x, Coord.y + texelIncrement * 3, TexCoord.z, 1);
    OUT.TexCoord3 = half4(Coord.x, Coord.y + texelIncrement * 4, TexCoord.z, 1);
    OUT.TexCoord4 = half4(Coord.x, Coord.y, TexCoord.z, 1);
    OUT.TexCoord5 = half4(Coord.x, Coord.y - texelIncrement, TexCoord.z, 1);
    OUT.TexCoord6 = half4(Coord.x, Coord.y - texelIncrement * 2, TexCoord.z, 1);
    OUT.TexCoord7 = half4(Coord.x, Coord.y - texelIncrement * 3, TexCoord.z, 1);
    OUT.TexCoord8 = half4(Coord.x, Coord.y - texelIncrement * 4, TexCoord.z, 1);
    return OUT;
}

VS_OUTPUT_BLUR VS_Quad_Horizontal_9tap(half3 Position : POSITION, 
			half3 TexCoord : TEXCOORD0)
{
    VS_OUTPUT_BLUR OUT = (VS_OUTPUT_BLUR)0;
    OUT.Position = half4(Position, 1);
	half texelIncrement = NPixels/QuadScreenSize.x;
    half3 Coord = half3(TexCoord.xy, 1);
    OUT.TexCoord0 = half4(Coord.x + texelIncrement, Coord.y, TexCoord.z, 1);
    OUT.TexCoord1 = half4(Coord.x + texelIncrement * 2, Coord.y, TexCoord.z, 1);
    OUT.TexCoord2 = half4(Coord.x + texelIncrement * 3, Coord.y, TexCoord.z, 1);
    OUT.TexCoord3 = half4(Coord.x + texelIncrement * 4, Coord.y, TexCoord.z, 1);
    OUT.TexCoord4 = half4(Coord.x, Coord.y, TexCoord.z, 1);
    OUT.TexCoord5 = half4(Coord.x - texelIncrement, Coord.y, TexCoord.z, 1);
    OUT.TexCoord6 = half4(Coord.x - texelIncrement * 2, Coord.y, TexCoord.z, 1);
    OUT.TexCoord7 = half4(Coord.x - texelIncrement * 3, Coord.y, TexCoord.z, 1);
    OUT.TexCoord8 = half4(Coord.x - texelIncrement * 4, Coord.y, TexCoord.z, 1);
    return OUT;
}

//////////////////////////////////////////////////////
////////////////////////////////// pixel shaders /////
//////////////////////////////////////////////////////

// For two-pass blur, we have chosen to do  the horizontal blur FIRST. The
//	vertical pass includes a post-blur scale factor.

// Relative filter weights indexed by distance from "home" texel
//    This set for 9-texel sampling
#define WT9_0 1.0
#define WT9_1 0.8
#define WT9_2 0.6
#define WT9_3 0.4
#define WT9_4 0.2

// Alt pattern -- try your own!
// #define WT9_0 0.1
// #define WT9_1 0.2
// #define WT9_2 3.0
// #define WT9_3 1.0
// #define WT9_4 0.4

#define WT9_NORMALIZE (WT9_0+2.0*(WT9_1+WT9_2+WT9_3+WT9_4))

#define CUT(x) max((scale*(tex2D(SceneSampler,(x))-Cutoff)),0)

half4 PS_Blur_Horizontal_9tap(VS_OUTPUT_BLUR IN) : COLOR
{
	half scale = 1.0/(1.0-Cutoff);
    half4 OutCol = CUT(IN.TexCoord0) * (WT9_1/WT9_NORMALIZE);
    OutCol += CUT(IN.TexCoord1) * (WT9_2/WT9_NORMALIZE);
    OutCol += CUT(IN.TexCoord2) * (WT9_3/WT9_NORMALIZE);
    OutCol += CUT(IN.TexCoord3) * (WT9_4/WT9_NORMALIZE);
    OutCol += CUT(IN.TexCoord4) * (WT9_0/WT9_NORMALIZE);
    OutCol += CUT(IN.TexCoord5) * (WT9_1/WT9_NORMALIZE);
    OutCol += CUT(IN.TexCoord6) * (WT9_2/WT9_NORMALIZE);
    OutCol += CUT(IN.TexCoord7) * (WT9_3/WT9_NORMALIZE);
    OutCol += CUT(IN.TexCoord8) * (WT9_3/WT9_NORMALIZE);
    return OutCol;
} 

half4 PS_Blur_Horizontal_9tap_old(VS_OUTPUT_BLUR IN) : COLOR
{
    half4 OutCol = tex2D(SceneSampler, IN.TexCoord0) * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2D(SceneSampler, IN.TexCoord1) * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2D(SceneSampler, IN.TexCoord2) * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2D(SceneSampler, IN.TexCoord3) * (WT9_4/WT9_NORMALIZE);
    OutCol += tex2D(SceneSampler, IN.TexCoord4) * (WT9_0/WT9_NORMALIZE);
    OutCol += tex2D(SceneSampler, IN.TexCoord5) * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2D(SceneSampler, IN.TexCoord6) * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2D(SceneSampler, IN.TexCoord7) * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2D(SceneSampler, IN.TexCoord8) * (WT9_3/WT9_NORMALIZE);
    return OutCol;
} 

half4 PS_Blur_Vertical_9tap(VS_OUTPUT_BLUR IN) : COLOR
{   
    half4 OutCol = tex2D(HBlurSampler, IN.TexCoord0) * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2D(HBlurSampler, IN.TexCoord1) * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2D(HBlurSampler, IN.TexCoord2) * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2D(HBlurSampler, IN.TexCoord3) * (WT9_4/WT9_NORMALIZE);
    OutCol += tex2D(HBlurSampler, IN.TexCoord4) * (WT9_0/WT9_NORMALIZE);
    OutCol += tex2D(HBlurSampler, IN.TexCoord5) * (WT9_1/WT9_NORMALIZE);
    OutCol += tex2D(HBlurSampler, IN.TexCoord6) * (WT9_2/WT9_NORMALIZE);
    OutCol += tex2D(HBlurSampler, IN.TexCoord7) * (WT9_3/WT9_NORMALIZE);
    OutCol += tex2D(HBlurSampler, IN.TexCoord8) * (WT9_3/WT9_NORMALIZE);
    return Glowness*OutCol;
} 

////////

half4 PS_GlowPass(QuadVertexOutput IN) : COLOR
{   
	half4 tex = tex2D(SceneSampler, half2(IN.UV.x, IN.UV.y));
	return tex;
}  

half4 PS_Comp(QuadVertexOutput IN) : COLOR
{   
	half4 prev = TrailDrop * tex2D(BlendSampler, half2(IN.UV.x, IN.UV.y));
	half4 orig = SceneBright * tex2D(SceneSampler, half2(IN.UV.x, IN.UV.y));
	half4 blur = GlowBright  * tex2D(FinalBlurSampler, half2(IN.UV.x, IN.UV.y));
	return (orig+blur+prev);
}  

half4 PS_Plop(QuadVertexOutput IN) : COLOR
{   
	half4 orig = tex2D(CompositeSampler, half2(IN.UV.x, IN.UV.y));
	return orig;
}  

half4 PS_Last(QuadVertexOutput IN) : COLOR
{   
	half4 orig = tex2D(BlendSampler, half2(IN.UV.x, IN.UV.y));
	return orig;
}

////////////////////////////////////////////////////////////
/////////////////////////////////////// techniques /////////
////////////////////////////////////////////////////////////

technique Main <
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	string Script =
			"LoopByCount=bReset;"
				"RenderColorTarget0=BlendMap;"
				"RenderDepthStencilTarget=DepthBuffer;"
				"ClearSetColor=ClearColor;"
	        	"ClearSetDepth=ClearDepth;"
				"Clear=Color0;"
				"Clear=Depth;"
			"LoopEnd=;"
			"RenderColorTarget0=SceneTexture;"
	        "RenderDepthStencilTarget=DepthBuffer;"
	        	"ClearSetColor=ClearColor;"
	        	"ClearSetDepth=ClearDepth;"
   				"Clear=Color;"
				"Clear=Depth;"
	        	"ScriptExternal=color;"
        	"Pass=GlowH;"
        	"Pass=GlowV;"
        	"Pass=FinalComp;"
        	"Pass=Plop;"
        	"Pass=Last;";
> {
    pass GlowH <
    	string Script = "RenderColorTarget0=HBlurMap;"
								"Draw=Buffer;";
    > {
		cullmode = none;
		ZEnable = false;
		AlphaBlendEnable = false;
		VertexShader = compile vs_2_0 VS_Quad_Horizontal_9tap();
		PixelShader  = compile ps_2_0 PS_Blur_Horizontal_9tap();
    }
    pass GlowV <
    	string Script = "RenderColorTarget0=FinalBlurMap;"
								"Draw=Buffer;";
    > {
		cullmode = none;
		ZEnable = false;
		AlphaBlendEnable = false;
		VertexShader = compile vs_2_0 VS_Quad_Vertical_9tap();
		PixelShader  = compile ps_2_0 PS_Blur_Vertical_9tap();
    }
    pass FinalComp <
    	string Script = "RenderColorTarget0=CompositeMap;"
								"Draw=Buffer;";
    > {
		cullmode = none;
		ZEnable = false;
		AlphaBlendEnable = false;
		VertexShader = compile vs_2_0 ScreenQuadVS();
		PixelShader  = compile ps_2_0 PS_Comp();
    }
    pass Plop <
    	string Script = "RenderColorTarget0=BlendMap;"
								"Draw=Buffer;";
    > {
		cullmode = none;
		ZEnable = false;
		AlphaBlendEnable = false;
		VertexShader = compile vs_2_0 ScreenQuadVS();
		PixelShader  = compile ps_2_0 PS_Plop();
    }
    pass Last <
    	string Script = "RenderColorTarget0=;"
								"Draw=Buffer;";
    > {
		cullmode = none;
		ZEnable = false;
		AlphaBlendEnable = false;
		VertexShader = compile vs_2_0 ScreenQuadVS();
		PixelShader  = compile ps_2_0 PS_Last();
    }
}

////////////// eof ///
