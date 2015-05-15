void PS(out float4 Depth:COLOR0, out float4 Normal:COLOR1)
{
    // Normals are mapped from [-1, 1] to [0, 1] space. Clear normals to (0, 1, 0)
    Normal = float4(0.5, 1, 0.5, 16.0f / 255);
    Depth = float4(1, 1, 1, 1);
}

Technique Default
{
    Pass
    {
#if DirectX
        PixelShader = compile ps_4_0 PS();
#elif OpenGL
        PixelShader = compile ps_3_0 PS();
#else
        PixelShader = compile ps_2_0 PS();
#endif
    }
}
