
#define MaxBones 59


// Input parameters.
float4x4 World;
float4x4 Bones[MaxBones];
float4x4 ViewProjection;


//-----------------------------------------------------------------------------
// Vertex Shader: VertShadow
// Desc: Process vertex for the shadow map
//-----------------------------------------------------------------------------
void VertShadow( float4 Pos : POSITION,
                 out float4 oPos : POSITION,
                 out float2 Depth : TEXCOORD0 )
{
    //
    // Compute the projected coordinates
    //
    oPos = mul( Pos, World );
    oPos = mul( oPos, ViewProjection );

    //
    // Store z and w in our spare texcoord
    //
    Depth.xy = oPos.zw;
}


//-----------------------------------------------------------------------------
// Vertex Shader: VertShadow
// Desc: Process vertex for the shadow map
//-----------------------------------------------------------------------------
void VertShadowSkinned( float4 Pos : POSITION,
						float4 BoneIndices : BLENDINDICES0,
						float4 BoneWeights : BLENDWEIGHT0,
                 out float4 oPos : POSITION,
                 out float2 Depth : TEXCOORD0 )
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
    oPos = mul( oPos, ViewProjection );

    //
    // Store z and w in our spare texcoord
    //
    Depth.xy = oPos.zw;
}


//-----------------------------------------------------------------------------
// Pixel Shader: PixShadow
// Desc: Process pixel for the shadow map
//-----------------------------------------------------------------------------
void PixShadow( float2 Depth : TEXCOORD0,
                out float4 Color : COLOR )
{
    //
    // Depth is z / w
    //
    Color.rgba = 1 - Depth.x / Depth.y;
}


//-----------------------------------------------------------------------------
// Technique: RenderShadow
// Desc: Renders the shadow map
//-----------------------------------------------------------------------------
technique RenderShadow
{
    pass p0
    {
        VertexShader = compile vs_2_0 VertShadow();
        PixelShader = compile ps_2_0 PixShadow();
    }
    
    pass p1
    {
        VertexShader = compile vs_2_0 VertShadowSkinned();
        PixelShader = compile ps_2_0 PixShadow();
    }
}
