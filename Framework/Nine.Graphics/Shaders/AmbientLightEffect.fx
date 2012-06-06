
#define MaxBones 72

// Input parameters.
float4x4 World;
float4x4 worldViewProjection;
float4x3 bones[MaxBones];

float3 eyePosition;
float3 ambientLightColor;

texture2D diffuseTexture;
sampler diffuseSampler = sampler_state
{
    Texture = <diffuseTexture>;
};
bool diffuseTextureEnabled = false;

//-----------------------------------------------------------------------------
// Vertex Shader: VertShadow
// Desc: Process vertex for the shadow map
//-----------------------------------------------------------------------------
void VertShadow( float4 Pos      : POSITION,
                 float2 TexCoord : TEXCOORD0,
                 out float4 oPos : POSITION,
                 out float2 oTexCoord : TEXCOORD0)
{
    //
    // Compute the projected coordinates
    //    
    oPos = mul( Pos, worldViewProjection );

    oTexCoord = TexCoord;
}


//-----------------------------------------------------------------------------
// Vertex Shader: VertShadow
// Desc: Process vertex for the shadow map
//-----------------------------------------------------------------------------
void VertShadowSkinned( float4 Pos : POSITION,
                        float2 TexCoord : TEXCOORD0,
                        float4 BoneIndices : BLENDINDICES0,
                        float4 BoneWeights : BLENDWEIGHT0,
                 out float4 oPos : POSITION,
                 out float2 oTexCoord : TEXCOORD0)
{
    // Blend between the weighted bone matrices.
    float4x3 skinTransform = 0;
    
    skinTransform += bones[BoneIndices.x] * BoneWeights.x;
    skinTransform += bones[BoneIndices.y] * BoneWeights.y;
    skinTransform += bones[BoneIndices.z] * BoneWeights.z;
    skinTransform += bones[BoneIndices.w] * BoneWeights.w;
    
    //
    // Compute the projected coordinates
    //
    oPos = float4( mul( Pos, skinTransform ), 1 );
    oPos = mul( oPos, worldViewProjection );

    oTexCoord = TexCoord;
}


//-----------------------------------------------------------------------------
// Pixel Shader: PixShadow
// Desc: Process pixel for the shadow map
//-----------------------------------------------------------------------------
void PixShadow( float2 TexCoord : TEXCOORD0,
                float4 positionWorld : TEXCOORD1, 
                float3 normal : TEXCOORD2,
                out float4 Color : COLOR )
{
    float3 diffuseTextureColor = 1;
    if(diffuseTextureEnabled)
    {
        diffuseTextureColor *= tex2D(diffuseSampler, TexCoord);
    }
    Color = float4(diffuseTextureColor * ambientLightColor, 1);
}


int shaderIndex = 0;


VertexShader VSArray[2] =
{
	compile vs_2_0 VertShadow(),
	compile vs_2_0 VertShadowSkinned(),
};
PixelShader PSArray[2] =
{
	compile ps_2_0 PixShadow(),
	compile ps_2_0 PixShadow(),
};

Technique BasicEffect
{
	Pass
	{
		VertexShader = (VSArray[shaderIndex]);
		PixelShader	 = (PSArray[shaderIndex]);
	}
}
