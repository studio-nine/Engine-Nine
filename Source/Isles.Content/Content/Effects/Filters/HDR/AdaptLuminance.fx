//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//  Written By  : Mahdi Khodadadi Fard.
//  Date        : 2010-Feb-19
//  Edit        : -
//=============================================================================

#include "PostProcessVertexShader.fxh"
float DeltaTime;

float4 AdaptLuminancePS() : COLOR0
{
    float2 currentFrame = tex2D(PointSampler0, float2(0.5f, 0.5f)).rg;
    float2 lastFrame = tex2D(PointSampler1, float2(0.5f, 0.5f)).rg;
    
    float2 nextFrame = lastFrame + (currentFrame - lastFrame) * (1 - exp(-DeltaTime * 0.5));
    
    return float4(nextFrame, 1.0f, 1.0f);
}

technique AdaptLuminance
{
    pass p0
    {
        VertexShader = compile vs_2_0 PostProcessVS();
        PixelShader = compile ps_2_0 AdaptLuminancePS();
        
        ZEnable = false;
        ZWriteEnable = false;
        AlphaBlendEnable = false;
        AlphaTestEnable = false;
        StencilEnable = false;
    }
}

