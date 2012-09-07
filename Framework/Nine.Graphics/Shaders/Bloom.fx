float BloomIntensity;

sampler BasicSampler : register(s0);
sampler BloomSampler : register(s2);

float4 PS(float2 TexCoord : TEXCOORD0) : COLOR0
{
    float4 color = tex2D( BasicSampler, TexCoord );
    float4 bloom = tex2D( BloomSampler, TexCoord );
    float4 final = color + bloom * BloomIntensity;
    return float4(final.rgb, 1);
}

technique Bloom
{
    pass p0
    {
        PixelShader = compile ps_2_0 PS();
    }
}