
#define MaxBones 59


// Input parameters.
float4x4 World;
float4x4 Bones[MaxBones];
float4x4 View;
float4x4 Projection;
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
    oPos = mul( Pos, World );
    oPos = mul( oPos, View );
    oPos = mul( oPos, Projection );

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
    float4x4 skinTransform = 0;
    
    skinTransform += Bones[BoneIndices.x] * BoneWeights.x;
    skinTransform += Bones[BoneIndices.y] * BoneWeights.y;
    skinTransform += Bones[BoneIndices.z] * BoneWeights.z;
    skinTransform += Bones[BoneIndices.w] * BoneWeights.w;
    
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
    Color.rgba = Depth;
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
