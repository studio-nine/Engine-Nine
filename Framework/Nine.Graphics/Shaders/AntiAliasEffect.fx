#define FXAA_PC 1
#define FXAA_HLSL_3 1
#define FXAA_QUALITY__PRESET 12
#define FXAA_GREEN_AS_LUMA 1

#include "Fxaa3_11.fxh"

float4 PixelSize;
sampler TextureSampler:register(s0);

void VS(inout float2 uv:TEXCOORD0, inout float4 position:POSITION0)
{  
    uv = uv;
    position.xy += PixelSize.zw;
    position = position;
}

float4 PS(float2 uv:TEXCOORD0):COLOR0
{
    return FxaaPixelShader( 
		uv,                 //pos 
		0,                  //fxaaConsolePosPos (not used) 
		TextureSampler,     //tex 
		TextureSampler,     //fxaaConsole360TexExpBiasNegOne (not used)
		TextureSampler,     //fxaaConsole360TexExpBiasNegTwo (not used)
		PixelSize.xy,       //fxaaQualityRcpFrame 
		0,                  //fxaaConsoleRcpFrameOpt (not used) 
		0,                  //fxaaConsoleRcpFrameOpt2 (not used)
		0,                  //fxaaConsole360RcpFrameOpt2 (not used)
		0.6,                //fxaaQualitySubpix, 
		0.166,              //fxaaQualityEdgeThreshold, 
		0.0625,             //fxaaQualityEdgeThresholdMin, 
		8.0,                //fxaaConsoleEdgeSharpness (not used)
		0.125,              //fxaaConsoleEdgeThreshold (not used)
		0.05,               //fxaaConsoleEdgeThresholdMin (not used)
		0                   //fxaaConsole360ConstDir (not used)
    );
} 

technique Default
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS();
    }
}