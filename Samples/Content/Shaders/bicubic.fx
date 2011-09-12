/*
	Bicubic texture filtering
	This effect demonstrates bicubic texture filtering, a higher quality
	filtering method that uses a 4x4 footprint rather than the usual 2x2.
	
	There are three versions included, with increasing levels of optimization:
	
				Passes		Comment
	Bicubic		141			Full precision, all math
	Bicubic2	78			Uses texture lookup, half precision
	Bicubic3	52			Calculates texcoord in vertex shader
	Bilinear	1			Hardware
	
	References:
	http://www.engineering.uiowa.edu/~gec/248_s00_students/blake_carlson/hw2/
	http://www.path.unimelb.edu.au/~dersch/interpolator/interpolator.html
*/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?Bicubic:Bicubic2:Bicubic3:Bilinear;";
> = 0.8;

float4x4 worldViewProj : WorldViewProjection;

// you need to change these if you load a different texture:
const float2 imageSize = { 32.0, 32.0 };
const float2 texelSize = { 1.0 / 32.0, 1.0 / 32.0 };

texture imageTexture
<
	string ResourceName = "lowres.png";
>;

texture weightTex
<
	string ResourceType = "2D";
//	string format = "A16B16G16R16F";	// should use a fp texture for this
	string function = "genWeightTex";
	float2 Dimensions = { 256.0f, 1.0f};
>;

sampler imageSampler = sampler_state 
{
    texture = <imageTexture>;
    MagFilter = Point;
    MinFilter = Point;
    MipFilter = None;
};

sampler imageSamplerBilinear = sampler_state 
{
    texture = <imageTexture>;
    MagFilter = Linear;
    MinFilter = Linear;
    MipFilter = None;
};

sampler weightSampler = sampler_state 
{
    texture = <weightTex>;
    MagFilter = Linear;
    MinFilter = Linear;
    MipFilter = None;
    AddressU = Clamp;
    AddressV = Clamp;
};

// vertex shader
struct a2v {
    float4 pos		: POSITION;
    float4 texcoord	: TEXCOORD0;
};
struct v2f {
    float4 pos		: POSITION;
    float4 texcoord	: TEXCOORD0;
};

v2f bicubicVS(a2v IN)
{
	v2f OUT;
	OUT.pos = mul(IN.pos, worldViewProj);
	OUT.texcoord = IN.texcoord;
    return OUT;
}

// compute 4 weights using the cubic polynomial:
// w = (A+2.0)*x^3 - (A+3.0)*x^2         + 1.0    for 0<x<1
// w = A*x^3 -       5.0*A*x^2 + 8.0*A*x - 4.0*A  for 1<x<2
// A controls sharpness
float4 computeWeights(float x)
{
	// calculate weights in parallel
	float4 x1 = x*float4(1,1,-1,-1) + float4(1,0,1,2); // (x+1, x, 1-x, 2-x)
	float4 x2 = x1*x1;
	float4 x3 = x2*x1;
	
//	const float A = -1;
	const float A = -0.75;
	float4 w;	
	w =  x3 * float2(  A,      A+2.0).xyyx;
	w += x2 * float2( -5.0*A, -(A+3.0)).xyyx;
	w += x1 * float2(  8.0*A,  0).xyyx;
	w +=      float2( -4.0*A,  1.0).xyyx;
	return w;
}

// filter 4 values
float4 cubicFilter(float4 w, float4 c0, float4 c1, float4 c2, float4 c3)
{
	return c0*w[0] + c1*w[1] + c2*w[2] + c3*w[3];
}

// pixel shaders

float4 tex2D_bicubic(sampler2D tex, float2 t)
{
	float2 f = frac(t*imageSize);	// fractional position within texel
	// filter in x
	float4 w = computeWeights(f.x);
	float4 t0 = cubicFilter(w,	tex2D(tex, t+float2(-1, -1)*texelSize),
							  	tex2D(tex, t+float2(0, -1)*texelSize),
							  	tex2D(tex, t+float2(1, -1)*texelSize),
							  	tex2D(tex, t+float2(2, -1)*texelSize) );
	float4 t1 = cubicFilter(w,	tex2D(tex, t+float2(-1, 0)*texelSize),
								tex2D(tex, t+float2(0, 0)*texelSize),
								tex2D(tex, t+float2(1, 0)*texelSize),
								tex2D(tex, t+float2(2, 0)*texelSize) );
	float4 t2 = cubicFilter(w,	tex2D(tex, t+float2(-1, 1)*texelSize),
								tex2D(tex, t+float2(0, 1)*texelSize),
								tex2D(tex, t+float2(1, 1)*texelSize),
								tex2D(tex, t+float2(2, 1)*texelSize) );
	float4 t3 = cubicFilter(w,	tex2D(tex, t+float2(-1, 2)*texelSize),
								tex2D(tex, t+float2(0, 2)*texelSize),
								tex2D(tex, t+float2(1, 2)*texelSize),
								tex2D(tex, t+float2(2, 2)*texelSize) );
	// filter in y								
	w = computeWeights(f.y);
	return cubicFilter(w, t0, t1, t2, t3);
	return w;
}

// version 2 - uses texture lookups to obtain the weights
// uses half precision where possible
float4 genWeightTex(float2 p : POSITION) : COLOR
{
	return computeWeights(p.x)*0.5+0.5;
}

half4 cubicFilterh(half4 w, half4 c0, half4 c1, half4 c2, half4 c3)
{
	return c0*w[0] + c1*w[1] + c2*w[2] + c3*w[3];
}

