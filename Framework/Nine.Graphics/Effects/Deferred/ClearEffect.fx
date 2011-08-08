float3 normal = float3(0, 0, 1);
float depth = 1;


void VS(float4 Pos : POSITION, out float4 oPos : POSITION)
{
    oPos = Pos;
}

void PS(out float4 Normal:COLOR0, out float4 Depth:COLOR1)
{
    Normal = float4(normal, 1);
    Depth = float4(depth, 0, 0, 0);
}

Technique Default
{
	Pass
	{
		VertexShader = compile vs_2_0 VS();
		PixelShader	 = compile ps_2_0 PS();
	}
}
