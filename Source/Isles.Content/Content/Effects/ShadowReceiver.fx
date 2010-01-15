
#define MaxBones 59


// Input parameters.
float4x4 World;
float4x4 Bones[MaxBones];
float4x4 ViewProjection;
float4x4 LightViewProjection;


texture2D ShadowMap;


sampler2D ShadowSampler = sampler_state
{
	Texture = <ShadowMap>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};


//-----------------------------------------------------------------------------
// Vertex Shader: VertShadow
// Desc: Process vertex for the shadow map
//-----------------------------------------------------------------------------
void VertShadow( float4 Pos : POSITION,
                 out float4 oPos : POSITION,
                 out float2 Depth : TEXCOORD0,
                 out float2 shadow : TEXCOORD1 )
{
    Pos = mul( Pos, World );
    oPos = mul( Pos, ViewProjection );
    
    float4 v = mul( Pos, LightViewProjection );
    
    shadow.xy = v.xy / v.w;
    Depth.xy = v.zw;
}


//-----------------------------------------------------------------------------
// Vertex Shader: VertShadow
// Desc: Process vertex for the shadow map
//-----------------------------------------------------------------------------
void VertShadowSkinned( float4 Pos : POSITION,
						float4 BoneIndices : BLENDINDICES0,
						float4 BoneWeights : BLENDWEIGHT0,
                 out float4 oPos : POSITION,
                 out float2 Depth : TEXCOORD0,
                 out float2 shadow : TEXCOORD1 )
{
    // Blend between the weighted bone matrices.
    float4x4 skinTransform = 0;
    
    skinTransform += Bones[BoneIndices.x] * BoneWeights.x;
    skinTransform += Bones[BoneIndices.y] * BoneWeights.y;
    skinTransform += Bones[BoneIndices.z] * BoneWeights.z;
    skinTransform += Bones[BoneIndices.w] * BoneWeights.w;
    
    
    Pos = mul( Pos, skinTransform );
    oPos = mul( Pos, ViewProjection );
    
    float4 v = mul( Pos, LightViewProjection );
    
    shadow.xy = v.xy / v.w;
	shadow.xy = 0.5 * shadow.xy + float2(0.5, 0.5);
	shadow.y = 1.0 - shadow.y;
	
    Depth.xy = v.zw;
}


//-----------------------------------------------------------------------------
// Pixel Shader: PixShadow
// Desc: Process pixel for the shadow map
//-----------------------------------------------------------------------------
void PixShadow( float2 Depth : TEXCOORD0,
				float2 shadow	: TEXCOORD1,
                out float4 Color : COLOR )
{
	float caster = tex2D(ShadowSampler, shadow).x;
	float d = 1 - Depth.x / Depth.y;
	
	Color = (d > caster ? float4(0, 0, 0, 1) : float4(1, 1, 1, 1));
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
