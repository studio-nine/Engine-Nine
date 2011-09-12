/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/MrWiggle.fx#1 $

Copyright NVIDIA Corporation 2004
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
		Textured/Untextured versions are supplied
	Note that the normal is also easily distorted, based on the fact that
		dsin(x)/dx is just cos(x)
    Do not let your kids play with this shader, you will not get your
		computer back for a while.

******************************************************************************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?Untextured:Textured;";
> = 0.8;

/************* TWEAKABLES **************/

float4x4 WorldIT : WorldInverseTranspose < string UIWidget="None"; >;
float4x4 WorldViewProj : WorldViewProjection < string UIWidget="None"; >;
float4x4 World : World < string UIWidget="None"; >;
float4x4 ViewI : ViewInverse < string UIWidget="None"; >;

float Timer : Time < string UIWidget="None"; >;

float TimeScale <
    string UIWidget = "slider";
    string UIName = "Speed";
    float UIMin = 0.1;
    float UIMax = 10;
    float UIStep = .1;
> = 4.0f;

float Horizontal <
    string UIWidget = "slider";
    float UIMin = 0.001;
    float UIMax = 10;
    float UIStep = 0.01;
> = 0.5f;

float Vertical <
    string UIWidget = "slider";
    float UIMin = 0.001;
    float UIMax = 10.0;
    float UIStep = 0.1;
> = 0.5;

float3 LightPos : Position <
    string Object = "PointLight";
    string Space = "World";
> = {-10.0f, 10.0f, -10.0f};

float3 LightColor <
    string UIName =  "Lamp";
    string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f};

float3 AmbiColor : Ambient <
    string UIName =  "Ambient Light";
    string UIWidget = "Color";
> = {0.2f, 0.2f, 0.2f};

float3 SurfColor : DIFFUSE <
    string UIName =  "Surface";
    string UIWidget = "Color";
> = {0.9f, 0.9f, 0.9f};

float SpecExpon : SpecularPower <
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 128.0;
    float UIStep = 1.0;
    string UIName =  "Specular Power";
> = 5.0;

texture colorTexture : DIFFUSE <
    string ResourceName = "default_color.dds";
    string UIName =  "Color Texture (if used)";
    string ResourceType = "2D";
>;

sampler2D colorSampler = sampler_state {
	Texture = <colorTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = None;
};

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

vertexOutput MrWiggleVS(appdata IN) {
    vertexOutput OUT;
    float3 Nn = normalize(mul(IN.Normal, WorldIT).xyz);
    float timeNow = Timer*TimeScale;
    float4 Po = float4(IN.Position.xyz,1);
    float iny = Po.y * Vertical + timeNow;
    float wiggleX = sin(iny) * Horizontal;
    float wiggleY = cos(iny) * Horizontal; // deriv
    Nn.y = Nn.y + wiggleY;
    Nn = normalize(Nn);
    Po.x = Po.x + wiggleX;
    OUT.HPosition = mul(Po, WorldViewProj);
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

/********* pixel shader ********/

float4 MrWigglePS_t(vertexOutput IN) : COLOR {
    float4 result = IN.diffCol * tex2D(colorSampler, IN.TexCoord0) + IN.specCol;
    return result;
}

/*************/

technique Untextured <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
> {		
		VertexShader = compile vs_1_1 MrWiggleVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
    }
}

technique Textured <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
> {		
		VertexShader = compile vs_2_0 MrWiggleVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_2_0 MrWigglePS_t();
    }
}

/***************************** eof ***/
