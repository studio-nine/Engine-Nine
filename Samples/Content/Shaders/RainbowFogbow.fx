/*
Rainbow.fx

rainbow simulation using precomputed light scattering and interference.

*/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?Rainbow:RainbowAndCorona;";
> = 0.8;

texture tRainbowLookup : DiffuseMap
<
 string name = "rainbow_Scatter_FakeWidet.tga"; 
 //I've manually tweaked this texture to widen the color bands, 
 //not perfectly realistic, but looked better to me.
>;


texture tCoronaLookup : DiffuseMap
<
 string name = "rainbow_plot_i_vs_a_diffract_0_90_1024.tga";
>;


texture tMoisture : DiffuseMap
<
 string name = "env3_rainbow.bmp";
>;


float4x4 View : View;
float4x4 ProjInv: ProjectionInverse;

float3 LightVec : Direction
<
	string UIObject = "DirectionalLight";
    string Space = "World";
> = {1.0f, -1.0f, 1.0f};

half dropletRadius : Radius
<
    string UIType = "slider";
    float UIMin = 0.01;
    float UIMax = 0.99;
    float UIStep = 0.01;
    string UIName = "rainbow: droplet radius";
> = 0.81;


half  rainbowIntensity : Intensity
<
    string UIType = "slider";
    float UIMin = 0.0;
    float UIMax = 5.0;
    float UIStep = 0.1;
    string UIName = "rainbow: intensity";
> = 1.3;


struct VS_INPUT { 
    float3 Position : POSITION;
    float4 vTexCoord   : TEXCOORD0;
};

struct VS_OUTPUT {
    float4 vPosition  : POSITION;
    half4 vTexCoord : TEXCOORD0;// quad texture coordinates
    float3 vEyeVec : TEXCOORD1;// eye vector
    float3 vLightVec: TEXCOORD2;// light vector
};

VS_OUTPUT VS_rainbow(VS_INPUT IN)
{
    VS_OUTPUT OUT;

	OUT.vTexCoord = IN.vTexCoord;
	// our input is a full screen quad in homogeneous-clip space
    OUT.vPosition = float4(IN.Position,1.0);
    
    //we need to unproject the position, this moves it back into camera space
    half4 tempPos = float4(IN.Position,1.0);
    tempPos = mul(tempPos, ProjInv);

	//while in camera space, the eye is at 0,0,0
    //vector from vertex to eye, no need to normalize here since we
    //will be normalizing in the pixel shader
    OUT.vEyeVec =  float3(0.0, 0.0, 0.0 ) - tempPos;

	//transform light into eyespace
	float4 tempLightDir;
	tempLightDir = float4(-LightVec , 0.0);
	OUT.vLightVec = normalize(mul(tempLightDir, View ).xyz);
	

    return OUT;
}

sampler LookupMap = sampler_state
{
	Texture   = <tRainbowLookup>;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
    MipFilter = NONE;
	AddressU = CLAMP;
	AddressV = CLAMP;
};


sampler CoronaLookupMap = sampler_state
{
	Texture   = <tCoronaLookup>;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
    MipFilter = NONE;
	AddressU = CLAMP;
	AddressV = CLAMP;
};


sampler MoistureMap = sampler_state
{
	Texture   = <tMoisture>;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
    MipFilter = NONE;
    AddressU = CLAMP;
	AddressV = CLAMP;
};


void CalculateRainbowColor(VS_OUTPUT IN, out float d, out half4 scattered, out half4 moisture )
{
/*
  notes about rainbows

  -the lookuptexture should be blurred by the suns angular size 0.5 degrees.
   this should be baked into the texture

  -rainbow light blends additively to existing light in the scene.
    aka current scene color + rainbow color
    aka alpha blend, one, one
    
  -horizontal thickness of moisture, 
  	a thin sheet of rain will produce less bright rainbows than a thick sheet
  	aka rainbow color  * water ammount, where water ammount ranges from 0 to 1
  
  -rainbow light can be scattered and absorbed by other atmospheric particles.
    aka simplified..rainbow color * light color
    
*/

	d =  dot( 
				IN.vLightVec,			//this can be normalized per vertex  
	          	normalize(IN.vEyeVec ) 	//this must be normalized per pixel to prevent banding
	         );
 
	//d will be clamped between 0 and 1 by the texture sampler
	// this gives up the dot product result in the range of [0 to 1]
	// that is to say, an angle of 0 to 90 degrees
	 scattered = tex2D(LookupMap, float2( dropletRadius, d));
	 moisture = tex2D(MoistureMap,IN.vTexCoord.xy);

}

float4 PS_rainbowOnly(VS_OUTPUT IN) : COLOR
{
	//note: I can use a half for d here, since there are no corruptions
	half d;
 	half4 scattered;
 	half4 moisture; 
	CalculateRainbowColor(IN, d, scattered, moisture );	
	return scattered*rainbowIntensity*moisture.x;
	
}


half4 PS_rainbowAndCorona(VS_OUTPUT IN) : COLOR
{
/*
    Same as rainbow shader, but adds corona arround sun.
*/

	float d; //note: I use a float for d here, since a half corrupts the corona
	half4 scattered;
	half4 moisture;

	CalculateRainbowColor(IN, d, scattered, moisture );	

	//(1 + d) will be clamped between 0 and 1 by the texture sampler
	// this gives up the dot product result in the range of [-1 to 0]
	// that is to say, an angle of 90 to 180 degrees
	half4 coronaDiffracted = tex2D(CoronaLookupMap, float2(dropletRadius, 1 + d));

	return (coronaDiffracted + scattered)*rainbowIntensity*moisture.x;
}


technique Rainbow <
	string Script = "Pass=P0;";
> {

    pass P0    <
		string Script = "Draw=geometry;";
 
		string geometry = "fullscreenquad";
	>
    {
        // Shaders
        VertexShader = compile vs_1_1 VS_rainbow();
        PixelShader  = compile ps_2_0 PS_rainbowOnly();  
       
        // Render states:
        lighting  = false;
		zenable = false;
		alphablendenable = true; 
		srcblend = one; 
		destblend = invsrccolor;
    }
 
    
}

technique RainbowAndCorona <
	string Script = "Pass=P0;";
> {

    pass P0    <
		string Script = "Draw=geometry;";
    
		string geometry = "fullscreenquad";
	>
    {
        // Shaders
        VertexShader = compile vs_1_1 VS_rainbow();
        PixelShader  = compile ps_2_0 PS_rainbowAndCorona();  
       
        // Render states:
        lighting  = false;
		zenable = false;
		alphablendenable = true; 
		srcblend = one; 
		destblend = invsrccolor;
    }
 
    
}
