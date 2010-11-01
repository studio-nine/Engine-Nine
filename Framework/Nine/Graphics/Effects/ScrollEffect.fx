//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================


// Input parameters.
float4x4 World;
float4x4 View;
float4x4 Projection;
float2 TextureScale;
float2 textureOffset;

float Alpha = 1.0f;

texture Texture;

sampler Sampler : register(s8) = sampler_state
{
    Texture = (Texture);
    
    AddressU = Wrap;
    AddressV = Wrap;
};


// Vertex shader input structure.
struct VS_INPUT
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};


// Vertex shader output structure.
struct VS_OUTPUT
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};


// Vertex shader program.
VS_OUTPUT VS(VS_INPUT input)
{
    VS_OUTPUT output;
    
    float4 position = mul(input.Position, World);
    
    output.Position = mul(mul(position, View), Projection);

    output.TexCoord = -textureOffset + input.TexCoord / TextureScale;
    
    return output;
}


// Pixel shader input structure.
struct PS_INPUT
{
    float2 TexCoord : TEXCOORD0;
};


// Pixel shader program.
float4 PS(PS_INPUT input) : COLOR0
{
    float4 color = tex2D(Sampler, input.TexCoord);

    color *= Alpha;
    
    return color;
}


technique DefaultTechnique
{
    pass DefaultPass
    {
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 PS();
    }
}
