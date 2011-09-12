/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/SeeValues.fx#1 $

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
    This .fx file uses colors
    to illustrate a number of shading vectors.
       Works fine with EffectEdit, though not
    all transforms may be properly loaded...
    $Date: 2004/09/24 $

******************************************************************************/

// string XFile = "teapot.x";
string XFile = "bigship1.x";

/************************************************************/
/*** TWEAKABLES *********************************************/
/************************************************************/

float3 LightPos : Position <
    string Object = "PointLight";
    string Space = "World";
> = {-10.0f, 10.0f, -10.0f};

float Scale : UNITSSCALE
<
    string units = "inches";
    string UIWidget = "slider";
    float UIMin = 0.001;
    float UIMax = 40.0;
    float UIStep = 0.01;
    string UIName = "Deriv Scale";
> = 64.0;

/************* UN-TWEAKABLES **************/

float4x4 WorldITXf : WorldInverseTranspose < string UIWidget="None"; >;
float4x4 WorldViewProjectionXf : WorldViewProjection < string UIWidget="None"; >;
float4x4 WorldXf : World < string UIWidget="None"; >;
float4x4 ViewIXf : ViewInverse < string UIWidget="None"; >;

float2 ScreenSize : VIEWPORTPIXELSIZE < string UIWidget="None"; >;

/************* DATA STRUCTS **************/

/* data from application vertex buffer */
struct appdata {
    float3 Pos    : POSITION;
    float4 UV        : TEXCOORD0;
    float4 Normal    : NORMAL;
    float4 Tangent    : TANGENT0;
    float4 Binormal    : BINORMAL0;
};

/* data passed from vertex shader to pixel shader */
struct vertexOutput {
    float4 HPosition    : POSITION;
    float2 TexCoord    : TEXCOORD0;
    float3 LightVec    : TEXCOORD1;
    float3 WorldNormal    : TEXCOORD2;
    float3 WorldEyeVec    : TEXCOORD3;
    float3 WorldTangent    : TEXCOORD4;
    float3 WorldBinorm    : TEXCOORD5;
};

/*********** vertex shader for all ******/

vertexOutput simpleVS(appdata IN) {
    vertexOutput OUT;
    float4 Po = float4(IN.Pos,1.0);
    OUT.HPosition = mul(Po, WorldViewProjectionXf);
    // OUT.HPosition = mul(WorldViewProjectionXf, Po);
    OUT.WorldNormal = mul(WorldITXf, IN.Normal).xyz;
    OUT.WorldTangent = mul(WorldITXf, IN.Tangent).xyz;
    OUT.WorldBinorm = mul(WorldITXf, IN.Binormal).xyz;
    float4 Pw = mul(WorldXf, Po);
    OUT.LightVec = (LightPos-Pw).xyz;
    OUT.TexCoord = IN.UV.xy;
    OUT.WorldEyeVec = normalize(ViewIXf[3].xyz - Pw.xyz);
    return OUT;
}

/********* utility functions for pixel shaders ********/

float4 vecColor(float4 V) {
    float3 Nc = 0.5 * ((V.xyz) + ((1.0).xxx));
    return float4(Nc,1);
}

float4 vecColor(float3 V) {
    float3 Nc = 0.5 * ((V.xyz) + ((1.0).xxx));
    return float4(Nc,1);
}

float4 vecColorN(float4 V) {
    float3 Nc = 0.5 * (normalize(V.xyz) + ((1.0).xxx));
    return float4(Nc,1);
}

float4 vecColorN(float3 V) {
    float3 Nc = 0.5 * (normalize(V.xyz) + ((1.0).xxx));
    return float4(Nc,1);
}

/********* pixel shaders ********/

float4 normalsRawPS(vertexOutput IN) : COLOR { return vecColor(IN.WorldNormal); }
float4 normalsNPS(vertexOutput IN)   : COLOR { return vecColorN(IN.WorldNormal); }
float4 tangentRawPS(vertexOutput IN) : COLOR { return vecColor(IN.WorldTangent); }
float4 tangentNPS(vertexOutput IN)   : COLOR { return vecColorN(IN.WorldTangent); }
float4 binormRawPS(vertexOutput IN)  : COLOR { return vecColor(IN.WorldBinorm); }
float4 binormNPS(vertexOutput IN)    : COLOR { return vecColorN(IN.WorldBinorm); }
float4 viewNPS(vertexOutput IN)      : COLOR { return vecColorN(IN.WorldEyeVec); }
float4 lightNPS(vertexOutput IN)     : COLOR { return vecColorN(IN.LightVec); }

float4 uvcPS(vertexOutput IN) : COLOR { return float4(IN.TexCoord,0,1); }

float4 vFacePS(vertexOutput IN,float Vf : VFACE) : COLOR {
	float d = 0;
	if (Vf>0) d = 1;
	return float4(d,d,d,1);
}

float4 vPosPS(vertexOutput IN,float2 Vpos : VPOS) : COLOR {
	float2 c = Vpos.xy / ScreenSize.xy;
	return float4(c.xy,0,1);
}

float4 uvDerivsPS(vertexOutput IN) : COLOR
{
    float2 dd = Scale * (abs(ddx(IN.TexCoord)) + abs(ddy(IN.TexCoord)));
    return float4(dd,0,1);
}

float4 halfAnglePS(vertexOutput IN) :COLOR {
    float3 Ln = normalize(IN.LightVec);
    float3 Vn = normalize(IN.WorldEyeVec);
    // float3 Hn = normalize(Vn + Ln);
    return vecColorN(Vn+Ln);
}

float4 facingPS(vertexOutput IN) :COLOR {
    float3 Nn = normalize(IN.WorldNormal);
    float3 Vn = normalize(IN.WorldEyeVec);
    return float4(abs(dot(Nn,Vn)).xxx,1);
}

/****************************************************************/
/****************************************************************/
/******* TECHNIQUES *********************************************/
/****************************************************************/
/****************************************************************/

#define TECH(name,vertShader,pixShader) technique name { pass p0 { \
		VertexShader = compile vs_2_0 vertShader (); \
		ZEnable = true; ZWriteEnable = true; CullMode = None; \
		PixelShader = compile ps_2_a pixShader (); } }

#define TECH3(name,vertShader,pixShader) technique name { pass p0 { \
		VertexShader = compile vs_3_0 vertShader (); \
		ZEnable = true; ZWriteEnable = true; CullMode = None; \
		PixelShader = compile ps_3_0 pixShader (); } }

TECH(uvValues,simpleVS,uvcPS)
TECH(worldNormalVecsRaw,simpleVS,normalsRawPS)
TECH(worldNormalVecsNormalized,simpleVS, normalsNPS)
TECH(worldTangentVecsRaw,simpleVS,tangentRawPS)
TECH(worldTangentVecsNormalized,simpleVS,tangentNPS)
TECH(worldBinormalVecsRaw,simpleVS,binormRawPS)
TECH(worldBinormalVecsNormalized,simpleVS, binormNPS)
TECH(worldViewVecNormalized,simpleVS,viewNPS)
TECH(worldLightVecNormalized,simpleVS,lightNPS)
TECH(halfAngles,simpleVS,halfAnglePS)
TECH(facingRatio,simpleVS,facingPS)
TECH(uvDerivs,simpleVS,uvDerivsPS)
TECH3(vFaceReg,simpleVS,vFacePS)
TECH3(vPosReg,simpleVS,vPosPS)

technique flatBlack {
    pass p0 {        
		VertexShader = compile vs_1_1 simpleVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
    }
}

/***************************** eof ***/
