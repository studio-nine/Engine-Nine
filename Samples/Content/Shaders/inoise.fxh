/*
    FX implementation of Ken Perlin's "Improved Noise"
    sgg 6/26/04
    http://mrl.nyu.edu/~perlin/noise/
*/

// permutation table
static int permutation[] = { 151,160,137,91,90,15,
131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
};

// gradients for 3d noise
static float3 g[] = {
    1,1,0,
    -1,1,0,
    1,-1,0,
    -1,-1,0,
    1,0,1,
    -1,0,1,
    1,0,-1,
    -1,0,-1, 
    0,1,1,
    0,-1,1,
    0,1,-1,
    0,-1,-1,
    1,1,0,
    0,-1,1,
    -1,1,0,
    0,-1,-1,
};

// gradients for 4D noise
static float4 g4[] = {
	0, -1, -1, -1,
	0, -1, -1, 1,
	0, -1, 1, -1,
	0, -1, 1, 1,
	0, 1, -1, -1,
	0, 1, -1, 1,
	0, 1, 1, -1,
	0, 1, 1, 1,
	-1, -1, 0, -1,
	-1, 1, 0, -1,
	1, -1, 0, -1,
	1, 1, 0, -1,
	-1, -1, 0, 1,
	-1, 1, 0, 1,
	1, -1, 0, 1,
	1, 1, 0, 1,
	-1, 0, -1, -1,
	1, 0, -1, -1,
	-1, 0, -1, 1,
	1, 0, -1, 1,
	-1, 0, 1, -1,
	1, 0, 1, -1,
	-1, 0, 1, 1,
	1, 0, 1, 1,
	0, -1, -1, -1,
	0, -1, -1, 1,
	0, -1, 1, -1,
	0, -1, 1, 1,
	0, 1, -1, -1,
	0, 1, -1, 1,
	0, 1, 1, -1,
	0, 1, 1, 1,
};

// Generate permutation and gradient textures using CPU runtime
texture permTexture
<
    string texturetype = "2D";
    string format = "l8";
	string function = "GeneratePermTexture";
	int width = 256, height = 1;
>;

texture gradTexture
<
    string texturetype = "2D";
	string format = "q8w8v8u8";
	string function = "GenerateGradTexture";
	int width = 16, height = 1;
>;

texture gradTexture4d
<
    string texturetype = "2D";
	string format = "q8w8v8u8";
	string function = "GenerateGradTexture4d";
	int width = 32, height = 1;
>;

float4 GeneratePermTexture(float p : POSITION) : COLOR
{
	return permutation[p*255] / 255.0;
}

float3 GenerateGradTexture(float p : POSITION) : COLOR
{
	return g[p*16];
}

float4 GenerateGradTexture4d(float p : POSITION) : COLOR
{
	return g4[p*32];
}

sampler permSampler = sampler_state 
{
    texture = <permTexture>;
    AddressU  = Wrap;        
    AddressV  = Clamp;
    MAGFILTER = POINT;
    MINFILTER = POINT;
    MIPFILTER = NONE;   
};

sampler gradSampler = sampler_state 
{
    texture = <gradTexture>;
    AddressU  = Wrap;        
    AddressV  = Clamp;
    MAGFILTER = POINT;
    MINFILTER = POINT;
    MIPFILTER = NONE;
};

sampler gradSampler4d = sampler_state 
{
    texture = <gradTexture4d>;
    AddressU  = Wrap;        
    AddressV  = Clamp;
    MAGFILTER = POINT;
    MINFILTER = POINT;
    MIPFILTER = NONE;
};

float3 fade(float3 t)
{
	return t * t * t * (t * (t * 6 - 15) + 10); // new curve
//	return t * t * (3 - 2 * t); // old curve
}

float4 fade(float4 t)
{
	return t * t * t * (t * (t * 6 - 15) + 10); // new curve
//	return t * t * (3 - 2 * t); // old curve
}

float perm(float x)
{
	return tex1D(permSampler, x / 256.0) * 256;	// need to optimize this
}

float grad(float x, float3 p)
{
	return dot(tex1D(gradSampler, x), p);
}

float grad(float x, float4 p)
{
	return dot(tex1D(gradSampler4d, x), p);
}

