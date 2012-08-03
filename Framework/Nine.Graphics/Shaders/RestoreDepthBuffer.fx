sampler DepthSampler : register(s0);

void PS(float2 uv:TEXCOORD0, out float Depth:DEPTH0, out float4 Color:COLOR0)
{
    Depth = tex2D(DepthSampler, uv).r;
    Color = float4(0,0,0,1);
}

Technique Default
{
    Pass
    {
        PixelShader	 = compile ps_2_0 PS();
    }
}
