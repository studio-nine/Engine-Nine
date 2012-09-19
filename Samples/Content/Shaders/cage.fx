float4x4 WvpXf : WorldViewProjection;
float4x4 WorldXf : World;
float4 BrightColor : Diffuse = {1.0f, 0.8f, 0.0f, 1.0f};
float4 EmptyColor = {0.0f, 0.0f, 0.0f, 0.0f};
float Balance = 0.1;
float Scale : UNITSSCALE = 5.1;

texture stripeTex <
    string function = "MakeStripe";
    string UIWidget = "None";
    float2 Dimensions = { 64, 64 };
>;

sampler2D stripeTexSampler = sampler_state
{
    Texture = <stripeTex>;
};

float4 MakeStripe(float2 Pos : POSITION,float ps : PSIZE) : COLOR
{
   float v = 0;
   float nx = Pos.x+ps; // keep the last column full-on, always
   v = nx > Pos.y;
   return float4(v.xxxx);
}

struct appdata {
    float3 Position    : POSITION;
    float4 UV        : TEXCOORD0;
    float4 Normal    : NORMAL;
};

struct vertexOutput {
    float4 HPosition    : POSITION;
    float4 TexCoord    : TEXCOORD0;
};

vertexOutput mainVS(appdata IN) {
    vertexOutput OUT;
    float4 Po = float4(IN.Position.x,IN.Position.y,IN.Position.z,1.0);    // object space
    float4 hpos  = mul(Po, WvpXf);        // position (projected)
    OUT.HPosition  = hpos;
    float4 Pw = mul(Po, WorldXf); // world coords
    OUT.TexCoord = Pw * Scale;
    return OUT;
}

float4 strokeTexPS(vertexOutput IN) : COLOR {
    float stripex = tex2D(stripeTexSampler,float2(IN.TexCoord.x,Balance)).x;
    float stripey = tex2D(stripeTexSampler,float2(IN.TexCoord.y,Balance)).x;
    float stripez = tex2D(stripeTexSampler,float2(IN.TexCoord.z,Balance)).x;
    float check = stripex * stripey * stripez;
    float4 dColor = lerp(BrightColor,EmptyColor,check);
    return dColor;
}

technique wires
{
    pass p0  
    {        
        VertexShader = compile vs_2_0 mainVS();
        PixelShader = compile ps_2_0 strokeTexPS();
    }
}
