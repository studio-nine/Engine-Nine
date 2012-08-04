#include "DeferredLighting.fxh"

// Input parameters.
float4x4 ViewProjectionInverse;
float3 EyePosition;
float3 Direction;
float3 DiffuseColor;
float3 SpecularColor;

sampler DepthBufferSampler : register(s0);
sampler NormalBufferSampler : register(s1);

void PS(float2 uv:TEXCOORD0, out float4 Color:COLOR, uniform bool specularEnabled)
{
    float3 normal;
    float3 position;
    float specularPower;

    Extract(NormalBufferSampler, DepthBufferSampler, ViewProjectionInverse, uv, normal, position, specularPower);
    
    float3 positionToEye = normalize(EyePosition - position);
    float3 L = -Direction; 
    float dotL = dot(L, normal);
    float dotH = dot(normalize(positionToEye + L), normal);
    float zeroL = step(0, dotL);
    
    float3 diffuse = DiffuseColor * zeroL * dotL;
    float specularIntenisty = pow(max(dotH, 0.000001) * zeroL, specularPower) * SpecularColor.x;

    Color = specularEnabled ? float4(diffuse, specularIntenisty) : float4(diffuse, 0);
}

Technique Specular
{
    Pass
    {
        PixelShader	 = compile ps_2_0 PS(true);
    }
}

Technique NoSpecular
{
    Pass
    {
        PixelShader	 = compile ps_2_0 PS(false);
    }
}