

#define MaxSpecular 255

void Extract(sampler normalBuffer,
             sampler depthBuffer,
             float4 posProjection,
             float2 halfPixel,
             float4x4 viewProjectionInverse,
             out float3 normal, 
             out float3 position, 
             out float specularPower)
{
    posProjection.xy /= posProjection.w;

    float2 uv = float2(posProjection.x,-posProjection.y) * 0.5f + 0.5f + halfPixel;

    float4 g = tex2D(normalBuffer, uv);
    normal = g.xyz * 2 - 1;
    specularPower = g.w * MaxSpecular;
    
    float z = tex2D(depthBuffer, uv).x;

    float4 p = mul(float4(posProjection.xy, z, 1), viewProjectionInverse);
    position = p.xyz / p.w;
}


// Adopted from: http://aras-p.info/texts/CompactNormalStorage.html

float2 EncodeNormal(float3 n)
{   
    float f = sqrt(8*n.z+8);
    return n.xy / f + 0.5;
}

float3 DecodeNormal(float2 enc)
{    
    float2 fenc = enc*4-2;
    float f = dot(fenc,fenc);
    float g = sqrt(1-f/4);
    float3 n;
    n.xy = fenc*g;
    n.z = 1-f/2;
    return n;
}
