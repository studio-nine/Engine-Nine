// $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/post_moscowTV.fx#1 $

string description = "Post-processing Lab";

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";
	string Script = "Technique=Main;";
> = 0.8; // version #

float4 ClearColor <
	string UIWidget = "color";
	string UIName = "background";
> = {0,0,0,0};

float ClearDepth < string UIWidget = "none"; > = 1.0;

static const float ImageWidth = 1024;
static const float ImageHeight = 512;

//////////////////////////////////////////////////////////////////////////////
//
//  Load all the textures needed in this lab
//
//////////////////////////////////////////////////////////////////////////////


//  object color
texture ColorTexture
<
  string ResourceName = "gs_color_0.tga";
>;

sampler ColorSamp = sampler_state
{
  texture = <ColorTexture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  AddressW = CLAMP;
  MIPFILTER = LINEAR;
  MINFILTER = LINEAR;
  MAGFILTER = LINEAR;
};


//  per-pixel overbright (scaled luminance) term
texture OverbrightTexture
<
  string ResourceName = "gs_overbright_0.tga";
>;

sampler OverbrightSamp = sampler_state
{
  texture = <OverbrightTexture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  AddressW = CLAMP;
  MIPFILTER = LINEAR;
  MINFILTER = LINEAR;
  MAGFILTER = LINEAR;
};


//  diffuse lighting term
texture DiffuseTexture
<
  string ResourceName = "gs_diffuse_0.tga";
>;

sampler DiffuseSamp = sampler_state
{
  texture = <DiffuseTexture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  AddressW = CLAMP;
  MIPFILTER = LINEAR;
  MINFILTER = LINEAR;
  MAGFILTER = LINEAR;
};


//  ambient lighting term
texture AmbientTexture
<
  string ResourceName = "gs_ambient_0.tga";
>;

sampler AmbientSamp = sampler_state
{
  texture = <AmbientTexture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  AddressW = CLAMP;
  MIPFILTER = LINEAR;
  MINFILTER = LINEAR;
  MAGFILTER = LINEAR;
};


//  shadow / visibility term
texture ShadowTexture
<
  string ResourceName = "gs_shadow_0.tga";
>;

sampler ShadowSamp = sampler_state
{
  texture = <ShadowTexture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  AddressW = CLAMP;
  MIPFILTER = LINEAR;
  MINFILTER = LINEAR;
  MAGFILTER = LINEAR;
};

float str_32
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.01;
    string UIName = "Strength 32";
> = 0.35;

float str_128
<
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.01;
    string UIName = "Strength 128";
> = 0.12;

//////////////////////////////////////////////////////////////////////////
//
//  Render targets defined here
//
//////////////////////////////////////////////////////////////////////////

texture RenderTarget_Step2Texture : RENDERCOLORTARGET
<
	float2 Dimensions = { ImageWidth, ImageHeight };
	  int miplevels = 1;
  string format = "A8R8G8B8";
>;

sampler RenderTarget_Step2Samp = sampler_state
{
  texture = <RenderTarget_Step2Texture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  AddressW = CLAMP;
  MIPFILTER = NONE;
  MINFILTER = LINEAR;
  MAGFILTER = LINEAR;
};

texture RenderTarget_Step3Texture : RENDERCOLORTARGET
<
	float2 Dimensions = { ImageWidth / 2.0f, ImageHeight / 2.0f };
  int miplevels = 1;
  string format = "A8R8G8B8";
>;

sampler RenderTarget_Step3Samp = sampler_state
{
  texture = <RenderTarget_Step3Texture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  AddressW = CLAMP;
  MIPFILTER = NONE;
  MINFILTER = LINEAR;
  MAGFILTER = LINEAR;
};

texture RenderTarget_Step4Texture_256  : RENDERCOLORTARGET
<
  	float2 Dimensions = { ImageWidth / 4.0f, ImageHeight / 4.0f };
  	int miplevels = 1;
  string format = "A8R8G8B8";
