/*
File: ThinFilm.fx

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
*/

// Declarations for thinfilm effect

#ifndef _H_THINFILMTEX
#define _H_THINFILMTEX

//////////////////////////////////////////////////////////////////////////
// tweakables ////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////

float FilmDepth <
	string UIName = "Film Thickness";
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 0.25;
	float UIStep = 0.001;
> = 0.05f;

//============================================================================
// texture samplers
//============================================================================

#define FILM_TEX_SIZE 256

texture fringeMap <
    string function = "CreateFringeMap";
    string UIWidget = "None";
    int width = FILM_TEX_SIZE;
    int height = 1;	// 1D lookup
>;

sampler FringeMapSampler = sampler_state 
{
    Texture   = <fringeMap>;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU  = WRAP;
    AddressV  = CLAMP;
};

float4 CreateFringeMap(float2 Pos:POSITION, float2 Psize : PSIZE) : COLOR
{
    // these lambdas are in 100's of nm,
    //  they represent the wavelengths of light for each respective 
    //  color channel.  They are only approximate so that the texture
    //  can repeat.
	float3 lamRGB = float3(6,5,4); // (600,500,400)nm - should be more like (600,550,440)
    // these offsets are used to perturb the phase of the interference
    //   if you are using very thick "thin films" you will want to
    //   modify these offests to avoid complete contructive interference
    //   at a particular depth.. Just a tweak able.
	float3 offsetRGB = (0).xxx;
    // p is the period of the texture, it is the LCM of the wavelengths,
    //  this is the depth in nm when the pattern will repeat.  I was too
    //  lazy to write up a LCM function, so you have to provide it.
    float p = 60;   //lcm(6,5,4)
    // vd is the depth of the thin film relative to the texture index
    float vd = p;
    // now compute the color values using this formula:
    //  1/2 ( Sin( 2Pi * d/lam* + Pi/2 + O) + 1 )
    //   where d is the current depth, or "i*vd" and O is some offset* so that
    //   we avoid complete constructive interference in all wavelenths at some depth.
    float pi = 3.1415926535f;
	float3 rgb = 0.5*(sin(2*pi*(Pos.x*vd)/lamRGB + pi/2.0 + offsetRGB) + 1);
    return float4(rgb,0);
}

//////////////////////////////////////////////////////////////////////////
// Function to Index this texture - use in vertex or pixel shaders ///////
//////////////////////////////////////////////////////////////////////////

float calc_view_depth(float NDotV,float Thickness)
{
    // return (1.0 / NDotV) * Thickness;
    return (Thickness / NDotV);
}


#endif /* _H_THINFILMTEX */

/////////////////////////////////// eof ///
