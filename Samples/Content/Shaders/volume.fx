/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/volume.fx#1 $

Copyright NVIDIA Corporation 2004
TO THE MAXIMUM EXTENT PERMITTED BY APPLICABLE LAW, THIS SOFTWARE IS PROVIDED
*AS IS* AND NVIDIA AND ITS SUPPLIERS DISCLAIM ALL WARRANTIES, EITHER EXPRESS
OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, IMPLIED WARRANTIES OF MERCHANTABILITY
AND FITNESS FOR A PARTICULAR PURPOSE.  IN NO EVENT SHALL NVIDIA OR ITS SUPPLIERS
BE LIABLE FOR ANY SPECIAL, INCIDENTAL, INDIRECT, OR CONSEQUENTIAL DAMAGES
WHATSOEVER (INCLUDING, WITHOUT LIMITATION, DAMAGES FOR LOSS OF BUSINESS PROFITS,
BUSINESS INTERRUPTION, LOSS OF BUSINESS INFORMATION, OR ANY OTHER PECUNIARY LOSS)
ARISING OUT OF THE USE OF OR INABILITY TO USE THIS SOFTWARE, EVEN IF NVIDIA HAS
BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.

	Single-pass volume rendering using ray marching
		sgreen 6/04	
	See: http://www.siggraph.org/education/materials/HyperGraph/raytrace/rtinter3.htm

******************************************************************************/

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";
	string Script = "Technique=Technique?PS20:PS30;";
> = 0.8; // version #

float4x4 view : View < string UIWidget="none";>;
float4x4 viewInv : ViewInverse < string UIWidget="none";>;
float2 viewport : VIEWPORTPIXELSIZE < string UIWidget="none";>;

float3 boxMin = { -1.0, -1.0, -1.0 };
float3 boxMax = { 1.0, 1.0, 1.0 };

int steps = 30;

float foclen <
	string UIWidget="Slider";
	string UIName="Focal Length";
	float UIMin = 1.0f; float UIMax = 5000.0f; float UIStep = 10.0f;
> = 500.0;

float brightness
<
	string UIWidget = "slider";
	float UIMin = 0.0f; float UIStep = 0.1f; float UIMax = 100.0f;
> = 50.0;

texture volumeTexture
<
    string texturetype = "3D";
	string function = "GenerateVolumeNoise";
//	int width = 128, height = 128, depth = 128;
	int width = 64, height = 64, depth = 64;
>;

sampler3D volumeSampler = sampler_state
{
	Texture = <volumeTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

float4 GenerateVolumeNoise(float3 p : POSITION) : COLOR
{
	const float noisescale = 5.0;
	p *= noisescale;
	return float4(noise(p),
				  noise(p + float3(5, 7, 13)),
				  noise(p + float3(9, 37, 57)),
				  noise(p + float3(17, 21, 99)));
}

struct Ray {
	float3 o;	// origin
	float3 d;	// direction
};

////////////////////////////////////////////////////////

// Ray-box intesection using slabs method
// See: http://www.siggraph.org/education/materials/HyperGraph/raytrace/rtinter3.htm

// Intersect ray with slab in one axis
// o, d - ray origin, direction
// l, h - slab minimum, maximum
void IntersectSlab(float o, float d, float l, float h, inout bool hit, inout float tnear, inout float tfar)
{
	if (d==0.0) {
		// ray parallel to planes
		if ((o < l) || (o > h)) {
			// ray is not between slabs
			hit = false;
//			return;
		}
	}
	
	float t1 = (l - o) / d;
	float t2 = (h - o) / d;
	if (t1 > t2) {
		// swap so that t1 is nearest
		float temp = t1;
		t1 = t2;
		t2 = temp;
	}

	if (t1 > tnear) {
		tnear = t1;
	}
	if (t2 < tfar) {
		tfar = t2;
	}

	if (tnear > tfar) {
		// box missed
		hit = false;
	}
	if (tfar < 0) {
		// box behind ray
		hit = false;
	}
}

// Intersect ray with box
// note - could probably vectorize this better
bool IntersectBox(Ray r, float3 Bl, float3 Bh, out float tnear, out float tfar)
{
	tnear = -1e20;
	tfar = 1e20;
	bool hit = true;
	IntersectSlab(r.o.x, r.d.x, Bl.x, Bh.x, hit, tnear, tfar);
	IntersectSlab(r.o.y, r.d.y, Bl.y, Bh.y, hit, tnear, tfar);
	IntersectSlab(r.o.z, r.d.z, Bl.z, Bh.z, hit, tnear, tfar);
	return hit;
}

// Vertex shader
void RayMarchVS(inout float4 pos : POSITION,
				in float4 texcoord : TEXCOORD0,
				out Ray eyeray : TEXCOORD0
				)
{
	// calculate world space eye ray
	// origin
	eyeray.o = mul(float4(0, 0, 0, 1), viewInv);

	// direction
	eyeray.d.xy = ((texcoord*2.0)-1.0) * viewport;
	eyeray.d.y = -eyeray.d.y;	// flip y axis
	eyeray.d.z = foclen;
	
	eyeray.d = mul(eyeray.d, (float3x3) viewInv);
}

// Pixel shader
float4 RayMarchPS(Ray eyeray : TEXCOORD0,
				  uniform sampler3D volume,
				  uniform int steps) : COLOR
{
	eyeray.d = normalize(eyeray.d);

	// calculate ray intersection with bounding box
	float tnear, tfar;
	bool hit = IntersectBox(eyeray, boxMin, boxMax, tnear, tfar);
	if (!hit) discard;

	// calculate intersection points
	float3 Pnear = eyeray.o + eyeray.d*tnear;
	float3 Pfar = eyeray.o + eyeray.d*tfar;
		
	// map box world coords to texture coords
	Pnear = Pnear*0.5 + 0.5;
	Pfar = Pfar*0.5 + 0.5;
	
	// march along ray, accumulating color
	half4 c = 0;
	half3 Pstep = (Pnear - Pfar) / (steps-1);
	half3 P = Pfar;
	// this compiles to a real loop in PS3.0:
	for(int i=0; i<steps; i++) {		
		half4 s = tex3D(volume, P);
		c = (1.0-s.a)*c + s.a*s;
		P += Pstep;
	}
	c /= steps;
	c *= brightness;

//	return hit;
//	return tfar - tnear;
	return c;
}

////////////////////////////////////////////////////////

technique PS20
<
	string ScriptClass = "scene";
	string Script = "Pass=p0;";
>
{
    pass p0
    <
    	string Script = "Clear=color;"
						"Clear=depth;"
						"Draw=Buffer;";
    >
    {						
		VertexShader = compile vs_1_1 RayMarchVS();
		PixelShader = compile ps_2_a RayMarchPS(volumeSampler, 30);
    }
}

technique PS30
<
	string ScriptClass = "scene";
	string Script = "Pass=p0;";
>
{
    pass p0
    <
    	string Script = "Clear=color;"
						"Clear=depth;"
						"Draw=Buffer;";
    >
    {
		VertexShader = compile vs_3_0 RayMarchVS();
		PixelShader = compile ps_3_0 RayMarchPS(volumeSampler, 30);	// BUG - produces incorrect results if you use variable steps here
    }
}
