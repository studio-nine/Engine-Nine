//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//  Written By  : Mahdi Khodadadi Fard.
//  Date        : 2010-Feb-19
//  Edit        : -
//=============================================================================

#include "PostProcessVertexShader.fxh"

float Exposure = 0.0;
float Gamma = 1.0 / 2.2;


float4 ToneMapPS(float2 TexCoord : TEXCOORD0) : COLOR0
{
	float3 c = tex2D(LinearSampler0, TexCoord);
    c *= pow(2, Exposure);
    c = pow(c, Gamma);
    return float4(c, 1.0);

}

technique ToneMap
{
    pass p0
    {
        VertexShader = compile vs_2_0 PostProcessVS();
        PixelShader = compile ps_2_0 ToneMapPS();
        
        ZEnable = false;
        ZWriteEnable = false;
        AlphaBlendEnable = false;
        AlphaTestEnable = false;
        StencilEnable = false;
    }
}

