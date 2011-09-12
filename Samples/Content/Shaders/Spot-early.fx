/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/Spot-early.fx#1 $

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

Simple spotlight shader -- the same shader code compiled in
two different techniques: one for PS_3_0 and using pixel shader
branching (to avoid shading areas outside the spotlight cone), and
the other using PS_2_A with condition codes.

Check the Shader Perf analysis and your own frame rates to see
the optimization of ps_3_0.

******************************************************************************/

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "object";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";
	string Script = "Technique=Technique?PS2:PS3;";
> = 0.8; // version #

///////////////////////////////////////////////////////////////
/// TWEAKABLES ////////////////////////////////////////////////
///////////////////////////////////////////////////////////////

float3 UpVector <
	string UIName = "Up Vector";
	// bool Normalized = 1;
> = { 0.0f, 0.0f, 1.0f};

////////////////////////////////////////////// spot light

float3 SpotLightPos : POSITION <
	string UIName = "Spot Pos";
	string Object = "SpotLight";
	string Space = "World";
> = {-1.0f, 1.0f, 0.0f};

float3 SpotLightDir : DIRECTION <
	string UIName = "Spot Dir";
	string Object = "SpotLight";
	string Space = "World";
> = {-0.707f, 0.707f, 1.0f};

float SpotLightCone <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 90.5;
    float UIStep = 0.1;
    string UIName = "Cone Angle";
> = 40.0;

float3 SpotLightColor : Specular <
	string UIName = "Lamp";
	string Object = "SpotLight";
	string UIWidget = "Color";
> = {0.8f, 1.0f, 0.4f};

float SpotLightIntensity <
	string UIName = "Spot Intensity";
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 2;
	float UIStep = 0.1;
> = 1;

////////////////////////////////////////////// ambient light

float3 AmbiLightColor : Ambient
<
    string UIName = "Ambient";
> = {0.07f, 0.07f, 0.07f};

////////////////////////////////////////////// surface

float3 SurfColor : Diffuse
<
    string UIName = "Surface";
	string UIWidget = "Color";
> = {1.0f, 0.7f, 0.3f};

float Kd
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.5;
    float UIStep = 0.01;
    string UIName = "Diffuse";
> = 1.0;

float Ks
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.5;
    float UIStep = 0.01;
    string UIName = "Specular";
> = 1.0;


float SpecExpon : SpecularPower
<
    string UIWidget = "slider";
    float UIMin = 1.0;
    float UIMax = 128.0;
    float UIStep = 1.0;
    string UIName = "Specular power";
> = 12.0;

/************* "UN-TWEAKABLES," TRACKED BY CPU APPLICATION **************/

float4x4 WorldITXf : WorldInverseTranspose <string UIWidget="None";>;
float4x4 WorldViewProjXf : WorldViewProjection <string UIWidget="None";>;
float4x4 WorldXf : World <string UIWidget="None";>;
float4x4 ViewIXf : ViewInverse <string UIWidget="None";>;
float4x4 WorldViewITXf : WorldViewInverseTranspose <string UIWidget="None";>;
float4x4 WorldViewXf : WorldView <string UIWidget="None";>;
float4x4 ViewXf : View <string UIWidget="None";>;
float4x4 ViewITXf : ViewInverseTranspose <string UIWidget="None";>;

////////////////////////////////////////////////////////////////////////////
/// SHADER CODE BEGINS /////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////

/*********************************************************/
/************* SHARED DATA STRUCT ************************/
/*********************************************************/

/* data from application vertex buffer */
struct appdata {
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;
    float4 Normal	: NORMAL;
};

///////////////////////////////////////////////////////////////////////////////
/// Basic Version Does Almost All Calcs in the Pixel Shader ///////////////////
///////////////////////////////////////////////////////////////////////////////

struct vertexOutput {
    float4 HPosition	: POSITION;
    float2 UV		: TEXCOORD0;
    float3 LightVec	: TEXCOORD1;
    float3 WNormal	: TEXCOORD2;
    float3 WView		: TEXCOORD3;
};

////////////////////////////////////////////////////////////////////////////////
/// Vertex Shaders /////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////

// from scene camera POV
vertexOutput mainCamVS(appdata IN)
{
    vertexOutput OUT;
    OUT.WNormal = mul(IN.Normal,WorldITXf).xyz;
    float4 Po = float4(IN.Position.xyz,(float)1.0);	// object coordinates
    float4 Pw = mul(Po,WorldXf);			// world coordinates
    OUT.WView = normalize(ViewIXf[3].xyz - Pw.xyz);	// obj coords
    OUT.HPosition = mul(Po,WorldViewProjXf);	// screen clipspace coords
    OUT.UV = IN.UV.xy;
    OUT.LightVec = SpotLightPos - Pw.xyz; // in world coords
    return OUT;
}

/*********************************************************/
/*********** pixel shader ********************************/
/*********************************************************/

float4 spotLightPS(vertexOutput IN,uniform float CosSpotAng) : COLOR
{
    float3 result = SurfColor * AmbiLightColor;
    float3 Ln = normalize(IN.LightVec);
	float dl = dot(-SpotLightDir,Ln);
	dl = ((dl-CosSpotAng)/(((float)1.0)-CosSpotAng));
	if (dl>0) {
        float3 Nn = normalize(IN.WNormal);
    	float ldn = dot(Ln,Nn);
    	if (ldn>0) {
        	float3 Vn = normalize(IN.WView);
    		float3 Hn = normalize(Vn + Ln);
    		float hdn = dot(Hn,Nn);
    		float4 litVec = lit(ldn,hdn,SpecExpon);
    		ldn = litVec.y * SpotLightIntensity;
			ldn *= dl;
    		float3 diffContrib = SurfColor*(Kd*ldn * SpotLightColor);
    		float3 specContrib = ((ldn * litVec.z * Ks) * SpotLightColor);
    		float3 lighting = diffContrib + specContrib;
    		result = result + lighting;
    	}
    }
    return float4(result,1);
}

////////////////////////////////////////////////////////////////////
/// TECHNIQUES /////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////

technique PS3 <
	string Script = "Pass=drawPass;";
> {
	pass drawPass < string Script = "Draw=geometry;"; > {
		VertexShader = compile vs_3_0 mainCamVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_3_0 spotLightPS(cos(SpotLightCone*(float)(3.141592/180.0)));
	}
}

technique PS2 <
	string Script = "Pass=drawPass;";
> {
	pass drawPass < string Script = "Draw=geometry;"; > {
		VertexShader = compile vs_2_0 mainCamVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_2_a spotLightPS(cos(SpotLightCone*(float)(3.141592/180.0)));
	}
}
/***************************** eof ***/
