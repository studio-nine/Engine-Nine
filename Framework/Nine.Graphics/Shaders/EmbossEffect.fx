sampler samplerState : register(s0);
float2 pixelSize;
float Emboss = 1;

float4 PostProcessPS( float2 Tex : TEXCOORD0 ) : COLOR0
{
    float4 Color;
    Color.a = 1.0f;
    Color.rgb = 0.5f;
    Color -= tex2D( samplerState, Tex - pixelSize)*Emboss;
    Color += tex2D( samplerState, Tex + pixelSize)*Emboss;
    Color.rgb = (Color.r+Color.g+Color.b)/3.0f;
    return Color;
}

technique PostProcess
{
    pass p0
    {        
        PixelShader = compile ps_2_0 PostProcessPS();
    }
}
