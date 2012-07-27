void PS(out float4 Normal:COLOR0, out float4 Depth:COLOR1)
{
    Normal = float4(0, 0, 1, 1);
    Depth = float4(1, 0, 0, 0);
}

Technique Default
{
    Pass
    {
        PixelShader	 = compile ps_2_0 PS();
    }
}
