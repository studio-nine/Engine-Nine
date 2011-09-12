/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/Scaredy.fx#1 $

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
    "Run away" from the mouse (unless mouse button is pressed)

******************************************************************************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=TryTheMouseButton;";
> = 0.8;

/*********** Tweakables **********************/

half3 LightDirD : Direction <
    string UIName = "Light Direction"; 
    string Object = "DirectionalLight";
    string Space = "World";
> = {-10.0f, 15.0f, 30.0f};

half3 LightColorD : Specular <
    string UIName =  "Distant Light Color";
    string UIWidget = "Color";
> = {0.2f, 0.3f, 1.0f};

half LightIntensityD <
    string UIName =  "Distant Light Strength";
> = 0.5;

///// ambient light

half3 AmbiColor : Ambient <
    string UIName =  "Ambient Color";
    string UIWidget = "Color";
> = {0.1f, 0.1f, 0.1f};

///// Surface Parameters ///////////////////////

half4 SurfColor : Diffuse <
    string UIName =  "Surface Color";
    string UIWidget = "Color";
> = {0.8f, 0.8f, 1.0f, 1.0f};

half Ks <
    string UIWidget = "slider";
    half UIMin = 0.0;
    half UIMax = 1.0;
    half UIStep = 0.01;
    string UIName = "specular intensity";
> = 0.5;

half SpecExpon : SpecularPower <
    string UIWidget = "slider";
    half UIMin = 1.0;
    half UIMax = 128.0;
    half UIStep = 1.0;
    string UIName =  "specular power";
> = 30.0;

float Suction <
	string UIName = "Force";
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 0.2;
	float UIStep = 0.001;
> = 0.05f;

float PushMax <
	string UIName = "Push Max";
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 2.5;
	float UIStep = 0.001;
> = 0.8f;

/************* UN-TWEAKABLES **************/

half4x4 WorldITXf : WorldInverseTranspose <string UIWidget="None";>;
half4x4 WvpXf : WorldViewProjection <string UIWidget="None";>;
half4x4 WorldXf : World <string UIWidget="None";>;
half4x4 ViewIXf : ViewInverse <string UIWidget="None";>;

float4 MouseL : LEFTMOUSEDOWN <string UIWidget="None";>;
float3 MousePos : MOUSEPOSITION <string UIWidget="None";>;

/************* DATA STRUCTS **************/

/* data from application vertex buffer */
struct appdata {
    half3 Position	: POSITION;
    half4 UV		: TEXCOORD0;
    half4 Normal	: NORMAL;
};

// used for all other passes
struct vertexOutput {
    half4 HPosition	: POSITION;
    half2 UV		: TEXCOORD0;
    half3 LightVec	: TEXCOORD1;
    half3 WorldNormal	: TEXCOORD2;
    half3 WorldEyeVec	: TEXCOORD3;
};

/*********** vertex shader ******/

vertexOutput pushVS(appdata IN,uniform half3 LightDir)
{
    vertexOutput OUT;
    OUT.WorldNormal = mul(IN.Normal, WorldITXf).xyz;
    half4 Po = half4(IN.Position.xyz,1);
    half3 Pw = mul(Po, WorldXf).xyz;
    OUT.WorldEyeVec = (ViewIXf[3].xyz - Pw);
    half4 Ph = mul(Po, WvpXf);
    OUT.HPosition = Ph;
	float2 dm = 2*(float2(MousePos.x-.5,0.5-MousePos.y)) - (Ph.xy/Ph.w);
	float dx = Suction/dot(dm,dm);
	dx = min(PushMax,dx*Ph.w);
	dx = (MouseL.z * dx) + (MouseL.z-1)*dx;
	OUT.HPosition.xy -= (dx*dm);
    OUT.UV = IN.UV.xy;
    OUT.LightVec = -LightDir;
    return OUT;
}

//////////////////////////////////
/********* pixel shaders ********/
//////////////////////////////////

// this portion shared for all lamps
half4 sharedPS(vertexOutput IN,
    half4 DiffColor,
    half3 LightColor,
    half Intensity
) {
    half3 Ln = normalize(IN.LightVec);
    half3 Vn = normalize(IN.WorldEyeVec);
    half3 Nn = normalize(IN.WorldNormal);
    half3 Hn = normalize(Vn + Ln);
    half4 lv = lit(dot(Ln,Nn),dot(Hn,Nn),SpecExpon);
    half4 diffContrib = DiffColor * half4((Intensity*lv.y*LightColor + AmbiColor),1);
    half4 specContrib = Ks * lv.z * half4(Intensity*LightColor,0);
    return diffContrib + specContrib;
}

// lamps without falloff
half4 lampPS(vertexOutput IN,
    uniform half3 LightColor,
    uniform half Intensity
) : COLOR {
    return sharedPS(IN,SurfColor,LightColor,Intensity);
}

/*************/

technique TryTheMouseButton <
	string Script = "Pass=p0;";
> {
    pass p0 <
	string Script = "Draw=geometry;";
> {		
		VertexShader = compile vs_2_0 pushVS(LightDirD);
		ZEnable = true;
		ZWriteEnable = true;
		ZFunc = LessEqual;
		CullMode = None;
		AlphaBlendEnable = false;
		PixelShader = compile ps_2_0 lampPS(LightColorD,LightIntensityD);
    }
}


/***************************** eof ***/
