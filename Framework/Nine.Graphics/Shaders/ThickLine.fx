float4x4 WorldView;
float4x4 Projection;
float2 Scale;
float Thickness;
float NearPlane;

void VS(inout float2 uv:TEXCOORD0, inout float4 position:POSITION0, float4 endPosition:NORMAL0, inout float4 color:COLOR0)
{
    color = color;
    uv = uv;
        
    float4 position1View = mul(position, WorldView);
    float4 position2View = mul(endPosition, WorldView);

    // Clamp to z = 0 plane
    float zzz = abs(position1View.z - position2View.z);
    float ratio1 = saturate((position1View.z - NearPlane) / zzz);
    position1View.xyz = lerp(position1View.xyz, position2View.xyz, ratio1);
    
    float ratio2 = saturate((position2View.z - NearPlane) / zzz);
    position2View.xyz = lerp(position2View.xyz, position1View.xyz, ratio2);
    
    float4 position1 = mul(position1View, Projection);
    float4 position2 = mul(position2View, Projection);

    position = position1;

    position1 /= position1.w;
    position2 /= position2.w;
        
    float2 direction = normalize(position2.xy - position1.xy) * sign(uv.x - 0.5f);
    float2 perpendicular = float2(direction.y, -direction.x);
    
    position.xy += perpendicular * Scale * Thickness * position.w;
}

Technique T
{
    Pass
    {
        VertexShader = compile vs_2_0 VS();
    }
}