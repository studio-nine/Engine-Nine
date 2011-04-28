
float deltaTime;
float Speed = 1;

sampler TextureSampler : register(s0);

texture LastFrameTexture;
sampler lastSampler = sampler_state
{
   Texture = <LastFrameTexture>;
   MinFilter = POINT;
   MagFilter = POINT;
   MipFilter = POINT;   
   AddressU  = Clamp;
   AddressV  = Clamp;
   AddressW  = Clamp;
};

float4 PS(float2 texCoord : TEXCOORD0, uniform bool accurate) : COLOR0
{  
    float4 current = tex2D(TextureSampler, texCoord);
    float4 last = tex2D(lastSampler, texCoord);

    if (accurate)
    {
        float4 gap = abs(current - last);
        float4 increment = gap * min(deltaTime * Speed, 1);    
        return last + sign(current - last) * max(increment, min(gap, 1.0 / 255));
    }
    
    return last + (current - last) * min(deltaTime * Speed, 1);
}
 
int ShaderIndex = 0;

PixelShader PSArray[2] =
{
	compile ps_2_0 PS(false),
	compile ps_2_0 PS(true),
};


Technique Default
{
	Pass
	{
		PixelShader	 = (PSArray[ShaderIndex]);
	}
}
