/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/normalize.fxh#1 $

Copyright NVIDIA Corporation 2003
TO THE MAXIMUM EXTENT PERMITTED BY APPLICABLE LAW, THIS SOFTWARE IS PROVIDED
*AS IS* AND NVIDIA AND ITS SUPPLIERS DISCLAIM ALL WARRANTIES, EITHER EXPRESS
OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, IMPLIED WARRANTIES OF MERCHANTABILITY
AND FITNESS FOR A PARTICULAR PURPOSE.  IN NO EVENT SHALL NVIDIA OR ITS SUPPLIERS
BE LIABLE FOR ANY SPECIAL, INCIDENTAL, INDIRECT, OR CONSEQUENTIAL DAMAGES
WHATSOEVER (INCLUDING, WITHOUT LIMITATION, DAMAGES FOR LOSS OF BUSINESS PROFITS,
BUSINESS INTERRUPTION, LOSS OF BUSINESS INFORMATION, OR ANY OTHER PECUNIARY LOSS)
ARISING OUT OF THE USE OF OR INABILITY TO USE THIS SOFTWARE, EVEN IF NVIDIA HAS
BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.

Comments:
	Utility FX functions -- normalization cube map for pixel shaders.

	The following functions create and let you use a texture read to approximate 3D
	normalize() calls. In some cases, using normalize() may be just as fast!
	For details, please be sure to reference the Technical Brief "Normalization
	Heuristics" -- you can find the brief at http://developer.nvidia.com/

******************************************************************************/

#ifndef _H_NORMALIZE
#define _H_NORMALIZE

#define NORMALIZE_CUBE_SIZE 256

// for other texel formats, just define NORM_FORMAT before including normalize.fxh
#ifndef NORM_FORMAT
#define NORM_FORMAT "X8R8G8B8"
#endif /* NORM_FORMAT */

float4 make_normalization_cube(float3 Pos : POSITION) : COLOR {
	float3 n = normalize(Pos);
	float3 r = 0.5 + (0.5*n);
	return float4(r,0);
}

texture NormalizeTex <
	string TextureType = "CUBE";
    string function = "make_normalization_cube";
    string Format = (NORM_FORMAT);
    int width = NORMALIZE_CUBE_SIZE;
    int height = NORMALIZE_CUBE_SIZE;
    int depth = NORMALIZE_CUBE_SIZE;
>;

samplerCUBE NormalizeSampler = sampler_state 
{
    texture = <NormalizeTex>;
    AddressU  = CLAMP;        
    AddressV  = CLAMP;
    AddressW  = CLAMP;
    MIPFILTER = NONE;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};

// access the cube map instead of using normalize()
float3 my_normalize(float3 v)
{
	float3 v2 = texCUBE(NormalizeSampler,v);
	return (2*(v2-0.5));
}

// access the cube map instead of using normalize() -- only first three components!
float4 my_normalize(float4 v)
{
	float3 v2 = texCUBE(NormalizeSampler,v.xyz);
	return float4((2*(v2-0.5)),1);
}

// same as above, but for half data

half3 my_normalize(half3 v)
{
	half3 v2 = texCUBE(NormalizeSampler,v);
	return (2*(v2-0.5));
}

half4 my_normalize(half4 v)
{
	half3 v2 = texCUBE(NormalizeSampler,v.xyz);
	return half4((2*(v2-0.5)),1);
}

#endif /* _H_NORMALIZE */

// eof
