/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/DispersionFlip.fx#1 $

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
    string Script = "Technique=t0;";
> = 0.8;

float4x4 WorldViewProj : WorldViewProjection < string UIWidget="None"; >;
float4x4 World : World < string UIWidget="None"; >;
float4x4 ViewInverseT : ViewInverseTranspose < string UIWidget="None"; >;

texture envTexture : ENVIRONMENT
<
	string ResourceName = "sky_cube_mipmap.dds";
	string ResourceType = "Cube";
>;

///////////////////////

technique t0 <
	string Script = "Pass=p0;";
> {
	pass p0 <
		string Script = "Draw=geometry;";
	> {
		VertexShaderConstant[0] = <WorldViewProj>;
		VertexShaderConstant[4] = <World>;
		//VertexShaderConstant[8] = <ViewInverse>; // c11 contains eye position in world space.
		
		//VertexShaderConstant[11] = <displacement>;
		VertexShaderConstant[9] = {1.1, 1.08, 1.06, 1.0};	// Eta
		VertexShaderConstant[10] = {4.0, 4.0, 0.7, 0.0};	// Fresnel
		VertexShaderConstant[11] = {0.0, 0.0, 0.0, 0.0};  // displacement
		VertexShaderConstant[12] = {0.0, 0.5, 1.0, 0.0};
		VertexShaderConstant[13] = {0.25, 9.0, 0.75, 0.159155};
		VertexShaderConstant[14] = {24.9808, 60.1458, 85.4538, 64.9394};
		VertexShaderConstant[15] = {19.7392, 1.0, 2.0, 0.0};
		VertexShaderConstant[16] = <ViewInverseT>;
		VertexShaderConstant[20] = {-1.0, 0.0, 0.0, 0.0};
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

			mov r0.x, c13.x
			mov r0.y, -c13.y
			mov r0.z, c13.z
			mov r0.w, c13.w
			mad r1, v0.y, c11.x, c11.y
			mad r1, r0.w, r1.x, -r0.x
			expp r1.y, r1.x
			add r2, c12.xyz, -r1.yyy
			mul r2, r2.xyz, r2.xyz
			slt r3.x, r1.y, r0.x
			sge r0, r1.yy, r0.yz
			mov r3.yz, r0.xxyw
			mov r0.x, c15.x
			mov r0.y, -c15.x
			mov r0.z, -c15.y
			mov r0.w, c15.y
			dp3 r3.y, r3.xyz, r0.zwz
			mov r1.x, c14.x
			mov r1.y, -c14.x
			mov r1.z, -c14.y
			mov r1.w, c14.y
			mad r1, r1.xyx, r2.xyz, r1.zwz
			mov r4.x, c14.z
			mov r4.y, -c14.z
			mov r4.z, -c14.w
			mov r4.w, c14.w
			mad r1, r1.xyz, r2.xyz, r4.xyx
			mad r1, r1.xyz, r2.xyz, r4.zwz
			mad r1, r1.xyz, r2.xyz, r0.xyx
			mad r0, r1.xyz, r2.xyz, r0.zwz
			dp3 r2.x, r0.xyz, -r3.xyz
			mul r0, c11.z, r2.x
			mov r1, v3
			mad r0, r0.x, r1, v0
			dp4 r2.x, c0, r0
			dp4 r2.y, c1, r0
			dp4 r2.z, c2, r0
			dp4 r2.w, c3, r0
			mov oPos, r2
			dp4 r2.x, c4, r0
			dp4 r2.y, c5, r0
			dp4 r2.z, c6, r0
			dp4 r2.w, c7, r0
			add r0, r2.xyz, -c19.xyz
			dp3 r2, r0.xyz, r0.xyz
			rsq r2, r2.x
			mul r0, r2.x, r0.xyz
			dp3 r2.x, c4.xyz, r1.xyz
			dp3 r2.y, c5.xyz, r1.xyz
			dp3 r2.z, c6.xyz, r1.xyz
			dp3 r1, r2.xyz, r0.xyz
			mul r3, c15.z, r2.xyz
			mul r1, r3.xyz, r1.x
			add r1, r0.xyz, -r1.xyz
			mov r8, r1
			mul r8.x, r8.x, c20.x
			mul r8.y, r8.y, c20.x
			mov oT3.xyz, r8
			dp3 r1, r0.xyz, r2.xyz
			add r1, c15.y, r1.x
			mov r1.z, c10.x
			mov r1.w, c10.x
			lit r1, r1
			mad r1, c10.y, r1.z, c10.z
			mov oD0.xyz, r1.x
			dp3 r1, r0.xyz, r2.xyz
			mul r3, r1.x, r1.x
			add r3, c15.y, -r3.x
			mul r4, c9.xyz, c9.xyz
			mul r3, r4.xyz, r3.x
			add r3, c15.y, -r3.xyz
			rsq r4.x, r3.x
			rsq r4.y, r3.y
			rsq r4.z, r3.z
			rcp r3.z, r4.z
			rcp r3.y, r4.y
			rcp r3.x, r4.x
			mul r3, c15.y, r3.xyz
			mad r1, r1.x, c9.xyz, r3.xyz
			mul r3, r1.x, r2.xyz
			mad r3, c9.x, r0.xyz, -r3.xyz
			mov r8, r3
			mul r8.x, r8.x, c20.x
			mul r8.y, r8.y, c20.x
			mov oT0.xyz, r8
			mul r3, r1.y, r2.xyz
			mad r3, c9.y, r0.xyz, -r3.xyz
			mov r8, r3
			mul r8.x, r8.x, c20.x
			mul r8.y, r8.y, c20.x
			mov oT1.xyz, r8
			mul r1, r1.z, r2.xyz
			mad r0, c9.z, r0.xyz, -r1.xyz
			mov r8, r0
			mul r8.x, r8.x, c20.x
			mul r8.y, r8.y, c20.x
			mov oT2.xyz, r8
		};

		Zenable = true;
		ZWriteEnable = true;
		CullMode = None;

		Texture[0] = <envTexture>;
		MinFilter[0] = Linear;
		MagFilter[0] = Linear;
		MipFilter[0] = Linear;

		Texture[1] = <envTexture>;

		MinFilter[1] = Linear;
		MagFilter[1] = Linear;
		MipFilter[1] = Linear;

		Texture[2] = <envTexture>;
		MinFilter[2] = Linear;
		MagFilter[2] = Linear;
		MipFilter[2] = Linear;

		Texture[3] = <envTexture>;
		MinFilter[3] = Linear;
		MagFilter[3] = Linear;
		MipFilter[3] = Linear;

		PixelShaderConstant[0] = {1.0, 0.0, 0.0, 1.0};
		PixelShaderConstant[1] = {0.0, 1.0, 0.0, 1.0};
		PixelShaderConstant[2] = {0.0, 0.0, 1.0, 1.0};
		PixelShader = 
		asm
		{
			ps.1.1
			tex t0
			tex t1
			tex t2
			tex t3
			mul r0, t0, c0
			mad r0, t1, c1, r0 
			mad r0, t2, c2, r0
			lrp r0, v0, t3, r0
		};
	}

}
	
//////////////////////////// eof ///
