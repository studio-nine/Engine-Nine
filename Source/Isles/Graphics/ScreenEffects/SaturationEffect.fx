//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================


sampler Sampler : register(s0);


float Saturation
<
    string SasUiDescription =  "Gets or sets saturation amount.";
> = 0.5f;


// Helper for modifying the saturation of a color.
float4 AdjustSaturation(float4 color, float saturation)
{
    // The constants 0.3, 0.59, and 0.11 are chosen because the
    // human eye is more sensitive to green light, and less to blue.
    float grey = dot(color, float3(0.3, 0.59, 0.11));

    return lerp(grey, color, saturation);
}


float4 PixelShader(float2 texCoord : TEXCOORD0) : COLOR0
{
    // Look up the bloom and original base image colors.
    return AdjustSaturation(tex2D(Sampler, texCoord), Saturation);
}


technique Saturation
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShader();
    }
}
