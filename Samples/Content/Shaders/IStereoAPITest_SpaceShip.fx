string description = "Basic No Lighting with a Texture";

//------------------------------------
float4x4 matWVP : WorldViewProjection;

texture diffuseTexture : DiffuseMap
<
	string Name = "default_color.dds";
>;

float4 lightDir : Direction
<
	string Object = "DirectionalLight";
    string Space = "World";
> = {1.0f, 1.0f, 0.0f, 0.0f};

//------------------------------------
struct vertexInput {
    float3 Position	: POSITION;
    float3 normal	: NORMAL;
    float4 UV		: TEXCOORD0;
};

struct vertexOutput {
    float4 HPosition	: POSITION;
    float4 diffAmbColor		: COLOR0;
    float4 TexCoord0	: TEXCOORD0;
    
};


//------------------------------------
vertexOutput VS_TransformAndTexture(vertexInput IN ) 
{
    vertexOutput OUT;
    OUT.HPosition = mul( float4(IN.Position.xyz , 1.0) , matWVP);
    OUT.TexCoord0 = IN.UV;
    
    OUT.diffAmbColor = dot(IN.normal, normalize(lightDir.xyz) );

    return OUT;
}


//------------------------------------

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
float4 PS_Textured( vertexOutput IN): COLOR
{
  return IN.diffAmbColor * float4(0.6, 0.3, 0.4, 1.0)*tex2D( TextureSampler, IN.TexCoord0 );
}


//-----------------------------------
technique textured
{
	/*pass p0 
    {	
    	alphablendenable=true;	
		srcblend=one;
		destblend=invsrccolor;
		cullmode=ccw;		
		VertexShader = compile vs_1_1 VS_TransformAndTexture();
		PixelShader  = compile ps_1_1 PS_Textured();
    }*/
    pass p2 
    {		
    	//alphablendenable=true;	
		//srcblend=one;
		//destblend=invsrccolor;
		//
		//cullmode=cw;
		VertexShader = compile vs_1_1 VS_TransformAndTexture();
		PixelShader  = compile ps_1_1 PS_Textured();
    }
}