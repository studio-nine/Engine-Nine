
float2 HalfTexel;
float3 ProjectionParams;
float3 FocalParams;

sampler TextureSampler : register(s0);
sampler BlurSampler : register(s1);
sampler DepthSampler : register(s2);

float4 PS(float2 texCoord : TEXCOORD0) : COLOR0
{  
    float4 scene = tex2D(TextureSampler, texCoord);
    float4 blur = tex2D(BlurSampler, texCoord);    
    float4 depth = tex2D(DepthSampler, texCoord);

    // Even though Circle of Confusion is stored in the alpha channel of the blured texture,
    // that texture is 4x down scaled, so recompute coc per pixel
    
    float z = (depth.r - ProjectionParams.z) / (depth.r * ProjectionParams.x - ProjectionParams.y);
    float dofAmount = saturate((abs(z - FocalParams.x) - FocalParams.y) / FocalParams.z);

    scene.rgb = lerp(scene.rgb, blur.rgb, dofAmount);
    return scene;
}

static const float KernelOffsets[4] = {-3, -1, 1, 3};

float4 PSScale(float2 texCoord : TEXCOORD0) : COLOR0
{  
    float4 color = 0;
    float dofAmount = 0;
    for (int x = 0; x < 4; x++)
    {
        for (int y = 0; y < 4; y++)
        {
            float2 Offset = float2(KernelOffsets[x], KernelOffsets[y]) * HalfTexel;
            float2 uv = texCoord + Offset;
            color += tex2D(TextureSampler, uv);
            
            float4 depth = tex2D(DepthSampler, uv);
            float z = (depth.r - ProjectionParams.z) / (depth.r * ProjectionParams.x - ProjectionParams.y);
            dofAmount += saturate((abs(z - FocalParams.x) - FocalParams.y) / FocalParams.z);
        }
    }

    color *= (1.0f / 16.0f);
    dofAmount *= (1.0f / 16.0f);
    color.a = dofAmount;
    return color;
}


Technique Default
{
    Pass
    {
        PixelShader	 = compile ps_2_0 PS();
    }
}


Technique Scale
{
    Pass
    {
        PixelShader	 = compile ps_3_0 PSScale();
    }
}
