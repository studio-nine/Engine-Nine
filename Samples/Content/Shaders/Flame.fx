/*
	Volumetric flame effect
	based on Yury Uralsky's "Volumetric Fire"
	http://www.cgshaders.org/shaders/show.php?id=39
	
	This revolves a cross section of a flame image around the Y axis to
	produce a cylindrical volume, and then perturbs the texture coordinates
	with 4 octaves of animated 3D procedural noise to produce the flame effect.
*/

string XFile = "slices_10y.x";
string description = "3D Flame";

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=ps20;";
> = 0.8;

float ticks : Time
<
 string units = "sec";
>;

/************* TWEAKABLES **************/

float noiseFreq
<
    string UIWidget = "slider";
    float UIMin = 0.0; float UIMax = 1.0; float UIStep = 0.01;
> = 0.1;

float noiseStrength
<
    string UIWidget = "slider";
    float UIMin = 0.0; float UIMax = 5.0; float UIStep = 0.01;
> = 1.0;

float timeScale
<
    string UIWidget = "slider";
    float UIMin = 0.0; float UIMax = 1.0; float UIStep = 0.01;
> = 1.0;

float3 noiseScale = { 1.0, 1.0, 1.0 };
float3 noiseAnim = { 0.0, -0.1, 0.0 };

float4 flameColor = { 0.2, 0.2, 0.2, 1.0 };
float3 flameScale = { 1.0, -1.0, 1.0 };
float3 flameTrans = { 0.0, 0.0, 0.0 };

// Textures

#define VOLUME_SIZE 32

texture noiseTexture
<
//    string Name = "noiseL8_32x32x32.dds";
    string ResourceType = "3D";
	string function = "GenerateNoise1f";
	float3 Dimensions = { VOLUME_SIZE, VOLUME_SIZE, VOLUME_SIZE};
>;

texture flameTexture
<
    string ResourceName = "flame.png";
    string ResourceType = "2D";
>;

// Vector-valued noise
float4 GenerateNoise4f(float3 Pos : POSITION) : COLOR
{
	float4 c;
	float3 P = Pos*VOLUME_SIZE;
	c.r = noise(P);
	c.g = noise(P + float3(11, 17, 23));
	c.b = noise(P + float3(57, 93, 65));
	c.a = noise(P + float3(77, 15, 111));
//	return c*0.5+0.5;
	return abs(c);
}

// Scalar noise
float GenerateNoise1f(float3 Pos : POSITION) : COLOR
{
	float3 P = Pos*VOLUME_SIZE;
//	return noise(P)*0.5+0.5;
	return abs(noise(P));
}

// Tracked matricies
float4x4 wvp : WorldViewProjection;
float4x4 world : World;

// Structures
struct appdata {
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;
    float4 Tangent	: TANGENT0;
    float4 Binormal	: BINORMAL0;
    float4 Normal	: NORMAL;
};

struct vertexOutput {
    float4 HPosition	: POSITION;
    float3 NoisePos     : TEXCOORD0;
    float3 FlamePos     : TEXCOORD1;
    float2 UV           : TEXCOORD2;
};

// Vertex shader
vertexOutput flameVS(appdata IN,
					uniform float4x4 WorldViewProj,
					uniform float4x4 World,
					uniform float3 noiseScale,					
					uniform float noiseFreq,
					uniform float3 noiseAnim,
					uniform float3 flameScale,
					uniform float3 flameTrans,
					uniform float timeScale
					)
{
    vertexOutput OUT;
    float4 objPos = float4(IN.Position.x,IN.Position.y,IN.Position.z,1.0);
    float3 worldPos = mul(objPos, World).xyz;
 
    OUT.HPosition = mul(objPos, WorldViewProj);
    float time = fmod(ticks, 10.0);	// avoid large texcoords
    OUT.NoisePos = worldPos*noiseScale*noiseFreq + time*timeScale*noiseAnim;
	OUT.FlamePos = worldPos*flameScale + flameTrans;
	
	OUT.UV = IN.UV;
    return OUT;
}

// Pixel shaders
half4 noise3D(uniform sampler3D NoiseMap, float3 P)
{
//	return tex3D(NoiseMap, P)*2-1;
	return tex3D(NoiseMap, P);
}

half4 turbulence4(uniform sampler3D NoiseMap, float3 P)
{
	half4 sum = noise3D(NoiseMap, P)*0.5 +
 				noise3D(NoiseMap, P*2)*0.25 +
 				noise3D(NoiseMap, P*4)*0.125 +
				noise3D(NoiseMap, P*8)*0.0625;
	return sum;
}

half4 flamePS(vertexOutput IN,
			  uniform sampler3D NoiseMap,
			  uniform sampler2D FlameTex,
			  uniform half noiseStrength,
			  uniform half4 flameColor
			  ) : COLOR
{
//	return tex3D(NoiseMap,IN.NoisePos) * flameColor;
//  return turbulence4(NoiseMap, IN.NoisePos) * flameColor;

	half2 uv;
	uv.x = length(IN.FlamePos.xz);	// radial distance in XZ plane
	uv.y = IN.FlamePos.y;
	
//	uv.y += turbulence4(NoiseMap, IN.NoisePos) * noiseStrength;
	uv.y += turbulence4(NoiseMap, IN.NoisePos) * noiseStrength / uv.x;

	return tex2D(FlameTex, uv) * flameColor;
}

/****************************************************/
/********** SAMPLERS ********************************/
/****************************************************/

sampler3D noiseTextureSampler = sampler_state
{
	Texture = <noiseTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

sampler2D flameTextureSampler = sampler_state
{
	Texture = <flameTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Clamp;
	AddressV = Clamp;	
};


/****************************************************/
/********** TECHNIQUES ******************************/
/****************************************************/

technique ps20 <
	string Script = "Pass=p1d;";
> {
    pass p1d <
		string Script = "Draw=geometry;";
    > {		
	VertexShader = compile vs_1_1 flameVS(wvp, world,
										  noiseScale, noiseFreq, noiseAnim,
										  flameScale, flameTrans, timeScale
										  );

	ZEnable = true;
	ZWriteEnable = true;
	CullMode = None;

	AlphaBlendEnable = true;
	BlendOp = Add;		
	SrcBlend = One;
	DestBlend = One;
	
	PixelShader = compile ps_2_0 flamePS(noiseTextureSampler, flameTextureSampler, noiseStrength, flameColor);
    }
}

/***************************** eof ***/
