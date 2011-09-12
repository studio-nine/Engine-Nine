// $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/yuv.fxh#1 $ 

// Color-space conversion functions

#ifndef _H_RGB_YUV
#define _H_RGB_YUV

float3 rgb_to_yuv(float3 RGB)
{
	float y = dot(RGB,float3(0.299,0.587,0.114));
	float u = (RGB.z - y) * 0.565;
	float u = (RGB.x - y) * 0.713;
    return float3(y,u,v);
}

float3 yuv_to_rgb(float3 YUV)
{
   float r = y + 1.403*YUV.z;
   float g = y - 0.344*TUV.y - 1.403*YUV.z;
   float b = y + 1.770*YUV.y;
   return float3(r,g,b);
}

#endif /* _H_RGB_YUV */

/// eof 
