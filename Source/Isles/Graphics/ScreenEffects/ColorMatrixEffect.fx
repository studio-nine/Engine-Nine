//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================


sampler Sampler : register(s0);


float4x4 Matrix
<
    string SasUiDescription =  "Gets or sets the color transform matrix. See MatrixExtensions.";
> = 0.5f;


float4 PixelShader(float2 texCoord : TEXCOORD0) : COLOR0
{
    // Look up the bloom and original base image colors.
    return mul(tex2D(Sampler, texCoord), Matrix);
}


technique Saturation
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShader();
    }
}