>;

sampler RenderTarget_Step4Samp_256 = sampler_state
{
  texture = <RenderTarget_Step4Texture_256>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  AddressW = CLAMP;
  MIPFILTER = NONE;
  MINFILTER = LINEAR;
  MAGFILTER = LINEAR;
};

texture RenderTarget_Step4Texture_128 : RENDERCOLORTARGET
<
	float2 Dimensions = { ImageWidth / 8.0f, ImageHeight / 8.0f };
	  int miplevels = 1;
  string format = "A8R8G8B8";
>;

sampler RenderTarget_Step4Samp_128 = sampler_state
{
  texture = <RenderTarget_Step4Texture_128>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  AddressW = CLAMP;
  MIPFILTER = NONE;
  MINFILTER = LINEAR;
  MAGFILTER = LINEAR;
};

texture RenderTarget_Step4Texture_64  : RENDERCOLORTARGET
<
  	float2 Dimensions = { ImageWidth / 16.0f, ImageHeight / 16.0f };
  	int miplevels = 1;
  string format = "A8R8G8B8";
>;

sampler RenderTarget_Step4Samp_64 = sampler_state
{
  texture = <RenderTarget_Step4Texture_64>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  AddressW = CLAMP;
  MIPFILTER = NONE;
  MINFILTER = LINEAR;
  MAGFILTER = LINEAR;
};


texture RenderTarget_Step4Texture_32  : RENDERCOLORTARGET
<
  	float2 Dimensions = { ImageWidth / 32.0f, ImageHeight / 32.0f };
  	int miplevels = 1;
  string format = "A8R8G8B8";
>;

sampler RenderTarget_Step4Samp_32 = sampler_state
{
  texture = <RenderTarget_Step4Texture_32>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  AddressW = CLAMP;
  MIPFILTER = NONE;
  MINFILTER = LINEAR;
  MAGFILTER = LINEAR;
};

#define DEPTH_BUFFER(TextureName,W,H) texture TextureName : RENDERDEPTHSTENCILTARGET < \
	float2 Dimensions = { ImageWidth, ImageHeight }; string Format = "D24S8"; string UIWidget = "None"; >;

DEPTH_BUFFER(DepthStep2,ImageWidth, ImageHeight);
DEPTH_BUFFER(DepthStep3,(ImageWidth/2.0),(ImageHeight/2.0));
DEPTH_BUFFER(DepthStep4_256,(ImageWidth/4.0),(ImageHeight/4.0));
DEPTH_BUFFER(DepthStep4_128,(ImageWidth/8.0),(ImageHeight/8.0));
DEPTH_BUFFER(DepthStep4_64,(ImageWidth/16.0),(ImageHeight/16.0));
DEPTH_BUFFER(DepthStep4_32,(ImageWidth/32.0),(ImageHeight/32.0));

//////////////////////////////////////////////////////////////////////////
//
//  These are the vertex shader outputs used in Step 5
//
//////////////////////////////////////////////////////////////////////////

struct VS_OUTPUT {
  float4 Position  : POSITION;
  float4 TexCoord0 : TEXCOORD0;
  float4 TexCoord1 : TEXCOORD1;
  float4 TexCoord2 : TEXCOORD2;
  float4 TexCoord3 : TEXCOORD3;
  float4 TexCoord4 : TEXCOORD4;
};

//////////////////////////////////////////////////////////////////////////
//
//  These are shaders used in Step 2
//
//////////////////////////////////////////////////////////////////////////

VS_OUTPUT VS_Step1( float3 Position : POSITION,
                    float3 TexCoord : TEXCOORD0,
                    uniform float2 ImageSize )
{
  VS_OUTPUT Out = (VS_OUTPUT)0;
  Out.Position = float4( Position, 1.0 );
  
  Out.TexCoord0.xy = TexCoord.xy + 0.5/ImageSize;  // shift X, Y by 1/2 texel
  Out.TexCoord0.z  = TexCoord.z;
  Out.TexCoord0.w  = 1.0;

  return Out;
}

