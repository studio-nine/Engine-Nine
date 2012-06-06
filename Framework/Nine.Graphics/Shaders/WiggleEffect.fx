/*
 * Tutorial
 * XNA Shader programming
 * www.gamecamp.no
 * 
 * by: Petri T. Wilhelmsen
 * e-mail: petriw@gmail.com
 * 
 * Feel free to ask me a question, give feedback or correct mistakes!
 * This shader is mostly based on the shader "post edgeDetect" from nVidias Shader library:
 * http://developer.download.nvidia.com/shaderlibrary/webpages/shader_library.html
 */


// Global variables
// This will use the texture bound to the object( like from the sprite batch ).
sampler ColorMapSampler : register(s0);

//A timer we can use for whatever purpose we want
float fTimer;
float Speed;
float Amplitude;
float Repeat;
float2 pixelSize;

float4 PS(float2 Tex: TEXCOORD0) : COLOR
{
	// Use the timer to move the texture coordinated before using them to lookup
	// in the ColorMapSampler. This makes the scene look like its underwater
	// or something similar :)
	float2 border = saturate(Tex * (1 - Tex) * 10);
	
	Tex.x += sin(fTimer*Speed+Tex.x*Repeat)*pixelSize.x*Amplitude*border.x;
	Tex.y += cos(fTimer*Speed+Tex.y*Repeat)*pixelSize.y*Amplitude*border.y;
	
	return tex2D(ColorMapSampler, Tex);
}

technique PostProcess
{
	pass P0
	{
		// A post process shader only needs a pixel shader.
		PixelShader = compile ps_2_0 PS();
	}
}