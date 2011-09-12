// $Revision: #1 $ 

// Color-space conversion functions

#ifndef _H_RGB_HSV
#define _H_RGB_HSV

#include "Quad.fxh"

QUAD_REAL min_channel(QUAD_REAL3 v)
{
	QUAD_REAL t = (v.x<v.y) ? v.x : v.y;
	t = (t<v.z) ? t : v.z;
	return t;
}

QUAD_REAL max_channel(QUAD_REAL3 v)
{
	QUAD_REAL t = (v.x>v.y) ? v.x : v.y;
	t = (t>v.z) ? t : v.z;
	return t;
}

QUAD_REAL3 rgb_to_hsv(QUAD_REAL3 RGB)
{
    QUAD_REAL3 HSV = (0.0).xxx;
    QUAD_REAL minVal = min_channel(RGB);
    QUAD_REAL maxVal = max_channel(RGB);
    QUAD_REAL delta = maxVal - minVal;             //Delta RGB value 
    HSV.z = maxVal;
    if (delta != 0) {                    // If gray, leave H & S at zero
       HSV.y = delta / maxVal;
       QUAD_REAL3 delRGB;
       delRGB = ( ( ( maxVal.xxx - RGB ) / 6 ) + ( delta / 2 ) ) / delta;
       if      ( RGB.x == maxVal ) HSV.x = delRGB.z - delRGB.y;
       else if ( RGB.y == maxVal ) HSV.x = ( 1 / 3 ) + delRGB.x - delRGB.z;
       else if ( RGB.z == maxVal ) HSV.x = ( 2 / 3 ) + delRGB.y - delRGB.x;
       if ( HSV.x < 0 ) { HSV.x += 1; }
       if ( HSV.x > 1 ) { HSV.x -= 1; }
    }
    return (HSV);
}

QUAD_REAL3 hsv_to_rgb(QUAD_REAL3 HSV)
{
    QUAD_REAL3 RGB = HSV.z;
    if ( HSV.y != 0 ) {
       QUAD_REAL var_h = HSV.x * 6;
       QUAD_REAL var_i = floor(var_h);             //Or ... var_i = floor( var_h )
       QUAD_REAL var_1 = HSV.z * (1 - HSV.y);
       QUAD_REAL var_2 = HSV.z * (1 - HSV.y * (var_h-var_i));
       QUAD_REAL var_3 = HSV.z * (1 - HSV.y * (1-(var_h-var_i)));
       if      (var_i == 0) { RGB = QUAD_REAL3(HSV.z, var_3, var_1); }
       else if (var_i == 1) { RGB = QUAD_REAL3(var_2, HSV.z, var_1); }
       else if (var_i == 2) { RGB = QUAD_REAL3(var_1, HSV.z, var_3); }
       else if (var_i == 3) { RGB = QUAD_REAL3(var_1, var_2, HSV.z); }
       else if (var_i == 4) { RGB = QUAD_REAL3(var_3, var_1, HSV.z); }
       else                 { RGB = QUAD_REAL3(HSV.z, var_1, var_2); }
   }
   return (RGB);
}

#endif /* _H_RGB_HSV */

/// eof 