float4 PS_Step1( VS_OUTPUT In ) : COLOR
{
  float4 color       = tex2D( ColorSamp,      In.TexCoord0 );   // read color
  float4 diffuse     = tex2D( DiffuseSamp,    In.TexCoord0 );   // read diffuse
  float4 ambient     = tex2D( AmbientSamp,    In.TexCoord0 );   // read ambient
  float4 shadow      = tex2D( ShadowSamp,     In.TexCoord0 );   // read shadow
  float4 overbright  = tex2D( OverbrightSamp, In.TexCoord0 );   // read overbright
  
  color = color * ( diffuse*shadow + ambient );
  
  return float4( color.xyz, overbright.x );
}

/////////////////////////////////////////////////

//  copies result to framebuffer
float4 PS_Step2( VS_OUTPUT In,
                 uniform sampler SrcImage ) : COLOR
{ 
  return tex2D( SrcImage, In.TexCoord0 );
}

/////////////////////////////////////////////////

//  VS_Step3 offsets the texture coordinates in a 2x2 area, for the HDR blur
VS_OUTPUT VS_Step3( float3 Position : POSITION,
                    float3 TexCoord : TEXCOORD0,
                    uniform float2 ImageSize )
{
  VS_OUTPUT Out = (VS_OUTPUT)0;
  Out.Position = float4( Position, 1.0 );
  
  float3 texelShift = float3( 1.0 / ImageSize, 0 );
  float2 dxTexCoord = TexCoord.xy + 0.5/ImageSize;  // shift by 1/2 texel
  
  
  Out.TexCoord0    = float4( dxTexCoord, TexCoord.z, 1.0 );                   // (0,0)
  Out.TexCoord1    = float4( dxTexCoord + texelShift.xz, TexCoord.z, 1.0 );  // 1, 0
  Out.TexCoord2    = float4( dxTexCoord + texelShift.zy, TexCoord.z, 1.0 );  // 0, 1
  Out.TexCoord3    = float4( dxTexCoord + texelShift.xy, TexCoord.z, 1.0 );  // 1, 1

  return Out;
}

float4 PS_Step3( VS_OUTPUT In,
                 uniform sampler SrcImage ) : COLOR
{
  float4 c0 = tex2D( SrcImage, In.TexCoord0 );
  float4 c1 = tex2D( SrcImage, In.TexCoord1 );
  float4 c2 = tex2D( SrcImage, In.TexCoord2 );
  float4 c3 = tex2D( SrcImage, In.TexCoord3 );
  
  float3 color = 0.25*(c0.rgb*c0.a + c1.rgb*c1.a + c2.rgb*c2.a + c3.rgb*c3.a);
  float  alpha = 0.25*(c0.a + c1.a + c2.a + c3.a);
  
  return float4( color, alpha );
  
}

/////////////////////////////////////////////////

