/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/deepColor.fx#1 $

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
	Depth as color

******************************************************************************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Depth;";
> = 0.8;

/************* UN-TWEAKABLES **************/

float4x4 WorldViewProj : WorldViewProjection < string UIWidget="None"; >;

/************* DATA STRUCTS **************/

/* data from application vertex buffer */
struct appdata {
    float3 Position	: POSITION;
    // float4 UV		: TEXCOORD0;
};

/* data passed from vertex shader to pixel shader */
struct vertexOutput {
    float4 HPosition	: POSITION;
    float4 Color	: COLOR0;
};

/************************************************************/

float4 NearColor <
    string UIName =  "FG Color";
	string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f, 1.0f};

float4 FarColor <
    string UIName =  "BG Color";
	string UIWidget = "Color";
> = {0.0f, 0.0f, 0.0f, 1.0f};

float Hither <
    string UIName =  "near distance";
> = 1.0;

float Yon <
    string UIName =  "far distance";
> = 3.0;

float Gamma <
    string UIWidget = "slider";
    float UIMin = 0.1;
    float UIMax = 5.0;
    float UIStep = 0.05;
    string UIName =  "adjust rolloff";
> = 1.0;

/*********** vertex shader ******/

vertexOutput mainVS(appdata IN)
{
    vertexOutput OUT;
    float4 Po = float4(IN.Position.x,IN.Position.y,IN.Position.z,1.0);
    float4 hpos = mul(Po, WorldViewProj);
    float dl = (hpos.z-Hither)/(Yon-Hither);
    dl = min(dl,1.0);
    dl = max(dl,0.0);
    dl = pow(dl,Gamma);
    OUT.Color = lerp(NearColor,FarColor,dl);
    OUT.HPosition = hpos;
    return OUT;
}

/*************/

technique Depth <
	string Script = "Pass=p0;";
> {
	pass p0  <
		string Script = "Draw=geometry;";
	> {		
		VertexShader = compile vs_1_1 mainVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		// no pixel shader
	}
}

/***************************** eof ***/
