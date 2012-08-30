sampler TextureSampler : register(s0);

float2 sampleOffsets[15];
float sampleWeights[15];

float4 PS(float2 texCoord : TEXCOORD0, uniform int sampleCount) : COLOR0
{
    float4 c = 0;
    
    // Combine a number of weighted image filter taps.
    [unroll]
    for (int i = 0; i < sampleCount; i++)
    {
        c += tex2D(TextureSampler, texCoord + sampleOffsets[i]) * sampleWeights[i];
    }
    
    return c;
}

float4 PSDepth(float2 texCoord : TEXCOORD0, uniform int sampleCount) : COLOR0
{
    float4 c0 = tex2D(TextureSampler, texCoord);
    float3 c = c0.rgb * sampleWeights[0];
    
    // Combine a number of weighted image filter taps.
    [unroll]
    for (int i = 1; i < sampleCount; i++)
    {
        float4 map = tex2D(TextureSampler, texCoord + sampleOffsets[i]);
        c += lerp(c0.rgb, map.rgb, map.a) * sampleWeights[i];
    }
    
    return float4(c, c0.a);
}

float4 PS1(float2 texCoord : TEXCOORD0) : COLOR0 { return PS(texCoord,1); }
float4 PS3(float2 texCoord : TEXCOORD0) : COLOR0 { return PS(texCoord,3); }
float4 PS5(float2 texCoord : TEXCOORD0) : COLOR0 { return PS(texCoord,5); }
float4 PS7(float2 texCoord : TEXCOORD0) : COLOR0 { return PS(texCoord,7); }
float4 PS9(float2 texCoord : TEXCOORD0) : COLOR0 { return PS(texCoord,9); }
float4 PS11(float2 texCoord : TEXCOORD0) : COLOR0 { return PS(texCoord,11); }
float4 PS13(float2 texCoord : TEXCOORD0) : COLOR0 { return PS(texCoord,13); }
float4 PS15(float2 texCoord : TEXCOORD0) : COLOR0 { return PS(texCoord,15); }

float4 PSDepth1(float2 texCoord : TEXCOORD0) : COLOR0 { return PSDepth(texCoord,1); }
float4 PSDepth3(float2 texCoord : TEXCOORD0) : COLOR0 { return PSDepth(texCoord,3); }
float4 PSDepth5(float2 texCoord : TEXCOORD0) : COLOR0 { return PSDepth(texCoord,5); }
float4 PSDepth7(float2 texCoord : TEXCOORD0) : COLOR0 { return PSDepth(texCoord,7); }
float4 PSDepth9(float2 texCoord : TEXCOORD0) : COLOR0 { return PSDepth(texCoord,9); }
float4 PSDepth11(float2 texCoord : TEXCOORD0) : COLOR0 { return PSDepth(texCoord,11); }
float4 PSDepth13(float2 texCoord : TEXCOORD0) : COLOR0 { return PSDepth(texCoord,13); }
float4 PSDepth15(float2 texCoord : TEXCOORD0) : COLOR0 { return PSDepth(texCoord,15); }

technique t1 { pass p0 { PixelShader = compile ps_2_0 PS1(); } }
technique t2 { pass p0 { PixelShader = compile ps_2_0 PS3(); } }
technique t3 { pass p0 { PixelShader = compile ps_2_0 PS5(); } }
technique t4 { pass p0 { PixelShader = compile ps_2_0 PS7(); } }
technique t5 { pass p0 { PixelShader = compile ps_2_0 PS9(); } }
technique t6 { pass p0 { PixelShader = compile ps_2_0 PS11(); } }
technique t7 { pass p0 { PixelShader = compile ps_2_0 PS13(); } }
technique t8 { pass p0 { PixelShader = compile ps_2_0 PS15(); } }


technique td1 { pass p0 { PixelShader = compile ps_2_0 PSDepth1(); } }
technique td2 { pass p0 { PixelShader = compile ps_2_0 PSDepth3(); } }
technique td3 { pass p0 { PixelShader = compile ps_2_0 PSDepth5(); } }
technique td4 { pass p0 { PixelShader = compile ps_2_0 PSDepth7(); } }
technique td5 { pass p0 { PixelShader = compile ps_2_0 PSDepth9(); } }
technique td6 { pass p0 { PixelShader = compile ps_2_0 PSDepth11(); } }
technique td7 { pass p0 { PixelShader = compile ps_2_0 PSDepth13(); } }
technique td8 { pass p0 { PixelShader = compile ps_2_0 PSDepth15(); } }