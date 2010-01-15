// Pixel shader combines the bloom image with the original
// scene, using tweakable intensity levels and saturation.
// This is the final step in applying a bloom postprocess.

sampler Sampler : register(s0);


float4x4 Matrix;



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
