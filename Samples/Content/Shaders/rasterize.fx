/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/rasterize.fx#1 $

Copyright NVIDIA Corporation 2002
TO THE MAXIMUM EXTENT PERMITTED BY APPLICABLE LAW, THIS SOFTWARE IS PROVIDED
*AS IS* AND NVIDIA AND ITS SUPPLIERS DISCLAIM ALL WARRANTIES, EITHER EXPRESS
OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, IMPLIED WARRANTIES OF MERCHANTABILITY
AND FITNESS FOR A PARTICULAR PURPOSE.  IN NO EVENT SHALL NVIDIA OR ITS SUPPLIERS
BE LIABLE FOR ANY SPECIAL, INCIDENTAL, INDIRECT, OR CONSEQUENTIAL DAMAGES
WHATSOEVER (INCLUDING, WITHOUT LIMITATION, DAMAGES FOR LOSS OF BUSINESS PROFITS,
BUSINESS INTERRUPTION, LOSS OF BUSINESS INFORMATION, OR ANY OTHER PECUNIARY LOSS)
ARISING OUT OF THE USE OF OR INABILITY TO USE THIS SOFTWARE, EVEN IF NVIDIA HAS
BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.

Comments:
	triangle rasterization in the pixel shader, PixelPlanes-style
	sgreen 1/2004
******************************************************************************/

float Script : STANDARDSGLOBAL <
	string UIWidget = "none";
	string ScriptClass = "scene";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";
	string Script = "Technique=Type?RasterizePlane:RasterizeBarycentric:RasterizeNoInterpolation;";
> = 0.8; // version #

// triangle vertices
float3 tri[3] = {
	{ 0.1, 0.1, 1.0 },
	{ 0.8, 0.3, 1.0 },
	{ 0.5, 0.9, 1.0 }
};

// triangle colors
float4 col[3] = {
	{ 1, 0, 0, 1 },
	{ 0, 1, 0, 1 },
	{ 0, 0, 1, 1 },
};

float2 winSize : VIEWPORTPIXELSIZE
	< string UIWidget="None"; >;
float time : TIME;

/*
	calculate edge equation from two end points
	Ax + By + C = 0
	A = y1 - y2
	B = x2 - x1
	C = x1y2 - x2y1
*/
float3 edge_setup(float2 v0, float2 v1)
{
	return float3(v0.y - v1.y, v1.x - v0.x, v0.x*v1.y - v1.x*v0.y);
}

// calculate 3 edges from triangle
void tri_setup(float3 v[3], out float3 e[3])
{
	e[0] = edge_setup(v[1].xy, v[2].xy);
	e[1] = edge_setup(v[2].xy, v[0].xy);
	e[2] = edge_setup(v[0].xy, v[1].xy);
}

void animate_tri(float t)
{
	tri[0].x = sin(t)*0.2 + 0.5;
}

// vertex shaders
struct a2v {
    float4 pos		: POSITION;
    float4 texcoord	: TEXCOORD0;
};

struct v2f {
    float4 pos	    : POSITION;
   	float3 uv	    : TEXCOORD0;
   	float3 edges[3] : TEXCOORD1;
};

v2f RasterizeVS(a2v IN)
{
	v2f OUT;
	OUT.pos = IN.pos;
	OUT.uv = float3(IN.texcoord.xy, 1.0);

//	animate_tri(time);
	tri_setup(tri, OUT.edges);
    return OUT;
}

// evaluate edge equation
// p is position, p.z==1.0
// e is edge (A, B, C)
bool inside_edge(float3 p, float3 e)
{
	return dot(p, e) > 0;
}

// test if are we inside triangle formed by three edges
bool inside_tri(float3 p, float3 e[3]) 
{
	return inside_edge(p, e[0]) && inside_edge(p, e[1]) && inside_edge(p, e[2]);
}

// interpolate using barycentric coordinates
// p is current pixel position, v are 3 triangle vertices, e are edges, c are colors
// returns interpolated color, and flag indicating if pixel is inside triangle
float4 interpolate(float3 p, float3 v[3], float3 e[3], float4 c[3], out bool inside)
{
	// calculate barycentric weights
	float alpha = dot(p, e[0]) / dot(v[0], e[0]);
	float beta  = dot(p, e[1]) / dot(v[1], e[1]);
//	float gamma = dot(p, e[2]) / dot(v[2], e[2]);
	float gamma = 1.0 - alpha - beta;
	inside = alpha > 0 && beta > 0 && gamma > 0;
	// interpolate
	return alpha*c[0] + beta*c[1] + gamma*c[2];
}

// interpolate using plane equations
float4 interpolate2(float3 p, float3 e[3], float4 c[3], out bool inside)
{
	float f0 = dot(p, e[0]);
	float f1 = dot(p, e[1]);
	float f2 = dot(p, e[2]);
	inside = f0 > 0 && f1 > 0 && f2 > 0;
	float r = 1 / (f0 + f1 + f2);
	f0 *= r;
	f1 *= r;
	f2 *= r;
	return c[0]*f0 + c[1]*f1 + c[2]*f2;
}

float4 RasterizePS(v2f IN) : COLOR
{
	return inside_tri(IN.uv, IN.edges);
//	return inside_edge(IN.uv, IN.edges[0]);
//	return inside_edge(IN.uv, IN.edges[1]);
//	return inside_edge(IN.uv, IN.edges[2]);
}

// barycentric
float4 RasterizePS2(v2f IN) : COLOR
{
	bool inside;
	float4 c = interpolate(IN.uv, tri, IN.edges, col, inside);
	if (!inside) discard;
	return c;
}

// plane equation
float4 RasterizePS3(v2f IN) : COLOR
{
	bool inside;
	float4 c = interpolate2(IN.uv, IN.edges, col, inside);
	if (!inside) discard;
	return c;
}

//////////////////////////////////////

technique RasterizePlane <
	string ScriptClass = "scene";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";
	string Script = "Pass=p0;";
> {
    pass p0 <
    	string Script = "RenderColorTarget0=;"
								"Draw=Buffer;";
    > {
		VertexShader = compile vs_3_0 RasterizeVS();
		PixelShader = compile ps_3_0 RasterizePS3();
    }
}

technique RasterizeBarycentric <
	string ScriptClass = "scene";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";
	string Script = "Pass=p0;";
> {
    pass p0 <
    	string Script = "RenderColorTarget0=;"
								"Draw=Buffer;";
    > {
		VertexShader = compile vs_1_1 RasterizeVS();
		PixelShader = compile ps_2_0 RasterizePS2();
    }
}

technique RasterizeNoInterpolation <
	string ScriptClass = "scene";
	string ScriptOrder = "standard";
	string ScriptOutput = "color";
	string Script = "Pass=p0;";
> {
    pass p0 <
    	string Script = "RenderColorTarget0=;"
								"Draw=Buffer;";
    > {
		VertexShader = compile vs_1_1 RasterizeVS();
		PixelShader = compile ps_2_0 RasterizePS();
    }
}
