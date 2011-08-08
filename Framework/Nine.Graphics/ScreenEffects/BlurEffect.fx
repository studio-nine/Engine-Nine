//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================


sampler TextureSampler : register(s0);

#define SAMPLE_COUNT 15

float2 sampleOffsets[SAMPLE_COUNT];
float sampleWeights[SAMPLE_COUNT];

float4 PixelShader15(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 c = 0;
    
    // Combine a number of weighted image filter taps.
    for (int i = 0; i < 15; i++)
    {
        c += tex2D(TextureSampler, texCoord + sampleOffsets[i]) * sampleWeights[i];
    }
    
    return c;
}
float4 PixelShader11(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 c = 0;
    
    // Combine a number of weighted image filter taps.
    for (int i = 0; i < 11; i++)
    {
        c += tex2D(TextureSampler, texCoord + sampleOffsets[i]) * sampleWeights[i];
    }
    
    return c;
}
float4 PixelShader7(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 c = 0;
    
    // Combine a number of weighted image filter taps.
    for (int i = 0; i < 7; i++)
    {
        c += tex2D(TextureSampler, texCoord + sampleOffsets[i]) * sampleWeights[i];
    }
    
    return c;
}
float4 PixelShader3(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 c = 0;
    
    // Combine a number of weighted image filter taps.
    for (int i = 0; i < 3; i++)
    {
        c += tex2D(TextureSampler, texCoord + sampleOffsets[i]) * sampleWeights[i];
    }
    
    return c;
}

int shaderIndex = 3;

PixelShader PSArray[4] =
{
	compile ps_2_0 PixelShader3(),
	compile ps_2_0 PixelShader7(),
	compile ps_2_0 PixelShader11(),
	compile ps_2_0 PixelShader15(),
};

technique GaussianBlur
{
    pass Pass1
    {
        PixelShader = (PSArray[shaderIndex]);
    }
}
