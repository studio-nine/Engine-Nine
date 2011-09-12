// noise_2d VM function
// $Revision: #1 $ 

#ifndef _H_NOISE2D
#define _H_NOISE2D

#ifndef NOISE2D_SCALE
#define NOISE2D_SCALE 500
#endif /* NOISE2D_SCALE */

// function used to fill the volume noise texture
float4 noise_2d(float2 Pos : POSITION) : COLOR
{
    float4 Noise = (float4)0;
    for (int i = 1; i < 256; i += i) {
        Noise.r += abs(noise(Pos * NOISE2D_SCALE * i)) / i;
        Noise.g += abs(noise((Pos + 1)* NOISE2D_SCALE * i)) / i;
        Noise.b += abs(noise((Pos + 2) * NOISE2D_SCALE * i)) / i;
        Noise.a += abs(noise((Pos + 3) * NOISE2D_SCALE * i)) / i;
    }
    return Noise;
}

#ifndef NOISE_SHEET_SIZE
#define NOISE_SHEET_SIZE 128
#endif /* NOISE_SHEET_SIZE */

texture Noise2DTex  <
    string TextureType = "2D";
    string function = "noise_2d";
    string UIWidget = "None";
    int width = NOISE_SHEET_SIZE, height = NOISE_SHEET_SIZE;
>;

// samplers
sampler Noise2DSamp = sampler_state 
{
    texture = <Noise2DTex>;
    AddressU  = WRAP;        
    AddressV  = WRAP;
    AddressW  = WRAP;
    MIPFILTER = LINEAR;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};

#define NOISE2D(p) tex2D(Noise2DSamp,(p))
#define SNOISE2D(p) (NOISE2D(p)-0.5)

#endif /* _H_NOISE2D */

