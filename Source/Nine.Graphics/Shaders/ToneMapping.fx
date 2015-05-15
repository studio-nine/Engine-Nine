float Exposure;
float BloomIntensity;

sampler BasicSampler : register(s0);
sampler LuminanceSampler : register(s1);
sampler BloomSampler : register(s2);

float3 Tonemap(float3 x)
{
    float A = 0.22;
    float B = 0.30;
    float C = 0.10;
    float D = 0.20;
    float E = 0.02;
    float F = 0.30;

    // ((x*(0.22*x+0.1*0.3)+0.2*0.02)/(x*(0.22*x+0.3)+0.2*0.3))-0.02/0.3
    return ((x*(A*x+C*B)+D*E)/(x*(A*x+B)+D*F))-E/F;
}

float4 PS(float2 TexCoord : TEXCOORD0) : COLOR0
{
    float4 color = tex2D( BasicSampler, TexCoord );
    float4 bloom = tex2D( BloomSampler, TexCoord );
    float4 final = color + bloom * BloomIntensity;    
    float avgLum = tex2D(LuminanceSampler, float2(0.5f, 0.5f)).x;    
    final.rgb = Tonemap(Exposure * final) / Tonemap(max(0.002f, avgLum));
    return float4(final.rgb, 1);
}

technique ToneMapping
{
    pass p0
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