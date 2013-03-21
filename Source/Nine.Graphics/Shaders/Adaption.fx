float Delta;

sampler TextureSampler:register(s0);
sampler LastSampler:register(s1);

float4 PS(float2 texCoord:TEXCOORD0) : COLOR0
{  
    float4 current = tex2D(TextureSampler, texCoord);
    float4 last = tex2D(LastSampler, texCoord);
    
    float4 gap = abs(current - last);
    float4 increment = gap * min(Delta, 1);    

    return last + sign(current - last) * max(increment, min(gap, 1.0 / 255));
}

Technique Default
{
    Pass
    {
        PixelShader	= compile ps_2_0 PS();
    }
}
