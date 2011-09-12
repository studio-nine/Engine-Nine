/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/cardProxy.fx#1 $

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

Apply this to an XY Plane object -- it's meant to snap the object location and orientation
	to the location and direction of a designated spotlight. The object will glow,
	and can be textured. It functions as a "proxy object" for the spotlight.

******************************************************************************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?textured:untextured;";
> = 0.8;

///////////////////////////////////////////////////////////////
/// TWEAKABLES ////////////////////////////////////////////////
///////////////////////////////////////////////////////////////

float3 UpVector <
	string UIName = "Up Vector";
	// bool Normalized = 1;
> = { 0.0f, 0.0f, 1.0f};

////////////////////////////////////////////// spot light

float3 CardCenter : POSITION <
	string UIName = "Card Posistion";
	string Object = "SpotLight";
	string Space = "World";
> = {-1.0f, 1.0f, 0.0f};

float3 CardNormal : DIRECTION <
	string UIName = "Card Normal Direction";
	string Object = "SpotLight";
	string Space = "World";
> = {-0.707f, 0.707f, 1.0f};

float CardWidth <
    string UIWidget = "slider";
    float UIMin = 0.1;
    float UIMax = 10.0;
    float UIStep = 0.1;
    string UIName = "Card Width";
> = 2.0;

float CardHeight <
    string UIWidget = "slider";
    float UIMin = 0.1;
    float UIMax = 10.0;
    float UIStep = 0.1;
    string UIName = "Card Height";
> = 2.0;

float CardIntensity <
    string UIWidget = "slider";
    float UIMin = 0.1;
    float UIMax = 10.0;
    float UIStep = 0.01;
    string UIName = "Card Brightness";
> = 1.0;

float ZAdjust <
    string UIWidget = "slider";
    float UIMin = -1.0;
    float UIMax = 1.0;
    float UIStep = 0.01;
    string UIName = "Card Fwd/Back";
> = 0.0;

////////////////////////////////////////////////////////
/// TEXTURES ///////////////////////////////////////////
////////////////////////////////////////////////////////

texture CardTex <
	string ResourceName = "default_color.dds";
	string TextureType = "2D";
>;

sampler2D CardSampler = sampler_state
{
	Texture = <CardTex>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = CLAMP;
	AddressV = CLAMP;
};
//////////////////

/************* "UN-TWEAKABLES," TRACKED BY CPU APPLICATION **************/

float4x4 ViewProjXf : ViewProjection <string UIWidget="None";>;
float4x4 WorldXf : World <string UIWidget="None";>;

////////////////////////////////////////////////////////////////////////////
/// SHADER CODE BEGINS /////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////
// Structs /////////////////////////////////////////
////////////////////////////////////////////////////

/* data from application vertex buffer */
struct appdata {
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;
};

struct vertexOutput {
    float4 HPosition	: POSITION;		// clipspace coords
    float2 UV		: TEXCOORD0;	// surface UV
};

////////////////////////////////////////////////////
// VM Function Creates new object xform ////////////
////////////////////////////////////////////////////

float4x4 card_xf(float3 Pos,float3 Aim,float3 Up)
{
	float3 side = cross(Up,Aim);	// to the side
	side = normalize(side);		// normalize() needed here? probably not
	float3 top = cross(side,Aim);
	top = normalize(top);		// normalize() needed here? probably not
	float4x4 rota = float4x4(side.xyz,0,
							top.xyz,0,
							Aim.xyz,0,
							Pos.xyz,1);
	return(rota);
}

/*********************************************************/
/*********** vertex shader *******************************/
/*********************************************************/

vertexOutput sa2VS(appdata IN,
					uniform float4x4 CardXf)
{
    vertexOutput OUT = (vertexOutput)0;
    float4 Po = float4((IN.UV.xy-float2(0.5,0.5))*float2(CardWidth,CardHeight),ZAdjust,(float)1.0);	// object coordinates
    float4 Pc = mul(Po,CardXf);		// screen clipspace coords
    OUT.HPosition = mul(Pc,ViewProjXf);	// screen clipspace coords
    OUT.UV = IN.UV.yx;
    return OUT;
}

/*********************************************************/
/*********** pixel shader ********************************/
/*********************************************************/

float4 reflCardPS_t(vertexOutput IN) : COLOR
{
	float4 tv = tex2D(CardSampler,IN.UV);
	return float4(CardIntensity*tv.xyz,tv.w);
}

float4 reflCardPS(vertexOutput IN) : COLOR { return float4(CardIntensity.xxx,1.0); }

////////////////////////////////////////////////////////////////////
/// TECHNIQUES /////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////

technique textured < string Script="Pass=p0;"; > {
	pass p0 < string Script="Draw=geometry;"; > {
		VertexShader = compile vs_2_0 sa2VS(card_xf(CardCenter,CardNormal,UpVector));
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		AlphaBlendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		PixelShader = compile ps_2_0 reflCardPS_t();
	}
}

technique untextured < string Script="Pass=p0;"; > {
	pass p0 < string Script="Draw=geometry;"; > {
		VertexShader = compile vs_2_0 sa2VS(card_xf(CardCenter,CardNormal,UpVector));
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		AlphaBlendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = InvSrcAlpha;
		PixelShader = compile ps_2_0 reflCardPS();
	}
}

/***************************** eof ***/
