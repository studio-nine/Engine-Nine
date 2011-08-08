
#define MaxBones 72


// Input parameters.
float4x3 bones[MaxBones];
float4x4 worldViewProjection;
float    farClip;


//-----------------------------------------------------------------------------
// Vertex Shader: VertShadow
// Desc: Process vertex for the shadow map
//-----------------------------------------------------------------------------
void VertShadow( float4 Pos : POSITION,
                 out float4 oPos : POSITION,
                 out float Depth : TEXCOORD0 )
{
    //
    // Compute the projected coordinates
    //
    oPos = mul( Pos, worldViewProjection );

    //
    // Store z and w in our spare texcoord
    //
    Depth = oPos.z / farClip;
}


//-----------------------------------------------------------------------------
// Vertex Shader: VertShadow
// Desc: Process vertex for the shadow map
//-----------------------------------------------------------------------------
void VertShadowSkinned( float4 Pos : POSITION,
						float4 BoneIndices : BLENDINDICES0,
						float4 BoneWeights : BLENDWEIGHT0,
                 out float4 oPos : POSITION,
                 out float Depth : TEXCOORD0 )
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

    //
    // Store z and w in our spare texcoord
    //
    Depth = oPos.z / farClip;
}


//-----------------------------------------------------------------------------
// Pixel Shader: PixShadow
// Desc: Process pixel for the shadow map
//-----------------------------------------------------------------------------
void PixShadow( float Depth : TEXCOORD0,
                out float4 Color : COLOR )
{
    //
    // Depth is z / w
    //
    Color = Depth;
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
