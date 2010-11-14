//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================


// Pixel shader combines the bloom image with the original
// scene, using tweakable intensity levels and saturation.
// This is the final step in applying a bloom postprocess.

sampler BaseSampler : register(s0);

float BloomIntensity = 1.0f;
float BloomSaturation = 1.0f;


float BrightnessThreshold = 0.5f;


float2 Center
<
    string SasUiDescription =  "Gets or sets the center of the radial blur.";
> = 0.5f;

float BlurAmount 
<
    string SasUiDescription =  "Gets or sets the blur amount.";
> = 1;


float4 RadicalBlur(float2 texCoord : TEXCOORD0) : COLOR0
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
		final += tex2D(BaseSampler, coords[i]);
    }
	
    final *= 0.0625f;
      
  	return final;
} 

// Helper for modifying the saturation of a color.
float4 AdjustSaturation(float4 color, float saturation)
{
    // The constants 0.3, 0.59, and 0.11 are chosen because the
    // human eye is more sensitive to green light, and less to blue.
    float grey = dot(color, float3(0.3, 0.59, 0.11));

    return lerp(grey, color, saturation);
}


float4 PS(float2 texCoord : TEXCOORD0) : COLOR0
{
	float4 c = tex2D(BaseSampler, texCoord);

    // Adjust it to keep only values brighter than the specified threshold.	
	float4 bloom = RadicalBlur(texCoord);
    bloom = saturate((bloom - BrightnessThreshold) / (1 - BrightnessThreshold));

	float4 base = AdjustSaturation(c, 0.9);

    // Combine the two images.
    return base + bloom;
}


technique BloomCombine
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PS();
    }
}
