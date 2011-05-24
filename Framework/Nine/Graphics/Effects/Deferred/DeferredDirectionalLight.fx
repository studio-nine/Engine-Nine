#include "DeferredLighting.fxh"

// Input parameters.
float4x4 viewProjectionInverse;

float2 halfPixel;
float3 eyePosition;

float3 Direction;
float3 DiffuseColor;

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
        float2 TexCoord : TEXCOORD0,
        out float4 oPos : POSITION,
        out float4 oPosProjection : TEXCOORD0)
{
    oPos = Pos;
    oPos.xy -= halfPixel;
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
	float3 L = -Direction; 
    float dotL = dot(L, normal);
	float dotH = dot(normalize(positionToEye + L), normal);
	float zeroL = step(0, dotL);
    
	float3 diffuse = DiffuseColor * zeroL * dotL;
    float specular = pow(max(dotH, 0.000001), specularPower);

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