float4 PixelSizeAndViewport;
float3x3 TextureTransform;
float4 ScaleAndRotation;
float4 ScreenPositionAndAnchorPoint;

void VS(inout float2 uv:TEXCOORD0, inout float4 position:POSITION0)
{  
    uv = mul(uv, TextureTransform);

    position.xy -= ScreenPositionAndAnchorPoint.zw;
    position.xy *= ScaleAndRotation.xy;

    float2x2 rotation = 
    {
        ScaleAndRotation.z, -ScaleAndRotation.w,
        ScaleAndRotation.w, ScaleAndRotation.z,
    };

    position.xy = mul(position.xy, rotation);
    position.xy += ScreenPositionAndAnchorPoint.zw;
    position.xy += ScreenPositionAndAnchorPoint.xy;
    position.xy *= PixelSizeAndViewport.zw;
    position.xy += PixelSizeAndViewport.xy;    
    position = position;
}

Technique T2
{
    Pass
    {
        VertexShader = compile vs_2_0 VS();
    }
}