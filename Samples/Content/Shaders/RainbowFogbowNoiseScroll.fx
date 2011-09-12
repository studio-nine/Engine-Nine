string description = "scrolls a noise texture down and fades alpha out on top. used for skybox with scrolling noise on sides";

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=ScrollingNoise;";
> = 0.8;

//------------------------------------
float4x4 worldViewProj : WorldViewProjection;
float4x4 world : World;

texture diffuseTexture : DiffuseMap
<
	string Name = "default_color.dds";
>;

float time: TIME ;
float speed
<
> = {10.0f};

float3 rainVec : Direction
<
> = {1.0f, -1.0f, 0.0f};


//------------------------------------
struct vertexInput {
    float3 Position	: POSITION;
    float4 UV		: TEXCOORD0;
};

struct vertexOutput {
    float4 HPosition	: POSITION;
    float4 TexCoord0	: TEXCOORD0;
    float4 color		: COLOR0;
};




//------------------------------------
vertexOutput VS_TransformAndTextureScrollNoise(vertexInput IN) 
{
    vertexOutput OUT;
    OUT.HPosition = mul( float4(IN.Position.xyz , 1.0) , worldViewProj);
    OUT.TexCoord0 = IN.UV ;
    OUT.TexCoord0.y *= 0.21;
    OUT.TexCoord0.xy +=  (rainVec* frac(time/speed));
    
    float4 worldPos = mul( float4(IN.Position.xyz , 1.0) , world);
    //I know the mesh ranges from -10 to 10
    float alpha = 1.0 - (worldPos.y  + 10)/20;
    OUT.color = alpha.xxxx;
    
    return OUT;
}

//------------------------------------
sampler TextureSampler = sampler_state 
{
    texture = <diffuseTexture>;
    AddressU  = WRAP;        
    AddressV  = WRAP;
    AddressW  = CLAMP;
    MIPFILTER = LINEAR;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};


//-----------------------------------
float4 PS_Texture( vertexOutput IN): COLOR
{
  return 1.0 - tex2D( TextureSampler, IN.TexCoord0 ) * IN.color.r *2;
}


//-----------------------------------
technique ScrollingNoise <
	string Script = "Pass=p0;";
> {
    pass p0  <
		string Script = "Draw=geometry;";
    > {		
		VertexShader = compile vs_1_1 VS_TransformAndTextureScrollNoise();
		PixelShader  = compile ps_1_1 PS_Texture();
    }
}
