sampler TextureSampler : register(s0);

float2 PixelSize;

static const float KernelOffsets[4] = {-1.5f, -0.5f, 0.5f, 1.5f};

float4 SoftwareScale4x4PS(float2 texCoord:TEXCOORD0) : COLOR0
{
    float4 color = 0;
    for (int x = 0; x < 4; x++)
    {
        for (int y = 0; y < 4; y++)
        {
            float2 Offset = (KernelOffsets[x], KernelOffsets[y]) * PixelSize;
            color += tex2D(TextureSampler, texCoord + Offset);
        }
    }
    color *= (1.0f / 16.0f);
    return color;
}

technique Scale4x4
{
   pass p0
   {
       PixelShader = compile ps_2_0 SoftwareScale4x4PS();
   }
} 