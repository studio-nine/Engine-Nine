//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//  Written By  : Mahdi Khodadadi Fard.
//  Date        : 2010-Feb-19
//  Edit        : -
//=============================================================================


sampler Sampler : register(s0);

static const float3 LUM_CONVERT = float3(0.299f, 0.587f, 0.114f);

float4 LuminancePS(float2 TexCoord : TEXCOORD0) : COLOR0
{
    float4 sample = tex2D(Sampler, TexCoord);
    float luminance = dot(sample.rgb, LUM_CONVERT);
                
    float logLuminace = log(0.0001 + luminance); 
    return float4(logLuminace, 1.0f, 0.0f, 0.0f);
}

technique Luminance
{
    pass p0
    {
        PixelShader = compile ps_2_0 LuminancePS();
    }
}