/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/metal.fx#1 $

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
    A phong-shaded metal-style surface lit from a point source.
	Textured, untextured, vertex-shaded and pixel-shaded.

******************************************************************************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?UntexturedVS:TexturedVS:UntexturedPS:TexturedPS;";
> = 0.8;

/************* UN-TWEAKABLES **************/

half4x4 WorldIT : WorldInverseTranspose < string UIWidget="None"; >;
half4x4 WorldViewProj : WorldViewProjection < string UIWidget="None"; >;
half4x4 World : World < string UIWidget="None"; >;
half4x4 ViewI : ViewInverse < string UIWidget="None"; >;

/************* TWEAKABLES **************/

half3 LightPos : Position <
    string Object = "PointLight";
    string UIName =  "Lamp Position";
    string Space = "World";
> = {100.0f, 100.0f, -100.0f};

half3 LightColor <
    string UIName =  "Lamp";
    string Object = "PointLight";
    string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f};

half3 AmbiColor : Ambient <
    string UIName =  "Ambient Light";
    string UIWidget = "Color";
> = {0.07f, 0.07f, 0.07f};

half3 SurfColor : DIFFUSE <
    string UIName =  "Surface";
    string UIWidget = "Color";
> = {1.0f, 1.0f, 1.0f};

half SpecExpon : SpecularPower <
    string UIWidget = "slider";
    half UIMin = 1.0;
    half UIMax = 128.0;
    half UIStep = 1.0;
    string UIName =  "Specular Power";
> = 12.0;

half Kd <
    string UIWidget = "slider";
    half UIMin = 0.0;
    half UIMax = 1.0;
    half UIStep = 0.05;
    string UIName =  "Diffuse (from dirt)";
> = 0.1;

//////////

texture ColorTexture : DIFFUSE <
    string ResourceName = "default_color.dds";
    string UIName =  "Surface Texture (if used)";
    string ResourceType = "2D";
>;

sampler2D ColorSampler = sampler_state {
	Texture = <ColorTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

/************* DATA STRUCTS **************/

/* data from application vertex buffer */
struct appdata {
    half3 Position	: POSITION;
    half4 UV		: TEXCOORD0;
    half4 Normal	: NORMAL;
};

/* data passed from vertex shader to pixel shader */
struct vertexOutput {
    half4 HPosition	: POSITION;
    half4 TexCoord		: TEXCOORD0;
    half3 LightVec		: TEXCOORD1;
    half3 WorldNormal	: TEXCOORD2;
    half3 WorldView	: TEXCOORD5;
};

/* data passed from ps1 vertex shader to pixel shader */
struct vertexOutputPS1 {
    float4 HPosition	: POSITION;
    float4 TexCoord0	: TEXCOORD0;
    float4 diffCol	: COLOR0;
    float4 specCol	: COLOR1;
};

/*********** vertex shader for pixel-shaded versions ******/

vertexOutput mainVS(appdata IN) {
    vertexOutput OUT;
    half4 normal = normalize(IN.Normal);
    OUT.WorldNormal = mul(normal, WorldIT).xyz;
    half4 Po = half4(IN.Position.xyz,1);
    half3 Pw = mul(Po, World).xyz;
    OUT.LightVec = normalize(LightPos - Pw);
    OUT.TexCoord = IN.UV;
    OUT.WorldView = normalize(ViewI[3].xyz - Pw);
    OUT.HPosition = mul(Po, WorldViewProj);
    return OUT;
}

/*********** vertex shader for vertx-shaded versions ******/

vertexOutputPS1 metalPVS(appdata IN)
{
    vertexOutputPS1 OUT;
    float3 Nn = normalize(mul(IN.Normal, WorldIT).xyz);
    float4 Po = float4(IN.Position.xyz,1);
    float3 Pw = mul(Po, World).xyz;
    float3 Ln = normalize(LightPos - Pw);
    float ldn = dot(Ln,Nn);
    float diffComp = max(0,ldn) * Kd;
    OUT.diffCol = float4((SurfColor*(diffComp*LightColor+AmbiColor)),1);
    OUT.TexCoord0 = IN.UV;
    float3 Vn = normalize(ViewI[3].xyz - Pw);
    float3 Hn = normalize(Vn + Ln);
    float hdn = pow(max(0,dot(Hn,Nn)),SpecExpon);
    OUT.specCol = float4((hdn * LightColor * SurfColor),1);
    OUT.HPosition = mul(Po, WorldViewProj);
    return OUT;
}

/********* pixel shader for ps1 simple version ********/

float4 metalPPS(vertexOutputPS1 IN) : COLOR
{
	return (IN.diffCol + tex2D(ColorSampler,IN.TexCoord0) * IN.specCol);
}

/********* ps_2 pixel shaders ********/

void metal_shared(vertexOutput IN,
			out half3 DiffuseContrib,
			out half3 SpecularContrib)
{
    half3 Ln = normalize(IN.LightVec);
    half3 Nn = normalize(IN.WorldNormal);
    half3 Vn = normalize(IN.WorldView);
    half3 Hn = normalize(Vn + Ln);
    half4 litV = lit(dot(Ln,Nn),dot(Hn,Nn),SpecExpon);
    DiffuseContrib = litV.y * Kd * LightColor + AmbiColor;
    SpecularContrib = litV.z * LightColor;
}

half4 metalPS(vertexOutput IN) : COLOR {
    half3 diffContrib;
    half3 specContrib;
	metal_shared(IN,diffContrib,specContrib);
    half3 result = diffContrib + (SurfColor * specContrib);
    return half4(result,1);
}

half4 metalPS_t(vertexOutput IN) : COLOR {
    half3 diffContrib;
    half3 specContrib;
	metal_shared(IN,diffContrib,specContrib);
    half3 map = tex2D(ColorSampler,IN.TexCoord.xy).xyz;
    half3 result = diffContrib + (SurfColor * map * specContrib);
    return half4(result,1);
}

/*************/

technique UntexturedVS <
	string Script = "Pass=p0;";
> {
	pass p0 <
	string Script = "Draw=geometry;";
> {		
		VertexShader = compile vs_1_1 metalPVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		// no pixel shader needed
		SpecularEnable = true;
		ColorArg1[ 0 ] = Diffuse;
		ColorOp[ 0 ]   = SelectArg1;
		ColorArg2[ 0 ] = Specular;
		AlphaArg1[ 0 ] = Diffuse;
		AlphaOp[ 0 ]   = SelectArg1;
	}
}

technique TexturedVS <
	string Script = "Pass=p0;";
> {
	pass p0 <
	string Script = "Draw=geometry;";
> {		
		VertexShader = compile vs_1_1 metalPVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		PixelShader = compile ps_1_1 metalPPS();
	}
}

technique UntexturedPS <
	string Script = "Pass=p0;";
> {
	pass p0 <
	string Script = "Draw=geometry;";
> {		
        VertexShader = compile vs_2_0 mainVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
        PixelShader = compile ps_2_0 metalPS();
	}
}

technique TexturedPS <
	string Script = "Pass=p0;";
> {
	pass p0 <
	string Script = "Draw=geometry;";
> {		
        VertexShader = compile vs_2_0 mainVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
        PixelShader = compile ps_2_0 metalPS_t();
	}
}

/***************************** eof ***/
