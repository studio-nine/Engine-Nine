
float FocalPlane = 0;
float FocalLength = 0.2f;
float FocalDistance = 0.2f;

sampler TextureSampler : register(s0);
sampler BlurSampler : register(s1);
sampler DepthSampler : register(s2);

float4 PS(float2 texCoord : TEXCOORD0) : COLOR0
{  
    float4 scene = tex2D(TextureSampler, texCoord);
    float4 blur = tex2D(BlurSampler, texCoord);
    float4 depth = tex2D(DepthSampler, texCoord);

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
