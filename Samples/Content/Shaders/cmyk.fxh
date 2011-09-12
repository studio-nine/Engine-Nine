/*********************************************************************NVMH3****

File: $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/cmyk.fxh#1 $
Copyright NVIDIA Corporation 2002-2004

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
    Functions for RGB-CMY and CMYK conversions. Can be use in vertex, pixel, or texture shaders

******************************************************************************/

#ifndef _CMYK_
#define _CMYK_

#ifndef CREAL
#define CREAL half
#define CREAL2 half2
#define CREAL3 half3
#define CREAL4 half4
#define CREAL4x4 half4x4
#endif /* !defined(CREAL) */

///////////////////////////////////////////////////
// fluffy-logic functions /////////////////////////
///////////////////////////////////////////////////

CREAL3 rgb2cmy(CREAL3 rgbColor) {
	return (CREAL3(1,1,1) - rgbColor);	// simplest conversion
}

CREAL3 cmy2rgb(CREAL3 cmyColor) {
	return (CREAL3(1,1,1) - cmyColor);	// simplest conversion
}

/////////

CREAL4 cmy2cmyk(CREAL3 cmyColor)
{
	CREAL k = ((CREAL)1.0);
	k = min(k,cmyColor.x);
	k = min(k,cmyColor.y);
	k = min(k,cmyColor.z);
	CREAL4 cmykColor;
	cmykColor.xyz = (cmyColor - (CREAL3)k)/((CREAL3)(((CREAL)1.0)-k).xxx);
	cmykColor.w = k;
	return (cmykColor);
}

CREAL3 cmyk2cmy(CREAL4 cmykColor)
{
	CREAL3 k = cmykColor.www;
	return ((cmykColor.xyz * (CREAL3(1,1,1)-k)) + k);
}

/////////

CREAL4 rgb2cmyk(CREAL3 rgbColor) { return cmy2cmyk(rgb2cmy(rgbColor)); }

CREAL3 cmyk2rgb(CREAL4 cmykColor) { return cmy2rgb(cmyk2cmy(cmykColor)); }

#endif /* _CMYK_ */

// eof
