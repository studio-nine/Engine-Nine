/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/Cartoon.fx#1 $

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
	DX8-class toon shading.
	HLSL adds tune-able texturing via procedurals.

******************************************************************************/

#define PROCEDURAL_TOON_TEX

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?ToonShade:ToonShade_Eye;";
> = 0.8;

//// un-tweakables

float4x4 worldIT : WorldInverseTranspose < string UIWidget="None"; >;
float4x4 wvp : WorldViewProjection < string UIWidget="None"; >;
float4x4 world : World < string UIWidget="None"; >;
float4x4 viewInvTrans : ViewInverseTranspose < string UIWidget="None"; >;

//////// tweakables

float4 lightPos : Position
<
	string Object = "PointLight";
	string Space = "World";
> = { 100.0f, 100.0f, 100.0f, 0.0f };

float4 diffuseMaterial : DIFFUSE
<
	string UIWidget = "Color";
> = { .51f, .6f, 1.0f, 0.0f };

//// replace "toonshade" texture with procedural for HLSL


#ifndef PROCEDURAL_TOON_TEX

texture toonshade
<
	 string ResourceName = "toonshade.dds";
	 string ResourceType = "2D";
>;

#else /* PROCEDURAL_TOON_TEX */

#define FILM_TEX_SIZE 128

texture toonshade <
    string function = "create_toon_map";
    string UIWidget = "None";
    float2 Dimensions = { FILM_TEX_SIZE, FILM_TEX_SIZE };
>;

float4 create_toon_map(float2 Pos:POSITION, float2 Psize : PSIZE) : COLOR
{
    float tone = 0.25;
    if (Pos.y < 0.7) {
	    tone = 0.45;
    }
    if (Pos.y < 0.4) {
	    tone = 0.65;
    }
    if (Pos.y < (4*Psize.y)) {
	    tone = 1.0;
    }
	if (Pos.x < (6*Psize.x)) {
	    tone = 0.0;
	}
    return float4(tone.xxx,1);
}

#endif /* PROCEDURAL_TOON_TEX */

/////////////////// technique //////////

technique ToonShade <
	string Script = "Pass=p0;";
> {
	pass p0 <
		string Script = "Draw=geometry;";
	> {
		VertexShaderConstant[0] = <wvp>;
		VertexShaderConstant[4] = <worldIT>;
		VertexShaderConstant[8] = <world>;
		VertexShaderConstant[12] = <lightPos>;
		VertexShaderConstant[13] = {1.0,0.0,0.0,0.0};

		VertexShaderConstant[11] = {1.1,1.1,0.6,0.0};

		VertexShaderConstant[14] = <diffuseMaterial>;

		VertexShaderConstant[16] = <viewInvTrans>;

		VertexShader = 
		asm
		{
			vs.1.1

            dcl_position v0
            dcl_normal v3
            dcl_texcoord0 v7
            dcl_tangent0 v8
            dcl_binormal0 v9
            dcl_color0 v5

			// Transform pos to screen space.
			m4x4 oPos, v0, c0

			// Normal to world space:
			dp3 r0.x, v3, c4
			dp3 r0.y, v3, c5
			dp3 r0.z, v3, c6

			// normalize normal
			dp3 r0.w, r0, r0
			rsq r0.w, r0.w
			mul r0, r0, r0.w			// r0 has normalized normal.
			
			// vpos to world space.
			dp4 r1.x, v0, c8
			dp4 r1.y, v0, c9
			dp4 r1.z, v0, c10
			dp4 r1.w, v0, c11			// r1 has position in world space.

			// eye vector, normalize.
			add r2, c19, -r1

            //mul r2, r2, c11 - temp fix for ogl.

			dp3 r2.w, r2, r2
			rsq r2.w, r2.w
			mul r2, r2, r2.w			// r2 has normalized eye vector.

			add r3, c12, -r1
			dp3 r3.w, r3, r3
			rsq r3.w, r3.w
			mul r3, r3, r3.w

			// L dot N.
			dp3 oT0.y, r3, -r0

			// E dot N
			dp3 oT0.x, r2, r0

            mov oD0, c14
		};

		ZEnable = true;
		ZWriteEnable = true;
		AlphaBlendEnable = false;
		CullMode = None;

		Texture[0] = <toonshade>;
		MinFilter[0] = Linear;
		MagFilter[0] = Linear;
		MipFilter[0] = Linear;
        AddressU[0] = Clamp;
        AddressV[0] = Clamp;
	}
}

////////////

technique ToonShade_Eye <
	string Script = "Pass=p0;";
> {
	pass p0 <
		string Script = "Draw=geometry;";
	> {
		VertexShaderConstant[0] = <wvp>;
		VertexShaderConstant[4] = <worldIT>;
		VertexShaderConstant[8] = <world>;
		VertexShaderConstant[13] = {1.0,0.0,0.0,0.0};

		VertexShaderConstant[11] = {1.1,1.1,0.6,0.0};
		VertexShaderConstant[14] = <diffuseMaterial>;
		VertexShaderConstant[16] = <viewInvTrans>;

		VertexShader = 
		asm
		{
			vs.1.1

            dcl_position v0
            dcl_normal v3
            dcl_texcoord0 v7
            dcl_tangent0 v8
            dcl_binormal0 v9
            dcl_color0 v5

			// Transform pos to screen space.
			m4x4 oPos, v0, c0

			// Normal to world space:
			dp3 r0.x, v3, c4
			dp3 r0.y, v3, c5
			dp3 r0.z, v3, c6

			// normalize normal
			dp3 r0.w, r0, r0
			rsq r0.w, r0.w
			mul r0, r0, r0.w			// r0 has normalized normal.
			
			// vpos to world space.
			dp4 r1.x, v0, c8
			dp4 r1.y, v0, c9
			dp4 r1.z, v0, c10
			dp4 r1.w, v0, c11			// r1 has position in world space.

			// eye vector, normalize.
			add r2, c19, -r1

            //mul r2, r2, c11 - temp fix for ogl.

			dp3 r2.w, r2, r2
			rsq r2.w, r2.w
			mul r2, r2, r2.w			// r2 has normalized eye vector.

			// E dot N.
			dp3 oT0.y, -r2, r0

			// E dot N
			dp3 oT0.x, r2, r0

            mov oD0, c14
		};

		ZEnable = true;
		ZWriteEnable = true;
		AlphaBlendEnable = false;
		CullMode = None;

		Texture[0] = <toonshade>;
		MinFilter[0] = Linear;
		MagFilter[0] = Linear;
		MipFilter[0] = Linear;
        AddressU[0] = Clamp;
        AddressV[0] = Clamp;
	}
}

//////////////////// eof
