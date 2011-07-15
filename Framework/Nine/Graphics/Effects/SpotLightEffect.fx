
#define MaxBones 59

#ifdef Reach
#define MaxLights 1
#else
#define MaxLights 4
#endif


struct SpotLight
{
    float3 Position;
    float3 Direction;
    float3 DiffuseColor;
    float3 SpecularColor;
    float Range;
    float Attenuation;
    float Falloff;
    float InnerAngle;
    float OuterAngle;
};

// Input parameters.
float4x4 World;
float4x4 bones[MaxBones];
float4x4 viewProjection;

float3 eyePosition;
float SpecularPower = 16;

int numLights = MaxLights;
SpotLight lights[MaxLights];

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
                 float3 Normal   : NORMAL0,
                 float2 TexCoord : TEXCOORD0,
                 out float4 oPos : POSITION,
                 out float2 oTexCoord : TEXCOORD0,
                 out float4 oPosWorld : TEXCOORD1,
                 out float3 oNormal : TEXCOORD2 )
{
    //
    // Compute the projected coordinates
    //    
    oPosWorld = mul( Pos, World );
    oPos = mul( oPosWorld, viewProjection );

    oNormal = mul(Normal, (float3x3)World);

    oTexCoord = TexCoord;
}


//-----------------------------------------------------------------------------
// Vertex Shader: VertShadow
// Desc: Process vertex for the shadow map
//-----------------------------------------------------------------------------
void VertShadowSkinned( float4 Pos : POSITION,
                        float3 Normal   : NORMAL0,
                        float2 TexCoord : TEXCOORD0,
                        float4 BoneIndices : BLENDINDICES0,
                        float4 BoneWeights : BLENDWEIGHT0,
                 out float4 oPos : POSITION,
                 out float2 oTexCoord : TEXCOORD0,
                 out float4 oPosWorld : TEXCOORD1,
                 out float3 oNormal : TEXCOORD2 )
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
    oPosWorld = mul( Pos, skinTransform );
    oPosWorld = mul( oPosWorld, World );
    oPos = mul( oPosWorld, viewProjection );
    
    oNormal = mul(Normal, (float3x3)skinTransform);
    oNormal = mul(oNormal, (float3x3)World);

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
    normal = normalize(normal);
    float3 positionToEye = normalize(eyePosition - positionWorld.xyz);
    float3 diffuse = 0;
    float3 specular = 0;
    float3 diffuseTextureColor = 1;
    if(diffuseTextureEnabled)
    {
        diffuseTextureColor *= tex2D(diffuseSampler, TexCoord);
    }

    for(int i=0; i< numLights; i++)
    {
        float3 positionToVertex = lights[i].Position - positionWorld.xyz;
        float3 L = normalize(positionToVertex);
        float dotL = dot(L, normal);
        float dotH = dot(normalize(positionToEye + L), normal);
        float zeroL = step(0, dotL);

        float distanceSq = dot(positionToVertex, positionToVertex);
        float distance = sqrt(distanceSq);
    
        float angle = dot(L, -lights[i].Direction);
        float inner = lights[i].InnerAngle;
        float outer = lights[i].OuterAngle;

        float fade = 0;
        if (distance <= lights[i].Range && angle > outer)
        {
            fade = (pow(max(lights[i].Attenuation, 0.000001), 1 - distance / lights[i].Range) - 1) / (lights[i].Attenuation - 1);
            if (angle < inner)
                fade *= pow(max((angle - outer) / (inner - outer), 0.000001), lights[i].Falloff);
        }

        diffuse += lights[i].DiffuseColor * dotL * zeroL * fade;
        specular += lights[i].SpecularColor * zeroL * pow(max(dotH, 0.000001), SpecularPower) * fade;
    }
    Color = float4(diffuseTextureColor * diffuse + specular, 1);
}


int ShaderIndex = 0;


#ifdef Reach
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
#else
VertexShader VSArray[2] =
{
	compile vs_3_0 VertShadow(),
	compile vs_3_0 VertShadowSkinned(),
};
PixelShader PSArray[2] =
{
	compile ps_3_0 PixShadow(),
	compile ps_3_0 PixShadow(),
};
#endif


Technique BasicEffect
{
	Pass
	{
		VertexShader = (VSArray[ShaderIndex]);
		PixelShader	 = (PSArray[ShaderIndex]);
	}
}
