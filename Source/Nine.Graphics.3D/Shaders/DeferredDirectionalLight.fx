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

void PSUS(float2 uv:TEXCOORD0, out float4 Color : COLOR) { PS(uv, Color, true); }
void PSNS(float2 uv:TEXCOORD0, out float4 Color : COLOR) { PS(uv, Color, false); }

Technique Specular
{
    Pass
    {
#if DirectX
        PixelShader = compile ps_4_0 PSUS();
#elif OpenGL
        PixelShader = compile ps_3_0 PSUS();
#else
        PixelShader = compile ps_2_0 PSUS();
#endif
    }
}

Technique NoSpecular
{
    Pass
    {
#if DirectX
        PixelShader = compile ps_4_0 PSNS();
#elif OpenGL
        PixelShader = compile ps_3_0 PSNS();
#else
        PixelShader = compile ps_2_0 PSNS();
#endif
    }
}