
float delta;

sampler TextureSampler : register(s0);
sampler lastSampler : register(s1);

float4 PS(float2 texCoord : TEXCOORD0, uniform bool accurate) : COLOR0
{  
    float4 current = tex2D(TextureSampler, texCoord);
    float4 last = tex2D(lastSampler, texCoord);

    float4 diff = current - last;
    float4 inc = abs(diff);
    return last + sign(diff) * clamp(inc * delta, 1.0 / 255, inc);
}


Technique Default
{
    Pass
    {
        PixelShader	 = compile ps_2_0 PS(true);
    }
}
