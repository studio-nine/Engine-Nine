/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/MultiTechniques_test.fx#1 $

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
	Very early .FX test file

******************************************************************************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?t0:Rainbow;";
> = 0.8;

float4x4 ViewIT : ViewInverseTranspose < string UIWidget="None"; >;
float4x4 View : View < string UIWidget="None"; >;
float4x4 WorldView : WorldView < string UIWidget="None"; >;
float4x4 WorldIT : WorldInverseTranspose < string UIWidget="None"; >;
float4x4 World : World < string UIWidget="None"; >;
float4x4 WorldViewIT : WorldViewInverseTranspose < string UIWidget="None"; >;
float4x4 WorldViewProj : WorldViewProjection < string UIWidget="None"; >;

//////////

float4 diffuse : DIFFUSE < string UIWidget = "Color"; > = { 0.1f, 0.1f, 0.5f, 1.0f };
float4 specular : Specular < string UIWidget = "Color"; > = { 1.0f, 1.0f, 1.0f, 1.0f };
float4 ambient : Ambient < string UIWidget = "Color"; > = { 0.1f, 0.1f, 0.1f, 1.0f };

float SpecPower : SpecularPower <
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 128.0;
	float UIStep = 0.1;
> = 100;

float4 LightPos : Position <
	string Object = "PointLight";
	string Space = "World";
> = { 100.0f, 100.0f, 0.0f, 0.0f };

//////////////

texture diffuseTexture : DIFFUSE <
    string ResourceName = "default_color.dds";
    string ResourceType = "2D";
>;

texture colors
<
	 string ResourceName = "colors2.dds";
	 string ResourceType = "2D";
>;

texture swirl
<
	 string ResourceName = "swirl.dds";
	 string ResourceType = "2D";
>;

///////////////////////////////////

struct VS_INPUT
{
	float4 vPosition : POSITION;
	float4 vNormal : NORMAL;
	float4 vTexCoords : TEXCOORD0;
};

struct VS_OUTPUT
{
	float4 vPosition : POSITION;
	float4 vTexCoord0 : TEXCOORD0;
	float4 vDiffuse : COLOR0;
	float4 vSpecular : COLOR1;
};

///////////////////////////////////

VS_OUTPUT myvs( uniform float4x4 ModelViewProj, 
                uniform float4x4 ModelView,
                uniform float4x4 ModelViewIT,
                uniform float4x4 ViewIT, 
                uniform float4x4 View, 
                const VS_INPUT vin, 
                uniform float4 LightPos,
                uniform float4 diffuse, 
                uniform float4 specular, 
                uniform float4 ambient,
                uniform float SpecPower)
{
	VS_OUTPUT vout; 
    float4 position = mul(vin.vPosition, ModelView);
    float4 normal = normalize(mul(vin.vNormal, ModelViewIT));

    float4 viewLightPos = mul(LightPos, View);

    float4 lightvec = normalize(viewLightPos - position);
    float4 eyevec = normalize(ViewIT[3]);
	
    float ndotl = dot(normal,lightvec);
    float4 halfangle = normalize(lightvec + eyevec);
    float ndoth = dot(normal,halfangle);
	float4 litres = lit(ndotl, ndoth, SpecPower);

    float4 diff_term = (diffuse * litres.yyyy) + (litres.zzzz * specular) + ambient;
    vout.vDiffuse = diff_term;
    vout.vSpecular = specular;
	vout.vTexCoord0 = vin.vTexCoords;
    vout.vPosition = mul(vin.vPosition, ModelViewProj);
	return vout;
}

///////////////////////////////////

technique t0 <
	string Script = "Pass=p0;";
> {
	pass p0  <
		string Script = "Draw=geometry;";
	> {
		ZEnable = true;
        ZWriteEnable = true;
        CullMode = None;
		VertexShader = compile vs_1_1 myvs( WorldViewProj, WorldView, WorldViewIT,
					ViewIT, View, LightPos, diffuse, specular, ambient, SpecPower);

		Texture[0] = <diffuseTexture>;
		MinFilter[0] = Linear;
		MagFilter[0] = Linear;

		Texture[1] = <diffuseTexture>;
		MinFilter[1] = Linear;
		MagFilter[1] = Linear;

		// Assign diffuse color to be used
		ColorOp[0]   = Modulate;
		ColorArg1[0] = Texture;
		ColorArg2[0] = Diffuse;
		AlphaOp[0]   = SelectArg1;
		AlphaArg1[0] = Diffuse;

	}
}

//////////////////////

technique Rainbow <
	string Script = "Pass=p0;";
> {
	pass p0 <
		string Script = "Draw=geometry;";
	> {
		VertexShaderConstant[0] = <WorldViewProj>;
		VertexShaderConstant[4] = <WorldIT>;
		VertexShaderConstant[8] = <World>;
	    VertexShaderConstant[20] = <View>;
		VertexShaderConstant[13] = {1.0,0.0,0.0,0.0};
		VertexShaderConstant[16] = <ViewIT>;

		VertexShader = 
		asm
		{
		    ; v0  -- position
		    ; v3  -- normal
		    ; v7  -- tex coord
		    ; v8 -- tex coord1
		    ;
		    ; c0-3   -- world/view/proj matrix
		    ; c8-11   -- inverse/transpose world matrix
		    ; c9     -- {0.0, 0.5, 1.0, -1.0}
		    ; c10    -- eye point
		    ; c11-14 -- world matrix
		    vs.1.1
            dcl_position v0
            dcl_normal v3
            dcl_texcoord0 v7
            dcl_tangent0 v8
            dcl_binormal0 v9
            dcl_color0 v5

		    ;transform position

		    m4x4 oPos, v0, c0

		    ;transform normal

		    m3x3 r0, v3, c4

		    ;normalize normal
		    dp3 r0.w, r0, r0
		    rsq r0.w, r0.w
		    mul r0, r0, r0.w

		    ;compute world space position
		    m4x4 r1, v0, c8

		    m4x4 r2, r1, c20 // view

		    ;normalize e
		    dp3 r2.w, r2, r2
		    rsq r2.w, r2.w
		    mul r2, r2, r2.w

		    ;h = Normalize( n + e )
		    add r1, r0, r2

		    ;normalize h
		    dp3 r1.w, r1, r1
		    rsq r1.w, r1.w
		    mul r1, r1, r1.w

		    ;h dot n
		    dp3 oT0.x, r2, r0
		    dp3 oT0.y, r1, r0

		    mov oT1.x, r2
		    mov oT1.y, r1 
		};

		ZEnable = true;
		ZWriteEnable = true;
		AlphaBlendEnable = false;
		SrcBlend = SrcAlpha;
		DestBlend = Zero;
		CullMode = None;
		Lighting = false;

		Texture[0] = <colors>;
		MinFilter[0] = Linear;
		MagFilter[0] = Linear;
		MipFilter[0] = Linear;
		//AddressU[0] = Mirror;

		Texture[1] = <swirl>;
		MinFilter[1] = Linear;
		MagFilter[1] = Linear;
		MipFilter[1] = Linear;

		ColorOp[0] = Modulate2x;
		ColorArg1[0] = Texture;
		ColorArg2[0] = Texture | AlphaReplicate;

		AlphaOp[0] = SelectArg1;
		AlphaArg1[0] = Texture;
		AlphaArg2[0] = Current;

		ColorOp[1] = AddSigned;
		ColorArg1[1] = Current;
		ColorArg2[1] = Texture | AlphaReplicate;

		AlphaArg1[1] = Texture;
		AlphaOp[1] = SelectArg1;

	}
}

