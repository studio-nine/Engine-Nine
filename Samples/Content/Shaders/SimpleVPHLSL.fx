/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/SimpleVPHLSL.fx#1 $

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
	No pixel shader

******************************************************************************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=t0;";
> = 0.8;

float4x4 ViewIT : ViewInverseTranspose < string UIWidget="None"; >;
float4x4 View : View < string UIWidget="None"; >;
float4x4 WorldView : WorldView < string UIWidget="None"; >;
float4x4 WorldViewIT : WorldViewInverseTranspose < string UIWidget="None"; >;
float4x4 WorldViewProj : WorldViewProjection < string UIWidget="None"; >;

float4 Diffuse : Diffuse <string UIWidget = "Color"; > = { 1.0f, 0.1f, 0.5f, 1.0f };
float4 Specular : Specular <string UIWidget = "Color"; > = { 0.4f, 0.4f, 0.4f, 1.0f };
float4 Ambient : Ambient <string UIWidget = "Color"; > = { 0.1f, 0.1f, 0.1f, 1.0f };

float4 LightPos : Position
<
	string Object = "PointLight";
	string Space = "World";
> = { -10.0f, 10.0f, -10.0f, 1.0f };

////////////////////////

struct VS_INPUT
{
	float4 vPosition : POSITION;
	float4 vNormal : NORMAL;
	float4 vTexCoords : TEXCOORD0;
};

struct VS_OUTPUT
{
	float4 vPosition : POSITION;
	float4 vTexCoord0 : TEXCOORD0;
	float4 vDiffuse : COLOR0;
	float4 vSpecular : COLOR1;
};

VS_OUTPUT myvs(const VS_INPUT IN)
{
	VS_OUTPUT OUT; 
    float4 Pv = mul(IN.vPosition, WorldView);
    float4 Nv = mul(IN.vNormal, WorldViewIT);
    float4 lightPosV = mul(LightPos, View);
    float4 Ln = normalize(lightPosV - Pv);
    float4 Vn = normalize(ViewIT[3]);
    float self_shadow = max(dot(Nv,Ln),0);
    float4 Hn = normalize(Ln + Vn);
    float spec_term = max(dot(Nv,Hn),0);
    float4 diff_term = Diffuse * self_shadow + self_shadow * spec_term * Specular + Ambient;
    OUT.vDiffuse = diff_term;
    OUT.vPosition = mul(IN.vPosition, WorldViewProj);
    OUT.vSpecular = (1.0f).xxxx;
    OUT.vTexCoord0 = IN.vNormal;
	return OUT;
}

/////////////////////////////////

technique t0 <
	string Script = "Pass=p0;";
> {
	pass p0 <
	string Script = "Draw=geometry;";
> {
		Zenable = true;
        ZWriteEnable = true;
        CullMode = None;
		VertexShader = compile vs_1_1 myvs();
	}
}

////////////////////////////////////////////////// eof //
