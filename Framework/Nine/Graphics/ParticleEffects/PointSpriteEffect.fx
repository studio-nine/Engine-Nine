//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================



// Camera parameters.
float4x4 View;
float4x4 Projection;


// Particle texture and sampler.
texture Texture;

sampler Sampler : register(s0) = sampler_state
{
    Texture = (Texture);
};


// Vertex shader input structure describes the start position and
// velocity of the particle, and the time at which it was created,
// along with some random values that affect its size and rotation.
struct VertexShaderInput
{
    float3 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
    float2 Size : TEXCOORD1;
    float  Rotation : TEXCOORD2;
};


// Pixel shader input structure for particles that do not rotate.
struct PixelShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
};


// Vertex shader helper for computing the rotation of a particle.
float4 ComputeParticleRotation(float rotation)
{
    // Compute a 2x2 rotation matrix.
    float c = cos(rotation);
    float s = sin(rotation);
    
    float4 rotationMatrix = float4(c, -s, s, c);
    
    return rotationMatrix;
}


// Custom vertex shader animates particles entirely on the GPU.
PixelShaderInput VSNoRotation(VertexShaderInput input)
{
    PixelShaderInput output;
    
    output.Color = input.Color;
	output.TextureCoordinate = input.TextureCoordinate;
	output.TextureCoordinate.y = -output.TextureCoordinate.y;

    // Compute the particle position, size, color, and rotation.
	float4 viewPos = mul(float4(input.Position, 1), View);

	viewPos.xy += -input.Size / 2 + input.Size * input.TextureCoordinate;

    output.Position = mul(viewPos, Projection);
    
    return output;
}

// Custom vertex shader animates particles entirely on the GPU.
PixelShaderInput VSRotation(VertexShaderInput input)
{
    PixelShaderInput output;
    
    output.Color = input.Color;
	output.TextureCoordinate = input.TextureCoordinate;
	output.TextureCoordinate.y = -output.TextureCoordinate.y;

    // Compute the particle position, size, color, and rotation.
	float4 viewPos = mul(float4(input.Position, 1), View);

	float2 offset = -input.Size / 2 + input.Size * input.TextureCoordinate;

	viewPos.xy += mul(offset, (float2x2)ComputeParticleRotation(input.Rotation));

    output.Position = mul(viewPos, Projection);
    
    return output;
}

// Pixel shader for drawing particles that do not rotate.
float4 PS(PixelShaderInput input) : COLOR0
{
    return tex2D(Sampler, input.TextureCoordinate) * input.Color;
}


// Effect technique for drawing particles that do not rotate. Works with shader 1.1.
technique NonRotatingParticles
{
    pass P0
    {
        VertexShader = compile vs_2_0 VSNoRotation();
        PixelShader = compile ps_2_0 PS();
    }
}


// Effect technique for drawing particles that can rotate. Requires shader 2.0.
technique RotatingParticles
{
    pass P0
    {
        VertexShader = compile vs_2_0 VSRotation();
        PixelShader = compile ps_2_0 PS();
    }
}