// 3D version
float inoise(float3 p)
{
	float3 P = fmod(floor(p), 256.0);	// FIND UNIT CUBE THAT CONTAINS POINT
  	p -= floor(p);                      // FIND RELATIVE X,Y,Z OF POINT IN CUBE.
	float3 f = fade(p);                 // COMPUTE FADE CURVES FOR EACH OF X,Y,Z.
	
    // HASH COORDINATES OF THE 8 CUBE CORNERS
  	float A = perm(P.x) + P.y;
  	float AA = perm(A) + P.z;
  	float AB = perm(A + 1) + P.z;
  	float B =  perm(P.x + 1) + P.y;
  	float BA = perm(B) + P.z;
  	float BB = perm(B + 1) + P.z;

	// AND ADD BLENDED RESULTS FROM 8 CORNERS OF CUBE
  	return lerp( lerp( lerp( grad(perm(AA    ), p ),  
                             grad(perm(BA    ), p + float3(-1, 0, 0) ), f.x),
                       lerp( grad(perm(AB    ), p + float3(0, -1, 0) ),
                             grad(perm(BB    ), p + float3(-1, -1, 0) ), f.x), f.y),
                             
                 lerp( lerp( grad(perm(AA+1), p + float3(0, 0, -1) ),
                             grad(perm(BA+1), p + float3(-1, 0, -1) ), f.x),
                       lerp( grad(perm(AB+1), p + float3(0, -1, -1) ),
                             grad(perm(BB+1), p + float3(-1, -1, -1) ), f.x), f.y), f.z);
}

// 4D version
float inoise(float4 p)
{
	float4 P = fmod(floor(p), 256.0);	// FIND UNIT HYPERCUBE THAT CONTAINS POINT
  	p -= floor(p);                      // FIND RELATIVE X,Y,Z OF POINT IN CUBE.
	float4 f = fade(p);                 // COMPUTE FADE CURVES FOR EACH OF X,Y,Z, W
	
    // HASH COORDINATES OF THE 16 CORNERS OF THE HYPERCUBE
  	float A = perm(P.x) + P.y;
  	float AA = perm(A) + P.z;
  	float AB = perm(A + 1) + P.z;
  	float B =  perm(P.x + 1) + P.y;
  	float BA = perm(B) + P.z;
  	float BB = perm(B + 1) + P.z;

	float AAA = perm(AA)+P.w, AAB = perm(AA+1)+P.w;
    float ABA = perm(AB)+P.w, ABB = perm(AB+1)+P.w;
    float BAA = perm(BA)+P.w, BAB = perm(BA+1)+P.w;
    float BBA = perm(BB)+P.w, BBB = perm(BB+1)+P.w;

	// INTERPOLATE DOWN
  	return lerp(
  				lerp( lerp( lerp( grad(perm(AAA), p ),  
                                  grad(perm(BAA), p + float4(-1, 0, 0, 0) ), f.x),
                            lerp( grad(perm(ABA), p + float4(0, -1, 0, 0) ),
                                  grad(perm(BBA), p + float4(-1, -1, 0, 0) ), f.x), f.y),
                                  
                      lerp( lerp( grad(perm(AAB), p + float4(0, 0, -1, 0) ),
                                  grad(perm(BAB), p + float4(-1, 0, -1, 0) ), f.x),
                            lerp( grad(perm(ABB), p + float4(0, -1, -1, 0) ),
                                  grad(perm(BBB), p + float4(-1, -1, -1, 0) ), f.x), f.y), f.z),
                            
  				 lerp( lerp( lerp( grad(perm(AAA+1), p + float4(0, 0, 0, -1)),
                                   grad(perm(BAA+1), p + float4(-1, 0, 0, -1) ), f.x),
                             lerp( grad(perm(ABA+1), p + float4(0, -1, 0, -1) ),
                                   grad(perm(BBA+1), p + float4(-1, -1, 0, -1) ), f.x), f.y),
                                   
                       lerp( lerp( grad(perm(AAB+1), p + float4(0, 0, -1, -1) ),
                                   grad(perm(BAB+1), p + float4(-1, 0, -1, -1) ), f.x),
                             lerp( grad(perm(ABB+1), p + float4(0, -1, -1, -1) ),
                                   grad(perm(BBB+1), p + float4(-1, -1, -1, -1) ), f.x), f.y), f.z), f.w);
}

// utility functions

// fractal sum
float fBm(float3 p, int octaves, float lacunarity = 2.0, float gain = 0.5)
{
	float freq = 1.0, amp = 0.5;
	float sum = 0;	
	for(int i=0; i<octaves; i++) {
		sum += inoise(p*freq)*amp;
		freq *= lacunarity;
		amp *= gain;
	}
	return sum;
}

float turbulence(float3 p, int octaves, float lacunarity = 2.0, float gain = 0.5)
{
	float sum = 0;
	float freq = 1.0, amp = 1.0;
	for(int i=0; i<octaves; i++) {
		sum += abs(inoise(p*freq))*amp;
		freq *= lacunarity;
		amp *= gain;
	}
	return sum;
}

// Ridged multifractal
// See "Texturing & Modeling, A Procedural Approach", Chapter 12
float ridge(float h, float offset)
{
    h = abs(h);
    h = offset - h;
    h = h * h;
    return h;
}

float ridgedmf(float3 p, int octaves, float lacunarity = 2.0, float gain = 0.5, float offset = 1.0)
{
	float sum = 0;
	float freq = 1.0, amp = 0.5;
	float prev = 1.0;
	for(int i=0; i<octaves; i++) {
		float n = ridge(inoise(p*freq), offset);
		sum += n*amp*prev;
		prev = n;
		freq *= lacunarity;
		amp *= gain;
	}
	return sum;
}

