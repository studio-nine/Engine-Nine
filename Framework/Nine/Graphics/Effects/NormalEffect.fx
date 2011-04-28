
#define MaxBones 59


// Input parameters.
float4x4 World;
float4x4 bones[MaxBones];
float4x4 View;
float4x4 Projection;


//-----------------------------------------------------------------------------
// Vertex Shader: VertShadow
// Desc: Process vertex for the shadow map
//-----------------------------------------------------------------------------
void VertShadow( float4 Pos : POSITION,
				 float3 Normal : NORMAL0,
                 out float4 oPos : POSITION,
                 out float3 oNormal : TEXCOORD0 )
{
    //
    // Compute the projected coordinates
    //
    oPos = mul( Pos, World );
    oPos = mul( oPos, View );
    oPos = mul( oPos, Projection );
	
    oNormal = mul( Normal, (float3x3)World );
    oNormal = mul( oNormal, (float3x3)View );
}

//-----------------------------------------------------------------------------
// Vertex Shader: VertShadow
// Desc: Process vertex for the shadow map
//-----------------------------------------------------------------------------
void VertShadowSkinned( float4 Pos : POSITION,
					    float3 Normal : NORMAL0,
						float4 BoneIndices : BLENDINDICES0,
						float4 BoneWeights : BLENDWEIGHT0,
						out float4 oPos : POSITION,
						out float3 oNormal : TEXCOORD0)
{
    // Blend between the weighted bone matrices.
    float4x4 skinTransform = 0;
    
    skinTransform += bones[BoneIndices.x] * BoneWeights.x;
    skinTransform += bones[BoneIndices.y] * BoneWeights.y;
    skinTransform += bones[BoneIndices.z] * BoneWeights.z;
    skinTransform += bones[BoneIndices.w] * BoneWeights.w;
    
    //
    // Compute the projected coordinates
    //
    oPos = mul( Pos, skinTransform );
    oPos = mul( oPos, World );
    oPos = mul( oPos, View );
    oPos = mul( oPos, Projection );

    //
    // Store z and w in our spare texcoord
    //
    oNormal = mul( Normal, (float3x3)skinTransform );
    oNormal = mul( oNormal, (float3x3)World );
    oNormal = mul( oNormal, (float3x3)View );
}


//-----------------------------------------------------------------------------
// Pixel Shader: PixShadow
// Desc: Process pixel for the shadow map
//-----------------------------------------------------------------------------
void PixShadow( float3 Normal : TEXCOORD0,
                out float4 Color : COLOR )
{
    Color.xyz = Normal;
	Color.a = 1;
}

int ShaderIndex = 0;


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
		VertexShader = (VSArray[ShaderIndex]);
		PixelShader	 = (PSArray[ShaderIndex]);
	}
}
