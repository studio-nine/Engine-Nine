//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================


//-----------------------------------------------------------------------------
// Texture sampler
//-----------------------------------------------------------------------------
uniform const texture Texture;
uniform const texture ShadowMap;

uniform const sampler TextureSampler : register(s0) = sampler_state
{
	Texture = (Texture);
};

uniform const sampler ShadowMapSampler : register(s7) = sampler_state
{
	Texture = (ShadowMap);

	AddressU = Clamp;
	AddressV = Clamp;

	//MipFilter = Point;
    //MinFilter = Point;
	//MagFilter = Point;
};


//-----------------------------------------------------------------------------
// Fog settings
//-----------------------------------------------------------------------------

uniform const float		fogMask			: register(c0) = 0;
uniform const float		FogStart		: register(c1);
uniform const float		FogEnd			: register(c2);
uniform const float3	FogColor		: register(c3);

uniform const float3	eyePosition		: register(c4);		// in world space


//-----------------------------------------------------------------------------
// Material settings
//-----------------------------------------------------------------------------

uniform const float3	DiffuseColor	: register(c5) = 1;

uniform const float		Alpha			: register(c6) = 1;
uniform const float3	EmissiveColor	: register(c7) = 0;
uniform const float3	SpecularColor	: register(c8) = 1;
uniform const float		SpecularPower	: register(c9) = 16;


//-----------------------------------------------------------------------------
// Lights
// All directions and positions are in world space and must be unit vectors
//-----------------------------------------------------------------------------

uniform const float3	AmbientLightColor	: register(c10) = 0.2f;

uniform const float3	lightDirection		: register(c11) = float3(0, 0, -1);
uniform const float3	lightDiffuseColor	: register(c12) = 1;
uniform const float3	lightSpecularColor	: register(c13) = 0;


//-----------------------------------------------------------------------------
// Matrices
//-----------------------------------------------------------------------------

uniform const float4x4	World		: register(vs, c20);	// 20 - 23
uniform const float4x4	View		: register(vs, c24);	// 24 - 27
uniform const float4x4	Projection	: register(vs, c28);	// 28 - 31
uniform const float4x4	LightView;
uniform const float4x4	LightProjection;
uniform const float		DepthBias		= 0.0005f;
uniform const float		ShadowIntensity	= 0.5f;
uniform const float		farClip;


//-----------------------------------------------------------------------------
// Structure definitions
//-----------------------------------------------------------------------------

struct ColorPair
{
	float3 Diffuse;
	float3 Specular;
};


//-----------------------------------------------------------------------------
// Vertex shader inputs
//-----------------------------------------------------------------------------

struct VSInputNmTxVc
{
	float4	Position	: POSITION;
	float2	TexCoord	: TEXCOORD0;
	float3	Normal		: NORMAL;
	//float4	Color		: COLOR;
};


//-----------------------------------------------------------------------------
// Vertex shader outputs
//-----------------------------------------------------------------------------

struct PixelLightingVSOutputTx
{
	float4	PositionPS	: POSITION;		// Position in projection space
	float2	TexCoord	: TEXCOORD0;
	float4	PositionWS	: TEXCOORD1;
	float3	NormalWS	: TEXCOORD2;
	float3  Shadow		: TEXCOORD3;
	float4	Diffuse		: COLOR0;		// diffuse.rgb and alpha
};


//-----------------------------------------------------------------------------
// Pixel shader inputs
//-----------------------------------------------------------------------------

struct PixelLightingPSInputTx
{
	float2	TexCoord	: TEXCOORD0;
	float4	PositionWS	: TEXCOORD1;
	float3	NormalWS	: TEXCOORD2;
	float3  Shadow		: TEXCOORD3;
	float4	Diffuse		: COLOR0;		// diffuse.rgb and alpha
};



