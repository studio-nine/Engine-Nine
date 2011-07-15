#include "DeferredLighting.fxh"

// Input parameters.
float4x4 world;
float4x4 viewProjection;
float4x4 viewProjectionInverse;

float2 halfPixel;
float3 eyePosition;

float3 Direction;
float3 Position;
float3 DiffuseColor;

float Range = 100;
float Attenuation = 1;
float Falloff = 1;
float innerAngle;
float outerAngle;

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
    oPos = mul(Pos, world);
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
    
    float3 positionToEye = normalize(eyePosition - position);
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
        fade = (pow(max(Attenuation, 0.000001), 1 - distance / Range) - 1) / (Attenuation - 1);
		if (angle < inner)
			fade *= pow(max((angle - outer) / (inner - outer), 0.000001), Falloff);
    }

	float3 diffuse = DiffuseColor * zeroL * dotL* fade;
    float specular = zeroL * zeroL * pow(max(dotH, 0.000001), specularPower)* fade;
    
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