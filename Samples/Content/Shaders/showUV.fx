/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/showUV.fx#1 $

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

Draw UV Coordinates - mostly useful in tandem with scene_paint3D.fx

******************************************************************************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=ShowUV;";
> = 0.8;

/************* "UN-TWEAKABLES," TRACKED BY CPU APPLICATION **************/

float4x4 WorldViewProjXf : WorldViewProjection < string UIWidget="None"; >;

/*********************************************************/
/************* DATA STRUCTS ******************************/
/*********************************************************/

/* data from application vertex buffer */
struct appdata {
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;
};

struct vertexOutput {
    float4 HPosition	: POSITION;
    float2 UV			: TEXCOORD0;
};

/*********************************************************/
/*********** vertex shader *******************************/
/*********************************************************/

vertexOutput minVS(appdata IN) {
	vertexOutput OUT;
    float4 Po = float4(IN.Position.xyz,1.0);	// object coordinates
    OUT.HPosition = mul(Po,WorldViewProjXf);	// screen clipspace coords
    OUT.UV = IN.UV.xy;
    return OUT;
}

float4 uvPS(vertexOutput IN) : COLOR { return float4(IN.UV.xy,0,1); }

////////////////////////////////////////////////////////////////////
/// TECHNIQUES /////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////

// do all shading in a single pass

technique ShowUV <
	string Script = "Pass=p0;";
> {
	pass p0 <
	string Script = "Draw=geometry;";
> {
		VertexShader = compile vs_2_0 minVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_2_0 uvPS();
	}
}

/***************************** eof ***/
