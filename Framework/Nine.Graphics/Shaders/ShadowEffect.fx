
#define MaxBones 72


float4x4 World;
float4x3 bones[MaxBones];
float4x4 worldViewProjection;
matrix	LightViewProjection;
float3 	ShadowColor = 0.5;
float	DepthBias = 0.0005f;
float2  shadowMapTexelSize = float2(0.0009765625f, 0.0009765625f); // 1.0f/1024

// Poison filter pseudo random filter positions for PCF with 10 samples
float2 filterTaps[10] =
{
   // First test, still the best.
   {-0.84052f, -0.073954f},
   {-0.326235f, -0.40583f},
   {-0.698464f, 0.457259f},
   {-0.203356f, 0.6205847f},
   {0.96345f, -0.194353f},
   {0.473434f, -0.480026f},
   {0.519454f, 0.767034f},
   {0.185461f, -0.8945231f},
   {0.507351f, 0.064963f},
   {-0.321932f, 0.5954349f}
};

texture2D ShadowMap;
sampler ShadowMapSampler = sampler_state
{
    Texture = (ShadowMap);
    MipFilter = Point;
    MagFilter = Point;
    MinFilter = Point;
};

//-----------------------------------------------------------------------------
// Vertex Shader: VertShadow
// Desc: Process vertex for the shadow map
//-----------------------------------------------------------------------------
void VertShadow( float4 Pos      : POSITION,
                 out float4 oPos : POSITION,
                 out float3 oShadow : TEXCOORD0)
{
    //
    // Compute the projected coordinates
    //    
    float4 posWorld = mul( Pos, World );
    oPos = mul( Pos, worldViewProjection );

    float4 positionShadow = mul(posWorld, LightViewProjection);

    oShadow.x   	=  positionShadow.x / positionShadow.w / 2.0f + 0.5f;
    oShadow.y   	= -positionShadow.y / positionShadow.w / 2.0f + 0.5f;
    oShadow.z   	=  positionShadow.z / positionShadow.w;
}


//-----------------------------------------------------------------------------
// Vertex Shader: VertShadow
// Desc: Process vertex for the shadow map
//-----------------------------------------------------------------------------
void VertShadowSkinned( float4 Pos : POSITION,
                        float4 BoneIndices : BLENDINDICES0,
                        float4 BoneWeights : BLENDWEIGHT0,
                 out float4 oPos : POSITION,
                 out float3 oShadow : TEXCOORD0)
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
    float4 posWorld = mul( oPos, World );
    oPos = mul( oPos, worldViewProjection );

    float4 positionShadow = mul(posWorld, LightViewProjection);

    oShadow.x   	=  positionShadow.x / positionShadow.w / 2.0f + 0.5f;
    oShadow.y   	= -positionShadow.y / positionShadow.w / 2.0f + 0.5f;
    oShadow.z   	=  positionShadow.z / positionShadow.w;
}


//-----------------------------------------------------------------------------
// Pixel Shader: PixShadow
// Desc: Process pixel for the shadow map
//-----------------------------------------------------------------------------
void PixShadow( float3 shadow : TEXCOORD0, out float4 Color : COLOR )
{
    float intensity = 0;
    
    if ((saturate(shadow.x) == shadow.x) && (saturate(shadow.y) == shadow.y))
    {
        for (int i=0; i<10; i++)
        {
            intensity += shadow.z > DepthBias + tex2D(ShadowMapSampler, shadow.xy + filterTaps[i] * shadowMapTexelSize).x ? 1.0f / 10.0f : 0;
        }
    }
     
    Color = lerp(1, float4(ShadowColor, 1), intensity);
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
