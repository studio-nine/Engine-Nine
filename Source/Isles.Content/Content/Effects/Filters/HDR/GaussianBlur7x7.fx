//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//  Written By  : Mahdi Khodadadi Fard.
//  Date        : 2010-Feb-19
//  Edit        : -
//=============================================================================

#include "PostProcessVertexShader.fxh"

float Sigma = 2.5f;

float GaussWeight(int pixel)
{
	float sigmaPow2 = Sigma * Sigma;
	float g = sqrt(2.0f * 3.14159265 * sigmaPow2); 
	
	return ((exp(-(pixel * pixel) / g) / (2 * sigmaPow2)));
}

float4 GaussianBlurH(float2 TexCoord : TEXCOORD0, uniform int KernelSize) : COLOR0
{
    float4 Color = 0;
	float2 vTexCoord = TexCoord;
	
    for (int i = -KernelSize; i < KernelSize; i++)
    {
		vTexCoord.x = TexCoord.x + (i / SourceTextureDimensions.x);
		  
		float weight = GaussWeight(i);
		float4 sample = tex2D(PointSampler0, vTexCoord);
		
		Color += sample * weight;
    }
	
	return Color;
}

float4 GaussianBlurV(float2 TexCoord : TEXCOORD0, uniform int KernelSize) : COLOR0
{
    float4 Color = 0;
	float2 vTexCoord = TexCoord;

    for (int i = -KernelSize; i < KernelSize; i++)
    {
		vTexCoord.y = TexCoord.y + (i / SourceTextureDimensions.y); 
		
		float weight = GaussWeight(i);
		float4 sample = tex2D(PointSampler0, vTexCoord);
		
		Color += sample * weight;
    }

    return Color;
}

int ShaderIndex = 0;

PixelShader PSArray[] = 
{
	compile ps_2_0 GaussianBlurH(7),
	compile ps_2_0 GaussianBlurV(7),
};

technique GaussianBlur7x7
{
    pass p0
    {
        VertexShader = compile vs_2_0 PostProcessVS();
        PixelShader = (PSArray[ShaderIndex]);
        
        ZEnable = false;
        ZWriteEnable = false;
        AlphaBlendEnable = false;
        AlphaTestEnable = false;
        StencilEnable = false;
    }
}