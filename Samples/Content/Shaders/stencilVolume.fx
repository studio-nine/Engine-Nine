/*********************************************************************NVMH4****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/stencilVolume.fx#1 $

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

******************************************************************************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?stencil:showVolume:simple;";
> = 0.8;

/********* tweakables ********************/

float4x4 WorldViewProj : WorldViewProjection < string UIWidget="None"; >;

float4 LightPos : Position
<
    string Object = "PointLight";
    string Space = "World";
> = {100.0f, 100.0f, 100.0f, 0.0f};

float GeomInset <
    string UIWidget = "slider";
    float UIMin = 0.0;
    float UIMax = 1.0;
    float UIStep = 0.001;
    string UIName =  "Geometric Inset";
> = 0.02;

float ShadowExtrudeDist <
	string UIName = "Extrusion Distance";
	string UIWidget = "slider";
	float UIMin = 1.0;
	float UIMax = 1000.0;
	float UIStep = 1.0;
> = 1000.0f;

/************ structs and connections *****************************/

struct appdata
{
    float4 Position	: POSITION;
    // float2 TexCoord0	: TEXCOORD0;
    float3 Normal	: NORMAL;
};

/* data passed from vertex shader to pixel shader */
struct vertexOutput {
    float4 HPosition	: POSITION;
    // float4 TexCoord0	: TEXCOORD0; // dot prods against half-angle
    float4 diffCol	: COLOR0;
};

/*********** vertex shader ******/

vertexOutput extrudeVS( appdata IN)
{
    vertexOutput OUT;
    // Create normalized vector from vertex to light
    float4 Lvec = normalize( IN.Position - LightPos );
    // N dot L to decide if point should be moved away
    //   from the light to extrude the volume
    float ldn = dot( -Lvec.xyz, IN.Normal.xyz );
    //////////////////////////////////////////////////////////
    // Inset the position along the normal vector direction
    // This moves the shadow volume points inside the model
    //  slightly to minimize poping of shadowed areas as
    //  each facet comes in and out of shadow.
    float4 inset_pos = float4(((IN.Position.xyz - (IN.Normal * GeomInset)).xyz),IN.Position.w);
    // scale the vector from light to vertex
    float4 extrusion_vec = Lvec * ShadowExtrudeDist;
    // if ldn < 0 then the vertex faces away from the light, so
    //   move it.  It will be moved along the direction from
    //   light to vertex to extrude the shadow volume.
    // Consts_0512 = { 0.0f, 0.5f, 1.0f, 2.0f };
    //   So this does toggle = N dot L < 0 ? 1.0 : 0.0
    float toggle = (float) (ldn < 0.0);
    // Move the back-facing shadow volume points
    float4 new_position = extrusion_vec*toggle + inset_pos;
    OUT.HPosition = mul( new_position, WorldViewProj);
    ////////////////////////////////////////////////////////
    OUT.diffCol		= float4(0,0,0,1);
    // OUT.TexCoord0	= IN.TexCoord0.xyyy;
    return( OUT );
}

vertexOutput backingVS(appdata IN)
{
    vertexOutput OUT;
    OUT.HPosition = mul(IN.Position, WorldViewProj);
    OUT.diffCol = float4(0.1,0.1,0.1,1);
    // OUT.TexCoord0 = float4(0, 0, 0, 1);
    return( OUT );
}

vertexOutput simpleVS( appdata IN)
{
    vertexOutput OUT;
    float4 Lvec = normalize( IN.Position - LightPos );
    float ldn = abs(dot(-Lvec.xyz, IN.Normal.xyz ));
    OUT.HPosition = mul( IN.Position, WorldViewProj );
    OUT.diffCol = float4(ldn.xxx,1);
    // OUT.TexCoord0 = float4(0,0,0,1);
    return( OUT );
}

/*************** techniques *******************/

technique stencil <
	string Script = "Pass=layZ; Pass=back; Pass=front; Pass=lighting;";
> {
    pass layZ <
	string Script = "Draw=geometry;";
> {		
		VertexShader = compile vs_1_0 backingVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		StencilEnable = false;
    }
    pass back <
	string Script = "Draw=geometry;";
> {		
		VertexShader = compile vs_1_1 extrudeVS();
		ZEnable = true;
		ZWriteEnable = false;
		CullMode = CCW;
		StencilEnable = True;
		StencilPass = Keep;
		StencilFail = Keep;
		StencilZFail = IncrSat;
		StencilFunc = Always;
		ColorWriteEnable = 0;
    }
    pass front <
	string Script = "Draw=geometry;";
> {		
		VertexShader = compile vs_1_1 extrudeVS();
		ZEnable = true;
		ZWriteEnable = false;
		CullMode = CW;
		StencilEnable = True;
		StencilPass = Keep;
		StencilFail = Keep;
		StencilZFail = DecrSat;
		StencilFunc = Always;
		ColorWriteEnable = 0;
    }
    pass lighting <
	string Script = "Draw=geometry;";
> {		
		VertexShader = compile vs_1_1 simpleVS();
		ZEnable = true;
		// ZWriteEnable = true;
		CullMode = None;
		StencilEnable = True;
		StencilPass = Keep;
		StencilZFail = Keep;
		StencilFail = Keep;
		// StencilRef = 0;
		StencilFunc = Equal;
    }
}

technique showVolume <
	string Script = "Pass=layZ;";
> {
    pass layZ <
	string Script = "Draw=geometry;";
> {		
		VertexShader = compile vs_1_1 extrudeVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		StencilEnable = false;
    }
}

technique simple <
	string Script = "Pass=layZ;";
> {
    pass layZ <
	string Script = "Draw=geometry;";
> {		
		VertexShader = compile vs_1_1 simpleVS();
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		StencilEnable = false;
    }
}

/********************************** eof ***/