half4 tex2D_bicubic2(sampler2D tex, sampler2D weightTex, float2 t)
{
	half2 f = frac(t*imageSize);	// fractional position within texel
	// filter in x
	half4 w = tex2D(weightTex, f.x)*2-1;
	half4 t0 = cubicFilterh(w,	tex2D(tex, t+float2(-1, -1)*texelSize),
							  	tex2D(tex, t+float2(0, -1)*texelSize),
							  	tex2D(tex, t+float2(1, -1)*texelSize),
							  	tex2D(tex, t+float2(2, -1)*texelSize) );
	half4 t1 = cubicFilterh(w,	tex2D(tex, t+float2(-1, 0)*texelSize),
								tex2D(tex, t+float2(0, 0)*texelSize),
								tex2D(tex, t+float2(1, 0)*texelSize),
								tex2D(tex, t+float2(2, 0)*texelSize) );
	half4 t2 = cubicFilterh(w,	tex2D(tex, t+float2(-1, 1)*texelSize),
								tex2D(tex, t+float2(0, 1)*texelSize),
								tex2D(tex, t+float2(1, 1)*texelSize),
								tex2D(tex, t+float2(2, 1)*texelSize) );
	half4 t3 = cubicFilterh(w,	tex2D(tex, t+float2(-1, 2)*texelSize),
								tex2D(tex, t+float2(0, 2)*texelSize),
								tex2D(tex, t+float2(1, 2)*texelSize),
								tex2D(tex, t+float2(2, 2)*texelSize) );
	// filter in y									
	w = tex2D(weightTex, f.y)*2-1;
	return cubicFilter(w, t0, t1, t2, t3);
	return w;
}

// version 3 - calculate texcoord in vertex shader
struct v2f2 {
    float4 pos		 : POSITION;
    float4 TexCoord0 : TEXCOORD0;
    float4 TexCoord1 : TEXCOORD1;
    float4 TexCoord2 : TEXCOORD2;
    float4 TexCoord3 : TEXCOORD3;            
};

v2f2 bicubic2VS(a2v IN)
{
	v2f2 OUT;
	OUT.pos = mul(IN.pos, worldViewProj);
	float2 t = IN.texcoord - texelSize*0.5;
    float4 offsets = float4(-1.0f, 0.0f, 1.0f, 2.0f) * texelSize.xyxy;
	OUT.TexCoord0 = t.xyxy + offsets.xxyy;
	OUT.TexCoord1 = t.xyxy + offsets.zxwy;
	OUT.TexCoord2 = t.xyxy + offsets.xzyw;
	OUT.TexCoord3 = t.xyxy + offsets.zzww;
    return OUT;
}

half4 tex2D_bicubic3(sampler2D tex, sampler2D weightTex, v2f2 IN)
{
	half2 f = frac(IN.TexCoord0.zw*imageSize);	// fractional position within texel
	// filter in x
	half4 w = tex2D(weightTex, f.x)*2-1;
	half4 t0 = cubicFilterh(w,	tex2D(tex, IN.TexCoord0.xy),
							  	tex2D(tex, IN.TexCoord0.zy),
							  	tex2D(tex, IN.TexCoord1.xy),
							  	tex2D(tex, IN.TexCoord1.zy) );
	half4 t1 = cubicFilterh(w,	tex2D(tex, IN.TexCoord0.xw),
								tex2D(tex, IN.TexCoord0.zw),
								tex2D(tex, IN.TexCoord1.xw),
								tex2D(tex, IN.TexCoord1.zw) );
	half4 t2 = cubicFilterh(w,	tex2D(tex, IN.TexCoord2.xy),
								tex2D(tex, IN.TexCoord2.zy),
								tex2D(tex, IN.TexCoord3.xy),
								tex2D(tex, IN.TexCoord3.zy) );
	half4 t3 = cubicFilterh(w,	tex2D(tex, IN.TexCoord2.xw),
								tex2D(tex, IN.TexCoord2.zw),
								tex2D(tex, IN.TexCoord3.xw),
								tex2D(tex, IN.TexCoord3.zw) );
	// filter in y	
	w = tex2D(weightTex, f.y)*2-1;
	return cubicFilter(w, t0, t1, t2, t3);
	return w;
}


float4 bicubicPS(v2f IN) : COLOR
{
	return tex2D_bicubic(imageSampler, IN.texcoord - texelSize*0.5);
}

float4 bicubic2PS(v2f IN) : COLOR
{
	return tex2D_bicubic2(imageSampler, weightSampler, IN.texcoord - texelSize*0.5);
}

float4 bicubic3PS(v2f2 IN) : COLOR
{
	return tex2D_bicubic3(imageSampler, weightSampler, IN);
}

float4 bilinearPS(v2f IN) : COLOR
{
	return tex2D(imageSamplerBilinear, IN.texcoord);
}


technique Bicubic <
	string Script = "Pass=p0;";
> {
    pass p0 <
		string Script = "Draw=geometry;";
    > {
		VertexShader = compile vs_1_1 bicubicVS();
		PixelShader = compile ps_2_0 bicubicPS();
    }
}

technique Bicubic2 <
	string Script = "Pass=p0;";
> {
    pass p0 <
		string Script = "Draw=geometry;";
    > {
		VertexShader = compile vs_1_1 bicubicVS();
		PixelShader = compile ps_2_0 bicubic2PS();
    }
}

technique Bicubic3 <
	string Script = "Pass=p0;";
> {
    pass p0 <
		string Script = "Draw=geometry;";
    > {
		VertexShader = compile vs_1_1 bicubic2VS();
		PixelShader = compile ps_2_0 bicubic3PS();
    }
}

technique Bilinear <
	string Script = "Pass=p0;";
> {
    pass p0 <
		string Script = "Draw=geometry;";
    > {
		VertexShader = compile vs_1_1 bicubicVS();
		PixelShader = compile ps_2_0 bilinearPS();
    }
}
