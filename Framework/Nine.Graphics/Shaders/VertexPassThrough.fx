void VS(inout float2 uv:TEXCOORD0, inout float4 position:POSITION)
{  
    uv = uv;
    position = position;
}

Technique Default
{
    Pass
    {
        VertexShader = compile vs_2_0 VS();
    }
}
