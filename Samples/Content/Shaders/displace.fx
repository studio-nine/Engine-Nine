/*********************************************************************NVMH3****
File:  $Id: //devrel/Playpen/kbjorke/HLSL/displace.fx#1 $

Copyright NVIDIA Corporation 2004
TO THE MAXIMUM EXTENT PERMITTED BY APPLICABLE LAW, THIS SOFTWARE IS PROVIDED
*AS IS* AND NVIDIA AND ITS SUPPLIERS DISCLAIM ALL WARRANTIES, EITHER EXPRESS
OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, IMPLIED WARRANTIES OF MERCHANTABILITY
AND FITNESS FOR A PARTICULAR PURPOSE.  IN NO EVENT SHALL NVIDIA OR ITS SUPPLIERS
BE LIABLE FOR ANY SPECIAL, INCIDENTAL, INDIRECT, OR CONSEQUENTIAL DAMAGES
WHATSOEVER (INCLUDING, WITHOUT LIMITATION, DAMAGES FOR LOSS OF BUSINESS PROFITS,
BUSINESS INTERRUPTION, LOSS OF BUSINESS INFORMATION, OR ANY OTHER PECUNIARY LOSS)
ARISING OUT OF THE USE OF OR INABILITY TO USE THIS SOFTWARE, EVEN IF NVIDIA HAS
BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.

	Simple displacement mapping

******************************************************************************/

//#define TIMER

float4x4 WorldITXf : WorldInverseTranspose <string UIWidget="None";>;
float4x4 WvpXf : WorldViewProjection <string UIWidget="None";>;
float4x4 WorldXf : World <string UIWidget="None";>;
float4x4 ViewIXf : ViewInverse <string UIWidget="None";>;
#ifdef TIMER
float Timer : TIME <string UIWidget="None";>;
#endif

///////////////////////////////////////////////////////////
/////////////////////////////////////// Tweakables ////////
///////////////////////////////////////////////////////////

float3 LightDirD : Direction
<
    string UIName = "Light Direction"; 
    string Object = "DirectionalLight";
    string Space = "World";
> = {-10.0f, 15.0f, 30.0f};

#ifdef TIMER
float SpeedS <
    string UIWidget = "slider";
    float UIMin = 0.0f; float UIMax = 1.0f; float UIStep = 0.01f;
> = 0.4f;
#endif

float Wrap <
	string UIName = "Wrap lighting";
	string UIWidget = "slider";
	float UIMin = 0.0; float UIMax = 1.0; float UIStep = 0.01;
> = 0.5f;

float Ks <
    string UIName = "Specular";
    string UIWidget = "slider";
    float UIMin = 0.0; float UIMax = 1.5; float UIStep = 0.01;
> = 1.0;

float SpecExpon : SpecularPower <
    string UIName = "Specular power";
    string UIWidget = "slider";
    float UIMin = 1.0; float UIMax = 128.0; float UIStep = 1.0;
> = 12.0;

float Bumpy <
	string UIName = "Bump map scale";
	string UIWidget = "slider";
	float UIMin = 0.0; float UIMax = 10.0; float UIStep = 0.01;
> = 0.5f;

float DHeight <
	string UIName = "Displacement";
	string UIWidget = "slider";
	float UIMin = -2.0; float UIMax = 2.0; float UIStep = 0.01;
> = 0.5f;

float MinThreshold <
    string UIName = "Displacement min angle";
    string UIWidget = "slider";
    float UIMin = -1.0; float UIMax = 1.0; float UIStep = 0.01;
> = -1.0;

float MaxThreshold <
    string UIName = "Displacement max angle";
    string UIWidget = "slider";
    float UIMin = -1.0; float UIMax = 1.0; float UIStep = 0.01;
> = 1.0;

float Lod <
    string UIName = "Mipmap level of detail";
    string UIWidget = "slider";
    float UIMin = 0.0; float UIMax = 12.0; float UIStep = 1.0;
> = 1.0;

//////////////////////

texture DiffTex : Diffuse <
	string ResourceName = "default_color.dds";
	string TextureType = "2D";
>;

sampler2D DiffSamp = sampler_state
{
	Texture = <DiffTex>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};


texture BumpTex <
	string ResourceName = "default_bump_normal.dds";
	string TextureType = "2D";
>;

sampler BumpSamp = sampler_state
{
	Texture = <BumpTex>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
	AddressU = Wrap;
	AddressV = Wrap;
};

texture DispTex <
	string ResourceName = "default_bump_R32F.dds";
	string TextureType = "2D";
	//string Format = "r32f";
	//string Format = "a32b32g32r32f";
>;

sampler DispSamp = sampler_state
{
	Texture = <DispTex>;
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Point;
	AddressU = Wrap;
	AddressV = Wrap;
};

///////////////////////

