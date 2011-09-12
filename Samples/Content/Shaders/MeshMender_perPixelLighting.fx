// meshmender Test App
string description = "Basic Per Pixel Lighting with a Texture";

// light direction (view space)

float4 lightDir : Direction
<
	string UIObject = "DirectionalLight";
    string Space = "World";
> = {1.0f, -1.0f, 1.0f, 0.0f};

//normal map texture
texture Tex0;



// transformations
float4x4 World      : WORLD;
float4x4 WorldInv   : WORLDINVERSE;
float4x4 View       : VIEW;
float4x4 ViewInv    : VIEWINVERSE;
float4x4 Projection : PROJECTION;

sampler NormalMapSampler = sampler_state
{
    Texture   = (Tex0);
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};


struct VS_OUTPUT
{
    float4 Pos  : POSITION;
    float2 nmapTex  : TEXCOORD0; //normal map tex coord
    float3 lightDirection  : TEXCOORD1;//light direction
    float3 shadow : COLOR0;
    float3 H: TEXCOORD2;//half angle vector
};

VS_OUTPUT VS(
    float3 Pos  : POSITION, 
    float3 Norm : NORMAL,    //normal
    float2 Tex  : TEXCOORD0, //normal map tex coord
    float3 Tangent  : TEXCOORD1,//tangent
    float3 Binormal  : TEXCOORD2)//binormal
{ 
    VS_OUTPUT Out = (VS_OUTPUT)0;
  
  
  	float4x4 WorldView = mul(World , View);
    float3 P = mul(float4(Pos, 1), (float4x3)WorldView);  
    Out.Pos  = mul(float4(P, 1), Projection);             // position (projected)
    
    float4 vertexPos = mul(float4(Pos, 1), World); // world space position
    float3 eyeVec = ViewInv[3].xyz - vertexPos.xyz; // world space eye vector
    eyeVec = mul(eyeVec, (float3x3) WorldInv);  // transform back to object space
	eyeVec = normalize(eyeVec);
	    
	
	float4 l2 = mul(lightDir, WorldInv);
	float3 L = normalize(-l2.xyz);//light (in object space)
	
	float3 H = normalize(L + eyeVec);
	
	Out.shadow = dot(Norm, L);
  
    //create our object to texture space transform
    float3x3 tangentBasis = {Tangent,    
                             Binormal,
                             Norm};
	tangentBasis = transpose(tangentBasis);

    Out.lightDirection  =  ( mul( L , tangentBasis ) ) * 0.5f + 0.5f; //light (in texture space)
    Out.nmapTex  = Tex ;
    Out.H = ( mul( H , tangentBasis ) ) * 0.5f + 0.5f;
    return Out;
}


float4 PS(
    float3 shadow : COLOR0,
    float2 normalMap  : TEXCOORD0,
    float3 lightDirObject  : TEXCOORD1,
    float3 H : TEXCOORD2) : COLOR
{
    float3 norm = (tex2D(NormalMapSampler, normalMap  ) - 0.5)*2.0;
    float diffuse = saturate( dot(norm, ( (lightDirObject - 0.5)*2.0) ));
    float specular = pow(saturate( dot(norm, ( (H - 0.5)*2.0) )), 40);
    return  diffuse * shadow.x + specular *shadow.x;
}



technique PerPixelLighting
{
    pass P0
    {
        // shaders
        VertexShader = compile vs_1_1 VS();
        PixelShader = compile ps_2_0 PS();
        
    }

}
