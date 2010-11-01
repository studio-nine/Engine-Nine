//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================


// Input parameters.
float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 textureTransform;

float Alpha = 1.0f;

texture Texture;

sampler Sampler : register(s7) = sampler_state
{
    Texture = (Texture);
    
    AddressU = Clamp;
    AddressV = Clamp;
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

    output.TexCoord = mul(position, textureTransform).xy;
    
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
    return ((saturate(input.TexCoord.x) == input.TexCoord.x) &&
			(saturate(input.TexCoord.y) == input.TexCoord.y)) ?

			tex2D(Sampler, input.TexCoord) * Alpha : float4(0, 0, 0, 0);
}


technique DefaultTechnique
{
    pass DefaultPass
    {
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 PS();
    }
}
