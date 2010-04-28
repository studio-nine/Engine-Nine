

const float FarClip = 100.0f;

float4x4 View;
float4x4 Projection;

texture Texture;

// The ambient color for the sky, should be 1 for normal brightness.
float3 AmbientColor : Ambient = {1.0f, 1.0f, 1.0f};

samplerCUBE CubeSampler = sampler_state
{
	Texture = <Texture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Wrap;
};

struct VS_OUTPUT
{
    float4 Position : POSITION;
    float3 TexCoord	: TEXCOORD0;
};


VS_OUTPUT VS(float4 Position  : POSITION)
{
	VS_OUTPUT Out;
	    
    // Scale the box up so that we don't hit the near clip plane
    float3 pos = Position * FarClip;
    
    // In.pos is a float 3 for this calculation so that translation is ignored
    pos = mul(pos, View);
    
    // However, we need the translation for the projection
    Out.Position = mul(float4(pos, 1.0f), Projection);
    Out.Position.y = Out.Position.y - 10;
    
    // Just use the positions to infer the texture coordinates
    // Swap y and z because we use +z as up
    Out.TexCoord = float3(Position.xzy);
	
    return Out;
}

float4 PS( VS_OUTPUT In ) : COLOR
{
    return float4(AmbientColor, 1) * texCUBE(CubeSampler, In.TexCoord);
}


technique Default
{
    pass P0
    {
        vertexShader = compile vs_2_0 VS();
        pixelShader = compile ps_2_0 PS();
    }
}
