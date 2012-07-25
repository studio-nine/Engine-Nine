sampler TextureSampler : register(s0);

float2 HalfPixel;

float Threshold = 0.5f;

static const float KernelOffsets[2] = {-1, 1};


float4 PS(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 color = 0;
    for (int x = 0; x < 2; x++)
    {
        for (int y = 0; y < 2; y++)
        {
            float2 Offset = float2(KernelOffsets[x], KernelOffsets[y]) * HalfPixel;
            color += tex2D(TextureSampler, texCoord + Offset);
        }
    }
    color *= (1.0f / 4.0f);
    
    float luminance = dot(color.rgb, float3(0.299f, 0.587f, 0.114f));
    color.rgb = max(luminance - Threshold, 0);
    return color;
}


technique BloomExtract
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PS();
    }
}
