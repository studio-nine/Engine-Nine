//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================


// Pixel shader extracts the brighter areas of an image.
// This is the first step in applying a bloom postprocess.

sampler TextureSampler : register(s0);

float Threshold = 0.5f;


float4 PS(float2 texCoord : TEXCOORD0) : COLOR0
{
    // Look up the original image color.
    float4 c = tex2D(TextureSampler, texCoord);

    // Adjust it to keep only values brighter than the specified threshold.
    return max(c - Threshold, 0);
}


technique BloomExtract
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PS();
    }
}
