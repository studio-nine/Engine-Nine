//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//  Written By  : Mahdi Khodadadi Fard.
//  Date        : 2010-Feb-19
//  Edit        : -
//=============================================================================

#include "PostProcessVertexShader.fxh"

static const float KernelOffsets[4] = {-1.5f, -0.5f, 0.5f, 1.5f};

float4 LuminanceScale4x4PS(float2 TexCoord : TEXCOORD0) : COLOR0
{
	float4 Color = 0;
	for (int x = 0; x < 4; x++)
	{
		for (int y = 0; y < 4; y++)
		{
			float2 Offset = float2(KernelOffsets[x], KernelOffsets[y]) / SourceTextureDimensions;
			Color += tex2D(PointSampler0, TexCoord + Offset);;
		}
	}

	Color /= 16.0f;
		
	Color = float4(exp(Color.r), 1.0f, 1.0f, 1.0f);
	
	return Color;
}

technique LuminanceScale4x4
{
    pass p0
    {
        VertexShader = compile vs_2_0 PostProcessVS();
        PixelShader  = compile ps_2_0 LuminanceScale4x4PS();
        
        ZEnable = false;
        ZWriteEnable = false;
        AlphaBlendEnable = false;
        AlphaTestEnable = false;
        StencilEnable = false;
    }
}