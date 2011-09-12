/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/Ghostly.fx#1 $

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
    string Script = "Technique=Ghostly;";
> = 0.8;

float4x4 worldIT : WorldInverseTranspose < string UIWidget="None"; >;
float4x4 wvp : WorldViewProjection < string UIWidget="None"; >;
float4x4 world : World < string UIWidget="None"; >;
float4x4 viewInvTrans : ViewInverseTranspose < string UIWidget="None"; >;

int lodBias = 0;

texture diffuseTexture : DIFFUSE
<
	string ResourceName = "default_color.dds";
    string ResourceType = "2D";
>;

///////////////////////

technique Ghostly <
	string Script = "Pass=p0; Pass=p1;";
> {
	pass p0 <
		string Script = "Draw=geometry;";
	> {
		VertexShaderConstant[0] = <wvp>;
		VertexShaderConstant[4] = <worldIT>;
		VertexShaderConstant[8] = <world>;
		VertexShaderConstant[12] = {5.0,12.0,10.0,0.0};
		VertexShaderConstant[13] = {1.0,0.0,0.0,0.0};

		VertexShaderConstant[11] = {1.1,1.1,0.6,0.0};

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

			dp3 r2.w, r2, r2
			rsq r2.w, r2.w
			mul r2, r2, r2.w			// r2 has normalized eye vector.

			mov r3, c12
			dp3 r3.w, r3, r3
			rsq r3.w, r3.w
			mul r4, r3, r3.w

			// E dot N
			dp3 r0, r2, r0
			mul oD0, r0, r0

            mov oT0, v7
		};

		ZEnable = true;
		ZWriteEnable = true;
		AlphaBlendEnable = true;

		SrcBlend = One;
		DestBlend = InvSrcColor;

		CullMode = CCW;

		Texture[0] = <diffuseTexture>;
		MinFilter[0] = Linear;
		MagFilter[0] = Linear;
		MipFilter[0] = Linear;
        MipMapLodBias[0] = <lodBias>;

		MinFilter[1] = Linear;
		MagFilter[1] = Linear;
		MipFilter[1] = None;
        MipMapLodBias[1] = <lodBias>;
        AddressU[1] = Clamp;
        AddressV[1] = Clamp;

	}

	pass p1 <
		string Script = "Draw=geometry;";
	> {
		VertexShaderConstant[0] = <wvp>;
		VertexShaderConstant[4] = <worldIT>;
		VertexShaderConstant[8] = <world>;
		VertexShaderConstant[12] = {5.0,12.0,10.0,0.0};
		VertexShaderConstant[13] = {1.0,0.0,0.0,0.0};

		VertexShaderConstant[11] = {1.1,1.1,0.6,0.0};

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

			dp3 r2.w, r2, r2
			rsq r2.w, r2.w
			mul r2, r2, r2.w			// r2 has normalized eye vector.

			
			mov r3, c12
			dp3 r3.w, r3, r3
			rsq r3.w, r3.w
			mul r4, r3, r3.w

			// E dot N
			dp3 r0, r2, r0
			mul oD0, r0, r0

            mov oT0, v7
		};

		ZEnable = true;
		ZWriteEnable = true;
		AlphaBlendEnable = true;
		CullMode = CW;

		SrcBlend = One;
		DestBlend = InvSrcColor;

		Texture[0] = <diffuseTexture>;
		MinFilter[0] = Linear;
		MagFilter[0] = Linear;
		MipFilter[0] = Linear;

		MinFilter[1] = Linear;
		MagFilter[1] = Linear;
		MipFilter[1] = None;
        AddressU[1] = Clamp;
        AddressV[1] = Clamp;
	}
}

///////////////////////////////////////// eof //
