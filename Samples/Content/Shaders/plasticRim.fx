/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/plasticRim.fx#1 $

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
    Shader with a little extra kick in the rimlight

******************************************************************************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=rimPlastic;";
> = 0.8;

/************* UN-TWEAKABLES **************/

float4x4 WorldIT : WorldInverseTranspose < string UIWidget="None"; >;
float4x4 WorldViewProj : WorldViewProjection < string UIWidget="None"; >;
float4x4 World : World < string UIWidget="None"; >;
float4x4 ViewI : ViewInverse < string UIWidget="None"; >;

///////////////////////// TWEAKABLES ////////////

float3 LightPos : Position <
    string Object = "PointLight";
    string Space = "World";
> = {-10.0f, 10.0f, -10.0f};

float3 LightColor <
    string UIName = "Light Color";
    string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f};

float3 AmbiColor : Ambient
<
    string UIName = "Ambient Light Color";
    string UIWidget = "Color";
> = {0.07f, 0.07f, 0.07f};

////

float3 SurfColor : Diffuse
<
    string UIName = "Surface Color";
    string UIWidget = "Color";
> = {0.8f, 0.2f, 1.0f};

float3 SpecColor : Specular
<
    string UIName = "Specular Color";
    string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f};

float SpecExpon : SpecularPower
<
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 128.0;
    float UIStep = 1.0;
    string UIName = "Main Specular Exponent";
> = 12.0;

float EdgeExpon : SpecularPower
<
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 128.0;
    float UIStep = 1.0;
    string UIName = "Edge Specular Exponent";
> = 2.0;

float RimGamma : SpecularPower
<
    string UIWidget = "slider";
    float UIMin = 0.1;
    float UIMax = 5.0;
    float UIStep = 0.1;
    string UIName = "Specular Rollover";
> = 1.0;

float Wrap <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.01;
    string UIName = "Diffuse Wraparound";
> = 0.0;

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
    float2 UV			: TEXCOORD0;
    float3 LightVec		: TEXCOORD1;
    float3 WorldNormal	: TEXCOORD2;
    float3 WorldEyeVec	: TEXCOORD3;
};

/*********** vertex shader ******/

vertexOutput mainVS(appdata IN) {
    vertexOutput OUT;
    OUT.WorldNormal = mul(IN.Normal, WorldIT).xyz;
    float4 Po = float4(IN.Position.xyz,1);
    float3 Pw = mul(Po, World).xyz;
    // OUT.WorldPos = Pw;
    OUT.LightVec = normalize(LightPos-Pw);
    OUT.UV = IN.UV.xy;
    OUT.WorldEyeVec = normalize(ViewI[3].xyz - Pw);
    OUT.HPosition = mul(Po, WorldViewProj);
    return OUT;
}


/********* pixel shader ********/

float4 mainPS(vertexOutput IN) : COLOR
{
    float3 Ln = normalize(IN.LightVec);
    float3 Nn = normalize(IN.WorldNormal);
    float3 Vn = normalize(IN.WorldEyeVec);
    float3 Hn = normalize(Vn + Ln);
    float ldn = dot(Ln,Nn);
    ldn = smoothstep(-Wrap,1.0,ldn);
    float hdn = dot(Hn,Nn);
    float vdn = pow(dot(Vn,Nn),RimGamma);
    float exp2 = lerp(EdgeExpon,SpecExpon,vdn);
    float4 litVec = lit(ldn,hdn,exp2);
    float3 diffContrib = SurfColor * (litVec.y*LightColor + AmbiColor);
    float3 specContrib = litVec.y*litVec.z*LightColor * SpecColor;
    return float4((diffContrib + specContrib).xyz,1);
}

/*************/

technique rimPlastic <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
> {		
		VertexShader = compile vs_2_0 mainVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_2_0 mainPS();
    }
}

/***************************** eof ***/
