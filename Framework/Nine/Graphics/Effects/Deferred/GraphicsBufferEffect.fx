
#include "DeferredLighting.fxh"

#define MaxBones 59

// Input parameters.
float4x4 World;
float4x4 bones[MaxBones];
float4x4 View;
float4x4 Projection;

float SpecularPower = 16;

texture NormalMap;
sampler NormalSampler = sampler_state
{
    Texture = (NormalMap);
};

//-----------------------------------------------------------------------------
// Vertex Shader: Vert
//-----------------------------------------------------------------------------
void Vert( float4 Pos : POSITION,
			     float3 Normal : NORMAL0,
                 out float4 oPos : POSITION,
                 out float2 oDepth : TEXCOORD0,
                 out float3 oNormal : TEXCOORD1)
{
    oPos = mul( Pos, World );
    oPos = mul( oPos, View );
    oPos = mul( oPos, Projection );

    oDepth = oPos.zw;
    
    oNormal = mul( Normal, (float3x3)World );
    oNormal = mul( oNormal, (float3x3)View );
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
    float4x4 skinTransform = 0;
    
    skinTransform += bones[BoneIndices.x] * BoneWeights.x;
    skinTransform += bones[BoneIndices.y] * BoneWeights.y;
    skinTransform += bones[BoneIndices.z] * BoneWeights.z;
    skinTransform += bones[BoneIndices.w] * BoneWeights.w;

    oPos = mul( Pos, skinTransform );
    oPos = mul( oPos, World );
    oPos = mul( oPos, View );
    oPos = mul( oPos, Projection );

    oDepth = oPos.zw;

    oNormal = mul( Normal, (float3x3)skinTransform );
    oNormal = mul( oNormal, (float3x3)World );
    oNormal = mul( oNormal, (float3x3)View );
}


//-----------------------------------------------------------------------------
// Vertex Shader: VertNormalMapping
//-----------------------------------------------------------------------------
void VertNormalMapping(
                 float4 Pos : POSITION,
			     float3 Normal : NORMAL0,                 
                 float3 Binormal : BINORMAL0,
                 float3 Tangent	: TANGENT0,
                 float2 TexCoord : TEXCOORD0,
                 out float4 oPos : POSITION,
                 out float2 oTexCoord : TEXCOORD0,
                 out float2 oDepth : TEXCOORD1,
                 out float3 oNormal : TEXCOORD2,
                 out float3 oTangent : TEXCOORD3,
                 out float3 oBinormal : TEXCOORD4)
{
    oPos = mul( Pos, World );
    oPos = mul( oPos, View );
    oPos = mul( oPos, Projection );

    oTexCoord = TexCoord;
    oDepth = oPos.zw;

    oNormal = mul( Normal, (float3x3)World );    
    oBinormal = mul( Binormal, (float3x3)World );
    oTangent = mul(Tangent, (float3x3)World);
}

//-----------------------------------------------------------------------------
// Vertex Shader: VertNormalMappingSkinned
//-----------------------------------------------------------------------------
void VertNormalMappingSkinned(
                        float4 Pos : POSITION,
			            float3 Normal : NORMAL0,           
                        float3 Binormal : BINORMAL0,
                        float3 Tangent	: TANGENT0,
						float4 BoneIndices : BLENDINDICES0,
						float4 BoneWeights : BLENDWEIGHT0,
                        float2 TexCoord : TEXCOORD0,
                        out float4 oPos : POSITION,
                        out float2 oTexCoord : TEXCOORD0,
                        out float2 oDepth : TEXCOORD1,
                        out float3 oNormal : TEXCOORD2,
                        out float3 oTangent : TEXCOORD3,
                        out float3 oBinormal : TEXCOORD4)
{
    // Blend between the weighted bone matrices.
    float4x4 skinTransform = 0;
    
    skinTransform += bones[BoneIndices.x] * BoneWeights.x;
    skinTransform += bones[BoneIndices.y] * BoneWeights.y;
    skinTransform += bones[BoneIndices.z] * BoneWeights.z;
    skinTransform += bones[BoneIndices.w] * BoneWeights.w;
    
    oPos = mul( Pos, skinTransform );
    oPos = mul( oPos, World );
    oPos = mul( oPos, View );
    oPos = mul( oPos, Projection );

    oDepth = oPos.zw;

    oNormal = mul( Normal, (float3x3)skinTransform );    
    oNormal = mul( oNormal, (float3x3)World );   

    oBinormal = mul( Binormal, (float3x3)skinTransform );
    oBinormal = mul( oBinormal, (float3x3)World );

    oTangent = mul( Tangent, (float3x3)skinTransform );
    oTangent = mul( oTangent, (float3x3)World );

    oTexCoord = TexCoord;
}

//-----------------------------------------------------------------------------
// Pixel Shader: Pix
//-----------------------------------------------------------------------------
void Pix(       float2 Depth : TEXCOORD0,
                float3 Normal : TEXCOORD1,
                out float4 oNormal : COLOR0,
                out float4 oDepth : COLOR1  )
{
    oNormal.rgb = Normal * 0.5 + 0.5;
    oNormal.a = SpecularPower / MaxSpecular;
    oDepth = float4(Depth.x / Depth.y, 0, 0, 0);
}

//-----------------------------------------------------------------------------
// Pixel Shader: PixNormalMapping
//-----------------------------------------------------------------------------
void PixNormalMapping( 
                float2 TexCoord : TEXCOORD0,
                float2 Depth : TEXCOORD1, 
                float3 Normal : TEXCOORD2,
                float3 Tangent : TEXCOORD3,
                float3 Binormal : TEXCOORD4,
                out float4 oNormal : COLOR0,
                out float4 oDepth : COLOR1 )
{
	float3x3 tangentTransform;
	tangentTransform[0] = Tangent;
	tangentTransform[1] = Binormal;
	tangentTransform[2] = Normal;

    float3 normalFromMap = tex2D(NormalSampler, TexCoord).xyz * 2 - 1;
    normalFromMap = mul(normalFromMap, tangentTransform);
    
    oNormal.rgb = normalFromMap * 0.5 + 0.5;
    oNormal.a = SpecularPower / MaxSpecular;
    oDepth = float4(Depth.x / Depth.y, 0, 0, 0);
}


int shaderIndex = 0;


VertexShader VSArray[4] =
{
	compile vs_2_0 Vert(),
	compile vs_2_0 VertSkinned(),
	compile vs_2_0 VertNormalMapping(),
	compile vs_2_0 VertNormalMappingSkinned(),
};


PixelShader PSArray[4] =
{
	compile ps_2_0 Pix(),
	compile ps_2_0 Pix(),
	compile ps_2_0 PixNormalMapping(),
	compile ps_2_0 PixNormalMapping(),
};


Technique BasicEffect
{
	Pass
	{
		VertexShader = (VSArray[shaderIndex]);
		PixelShader	 = (PSArray[shaderIndex]);
	}
}
