float4x4 worldView;
float4x4 Projection;
float4x4 projectionInverse;
float DepthFade = 1;
texture Texture;
texture DepthBuffer;

sampler Sampler : register(s0) = sampler_state
{
    Texture = (Texture);
};

sampler DepthSampler : register(s1) = sampler_state
{
    Texture = (DepthBuffer);
};

void Vert(float4 Pos : POSITION,
          float2 UV  : TEXCOORD0,
          float4 Color : COLOR0,
          out float4 oPos : POSITION,
          out float2 oUV  : TEXCOORD0,
          out float4 oColor : COLOR0,
          out float4 oScreenTex : TEXCOORD1,
          out float oDepth : TEXCOORD2 )
{
    oUV = UV;
    oColor = Color;
    oPos = mul(Pos, worldView);
    oDepth = oPos.z;
    oPos = mul(oPos, Projection);
    oScreenTex = oPos;
}

void Pix(float2 TexCoord : TEXCOORD0,
         float4 ScreenTex : TEXCOORD1,
         float Depth : TEXCOORD2,
         float4 Color : COLOR0,
         out float4 oColor : COLOR )
{
    ScreenTex /= ScreenTex.w;

    float2 screenTex = 0.5*((ScreenTex.xy / ScreenTex.w) + float2(1,1));
    screenTex.y = 1 - screenTex.y;

    float4 color = tex2D(Sampler, TexCoord);
    float depth = tex2D(DepthSampler, screenTex).r;

    // Transform back to view space
    float4 depthSample = mul(float4(ScreenTex.xy, depth, 1), projectionInverse);
    float depthDiff = Depth - depthSample.z / depthSample.w;
    
    clip(depthDiff);
        
    float fade = saturate( depthDiff / DepthFade );

    // Use premultiplied alpha
    oColor = Color * color * fade;
}

Technique BasicEffect
{
    Pass
    {
        VertexShader = compile vs_2_0 Vert();
        PixelShader	 = compile ps_2_0 Pix();
    }
}
