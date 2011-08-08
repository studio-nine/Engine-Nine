//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//  Written By  : Mahdi Khodadadi Fard.
//  Date        : 2010-Feb-19
//  Edit        : -
//=============================================================================

float2    pixelSize;

sampler2D PointSampler0 : register(s0);

static const float KernelOffsets[4] = {-1.5f, -0.5f, 0.5f, 1.5f};

float4 SoftwareScale4x4PS(float2 TexCoord : TEXCOORD0) : COLOR0
{
    float4 Color = 0;
    for (int x = 0; x < 4; x++)
    {
        for (int y = 0; y < 4; y++)
        {
            float2 Offset = (KernelOffsets[x], KernelOffsets[y]) * pixelSize;
            Color += tex2D(PointSampler0, TexCoord + Offset);;
        }
    }

    Color /= 16.0f;
    
    return Color;
}

technique Scale4x4
{
   pass p0
   {
       PixelShader = compile ps_2_0 SoftwareScale4x4PS();
   }
} 