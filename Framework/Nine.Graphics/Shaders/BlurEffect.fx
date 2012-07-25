sampler TextureSampler : register(s0);

#define PROFILE ps_2_0
#define MAXSAMPLECOUNT 15

float2 sampleOffsets[MAXSAMPLECOUNT];
float sampleWeights[MAXSAMPLECOUNT];

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

int shaderIndex = 0;

PixelShader PSArray[] =
{
    compile PROFILE PS(1),
    compile PROFILE PS(3),
    compile PROFILE PS(5),
    compile PROFILE PS(7),
    compile PROFILE PS(9),
    compile PROFILE PS(11),
    compile PROFILE PS(13),
    compile PROFILE PS(15),
};

technique GaussianBlur
{
    pass Pass1
    {
        PixelShader = (PSArray[shaderIndex]);
    }
}
