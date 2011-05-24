#include "DeferredLighting.fxh"

// Input parameters.
float4x4 viewProjection;
float4x4 viewProjectionInverse;

float2 halfPixel;
float3 eyePosition;

float3 Position;
float3 DiffuseColor;

float Range = 100;
float Attenuation = 1;

texture2D NormalBuffer;
sampler NormalBufferSampler = sampler_state
{
    Texture = (NormalBuffer);
};

texture2D DepthBuffer;
sampler DepthBufferSampler = sampler_state
{
    Texture = (DepthBuffer);
    MipFilter = Point;
    MagFilter = Point;
    MinFilter = Point;
};

void VS(float4 Pos : POSITION,
        out float4 oPos : POSITION,
        out float4 oPosProjection : TEXCOORD0)
{
    oPos = float4(Pos * Range + Position, 1);
    oPos = mul(oPos, viewProjection);
    oPosProjection = oPos;
}

void PS(float4 PosProjection : TEXCOORD0, out float4 Color:COLOR)
{
    float3 normal;
    float3 position;
    float specularPower;
    
    Extract(NormalBufferSampler, DepthBufferSampler, 
            PosProjection, halfPixel, viewProjectionInverse, 
            normal, position, specularPower); 

    float3 positionToEye = eyePosition - position;
    float3 L = Position - position;
    float dotL = dot(normalize(L), normal);
	float dotH = dot(normalize(positionToEye + L), normal);
	float zeroL = step(0, dotL);
    
	float distanceSq = dot(L, L);
	float distance = sqrt(distanceSq);
    
    float fade = (pow(max(Attenuation, 0.000001), 1 - distance / Range) - 1) / (Attenuation - 1);
	float3 diffuse = DiffuseColor * zeroL * dotL* fade;
    float specular = pow(max(dotH, 0.000001), specularPower)* fade;

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