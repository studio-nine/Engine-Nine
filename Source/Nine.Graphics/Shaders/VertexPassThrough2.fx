float2 PixelSize;

void VS(inout float2 uv:TEXCOORD0, inout float4 position:POSITION0)
{  
    uv = uv;
    position.xy += PixelSize;
    position = position;
}

Technique T2
{
    Pass
    {
        VertexShader = compile vs_2_0 VS();
    }
}