//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//  Written By  : Mahdi Khodadadi Fard.
//  Date        : 2010-Feb-19
//  Edit        : -
//=============================================================================

#include "PostProcessVertexShader.fxh"
float Threshold = 0.25f;


float4 ThresholdPS(float2 TexCoord : TEXCOORD0) : COLOR0 
{
	float4 c = tex2D(PointSampler0, TexCoord);

    // Adjust it to keep only values brighter than the specified threshold.
    return saturate((c - Threshold) / (1 - Threshold));
}

technique ThresholdFilter
{
    pass p0
    {
        VertexShader = compile vs_2_0 PostProcessVS();
        PixelShader = compile ps_2_0 ThresholdPS();
        
        ZEnable = false;
        ZWriteEnable = false;
        AlphaBlendEnable = false;
        AlphaTestEnable = false;
        StencilEnable = false;
    }
}
