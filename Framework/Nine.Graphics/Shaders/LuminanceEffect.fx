float2  HalfTexel;
sampler Sampler : register(s0);

static const float KernelOffsets2[2] = {-1, 1};
static const float KernelOffsets4[4] = {-3, -1, 1, 3};

float4 LuminancePS(float2 TexCoord:TEXCOORD0) : COLOR0
{
    float average = 0.0f;
    float maximum = -1e20;
        
    for (int x = 0; x < 2; x++)
    {
        for (int y = 0; y < 2; y++)
        {
            float2 Offset = float2(KernelOffsets2[x], KernelOffsets2[y]) * HalfTexel;
            float4 color = tex2D(Sampler, TexCoord + Offset);

            float GreyValue = dot(color.rgb, float3(0.299f, 0.587f, 0.114f));

            // TODO: Apply a max luminance
            maximum = max(maximum, GreyValue);
            average += (0.25f * log( 1e-5 + GreyValue ));
        }
    }

    average = exp(average);
    return float4(average, maximum, 0, 1.0f);
}

float4 LuminanceScalePS(float2 TexCoord:TEXCOORD0) : COLOR0
{
    float average = 0.0f;
    float maximum = -1e20;
        
    for (int x = 0; x < 4; x++)
    {
        for (int y = 0; y < 4; y++)
        {
            float2 Offset = float2(KernelOffsets4[x], KernelOffsets4[y]) * HalfTexel;
            float4 color = tex2D(Sampler, TexCoord + Offset);

            average += color.r;
            maximum = max(maximum, color.g);
        }
    }

    average *= (1.0f / 16);
    return float4(average, maximum, 0, 1.0f);
}

int shaderIndex = 0;

PixelShader PSArray[] =
{
    compile ps_2_0 LuminancePS(),
    compile ps_2_0 LuminanceScalePS(),
};

technique Luminance
{
    pass p0
    {
        PixelShader = (PSArray[shaderIndex]);
    }
}