VS_OUTPUT VS_Step5( float3 Position : POSITION,
                    float3 TexCoord : TEXCOORD0,
                    uniform float2 ImageSize,
                    uniform float2 DownRes1Size,
                    uniform float2 DownRes2Size )
{
  VS_OUTPUT Out = (VS_OUTPUT)0;
  
  Out.Position = float4( Position, 1.0 );

  // TexCoord0 will contain the texture coordinate to read from the high-res
  // scene texture.  It is the same coordinate that was used in Step 1 & Step 2
  Out.TexCoord0 = float4( TexCoord.xy + 0.5/ImageSize, TexCoord.z, 1.0 );


  
  // TexCoord1 & TexCoord2 are packed texture coordinates for the 128x64 res texture
  // TexCoord1.xy = [-1, -1]
  // TexCoord1.zw = [-1,  1]
  // TexCoord2.xy = [ 1, -1]
  // TexCoord2.zw = [ 1,  1]
  float4 texelShift = float4( 1.0 / DownRes1Size, -1.0 / DownRes1Size ); 
  float2 dxTexCoord = TexCoord.xy + 0.5/DownRes1Size;  // shift by 1/2 texel
  
  Out.TexCoord1 = dxTexCoord.xyxy + texelShift.zwzy;   // [-x, -y, -x, +y]
  Out.TexCoord2 = dxTexCoord.xyxy + texelShift.xwxy;   // [+x, -y, +x, +y]
  
  // TexCoord3 & TexCoord4 are similar, but for the 32x16 texture
  texelShift = float4( 1.0 / DownRes2Size, -1.0 / DownRes2Size ); 
  dxTexCoord = TexCoord.xy + 0.5/DownRes2Size;  // shift by 1/2 texel
  
  Out.TexCoord3 = dxTexCoord.xyxy + texelShift.zwzy;   // [-x, -y, -x, +y]
  Out.TexCoord4 = dxTexCoord.xyxy + texelShift.xwxy;   // [+x, -y, +x, +y] 
  return Out;
}

float4 PS_Step5( VS_OUTPUT In ) : COLOR
{
  // read base texture:
  float4 baseColor = tex2D( RenderTarget_Step2Samp, In.TexCoord0 );

  // read 4 samples from 128x64 texture
  float4 rt_128_0 = tex2D( RenderTarget_Step4Samp_128, In.TexCoord1.xy );
  float4 rt_128_1 = tex2D( RenderTarget_Step4Samp_128, In.TexCoord1.zw );
  float4 rt_128_2 = tex2D( RenderTarget_Step4Samp_128, In.TexCoord2.xy );
  float4 rt_128_3 = tex2D( RenderTarget_Step4Samp_128, In.TexCoord2.zw );
  
  // filter the 4 texels
  float4 hdr_128  = 0.25 * ( rt_128_0 + rt_128_1 + rt_128_2 + rt_128_3 );
  
  // read 4 samples from 32x16 texture
  float4 rt_32_0 = tex2D( RenderTarget_Step4Samp_32, In.TexCoord3.xy );
  float4 rt_32_1 = tex2D( RenderTarget_Step4Samp_32, In.TexCoord3.zw );
  float4 rt_32_2 = tex2D( RenderTarget_Step4Samp_32, In.TexCoord4.xy );
  float4 rt_32_3 = tex2D( RenderTarget_Step4Samp_32, In.TexCoord4.zw );

  // filter the 4 texels
  float4 hdr_32  = 0.25 * ( rt_32_0 + rt_32_1 + rt_32_2 + rt_32_3 );
  
  // add hdr contributions to scene
  
  return baseColor + (str_128*hdr_128) + (str_32*hdr_32);

}

//////////////////////////////////////////////////////////////////////////
//
//  TODO: Fill in extra passes and techniques as needed
//
//////////////////////////////////////////////////////////////////////////

technique Main <
	string ScriptClass = "scene";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";
	string Script =
			"RenderColorTarget0=;"
			"RenderDepthStencilTarget=;"
				"ClearSetColor=ClearColor;"
				"ClearSetColor=ClearDepth;"
				"Clear=Color;"
				"Clear=Depth;"
        	"Pass=buildLighting;"
        	"Pass=downFilter;"
        	"Pass=downFilter_step4_0;"
        	"Pass=downFilter_step4_1;"
        	"Pass=downFilter_step4_2;"
        	"Pass=downFilter_step4_3;"
        	"Pass=addHDR_to_screen;";
