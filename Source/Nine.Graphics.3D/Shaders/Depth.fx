// Input parameters.
float4x3 bones[72];
float4x4 worldViewProjection;
texture Texture;
float referenceAlpha;

sampler Sampler : register(s0) = sampler_state
{
    Texture = (Texture);
};


//-----------------------------------------------------------------------------
// Vertex Shader: VertDepth
// Desc: Process vertex for the shadow map
//-----------------------------------------------------------------------------
void VertDepth( float4 Pos : POSITION,
                 out float4 oPos : POSITION,
                 out float2 Depth : TEXCOORD1 )
{
    //
    // Compute the projected coordinates
    //
    oPos = mul( Pos, worldViewProjection );

    //
    // Store z and w in our spare texcoord
    //
    Depth = oPos.zw;
}


//-----------------------------------------------------------------------------
// Vertex Shader: VertDepth
// Desc: Process vertex for the shadow map
//-----------------------------------------------------------------------------
void VertDepthSkinned( float4 Pos : POSITION,
                        float4 BoneIndices : BLENDINDICES0,
                        float4 BoneWeights : BLENDWEIGHT0,
                 out float4 oPos : POSITION,
                 out float2 Depth : TEXCOORD1 )
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
    Depth = oPos.zw;
}

//-----------------------------------------------------------------------------
// Vertex Shader: VertDepth
// Desc: Process vertex for the shadow map
//-----------------------------------------------------------------------------
void VertDepthTexture( float4 Pos : POSITION,
                        float2 TexCoord : TEXCOORD0,
                 out float4 oPos : POSITION,
                 out float2 oTexCoord : TEXCOORD0,
                 out float2 Depth : TEXCOORD1  )
{
    //
    // Compute the projected coordinates
    //
    oPos = mul( Pos, worldViewProjection );

    //
    // Store z and w in our spare texcoord
    //
    Depth = oPos.zw;

    oTexCoord = TexCoord;
}


//-----------------------------------------------------------------------------
// Vertex Shader: VertDepth
// Desc: Process vertex for the shadow map
//-----------------------------------------------------------------------------
void VertDepthSkinnedTexture( float4 Pos : POSITION,
                        float4 BoneIndices : BLENDINDICES0,
                        float4 BoneWeights : BLENDWEIGHT0,
                        float2 TexCoord : TEXCOORD0,
                 out float4 oPos : POSITION,
                 out float2 oTexCoord : TEXCOORD0,
                 out float2 Depth : TEXCOORD1 )
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
    Depth = oPos.zw;

    oTexCoord = TexCoord;
}



//-----------------------------------------------------------------------------
// Pixel Shader: PixDepth
// Desc: Process pixel for the shadow map
//-----------------------------------------------------------------------------
void PixDepth( float2 Depth : TEXCOORD1,
                out float4 Color : COLOR )
{
    //
    // Depth is z / w
    //
    Color = Depth.x / Depth.y;
}

void PixDepthTexture(float2 TexCoord : TEXCOORD0, float2 Depth : TEXCOORD1,
                out float4 Color : COLOR )
{
    float4 color = tex2D(Sampler, TexCoord);
    
    clip(color.a < referenceAlpha ? -1 : 1);

    //
    // Depth is z / w
    //
    Color = Depth.x / Depth.y;
}

Technique t1 { Pass { VertexShader = compile vs_2_0 VertDepth(); PixelShader = compile ps_2_0 PixDepth(); } }
Technique t2 { Pass { VertexShader = compile vs_2_0 VertDepthSkinned(); PixelShader = compile ps_2_0 PixDepth(); } }
Technique t3 { Pass { VertexShader = compile vs_2_0 VertDepthTexture(); PixelShader = compile ps_2_0 PixDepthTexture(); } }
Technique t4 { Pass { VertexShader = compile vs_2_0 VertDepthSkinnedTexture(); PixelShader = compile ps_2_0 PixDepthTexture(); } }