//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================


float4x4 View;
float4x4 Projection;

texture Texture;

sampler Sampler = sampler_state
{
    Texture = (Texture);
    
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Point;
    
    AddressU = Wrap;
    AddressV = Wrap;
};



struct VertexShaderInput
{
    float3 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};



struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};



VertexShaderOutput VertexShader(VertexShaderInput input)
{
    VertexShaderOutput output;
    
    output.Position = mul(mul(float4(input.Position, 1), View), Projection);
    output.TexCoord = input.TexCoord;
    output.Color = input.Color;
    
    return output;
}


struct PixelShaderInput
{
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};



float4 PixelShader(PixelShaderInput input) : COLOR0
{
    return tex2D(Sampler, input.TexCoord) * input.Color;
}


technique Default
{
    pass P0
    {
        VertexShader = compile vs_1_1 VertexShader();
        PixelShader = compile ps_1_1 PixelShader();
    }
}