/* data from application vertex buffer */
struct appdata {
    float4 Position	: POSITION;
    float4 UV		: TEXCOORD0;
    float3 Normal	: NORMAL;
    float3 Tangent	: TANGENT0;
    float3 Binormal	: BINORMAL0;
};

// used for all other passes
struct vertexOutput {
    float4 HPosition	: POSITION;
    float2 UV		: TEXCOORD0;
    float3 N		: TEXCOORD1;
    float3 T		: TEXCOORD2;
    float3 B		: TEXCOORD3;
    float3 V		: TEXCOORD4;
	float4 DiffCol  : COLOR0;
};

//////////////////////////////////////////////////////
////////////////////////////////// pixel shaders /////
//////////////////////////////////////////////////////

// vertex texture lookup with bilinear filtering
float4 tex2Dlod_bilinear(uniform sampler2D tex, float4 s, float2 texSize)
{
	float2 f = frac(s.xy*texSize);
	float4 s2 = s.xyxy + float4(0, 0, 1, 1)/texSize.xyxy;
	float4 t0 = tex2Dlod(tex, float4(s2.xy, s.zw));
	float4 t1 = tex2Dlod(tex, float4(s2.zy, s.zw));
	float4 t2  = lerp(t0, t1, f[0]);
	t0 = tex2Dlod(tex, float4(s2.xw, s.zw));
	t1 = tex2Dlod(tex, float4(s2.zw, s.zw));
	t0 = lerp(t0, t1, f[0]);
	t0 = lerp(t2, t0, f[1]);
	return t0;
}

vertexOutput displaceVS(appdata IN,uniform float3 LightDir)
{
    vertexOutput OUT;
	float2 nuv = IN.UV.xy;
#ifdef TIMER
	float a = Timer * SpeedS;
	float c = cos(a); float s = sin(a);
	nuv = nuv - float2(0.5,0.5);
	nuv = float2(nuv.x*c-nuv.y*s,
				 nuv.y*c+nuv.x*s);
	nuv = nuv + float2(0.5,0.5);
#endif
    OUT.UV = nuv;

    float3 No = normalize(IN.Normal);
    float4 Po = IN.Position;		// obj coords    
    float4 Pw = mul(Po, WorldXf);	// world coordinates    
    float3 Vo = normalize(ViewIXf[3].xyz - Pw.xyz);	// obj coords
	OUT.V = Vo;

    float tv = 0;
    float NdotV = dot(No, Vo);
  	// try and skip vertex texture fetch for non-silhouette vertices
    if (NdotV > MinThreshold && NdotV < MaxThreshold) {
		tv = tex2Dlod(DispSamp, float4(nuv, 1, Lod));
//		tv = tex2Dlod_bilinear(DispSamp, float4(nuv, 1, Lod), pow(2, 8-Lod) );
	}

    float3 Nn = normalize(mul(IN.Normal, (float4x3) WorldITXf));
	OUT.N = Nn;
    OUT.T = normalize(mul(IN.Tangent, (float4x3) WorldITXf));
    OUT.B = normalize(mul(IN.Binormal, (float4x3) WorldITXf));
  
	// displace vertex along normal
	Po.xyz += No * tv * DHeight;

    OUT.HPosition = mul(Po, WvpXf);
	OUT.DiffCol = float4(tv.xxxx*0.5 + 0.5);
    return OUT;
}

float4 nPS(vertexOutput IN) : COLOR
{
	float3 Nn = normalize(IN.N);
	float3 Tn = normalize(IN.T);
	float3 Bn = normalize(IN.B);
	float2 bumps = Bumpy * (tex2D(BumpSamp, IN.UV).xy - float2(0.5,0.5));
	Nn += (Tn * bumps.x + Bn * bumps.y);
	Nn = normalize(Nn);
	float3 Ln = normalize(-LightDirD);
	float ldn = dot(Ln, Nn);
	float grad = (ldn + Wrap) / (1.0 + Wrap);
    float3 Vn = normalize(IN.V);
    float3 Hn = normalize(Vn + Ln);
    float hdn = dot(Hn,Nn);
    float4 litVec = lit(grad, hdn, SpecExpon);
	float4 tc = tex2D(DiffSamp, IN.UV);
	float4 diff = litVec.y*tc;
	float spec = Ks * litVec.z;
	return float4(diff + float4(spec.xxx,0.0));
//	return float4(diff + float4(spec.xxx,0.0))*IN.DiffCol;
}

////////////////////////////////////////////////////////////
/////////////////////////////////////// techniques /////////
////////////////////////////////////////////////////////////

technique Displace {
    pass p0 {
		VertexShader = compile vs_3_0 displaceVS(-LightDirD);
		cullmode = none;
		ZEnable = true;
		PixelShader  = compile ps_3_0 nPS();
    }
}
