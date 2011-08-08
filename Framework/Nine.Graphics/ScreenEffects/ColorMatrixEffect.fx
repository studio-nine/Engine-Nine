//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================


sampler Sampler : register(s0);


float4x4 Transform
<
    string SasUiDescription =  "Gets or sets the color transform matrix. See MatrixExtensions.";
> = 
{
	1, 0, 0, 0,
	0, 1, 0, 0,
	0, 0, 1, 0,
	0, 0, 0, 1,
};


float4 PS(float2 texCoord : TEXCOORD0) : COLOR0
{
    // Look up the bloom and original base image colors.
    return mul(tex2D(Sampler, texCoord), Transform);
}


technique Saturation
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PS();
    }
}
