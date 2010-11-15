//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================


sampler TextureSampler : register(s0);


uniform float pixelCount = 80;
uniform float Threshhold = 0.15;
uniform float3 EdgeColor = {0.7, 0.7, 0.7};

float4 PS(float2 texCoord : TEXCOORD0) : COLOR0
{
    float size = 1.0f / pixelCount;
    half2 Pbase = texCoord - fmod(texCoord, size.xx);
    half2 PCenter = Pbase + (size/2.0).xx;
    half2 st = (texCoord - Pbase)/size;
    half4 c1 = (half4)0;
    half4 c2 = (half4)0;
    half4 invOff = half4((1-EdgeColor),1);
    if (st.x > st.y) { c1 = invOff; }
    float threshholdB =  1.0 - Threshhold;
    if (st.x > threshholdB) { c2 = c1; }
    if (st.y > threshholdB) { c2 = c1; }
    half4 cBottom = c2;
    c1 = (half4)0;
    c2 = (half4)0;
    if (st.x > st.y) { c1 = invOff; }
    if (st.x < Threshhold) { c2 = c1; }
    if (st.y < Threshhold) { c2 = c1; }
    half4 cTop = c2;
    half4 tileColor = tex2D(TextureSampler, PCenter);
    half4 result = tileColor + cTop - cBottom;
    return result;
} 

technique Tiling
{
	pass P0
	{ 
		PixelShader = compile ps_2_0 PS(); 
	}
}