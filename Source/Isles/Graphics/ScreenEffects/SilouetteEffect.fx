//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================

sampler TextureSampler : register(s0);

float4 Threshold = 1;

// Size of a one texel offset, this is hardcoded to 256
// for simplicity and since RenderMonkey cannot allow you
// to know the size of a texture,
const float off = 0.1 / 256.0;

float4 PS(float2 TexCoord : TEXCOORD0) : COLOR0
{
	// Sample the neighbor pixels
	float4 s00 = tex2D(TextureSampler, TexCoord + float2(-off, -off));
	float4 s01 = tex2D(TextureSampler, TexCoord + float2( 0, -off));
	float4 s02 = tex2D(TextureSampler, TexCoord + float2( off, -off));
	float4 s10 = tex2D(TextureSampler, TexCoord + float2(-off, 0));
	float4 s12 = tex2D(TextureSampler, TexCoord + float2( off, 0));
	float4 s20 = tex2D(TextureSampler, TexCoord + float2(-off, off));
	float4 s21 = tex2D(TextureSampler, TexCoord + float2( 0, off));
	float4 s22 = tex2D(TextureSampler, TexCoord + float2( off, off));
	
	// Sobel filter in X and Y directions
	float4 sobelX = s00 + 2 * s10 + s20 - s02 - 2 * s12 - s22;
	float4 sobelY = s00 + 2 * s01 + s02 - s20 - 2 * s21 - s22;

	// Find edge using a threshold of 0.07 which is sufficient
	// to detect most edges.
	float4 edgeSqr = (sobelX * sobelX + sobelY * sobelY);
	return 1.0 - (edgeSqr > (Threshold * Threshold * 0.0049));
}

technique RenderScene
{
    pass p0
    {
        PixelShader = compile ps_2_0 PS();
    }
}