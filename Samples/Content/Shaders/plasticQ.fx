/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/plasticQ.fx#1 $

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
    A phong-shaded plastic surface. The highlight is done in VERTEX shading --
		not as a texture.
    Plastic surface, vertex shaded phong model with quadratic falloff. The falloff is
		interpolated between vertices, so if the light is in the middle of a large
		triangle, it will be wrong. If at a distance from all vertices, it will
		look pretty good for many applications.

******************************************************************************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=ps11;";
> = 0.8;

/*********** UNTWEAKABLES **************/

float4x4 WorldIT : WorldInverseTranspose < string UIWidget="None"; >;
float4x4 WorldViewProj : WorldViewProjection < string UIWidget="None"; >;
float4x4 World : World < string UIWidget="None"; >;
float4x4 ViewI : ViewInverse < string UIWidget="None"; >;

/************* TWEAKABLES **************/

float3 LightPos : Position <
    string Object = "PointLight";
    string Space = "World";
> = {-10.0f, 10.0f, -10.0f};

float3 LightColor <
    string UIName =  "Light Color";
    string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f};

float LightIntensity
<
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 1000.0;
    float UIStep = 0.2;
    string UIName =  "Intensity";
> = 1.0;

float3 AmbiColor : Ambient
<
    string UIName =  "Ambient Light Color";
    string UIWidget = "Color";
> = {0.2f, 0.2f, 0.2f};

////

float3 SurfColor : Diffuse
<
    string UIName =  "Surface Color";
    string UIWidget = "Color";
> = {0.9f, 0.5f, 0.9f};

float SpecExpon : SpecularPower
<
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 128.0;
    float UIStep = 1.0;
    string UIName =  "specular power";
> = 22.0;

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
    float4 TexCoord0	: TEXCOORD0; // put light distance into "z"
    float4 diffCol	: COLOR0;
    float4 specCol	: COLOR1;
};

/*********** vertex shader ******/

vertexOutput plasticQVS(appdata IN)
{
    vertexOutput OUT;
    float3 Nn = normalize(mul(IN.Normal, WorldIT).xyz);
    float4 Po = float4(IN.Position.xyz,1);
    float3 Pw = mul(Po, World).xyz;
    OUT.HPosition = mul(Po, WorldViewProj);
    float3 LightDelta = (LightPos - Pw);
    float falloff = LightIntensity / dot(LightDelta,LightDelta);
    float3 Ln = normalize(LightDelta);
    float ldn = dot(Ln,Nn);
    float diffComp = max(0,ldn);
    float3 diffContrib = SurfColor * (diffComp * LightColor + AmbiColor);
    OUT.diffCol = float4(diffContrib,1);
    OUT.TexCoord0 = float4(IN.UV.xy,falloff,1.0);
    float3 Vn = normalize(ViewI[3].xyz - Pw);
    float3 Hn = normalize(Vn + Ln);
    float hdn = pow(max(0,dot(Hn,Nn)),SpecExpon);
    OUT.specCol = float4((diffComp * hdn * LightColor).xyz,1);
    return OUT;
}

/********* pixel shader ********/

float4 plasticQPS(vertexOutput IN) : COLOR {
    return (IN.TexCoord0.z * (IN.diffCol + IN.specCol));
}

/*************/

technique ps11 <
	string Script = "Pass=p0;";
> {
    pass p0  <
		string Script = "Draw=geometry;";
    > {		
		VertexShader = compile vs_1_1 plasticQVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_1_1 plasticQPS();
    }
}

/***************************** eof ***/
