/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/Rainbow.fx#1 $

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
	DX8-class tie-dye shader, optional procedural textures

******************************************************************************/

#define PROCEDURAL_COLORS

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Rainbow;";
> = 0.8;

float4x4 WorldIT : WorldInverseTranspose < string UIWidget="None"; >;
float4x4 WorldViewProj : WorldViewProjection < string UIWidget="None"; >;
float4x4 World : World < string UIWidget="None"; >;
float4x4 ViewInvTrans : ViewInverseTranspose < string UIWidget="None"; >;
float4x4 View : View < string UIWidget="None"; >;

/////////////////

#ifdef PROCEDURAL_COLORS

#include "rgb-hsv.fxh"

#define RAINBOW_SIZE 64

float4 rainbow_colors(float2 Pos : POSITION) : COLOR
{
	float3 hsv = float3(Pos.x,1.0,1.0);
    float3 rgb = hsv_to_rgb(hsv);
	float4 final = float4(rgb.xyz,1.0);
	return final;
}

texture colors  <
    string ResourceType = "2D";
    string function = "rainbow_colors";
    string UIWidget = "None";
    float2 Dimensions = { RAINBOW_SIZE, RAINBOW_SIZE };
>;

#else /* ! PROCEDURAL_COLORS */

texture colors
<
	 string ResourceName = "colors2.dds";
	 string ResourceType = "2D";
>;

#endif /* ! PROCEDURAL_COLORS */

texture swirl
<
	 string ResourceName = "swirl.dds";
	 string ResourceType = "2D";
>;

/////////////////

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
		VertexShaderConstant[16] = <ViewInvTrans>;

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

/////////////////////////////////////////////// eof ///
