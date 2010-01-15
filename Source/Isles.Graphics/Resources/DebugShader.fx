//-----------------------------------------------------------------------------
// BasicEffect.fx
//
// This is a simple shader that supports 1 ambient and 3 directional lights.
// All lighting computations happen in world space.
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


#define MaxWaves 3

uniform const float3	EyePosition		: register(c4);		// in world space


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

uniform const float3	AmbientLightColor		: register(c10);

uniform const float3	LightPosition		: register(c11);
uniform const float3	LightDiffuseColor	: register(c12);
uniform const float3	LightSpecularColor	: register(c13);


//-----------------------------------------------------------------------------
// Matrices
//-----------------------------------------------------------------------------

uniform const float4x4	World		: register(vs, c20);	// 20 - 23
uniform const float4x4	View		: register(vs, c24);	// 24 - 27
uniform const float4x4	Projection	: register(vs, c28);	// 28 - 31
uniform const float4x4  ReflectionViewProjection;



//-----------------------------------------------------------------------------
// Textures
//-----------------------------------------------------------------------------


texture ReflectionTexture;
texture RefractionTexture;
texture WaveTexture;

float2 WaveTextureScale;
float2 WaveTextureOffset;
float2 DistortionScale;


sampler ReflectionSampler = sampler_state
{
    Texture = (ReflectionTexture);

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    
	AddressU = Mirror;
	AddressV = Mirror;
};
sampler RefractionSampler = sampler_state
{
    Texture = (RefractionTexture);

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    
	AddressU = Mirror;
	AddressV = Mirror;
};
sampler WaveSampler = sampler_state
{
    Texture = (WaveTexture);

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    
	AddressU = Wrap;
	AddressV = Wrap;
};


//-----------------------------------------------------------------------------
// Structure definitions
//-----------------------------------------------------------------------------

struct ColorPair
{
	float3 Diffuse;
	float3 Specular;
};


//-----------------------------------------------------------------------------
// Compute lighting
// E: Eye-Vector
// N: Unit vector normal in world space
//-----------------------------------------------------------------------------
ColorPair ComputeLights(float3 L, float3 E, float3 N)
{
	ColorPair result;
	
	result.Diffuse = AmbientLightColor;
	result.Specular = 0;

	// Directional Light 0
	float3 H = normalize(E + L);
	float2 ret = lit(dot(N, L), dot(N, H), SpecularPower).yz;
	result.Diffuse += LightDiffuseColor * ret.x;
	result.Specular += LightSpecularColor * ret.y;
	
	
	result.Diffuse *= DiffuseColor;
	result.Diffuse	+= EmissiveColor;
	result.Specular	*= SpecularColor;
		
	return result;
}


// Vertex shader program.
void VertexShader(float4 position : POSITION0,
				  float2 uv : TEXCOORD0,
				  
				  out float4 oPosition : POSITION0,
				  out float2 oUV : TEXCOORD0,
				  out float2 oReflect : TEXCOORD2,
				  out float2 oWave : TEXCOORD1,
				  out float2 oPosWs : TEXCOORD3)
{
	float4 pos_ws = mul(position, World);
	float4 pos_vs = mul(pos_ws, View);
	float4 pos_ps = mul(pos_vs, Projection);
	float4 pos_rs = mul(pos_ws, ReflectionViewProjection);
	
	oPosition = pos_ps;
	oUV = uv;
	oPosWs = pos_ws.xyz;
	
	oReflect = pos_rs.xy / pos_rs.w;
	oReflect = oReflect * 0.5 + float2(0.5, 0.5);
	oReflect.y = 1 - oReflect.y;
	
	oWave.xy = pos_ws.xy;	
}



// Pixel shader program.
float4 PixelShader(float2 uv : TEXCOORD0,
				   float2 wave : TEXCOORD1,
				   float2 rUV : TEXCOORD2,
				   float3 pos_ws : TEXCOORD3) : COLOR0
{	
	// HACK: We always assume the water surface is facing up (positive z)
	float3 N1 = tex2D(WaveSampler, uv * WaveTextureScale + WaveTextureOffset).xyz;
	float3 N2 = tex2D(WaveSampler, uv).xyz;
	
	N1.xy = N1.xy * 2 - 1;
	N2.xy = N2.xy * 2 - 1;
	
	float3 N = normalize(N1 * 2 + N2);
	
	float3 posToEye = EyePosition - pos_ws;
	float3 E = normalize(posToEye);
	float3 L = normalize(LightPosition - pos_ws);
	
	ColorPair lightResult = ComputeLights(L, E, N);
	
	
	return tex2D(ReflectionSampler, rUV + N.xy * DistortionScale) *
		   float4(lightResult.Diffuse, Alpha) + float4(lightResult.Specular.rgb, 0);
}



technique Default
{
    pass Default
    {
        VertexShader = compile vs_2_0 VertexShader();
        PixelShader = compile ps_2_0 PixelShader();
    }
}