> {
	pass buildLighting <
    	string Script = "RenderColorTarget0=RenderTarget_Step2Texture;"
						"RenderDepthStencilTarget=DepthStep2;"
								"ClearSetColor=ClearColor;"
								"ClearSetColor=ClearDepth;"
								"Clear=Color;"
								"Clear=Depth;"
    							"Draw=Buffer;";
    > {
		cullmode = none;
		zenable = false;
		AlphaBlendEnable = false;
		VertexShader = compile vs_2_0 VS_Step1( float2(ImageWidth, ImageHeight) );
		PixelShader  = compile ps_2_0 PS_Step1( );
	}
	pass downfilter <
    	string Script ="RenderColorTarget0=RenderTarget_Step3Texture;"
						"RenderDepthStencilTarget=DepthStep3;"
    							"Draw=Buffer;";
    > {
		cullmode = none;
		zenable = false;
		AlphaBlendEnable = false;
		VertexShader = compile vs_2_0 VS_Step3( float2(ImageWidth, ImageHeight) );
		PixelShader  = compile ps_2_0 PS_Step3( RenderTarget_Step2Samp );
	}
	//  step 4 is just recursively applying step 3, so no extra shaders are required.
	//  just apply the output from each pass as the input to the next pass
	//  and change the offset, to match the new texture resolution
 
  //  generates 256x128 image
	pass downfilter_step4_0 <
    	string Script ="RenderColorTarget0=RenderTarget_Step4Texture_256;"
						"RenderDepthStencilTarget=DepthStep4_256;"
    							"Draw=Buffer;";
    > {
		cullmode = none;
		zenable = false;
		AlphaBlendEnable = false;
		VertexShader = compile vs_2_0 VS_Step3( float2(ImageWidth/2, ImageHeight/2) );
		PixelShader  = compile ps_2_0 PS_Step3( RenderTarget_Step3Samp );
	}
	// generates 128x64 image
	pass downfilter_step4_1 <
    	string Script ="RenderColorTarget0=RenderTarget_Step4Texture_128;"
						"RenderDepthStencilTarget=DepthStep4_128;"
    							"Draw=Buffer;";
    > {
		cullmode = none;
		zenable = false;
		AlphaBlendEnable = false;
		VertexShader = compile vs_2_0 VS_Step3( float2(ImageWidth/4, ImageHeight/4) );
		PixelShader  = compile ps_2_0 PS_Step3( RenderTarget_Step4Samp_256 );
	} 
    // generates 64x32 image
	pass downfilter_step4_2 <
    	string Script ="RenderColorTarget0=RenderTarget_Step4Texture_64;"
						"RenderDepthStencilTarget=DepthStep4_64;"
    							"Draw=Buffer;";
    > {
		cullmode = none;
		AlphaBlendEnable = false;
		zenable = false;
		VertexShader = compile vs_2_0 VS_Step3( float2(ImageWidth/8, ImageHeight/8) );
		PixelShader  = compile ps_2_0 PS_Step3( RenderTarget_Step4Samp_128 );
	} 
	// generates 32x16 image
	pass downfilter_step4_3 <
    	string Script ="RenderColorTarget0=RenderTarget_Step4Texture_32;"
						"RenderDepthStencilTarget=DepthStep4_32;"
    							"Draw=Buffer;";
    > {
		cullmode = none;
		AlphaBlendEnable = false;
		zenable = false;
		VertexShader = compile vs_2_0 VS_Step3( float2(ImageWidth/16, ImageHeight/16) );
		PixelShader  = compile ps_2_0 PS_Step3( RenderTarget_Step4Samp_64 );
	} 
	pass addHDR_to_screen <
    	string Script = "RenderColorTarget0=;"
						"RenderDepthStencilTarget=;"
						"Draw=Buffer;";
    > {
		cullmode = none;
		AlphaBlendEnable = false;
		zenable = false;
		VertexShader = compile vs_2_0 VS_Step5( float2(ImageWidth, ImageHeight), float2(ImageWidth/8, ImageHeight/8), float2(ImageWidth/32, ImageHeight/32) );
		PixelShader  = compile ps_2_0 PS_Step5( );
	}
}

/////////////////// eof //////