//-----------------------------------------------------------------------------
// Compute per-pixel lighting.
// When compiling for pixel shader 2.0, the lit intrinsic uses more slots
// than doing this directly ourselves, so we don't use the intrinsic.
// E: Eye-Vector
// N: Unit vector normal in world space
//-----------------------------------------------------------------------------
ColorPair ComputePerPixelLights(float3 E, float3 N)
{
	ColorPair result;
	
	result.Diffuse = AmbientLightColor;
	result.Specular = 0;
	
	// Light0
	float3 L = -lightDirection;
	float3 H = normalize(E + L);
	float dt = max(0,dot(L,N));
    result.Diffuse += lightDiffuseColor * dt;
    if (dt != 0)
		result.Specular += lightSpecularColor * pow(max(0.00001f,dot(H,N)), SpecularPower);
    
    result.Diffuse *= DiffuseColor;
    result.Diffuse += EmissiveColor;
    result.Specular *= SpecularColor;
		
	return result;
}


//-----------------------------------------------------------------------------
// Compute fog factor
//-----------------------------------------------------------------------------
float ComputeFogFactor(float d)
{
    return clamp((d - FogStart) / (FogEnd - FogStart), 0, 1) * fogMask;
}

//-----------------------------------------------------------------------------
// Compute shadow
//-----------------------------------------------------------------------------
float ComputeShadow(float3 shadow)
{
	return shadow.z - DepthBias > tex2D(ShadowMapSampler, shadow.xy).x ? ShadowIntensity : 1;
}

//-----------------------------------------------------------------------------
// Per-pixel lighting vertex shaders
//-----------------------------------------------------------------------------

PixelLightingVSOutputTx VSBasicPixelLightingNmTxVc(VSInputNmTxVc vin)
{
	PixelLightingVSOutputTx vout;
	
	float4 pos_ws = mul(vin.Position, World);
	float4 pos_vs = mul(pos_ws, View);
	float4 pos_ps = mul(pos_vs, Projection);
	float4 pos_ss = mul(mul(pos_ws, LightView), LightProjection);
	
	vout.PositionPS		= pos_ps;
	vout.PositionWS.xyz	= pos_ws.xyz;
	vout.PositionWS.w	= ComputeFogFactor(length(eyePosition - pos_ws));
	vout.NormalWS		= normalize(mul(vin.Normal, World));
	vout.Diffuse.rgb	= 1;//vin.Color.rgb;
	vout.Diffuse.a		= Alpha;//vin.Color.a * Alpha;
	vout.TexCoord		= vin.TexCoord;
	vout.Shadow.x   	=  pos_ss.x / pos_ss.w / 2.0f + 0.5f;
	vout.Shadow.y   	= -pos_ss.y / pos_ss.w / 2.0f + 0.5f;
	vout.Shadow.z   	= pos_ss.z / farClip;
	
	return vout;
}


//-----------------------------------------------------------------------------
// Pixel shaders
//-----------------------------------------------------------------------------

float4 PSBasicPixelLightingTx(PixelLightingPSInputTx pin) : COLOR
{
	float3 posToEye = eyePosition - pin.PositionWS.xyz;
	
	float3 N = normalize(pin.NormalWS);
	float3 E = normalize(posToEye);
	
	ColorPair lightResult = ComputePerPixelLights(E, N);
				
	float4 diffuse = tex2D(TextureSampler, pin.TexCoord);
		
	diffuse *= float4(lightResult.Diffuse * pin.Diffuse.rgb, pin.Diffuse.a);
		
	float4 color = diffuse + float4(lightResult.Specular, 0);
	color.rgb = lerp(color.rgb, FogColor, pin.PositionWS.w);
	color.rgb *= ComputeShadow(pin.Shadow);

	return color;
}


//-----------------------------------------------------------------------------
// Shader and technique definitions
//-----------------------------------------------------------------------------
Technique BasicEffect
{
	Pass
	{
		VertexShader = compile vs_2_0 VSBasicPixelLightingNmTxVc();
		PixelShader	 = compile ps_2_0 PSBasicPixelLightingTx();
	}
}
