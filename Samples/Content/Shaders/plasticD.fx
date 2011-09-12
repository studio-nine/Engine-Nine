/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/plasticD.fx#1 $

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

    Vertex-shaded plastic using directional lights

******************************************************************************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=ps11;";
> = 0.8;

/************* UNTWEAKABLES **************/

float4x4 WorldIT : WorldInverseTranspose < string UIWidget="None"; >;
float4x4 WorldViewProj : WorldViewProjection < string UIWidget="None"; >;
float4x4 World : World < string UIWidget="None"; >;
float4x4 ViewI : ViewInverse < string UIWidget="None"; >;

/************* TWEAKABLES **************/

float4 LightDir : Direction <
    string Object = "DirectionalLight";
    string Space = "World";
> = {1.0f, -1.0f, 1.0f, 0.0f};

float4 LightColor <
    string UIName =  "Light Color";
    string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f, 1.0f};

float4 AmbiColor : Ambient
<
    string UIName =  "Ambient Light Color";
    string UIWidget = "Color";
> = {0.2f, 0.2f, 0.2f, 1.0f};

float4 SurfColor : Diffuse
<
    string UIName =  "Surface Color";
    string UIWidget = "Color";
> = {0.9f, 0.5f, 0.9f, 1.0f};

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
    // float4 UV		: TEXCOORD0;
    float4 Normal	: NORMAL;
};

/* data passed from vertex shader to pixel shader */
struct vertexOutput {
    float4 HPosition	: POSITION;
    // float2 UV	: TEXCOORD0;
    float4 diffCol	: COLOR0;
    float4 specCol	: COLOR1;
};

/*********** vertex shader ******/

vertexOutput plasticDVS(appdata IN) {
    vertexOutput OUT;
    float3 Nn = normalize(mul(IN.Normal, WorldIT).xyz);
    float4 Po = float4(IN.Position.xyz,1);
    float3 Pw = mul(Po, World).xyz;
    OUT.HPosition = mul(Po, WorldViewProj);
    float3 Ln = normalize(-LightDir); // potentially already normalized
    float ldn = dot(Ln,Nn);
    float diffComp = max(0,ldn);
    float4 diffContrib = SurfColor * ( diffComp * LightColor + AmbiColor);
    OUT.diffCol = diffContrib;
    OUT.diffCol.w = 1.0;
    // OUT.UV = IN.UV.xy;
    float3 Vn = normalize(ViewI[3].xyz - Pw);
    float3 Hn = normalize(Vn + Ln);
    float hdn = pow(max(0,dot(Hn,Nn)),SpecExpon);
    OUT.specCol = hdn * LightColor;
    return OUT;
}

/********* pixel shader isn't needed ********/

// float4 plasticDPS(vertexOutput IN) : COLOR { return (IN.diffCol + IN.specCol); }

/*************/

technique ps11 <
	string Script = "Pass=p0;";
> {
    pass p0  <
		string Script = "Draw=geometry;";
    > {		
		VertexShader = compile vs_1_1 plasticDVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		//PixelShader = compile ps_1_1 plasticDPS();
		SpecularEnable = true;
		ColorArg1[ 0 ] = Diffuse;
		ColorOp[ 0 ]   = SelectArg1;
		ColorArg2[ 0 ] = Specular;
		AlphaArg1[ 0 ] = Diffuse;
		AlphaOp[ 0 ]   = SelectArg1;
    }
}

/***************************** eof ***/
