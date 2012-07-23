//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//  Written By  : Mahdi Khodadadi Fard.
//  Date        : 2010-Feb-19
//  Edit        : -
//=============================================================================


float2  halfPixel;
sampler Sampler : register(s0);

static const float KernelOffsets[2] = {-1, 1};

float4 LuminancePS(float2 TexCoord : TEXCOORD0) : COLOR0
{
        float average = 0.0f;
        float maximum = -1e20;
        float4 color = 0.0f;
        float3 WEIGHT = float3( 0.299f, 0.587f, 0.114f );
        
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                float2 Offset = (KernelOffsets[x], KernelOffsets[y]) * halfPixel;
                color += tex2D(Sampler, TexCoord + Offset);

                float GreyValue = dot( color.rgb, WEIGHT );

                maximum = max( maximum, GreyValue );
                average += (0.25f * log( 1e-5 + GreyValue ));
            }
        }

        average = exp( average );
        return float4( average, maximum, 0, 1.0f );
}

technique Luminance
{
    pass p0
    {
        PixelShader = compile ps_2_0 LuminancePS();
    }
}