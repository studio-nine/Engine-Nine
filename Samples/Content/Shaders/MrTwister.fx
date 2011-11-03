/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/MrTwister.fx#1 $

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
    Simple sinusoidal vertex animation on a phong-shaded plastic surface.
	The highlight is done in VERTEX shading -- not as a texture.
	Note that the normal is likewise distorted, based on the fact that
		dsin(x)/dx is just cos(x)
    Do not let your kids play with this shader, you will not get your
		computer back for a while.

******************************************************************************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=ps11;";
> = 0.8;

/************* TWEAKABLES **************/

float4x4 WorldIT : WorldInverseTranspose < string UIWidget="None"; >;
float4x4 WorldViewProj : WorldViewProjection < string UIWidget="None"; >;
float4x4 World : World < string UIWidget="None"; >;
float4x4 ViewI : ViewInverse < string UIWidget="None"; >;

float Timer : Time < string UIWidget="None"; >;

float TimeScaleH
<
    string UIWidget = "slider";
    float UIMin = 0.1;
    float UIMax = 10;
    float UIStep = 0.1;
> = 1.0;

float TimeScaleV
<
    string UIWidget = "slider";
    float UIMin = 0.1;
    float UIMax = 10;
    float UIStep = 0.1;
> = 2.7;

float Wobble
<
    string UIWidget = "slider";
    float UIMin = 0;
    float UIMax = 2.0;
    float UIStep = 0.01;
> = 0.2;

float Horizontal
<
    string UIWidget = "slider";
    float UIMin = 0;
    float UIMax = 2.0;
    float UIStep = 0.01;
> = 0.1;

float Vertical
<
    string UIWidget = "slider";
    float UIMin = 0.01;
    float UIMax = 10.0;
    float UIStep = 0.1;
> = 0.2;

float3 LightPos : Position
<
    string Object = "PointLight";
    string Space = "World";
> = {-10.0f, 10.0f, -10.0f};

float3 LightColor
<
    string UIName =  "Light Color";
    string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f};

float3 AmbiColor : Ambient
<
    string UIName =  "Ambient Light Color";
    string UIWidget = "Color";
> = {0.2f, 0.2f, 0.2f};

float3 SurfColor : Diffuse
<
    string UIName =  "DrawableSurface Color";
    string UIWidget = "Color";
> = {0.1f, 0.5f, 0.4f};

float SpecExpon : SpecularPower
<
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 128.0;
    float UIStep = 1.0;
    string UIName =  "specular power";
> = 42.0;

/************* DATA STRUCTS **************/

/* data from application vertex buffer */
struct appdata {
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;
    float4 Normal	: NORMAL;
};

/* data passed from vertex shader to pixel shader */
struct vertexOutput {
    float4 HPosition	: POSITION;
    float4 TexCoord0	: TEXCOORD0;
    float4 diffCol	: COLOR0;
    float4 specCol	: COLOR1;
};

/*********** vertex shader ******/

vertexOutput MrWiggleVS(appdata IN)
{
    vertexOutput OUT;
    float3 Nn = normalize(mul(IN.Normal, WorldIT).xyz);
    float timeNowV = (Timer*TimeScaleV);
    float timeNowH = (Timer*TimeScaleH);
    float4 Po = float4(IN.Position.xyz,1);
    float iny = Po.y * Vertical + timeNowV;
    float inxz = sqrt(dot(Po.xz,Po.xz)) * Horizontal * timeNowH;
    float hScale = Wobble * (1.0+sin(inxz));
    float wiggleX = sin(iny) * hScale;
    float wiggleY = cos(iny) * hScale;
    Po.x = Po.x + wiggleX;
    Nn.y = Nn.y + wiggleY;
    Nn = normalize(Nn);
    Po.z = Po.z + wiggleY;
    OUT.HPosition = mul(Po, WorldViewProj);
    Nn.y = Nn.y + wiggleX;
    Nn = normalize(Nn);
    float3 Pw = mul(Po, World).xyz;
    float3 Ln = normalize(LightPos - Pw);
    float ldn = dot(Ln,Nn);
    float diffComp = max(0,ldn);
    float3 diffContrib = SurfColor * ( diffComp * LightColor + AmbiColor);
    OUT.diffCol = float4(diffContrib,1);
    OUT.TexCoord0 = IN.UV;
    float3 Vn = normalize(ViewI[3].xyz - Pw);
    float3 Hn = normalize(Vn + Ln);
    float hdn = pow(max(0,dot(Hn,Nn)),SpecExpon);
    OUT.specCol = float4(hdn * LightColor,1);
    return OUT;
}

/*************/

technique ps11 <
	string Script = "Pass=p0;";
> {
	pass p0 <
	string Script = "Draw=geometry;";
> {		
		VertexShader = compile vs_2_0 MrWiggleVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
	}
}

/***************************** eof ***/
