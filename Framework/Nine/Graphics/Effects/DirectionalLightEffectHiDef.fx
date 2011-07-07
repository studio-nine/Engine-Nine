
#define MaxBones 59
#define MaxLights 8

#include "DirectionalLight.fxh"

int ShaderIndex = 0;


VertexShader VSArray[2] =
{
	compile vs_3_0 VertShadow(),
	compile vs_3_0 VertShadowSkinned(),
};


PixelShader PSArray[2] =
{
	compile ps_3_0 PixShadow(),
	compile ps_3_0 PixShadow(),
};


Technique BasicEffect
{
	Pass
	{
		VertexShader = (VSArray[ShaderIndex]);
		PixelShader	 = (PSArray[ShaderIndex]);
	}
}
