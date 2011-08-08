float3 AmbientLightColor;
float2 halfPixel;

void VS(float4 Pos : POSITION, out float4 oPos : POSITION)
{
    oPos = Pos;
    oPos.xy -= halfPixel;
}

void PS(out float4 Color:COLOR)
{
    Color = float4(AmbientLightColor, 0);
}


Technique BasicEffect
{
	Pass
	{
		VertexShader = compile vs_2_0 VS();
		PixelShader	 = compile ps_2_0 PS();
	}
}
