//-----------------------------------------------------------------------------
// ParticleEffect.fx
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------


// Camera parameters.
float4x4 View;
float4x4 Projection;
float ViewportHeight;


// Particle texture and sampler.
texture Texture;

sampler Sampler = sampler_state
{
    Texture = (Texture);
    
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Point;
    
    AddressU = Clamp;
    AddressV = Clamp;
};


// Vertex shader input structure describes the start position and
// velocity of the particle, and the time at which it was created,
// along with some random values that affect its size and rotation.
struct VertexShaderInput
{
    float3 Position : POSITION0;
    float4 Color : COLOR0;
    float  Size : TEXCOORD0;
    float  Rotation : TEXCOORD1;
};


// Vertex shader output structure specifies the position, size, and
// color of the particle, plus a 2x2 rotation matrix (packed into
// a float4 value because we don't have enough color interpolators
// to send this directly as a float2x2).
struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float Size : PSIZE0;
    float4 Color : COLOR0;
    float4 Rotation : COLOR1;
};


// Vertex shader helper for computing the rotation of a particle.
float4 ComputeParticleRotation(float rotation)
{
    // Compute a 2x2 rotation matrix.
    float c = cos(rotation);
    float s = sin(rotation);
    
    float4 rotationMatrix = float4(c, -s, s, c);
    
    // Normally we would output this matrix using a texture coordinate interpolator,
    // but texture coordinates are generated directly by the hardware when drawing
    // point sprites. So we have to use a color interpolator instead. Only trouble
    // is, color interpolators are clamped to the range 0 to 1. Our rotation values
    // range from -1 to 1, so we have to scale them to avoid unwanted clamping.
    
    rotationMatrix *= 0.5;
    rotationMatrix += 0.5;
    
    return rotationMatrix;
}


// Custom vertex shader animates particles entirely on the GPU.
VertexShaderOutput VertexShader(VertexShaderInput input)
{
    VertexShaderOutput output;
    
    // Compute the particle position, size, color, and rotation.
    output.Position = mul(mul(float4(input.Position, 1), View), Projection);
    output.Size = input.Size * Projection._m11 / output.Position.w * ViewportHeight / 2;
    output.Color = input.Color;
    output.Rotation = ComputeParticleRotation(input.Rotation);
    
    return output;
}


// Pixel shader input structure for particles that do not rotate.
struct NonRotatingPixelShaderInput
{
    float4 Color : COLOR0;
    
#ifdef XBOX
    float2 TextureCoordinate : SPRITETEXCOORD;
#else
    float2 TextureCoordinate : TEXCOORD0;
#endif
};


// Pixel shader for drawing particles that do not rotate.
float4 NonRotatingPixelShader(NonRotatingPixelShaderInput input) : COLOR0
{
    float2 textureCoordinate = input.TextureCoordinate;

	// To make sure two techniques produce the same output size
    textureCoordinate -= 0.5;
    textureCoordinate *= sqrt(2);
    textureCoordinate += 0.5;

    return tex2D(Sampler, textureCoordinate) * input.Color;
}


// Pixel shader input structure for particles that can rotate.
struct RotatingPixelShaderInput
{
    float4 Color : COLOR0;
    float4 Rotation : COLOR1;
    
#ifdef XBOX
    float2 TextureCoordinate : SPRITETEXCOORD;
#else
    float2 TextureCoordinate : TEXCOORD0;
#endif
};


// Pixel shader for drawing particles that can rotate. It is not actually
// possible to rotate a point sprite, so instead we rotate our texture
// coordinates. Leaving the sprite the regular way up but rotating the
// texture has the exact same effect as if we were able to rotate the
// point sprite itself.
float4 RotatingPixelShader(RotatingPixelShaderInput input) : COLOR0
{
    float2 textureCoordinate = input.TextureCoordinate;

    // We want to rotate around the middle of the particle, not the origin,
    // so we offset the texture coordinate accordingly.
    textureCoordinate -= 0.5;
    
    // Apply the rotation matrix, after rescaling it back from the packed
    // color interpolator format into a full -1 to 1 range.
    float4 rotation = input.Rotation * 2 - 1;
    
    textureCoordinate = mul(textureCoordinate, float2x2(rotation));
    
    // Point sprites are squares. So are textures. When we rotate one square
    // inside another square, the corners of the texture will go past the
    // edge of the point sprite and get clipped. To avoid this, we scale
    // our texture coordinates to make sure the entire square can be rotated
    // inside the point sprite without any clipping.
    textureCoordinate *= sqrt(2);
    
    // Undo the offset used to control the rotation origin.
    textureCoordinate += 0.5;

    return tex2D(Sampler, textureCoordinate) * input.Color;
}


// Effect technique for drawing particles that do not rotate. Works with shader 1.1.
technique NonRotatingParticles
{
    pass P0
    {
        VertexShader = compile vs_1_1 VertexShader();
        PixelShader = compile ps_2_0 NonRotatingPixelShader();
    }
}


// Effect technique for drawing particles that can rotate. Requires shader 2.0.
technique RotatingParticles
{
    pass P0
    {
        VertexShader = compile vs_1_1 VertexShader();
        PixelShader = compile ps_2_0 RotatingPixelShader();
    }
}
