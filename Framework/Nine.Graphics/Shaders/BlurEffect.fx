sampler TextureSampler : register(s0);
sampler DepthSampler : register(s1);

#define MAXSAMPLECOUNT 15

float2 depthTextureScale;
float2 sampleOffsets[MAXSAMPLECOUNT];
float sampleWeights[MAXSAMPLECOUNT];

float4 PS(float2 texCoord : TEXCOORD0, uniform int sampleCount) : COLOR0
{
    float4 c = 0;
    
    // Combine a number of weighted image filter taps.
    [unroll]
    for (int i = 0; i < sampleCount; i++)
    {
        c += tex2D(TextureSampler, texCoord + sampleOffsets[i]) * sampleWeights[i];
    }
    
    return c;
}

float4 PSDepth(float2 texCoord : TEXCOORD0, uniform int sampleCount) : COLOR0
{
    float4 c0 = tex2D(TextureSampler, texCoord);
    float4 c = c0 * sampleWeights[0];
    float d = tex2D(DepthSampler, texCoord);
    
    // Combine a number of weighted image filter taps.
    [unroll]
    for (int i = 1; i < sampleCount; i++)
    {
        if (tex2D(DepthSampler, texCoord + sampleOffsets[i] * depthTextureScale).r > d)        
            c += tex2D(TextureSampler, texCoord + sampleOffsets[i]) * sampleWeights[i];
        else
            c += c * sampleWeights[i];
    }
    
    return c;
}

int shaderIndex = 0;

PixelShader PSArray[] =
{
    compile ps_2_0 PS(1),
    compile ps_2_0 PS(3),
    compile ps_2_0 PS(5),
    compile ps_2_0 PS(7),
    compile ps_2_0 PS(9),
    compile ps_2_0 PS(11),
    compile ps_2_0 PS(13),
    compile ps_2_0 PS(15),
    
    compile ps_3_0 PSDepth(1),
    compile ps_3_0 PSDepth(3),
    compile ps_3_0 PSDepth(5),
    compile ps_3_0 PSDepth(7),
    compile ps_3_0 PSDepth(9),
    compile ps_3_0 PSDepth(11),
    compile ps_3_0 PSDepth(13),
    compile ps_3_0 PSDepth(15),
};

technique GaussianBlur
{
    pass Pass1
    {
        PixelShader = (PSArray[shaderIndex]);
    }
}
