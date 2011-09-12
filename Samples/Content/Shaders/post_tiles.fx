/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/post_tiles.fx#1 $

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

float Script : STANDARDSGLOBAL
<
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "postprocess";
	string ScriptOutput = "color";
	
	// We just call a script in the main technique.
	string Script = "Technique=tiles;";

> = 0.8;

float4 ClearColor : DIFFUSE = { 0.0f, 0.0f, 0.0f, 1.0f};
float ClearDepth = 1.0f;

/************* TWEAKABLES **************/

half NumTiles <
    string UIName = "# Tiles";
    string UIWidget = "slider";
    half UIMin = 1.0f;
    half UIMax = 100.0f;
    half UIStep = 1.0f;
> = 8.0;

half Threshhold <
    string UIWidget = "slider";
	string UIName = "Edge Width";
    half UIMin = 0.0f;
    half UIMax = 2.0f;
    half UIStep = 0.001f;
> = 0.15;

half3 EdgeColor <
	string UIWidget = "Color";
> = {0.7f, 0.7f, 0.7f};

//////////////////// RTT

texture SceneMap : RENDERCOLORTARGET < 
    float2 ViewportRatio = { 1.0f, 1.0f };
    int MIPLEVELS = 1;
    string format = "X8R8G8B8";
    string UIWidget = "None";
>;

texture DepthMap : RENDERDEPTHSTENCILTARGET
<
	float2 ViewportRatio = { 1.0f, 1.0f };
    string format = "D24S8";
    string UIWidget = "None";
>;

sampler SceneSampler = sampler_state {
    texture = <SceneMap>;
    AddressU  = CLAMP;        
    AddressV  = CLAMP;
    MIPFILTER = NONE;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};


/************* DATA STRUCTS **************/

/* data passed from vertex shader to pixel shader */
struct vertexOutput {
    half4 HPosition	: POSITION;
    half2 UV	: TEXCOORD0;
};

/*********** vertex shader ******/

vertexOutput VS_Quad(half3 Position : POSITION, 
			half3 TexCoord : TEXCOORD0)
{
    vertexOutput OUT = (vertexOutput)0;
    OUT.HPosition = half4(Position, 1);
    OUT.UV = TexCoord.xy; 
    return OUT;
}

/********* pixel shader ********/

half4 tilesPS(vertexOutput IN) : COLOR {
    half size = 1.0/NumTiles;
    half2 Pbase = IN.UV - fmod(IN.UV,size.xx);
    half2 PCenter = Pbase + (size/2.0).xx;
    half2 st = (IN.UV - Pbase)/size;
    half4 c1 = (half4)0;
    half4 c2 = (half4)0;
    half4 invOff = half4((1-EdgeColor),1);
    if (st.x > st.y) { c1 = invOff; }
    half threshholdB =  1.0 - Threshhold;
    if (st.x > threshholdB) { c2 = c1; }
    if (st.y > threshholdB) { c2 = c1; }
    half4 cBottom = c2;
    c1 = (half4)0;
    c2 = (half4)0;
    if (st.x > st.y) { c1 = invOff; }
    if (st.x < Threshhold) { c2 = c1; }
    if (st.y < Threshhold) { c2 = c1; }
    half4 cTop = c2;
    half4 tileColor = tex2D(SceneSampler,PCenter);
    half4 result = tileColor + cTop - cBottom;
    return result;
}

/*************/

technique tiles
<
	string Script = 
	      	"RenderColorTarget=SceneMap;"
	        "RenderDepthStencilTarget=DepthMap;"
	        	"ClearSetColor=ClearColor;"
	        	"ClearSetDepth=ClearDepth;"
   				"Clear=Color;"
				"Clear=Depth;"
	        	"ScriptExternal=color;"
	        "Pass=p0;";
>
{
    pass p0 
    <
    	string Script= "RenderColorTarget0=;"
       					"RenderDepthStencilTarget=;"
						"Draw=Buffer;";  
	>
    {		
		VertexShader = compile vs_2_0 VS_Quad();
		ZEnable = false;
		ZWriteEnable = false;
		CullMode = None;
		PixelShader = compile ps_2_0 tilesPS();
    }
}

/***************************** eof ***/
