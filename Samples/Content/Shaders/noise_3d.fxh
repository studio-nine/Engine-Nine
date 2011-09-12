// noise_3d VM function
// $Revision: #1 $ 

#ifndef _H_NOISE3D
#define _H_NOISE3D

#ifndef NOISE_SCALE
#define NOISE_SCALE 500
#endif /* NOISE_SCALE */

// function used to fill the volume noise texture
float4 noise_3d(float3 Pos : POSITION) : COLOR
{
    float4 Noise = (float4)0;
    for (int i = 1; i < 256; i += i) {
        Noise.r += abs(noise(Pos * NOISE_SCALE * i)) / i;
        Noise.g += abs(noise((Pos + 1)* NOISE_SCALE * i)) / i;
        Noise.b += abs(noise((Pos + 2) * NOISE_SCALE * i)) / i;
        Noise.a += abs(noise((Pos + 3) * NOISE_SCALE * i)) / i;
    }
    return Noise;
}

#ifndef NOISE_VOLUME_SIZE
#define NOISE_VOLUME_SIZE 32
#endif /* NOISE_VOLUME_SIZE */

texture NoiseTex  <
    string TextureType = "VOLUME";
    string function = "noise_3d";
    string UIWidget = "None";
    int width = NOISE_VOLUME_SIZE, height = NOISE_VOLUME_SIZE, depth = NOISE_VOLUME_SIZE;
>;

// samplers
sampler NoiseSamp = sampler_state 
{
    texture = <NoiseTex>;
    AddressU  = WRAP;        
    AddressV  = WRAP;
    AddressW  = WRAP;
    MIPFILTER = LINEAR;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};

#define NOISE3D(p) tex3D(NoiseSamp,(p))
#define SNOISE3D(p) (NOISE3D(p)-0.5)

#endif /* _H_NOISE3D */

