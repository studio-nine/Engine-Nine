float4x4 WorldViewProjection;
float2 Scale;
float Thickness;

void VS(inout float2 uv:TEXCOORD0, inout float4 position:POSITION0, float4 endPosition:NORMAL0, inout float4 color:COLOR0)
{
    color = color;
    uv = uv;
        
    float4 position1 = mul(position, WorldViewProjection);
    float4 position2 = mul(endPosition, WorldViewProjection);
    
    position1 /= position1.w;
    position2 /= position2.w;

    position = position1;
    
    float2 direction = normalize(position2.xy - position1.xy);
    float2 perpendicular = float2(direction.y, -direction.x);

    position.xy += perpendicular * Scale * Thickness;
}

Technique T
{
    Pass
    {
        VertexShader = compile vs_2_0 VS();
    }
}