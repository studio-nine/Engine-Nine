#include "DeferredLighting.fxh"

// Input parameters.
float2 HalfPixel;
float4x4 World;
float4x4 ViewProjection;
float4x4 ViewProjectionInverse;

float3 EyePosition;

float3 Direction;
float3 Position;
float3 DiffuseColor;
float3 SpecularColor;

float Range = 100;
float Attenuation = 1;
float Falloff = 1;
float innerAngle;
float outerAngle;

sampler DepthBufferSampler : register(s0);
sampler NormalBufferSampler : register(s1);

void VS(float4 Pos : POSITION,
        out float4 oPos : POSITION,
        out float4 oPosProjection : TEXCOORD0)
{
    oPos = mul(Pos, World);
    oPos = mul(oPos, ViewProjection);
    oPosProjection = oPos;
}

void PS(float4 PosProjection : TEXCOORD0, out float4 Color:COLOR, uniform bool specularEnabled)
{
    float3 normal;
    float3 position;
    float specularPower;

    Extract(NormalBufferSampler, DepthBufferSampler, ViewProjectionInverse, PosProjection, HalfPixel, normal, position, specularPower);
    
    float3 positionToEye = normalize(EyePosition - position);
    float3 positionToVertex = Position - position;
    float3 L = normalize(positionToVertex);
    float dotL = dot(L, normal);
    float dotH = dot(normalize(positionToEye + L), normal);
    float zeroL = step(0, dotL);
    
    float distanceSq = dot(positionToVertex, positionToVertex);
    float distance = sqrt(distanceSq);
    
    float angle = dot(L, -Direction);
    float inner = innerAngle;
    float outer = outerAngle;

    float fade = 0;
    if (distance <= Range && angle > outer)
    {
        fade = max(1 - pow(max(distance / Range, 0.000001), Attenuation), 0);
        if (angle < inner)
            fade *= pow(max((angle - outer) / (inner - outer), 0.000001), Falloff);
    }

    float3 diffuse = DiffuseColor * zeroL * dotL * fade;
    float specularIntenisty = pow(max(dotH, 0.000001) * zeroL, specularPower) * fade * SpecularColor.x;
    
    Color = specularEnabled ? float4(diffuse, specularIntenisty) : float4(diffuse, 0);
}

void PSUS(float4 PosProjection : TEXCOORD0, out float4 Color : COLOR) { PS(PosProjection, Color, true); }
void PSNS(float4 PosProjection : TEXCOORD0, out float4 Color : COLOR) { PS(PosProjection, Color, false); }

Technique Specular
{
    Pass
    {
#if DirectX
        VertexShader = compile vs_4_0 VS();
        PixelShader	 = compile ps_4_0 PSUS();
#else
        VertexShader = compile vs_3_0 VS();
        PixelShader  = compile ps_3_0 PSUS();
#endif
    }
}

Technique NoSpecular
{
    Pass
    {
#if DirectX
        VertexShader = compile vs_4_0 VS();
        PixelShader  = compile ps_4_0 PSNS();
#else
        VertexShader = compile vs_3_0 VS();
        PixelShader  = compile ps_3_0 PSNS();
#endif
    }
}