sampler TextureSampler : register(s0);

float2 HalfTexel;

static const float KernelOffsets[4] = {-3, -1, 1, 3};

float4 SoftwareScale4x4PS(float2 texCoord:TEXCOORD0) : COLOR0
{
    float4 color = 0;
    for (int x = 0; x < 4; x++)
    {
        for (int y = 0; y < 4; y++)
        {
            float2 Offset = float2(KernelOffsets[x], KernelOffsets[y]) * HalfTexel;
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
#if DirectX
       PixelShader = compile ps_4_0 SoftwareScale4x4PS();
#elif OpenGL
       PixelShader = compile ps_3_0 SoftwareScale4x4PS();
#else
       PixelShader = compile ps_2_0 SoftwareScale4x4PS();
#endif
   }
} 