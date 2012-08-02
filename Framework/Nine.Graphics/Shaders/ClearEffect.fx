void PS(out float4 Depth:COLOR0, out float4 Normal:COLOR1)
{
    Normal = float4(0, 0, 1, 16.0f / 255);
    Depth = float4(1, 1, 1, 1);
}

Technique Default
{
    Pass
    {
        PixelShader	 = compile ps_2_0 PS();
    }
}
