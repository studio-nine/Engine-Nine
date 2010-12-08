//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================


//-----------------------------------------------------------------------------
// Texture sampler
//-----------------------------------------------------------------------------
uniform const texture ShadowMap;

uniform const sampler ShadowMapSampler : register(s7) = sampler_state
{
	Texture = (ShadowMap);

	AddressU = Clamp;
	AddressV = Clamp;
};

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
// Vertex shader inputs
//-----------------------------------------------------------------------------

struct VSInputNmTxVc
{
	float4	Position	: POSITION;
	float2	TexCoord	: TEXCOORD0;
	float3	Normal		: NORMAL;
};


//-----------------------------------------------------------------------------
// Vertex shader outputs
//-----------------------------------------------------------------------------

struct PixelLightingVSOutputTx
{
	float4	PositionPS	: POSITION;		// Position in projection space
	float3  Shadow		: TEXCOORD3;
};


//-----------------------------------------------------------------------------
// Pixel shader inputs
//-----------------------------------------------------------------------------

struct PixelLightingPSInputTx
{
	float3  Shadow		: TEXCOORD3;
};


//-----------------------------------------------------------------------------
// Compute shadow
//-----------------------------------------------------------------------------
float ComputeShadow(float3 shadow)
{
	return shadow.z - DepthBias > tex2D(ShadowMapSampler, shadow.xy).x ? ShadowIntensity : 0;
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
	float shadow = ComputeShadow(pin.Shadow);

	return float4(0, 0, 0, shadow);
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
