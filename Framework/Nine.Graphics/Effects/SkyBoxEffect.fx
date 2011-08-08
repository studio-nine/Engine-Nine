float4x4 worldViewProjection;

textureCUBE Texture;

// The ambient color for the sky, should be 1 for normal brightness.
float3 Color = {1.0f, 1.0f, 1.0f};

samplerCUBE CubeSampler = sampler_state
{
	Texture = <Texture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

struct VS_OUTPUT
{
    float4 Position : POSITION;
    float3 TexCoord	: TEXCOORD0;
};


VS_OUTPUT VS(float4 Position  : POSITION)
{
	VS_OUTPUT Out;

    // However, we need the translation for the projection
    Out.Position = mul(Position, worldViewProjection).xyww;
    
    // Just use the positions to infer the texture coordinates
    // Swap y and z because we use +z as up
    Out.TexCoord = float3(Position.xzy);
	
    return Out;
}

float4 PS( VS_OUTPUT In ) : COLOR
{
    return float4(Color, 1) * texCUBE(CubeSampler, In.TexCoord);
}


technique Default
{
    pass P0
    {
        vertexShader = compile vs_2_0 VS();
        pixelShader = compile ps_2_0 PS();
    }
}
