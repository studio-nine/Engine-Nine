
float FocalPlane = 0;
float FocalLength = 0.2f;
float FocalDistance = 0.2f;

sampler TextureSampler : register(s0);

texture BlurTexture;
sampler blurSampler : register(s1) = sampler_state
{
   Texture = <BlurTexture>;
   MinFilter = POINT;
   MagFilter = POINT;
   MipFilter = POINT;   
   AddressU  = Clamp;
   AddressV  = Clamp;
   AddressW  = Clamp;
};

texture DepthTexture;
sampler depthSampler : register(s2) = sampler_state
{
   Texture = <DepthTexture>;
   MinFilter = POINT;
   MagFilter = POINT;
   MipFilter = POINT;   
   AddressU  = Clamp;
   AddressV  = Clamp;
   AddressW  = Clamp;
};

float4 PS(float2 texCoord : TEXCOORD0) : COLOR0
{  
    float4 scene = tex2D(TextureSampler, texCoord);
    float4 blur = tex2D(blurSampler, texCoord);
    float4 depth = tex2D(depthSampler, texCoord);

    float amount = (abs(depth.r - FocalPlane) - FocalLength) / FocalDistance;
    
    return lerp(scene, blur, saturate(amount));
}


Technique Default
{
	Pass
	{
		PixelShader	 = compile ps_2_0 PS();
	}
}
