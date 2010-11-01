//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================


// Camera parameters.
float4x4 View;
float4x4 Projection;


// The current time, in seconds.
float CurrentTime;


// Parameters describing how the particles animate.
float3 Gravity;
float EndVelocity;
float4 MinStartColor;
float4 MaxStartColor;
float4 MinEndColor;
float4 MaxEndColor;

// These float2 parameters describe the min and max of a range.
// The actual value is chosen differently for each particle,
// interpolating between x and y by some random amount.
float2 RotateSpeed;
float2 StartSize;
float2 EndSize;
float2 Duration;



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
   	float4 Random1 : COLOR0;
   	float4 Random2 : COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
   	float Time : TEXCOORD1;
	float3 Velocity : TEXCOORD2;
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


// Vertex shader helper for computing the position of a particle.
float4 ComputeParticlePosition(float3 position, float3 velocity,
                              float age, float normalizedAge, float duration)
{ 
   float startVelocity = length(velocity);

   // Work out how fast the particle should be moving at the end of its life,
   // by applying a constant scaling factor to its starting velocity.
   float endVelocity = startVelocity * EndVelocity;
   
   // Our particles have constant acceleration, so given a starting velocity
   // S and ending velocity E, at time T their velocity should be S + (E-S)*T.
   // The particle position is the sum of this velocity over the range 0 to T.
   // To compute the position directly, we must integrate the velocity
   // equation. Integrating S + (E-S)*T for T produces S*T + (E-S)*T*T/2.

   float velocityIntegral = startVelocity * normalizedAge +
                            (endVelocity - startVelocity) * normalizedAge *
                                                            normalizedAge / 2;
    
   position += normalize(velocity) * velocityIntegral * duration;
   
   // Apply the gravitational force.
   position += Gravity * age * normalizedAge;

   return float4(position.xyz, 1);
}


// Vertex shader helper for computing the size of a particle.
float ComputeParticleSize(float randomValue1, float randomValue2, float normalizedAge)
{
   // Apply a random factor to make each particle a slightly different size.
   float startSize = lerp(StartSize.x, StartSize.y, randomValue1);
   float endSize = lerp(EndSize.x, EndSize.y, randomValue2);
   
   // Compute the actual size based on the age of the particle.
   float size = lerp(startSize, endSize, normalizedAge);
   
   // Project the size into screen coordinates.
   return size;
}


// Vertex shader helper for computing the color of a particle.
float4 ComputeParticleColor(float4 projectedPosition,
                           float randomValue1, float randomValue2, float normalizedAge)
{
   // Apply a random factor to make each particle a slightly different color.
   float4 startColor = lerp(MinStartColor, MaxStartColor, randomValue1);
   float4 endColor = lerp(MinEndColor, MaxEndColor, randomValue2);
   float4 color = lerp(startColor, endColor, normalizedAge);
   
   // Fade the alpha based on the age of the particle. This curve is hard coded
   // to make the particle fade in fairly quickly, then fade out more slowly:
   // plot x*(1-x)*(1-x) for x=0:1 in a graphing program if you want to see what
   // this looks like. The 6.7 scaling factor normalizes the curve so the alpha
   // will reach all the way up to fully solid.
   
   color.a *= normalizedAge * (1-normalizedAge) * (1-normalizedAge) * 6.7;
   
   // On Xbox, point sprites are clipped away entirely as soon as their center
   // point moves off the screen, even when rest of the sprite should still be
   // visible. This causes an irritating flicker when large particles reach the
   // edge of the screen. We can hide this problem by fading the sprite out
   // slightly before it is about to get clipped.
#ifdef XBOX
   float2 screenPosition = abs(projectedPosition.xy / projectedPosition.w);
   
   float distanceFromBorder = 1 - max(screenPosition.x, screenPosition.y);
   
   // The value 16 is chosen arbitrarily. Make this smaller to fade particles
   // out sooner, or larger to fade them later (which can cause visible popping).
   color.a *= saturate(distanceFromBorder * 16);
#endif
   
   return color;
}


// Custom vertex shader animates particles entirely on the GPU.
PixelShaderInput VSNoRotation(VertexShaderInput input)
{
 	// Compute the age of the particle.
   	float age = CurrentTime - input.Time;

   	float duration = lerp(Duration.x, Duration.y, input.Random1.x);

   	// Normalize the age into the range zero to one.
   	float normalizedAge = saturate(age / duration);
	

    PixelShaderInput output;
    
	output.TextureCoordinate = input.TextureCoordinate;
	output.TextureCoordinate.y = -output.TextureCoordinate.y;

	input.Position = ComputeParticlePosition(input.Position, input.Velocity, age, normalizedAge, duration);

    // Compute the particle position, size, color, and rotation.
	float4 viewPos = mul(float4(input.Position, 1), View);

	float size = ComputeParticleSize(input.Random1.y, input.Random1.z, normalizedAge);

	viewPos.xy += -size / 2 + size * input.TextureCoordinate;

    output.Position = mul(viewPos, Projection);
    

    output.Color = ComputeParticleColor(output.Position, input.Random2.x, input.Random2.y, normalizedAge);


    return output;
}

// Custom vertex shader animates particles entirely on the GPU.
PixelShaderInput VSRotation(VertexShaderInput input)
{
 	// Compute the age of the particle.
   	float age = CurrentTime - input.Time;
	
   	float duration = lerp(Duration.x, Duration.y, input.Random1.x);

   	// Normalize the age into the range zero to one.
   	float normalizedAge = saturate(age / duration);
	

    PixelShaderInput output;
    
	output.TextureCoordinate = input.TextureCoordinate;
	output.TextureCoordinate.y = -output.TextureCoordinate.y;

	input.Position = ComputeParticlePosition(input.Position, input.Velocity, age, normalizedAge, duration);

    // Compute the particle position, size, color, and rotation.
	float4 viewPos = mul(float4(input.Position, 1), View);

	float size = ComputeParticleSize(input.Random1.y, input.Random1.z, normalizedAge);
	
	float rotation = age * lerp(RotateSpeed.x, RotateSpeed.y, input.Random1.w);

	float2 offset = -size / 2 + size * input.TextureCoordinate;

	viewPos.xy += mul(offset, (float2x2)ComputeParticleRotation(rotation));

    output.Position = mul(viewPos, Projection);
    

    output.Color = ComputeParticleColor(output.Position, input.Random2.x, input.Random2.y, normalizedAge);

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
