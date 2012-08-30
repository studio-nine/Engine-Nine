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

    return ((x*(A*x+C*B)+D*E)/(x*(A*x+B)+D*F))-E/F;
}

float4 PS(float2 TexCoord : TEXCOORD0) : COLOR0
{
    float4 color = tex2D( BasicSampler, TexCoord );
    float4 bloom = tex2D( BloomSampler, TexCoord );
    float4 final = color + bloom * BloomIntensity;    
    float avgLum = tex2D(LuminanceSampler, float2(0.5f, 0.5f)).x;
    
    final.rgb = Tonemap(Exposure * final) / Tonemap(avgLum);
    // Xna doesn't support gamma corrected rendering, so we won't do
    // gamma correction using post processing.
    //final.rgb = pow(final.rgb,1/2.2);
    return float4(final.rgb, 1);
}

technique ToneMapping
{
    pass p0
    {
        PixelShader = compile ps_2_0 PS();
    }
}