
#include "DeferredLighting.fxh"

#define MaxBones 72

// Input parameters.
float4x4 World;
float4x3 bones[MaxBones];
float4x4 View;
float4x4 Projection;

float2 halfPixel;
float3 DiffuseColor;
float3 SpecularColor;
float3 EmissiveColor;

texture Texture;
sampler BasicSampler = sampler_state
{
    Texture = (Texture);
};

texture LightTexture;
sampler LightSampler = sampler_state
{
    Texture = (LightTexture);
};

//-----------------------------------------------------------------------------
// Vertex Shader: Vert
//-----------------------------------------------------------------------------
void Vert(       float4 Pos : POSITION,
                 float2 TexCoord : TEXCOORD0,
                 out float4 oPos : POSITION,
                 out float2 oTexCoord : TEXCOORD0,
                 out float4 oPosProjection : TEXCOORD1)
{
    oPos = mul( Pos, World );
    oPos = mul( oPos, View );
    oPos = mul( oPos, Projection );
        
    oPosProjection = oPos;
    oTexCoord = TexCoord;
}

//-----------------------------------------------------------------------------
// Vertex Shader: VertSkinned
//-----------------------------------------------------------------------------
void VertSkinned(float4 Pos : POSITION,
                 float2 TexCoord : TEXCOORD0,
                 float4 BoneIndices : BLENDINDICES0,
                 float4 BoneWeights : BLENDWEIGHT0,
                 out float4 oPos : POSITION,
                 out float2 oTexCoord : TEXCOORD0,
                 out float4 oPosProjection : TEXCOORD1)
{
    // Blend between the weighted bone matrices.
    float4x3 skinTransform = 0;
    
    skinTransform += bones[BoneIndices.x] * BoneWeights.x;
    skinTransform += bones[BoneIndices.y] * BoneWeights.y;
    skinTransform += bones[BoneIndices.z] * BoneWeights.z;
    skinTransform += bones[BoneIndices.w] * BoneWeights.w;
    
    oPos = float4( mul( Pos, skinTransform ), 1 );
    oPos = mul( oPos, World );
    oPos = mul( oPos, View );
    oPos = mul( oPos, Projection );
        
    oPosProjection = oPos;
    oTexCoord = TexCoord;
}

//-----------------------------------------------------------------------------
// Pixel Shader: Pix
//-----------------------------------------------------------------------------
void Pix(       float2 TexCoord : TEXCOORD0,
                float4 PosProjection : TEXCOORD1,
                out float4 oColor : COLOR )
{
    float2 lightUV = PositionToUV(PosProjection) + halfPixel;
    float4 light = tex2D(LightSampler, lightUV);
    float4 diffuse = tex2D(BasicSampler, TexCoord);

    diffuse.rgb *= (DiffuseColor * light.rgb + EmissiveColor);
    diffuse.rgb += SpecularColor * light.a * diffuse.a;

    oColor = diffuse;
}

int shaderIndex = 0;


VertexShader VSArray[2] =
{
	compile vs_2_0 Vert(),
	compile vs_2_0 VertSkinned(),
};


PixelShader PSArray[2] =
{
	compile ps_2_0 Pix(),
	compile ps_2_0 Pix(),
};


Technique BasicEffect
{
	Pass
	{
		VertexShader = (VSArray[shaderIndex]);
		PixelShader	 = (PSArray[shaderIndex]);
	}
}
