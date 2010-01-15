//-----------------------------------------------------------------------------
// SkinnedModel.fx
//
// Microsoft Game Technology Group
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


// Input parameters.
float4x4 World;
float4x4 View;
float4x4 Projection;
float2 TextureOffset;
float2 TextureScale;

float Alpha = 1.0f;

texture Texture;

sampler Sampler = sampler_state
{
    Texture = (Texture);

    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    
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
VS_OUTPUT VertexShader(VS_INPUT input)
{
    VS_OUTPUT output;
    
    // Skin the vertex position.
    float4 position = mul(input.Position, World);
    
    output.Position = mul(mul(position, View), Projection);

    output.TexCoord = TextureOffset + TextureScale * input.TexCoord;
    
    return output;
}


// Pixel shader input structure.
struct PS_INPUT
{
    float2 TexCoord : TEXCOORD0;
};


// Pixel shader program.
float4 PixelShader(PS_INPUT input) : COLOR0
{
    float4 color = tex2D(Sampler, input.TexCoord);

    color *= Alpha;
    
    return color;
}


technique SkinnedModelTechnique
{
    pass SkinnedModelPass
    {
        VertexShader = compile vs_2_0 VertexShader();
        PixelShader = compile ps_2_0 PixelShader();
    }
}
