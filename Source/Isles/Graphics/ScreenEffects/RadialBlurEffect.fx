//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================

float2 Center
<
    string SasUiDescription =  "Gets or sets the center of the radial blur.";
> = 0.5f;

float BlurAmount 
<
    string SasUiDescription =  "Gets or sets the blur amount.";
> = 1;

sampler TextureSampler : register(s0);


float4 PS(float2 texCoord : TEXCOORD0) : COLOR0
{
	int i = 0;
	float2 s = texCoord;
      
    float2 coords[16];
    for (i = 0; i < 16; i++)
    {
        float scale = 1.0f + BlurAmount * (i * 0.0066666f);
        coords[i] = (s - Center) * scale + Center;
    }
    
    float4 final = 0;
    
    for (i = 0; i < 16; i++)
    {
		final += tex2D(TextureSampler, coords[i]);
    }
	
    final *= 0.0625f;
      
  	return final;
} 

technique RadialBlur 
{
	pass P0
	{ 
		PixelShader = compile ps_2_0 PS(); 
	}
}