float Exposure;
float MaxLuminanceSq;

sampler BasicSampler : register(s0);

texture LuminanceTexture;
sampler LuminanceSampler = sampler_state
{
   Texture = <LuminanceTexture>;
   MinFilter = POINT;
   MagFilter = POINT;
   MipFilter = POINT;   
   AddressU  = Clamp;
   AddressV  = Clamp;
   AddressW  = Clamp;
};

texture BloomTexture;
sampler BloomSampler = sampler_state
{
   Texture = <BloomTexture>;
   MinFilter = POINT;
   MagFilter = POINT;
   MipFilter = POINT;   
   AddressU  = Clamp;
   AddressV  = Clamp;
   AddressW  = Clamp;
};

static const float3 LUM_CONVERT = float3(0.299f, 0.587f, 0.114f);

float4 PS(float2 TexCoord : TEXCOORD0) : COLOR0
{
    float4 color = tex2D( BasicSampler, TexCoord );
    float4 bloom = tex2D( BloomSampler, TexCoord );
    float4 final = color + bloom;
    
    float4 lum = tex2D( LuminanceSampler, float2( 0.5f, 0.5f ) );
    float Lp = (Exposure / lum.r) * dot(final, LUM_CONVERT);
    
    float LmSqr = MaxLuminanceSq;
    float toneScalar = ( Lp * ( 1.0f + ( Lp / ( LmSqr ) ) ) ) / ( 1.0f + Lp );
    
    return final * toneScalar;
}

technique ToneMapping
{
    pass p0
    {
        PixelShader = compile ps_2_0 PS();
    }
}