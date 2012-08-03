#include "DeferredLighting.fxh"

// Input parameters.
float2 HalfPixel;
float4x4 ViewProjection;
float4x4 ViewProjectionInverse;

float3 EyePosition;
float3 Position;
float3 DiffuseColor;
float3 SpecularColor;

float Range = 100;
float Attenuation = 1;

sampler DepthBufferSampler : register(s0);
sampler NormalBufferSampler : register(s1);

void VS(float4 Pos : POSITION,
        out float4 oPos : POSITION,
        out float4 oPosProjection : TEXCOORD0)
{
    oPos = float4(Pos * Range + Position, 1);
    oPos = mul(oPos, ViewProjection);
    oPosProjection = oPos;
}

void PS(float4 PosProjection : TEXCOORD0, out float4 Color:COLOR)
{
    float3 normal;
    float3 position;
    float specularPower;

    Extract(NormalBufferSampler, DepthBufferSampler, PosProjection, ViewProjectionInverse, HalfPixel, normal, position, specularPower);

    float3 positionToEye = normalize(EyePosition - position);
    float3 positionToVertex = Position - position;
    float3 L = normalize(positionToVertex);
    float dotL = dot(L, normal);
    float dotH = dot(normalize(positionToEye + L), normal);
    float zeroL = step(0, dotL);
    
    float distanceSq = dot(positionToVertex, positionToVertex);
    float distance = sqrt(distanceSq);
    
    float fade = max(1 - pow(max(distance / Range, 0.000001), Attenuation), 0);
    float3 diffuse = DiffuseColor * zeroL * dotL * fade;
    float specular = pow(max(dotH, 0.000001) * zeroL, specularPower) * fade * SpecularColor.x;

    Color = float4(diffuse, specular);
}


Technique BasicEffect
{
    Pass
    {
        VertexShader = compile vs_2_0 VS();
        PixelShader	 = compile ps_2_0 PS();
    }
}