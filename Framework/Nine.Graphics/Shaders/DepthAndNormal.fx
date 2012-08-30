
#include "DeferredLighting.fxh"

#define MaxBones 72

// Input parameters.
float4x4 World;
float4x3 bones[MaxBones];
float4x4 WorldViewProjection;

float SpecularPower = 16;

//-----------------------------------------------------------------------------
// Vertex Shader: Vert
//-----------------------------------------------------------------------------
void Vert( float4 Pos : POSITION,
                 float3 Normal : NORMAL0,
                 out float4 oPos : POSITION,
                 out float2 oDepth : TEXCOORD0,
                 out float3 oNormal : TEXCOORD1)
{
    oPos = mul( Pos, WorldViewProjection );
    oDepth = oPos.zw;    
    oNormal = mul( Normal, (float3x3)World );
}


//-----------------------------------------------------------------------------
// Vertex Shader: Vert
//-----------------------------------------------------------------------------
void VertSkinned( float4 Pos : POSITION,
                        float3 Normal : NORMAL0,
                        float4 BoneIndices : BLENDINDICES0,
                        float4 BoneWeights : BLENDWEIGHT0,
                        out float4 oPos : POSITION,
                        out float2 oDepth : TEXCOORD0,
                        out float3 oNormal : TEXCOORD1)
{
    // Blend between the weighted bone matrices.
    float4x3 skinTransform = 0;
    
    skinTransform += bones[BoneIndices.x] * BoneWeights.x;
    skinTransform += bones[BoneIndices.y] * BoneWeights.y;
    skinTransform += bones[BoneIndices.z] * BoneWeights.z;
    skinTransform += bones[BoneIndices.w] * BoneWeights.w;

    oPos = float4( mul( Pos, skinTransform ), 1 );
    oPos = mul( oPos, WorldViewProjection );

    oDepth = oPos.zw;

    oNormal = mul( Normal, (float3x3)skinTransform );
    oNormal = mul( oNormal, (float3x3)World );
}


//-----------------------------------------------------------------------------
// Pixel Shader: Pix
//-----------------------------------------------------------------------------
void Pix(       float2 Depth : TEXCOORD0,
                float3 Normal : TEXCOORD1,
                out float4 oDepth : COLOR0,
                out float4 oNormal : COLOR1 )
{
    oNormal.rgb = Normal * 0.5 + 0.5;
    oNormal.a = SpecularPower / MaxSpecular;
    oDepth = float4(Depth.x / Depth.y, 0, 0, 0);
}

Technique t1 { Pass { VertexShader = compile vs_2_0 Vert(); PixelShader = compile ps_2_0 Pix(); } }
Technique t2 { Pass { VertexShader = compile vs_2_0 VertSkinned(); PixelShader = compile ps_2_0 Pix(); } }