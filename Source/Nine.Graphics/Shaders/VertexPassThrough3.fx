float2 PixelSize;

void VS(inout float2 uv:TEXCOORD0, inout float4 position:POSITION0)
{  
    uv = uv;
    position.xy += PixelSize;
    position = position;
}

Technique T3
{
    Pass
    {
#if DirectX
        VertexShader = compile vs_4_0 VS();
#elif OpenGL
        VertexShader = compile vs_3_0 VS();
#else
        VertexShader = compile vs_3_0 VS();
#endif
    }
}
