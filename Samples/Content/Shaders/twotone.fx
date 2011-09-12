/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/twotone.fx#1 $

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
	DX8

******************************************************************************/

#define PROC_NORMAL

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?PixelShader:FixedFunction;";
> = 0.8;

float4x4 World : World < string UIWidget="None"; >;	// World or Model matrix
float4x4 WorldViewProj : WorldViewProjection < string UIWidget="None"; >;	// Model*View*Projection
float4x4 ViewIT : ViewInverseTranspose < string UIWidget="None"; >;
float4x4 View : View < string UIWidget="None"; >;

float4 ToneOne <string UIWidget = "Color"; > = { 0.3, 0.2, 0.65, 0.5 };

float4 ToneTwo <string UIWidget = "Color"; > = { 0.4, 0.65, 0.32, 0.5 };

#ifdef PROC_NORMAL

#include "normalize.fxh"
#else /* ! PROC_NORMAL */
texture NormalizeTex
<
	string ResourceName = "Normalize.dds";
	string ResourceType = "Cube";
>;
#endif /* ! PROC_NORMAL */

////////////

technique PixelShader <
	string Script = "Pass=p0;";
> {
	pass p0 <
		string Script = "Draw=geometry;";
	> {
		VertexShaderConstant[0] = <World>; 		// c0-c3 is ModelView matrix
        VertexShaderConstant[4] = <WorldViewProj>;			// c4-c7 is ModelViewProjection matrix
		VertexShaderConstant[8] = <ViewIT>;

		VertexShaderConstant[20] = <View >;

		//VertexShaderConstant[11] = {1.0,1.0,1.0,1.0};
		VertexShaderConstant[12] = {0.1,-1.0,-1.0,1.0};
		VertexShaderConstant[13] = {1.0,0.0,0.0,1.0};

		VertexShader = 
		asm
        {
            vs.1.1					// version number
 
            dcl_position v0
            dcl_normal v3
            dcl_texcoord0 v7
            dcl_tangent0 v8
            dcl_binormal0 v9
            dcl_color0 v5

			
			m4x4 oPos, v0, c4		// pos in screen space.

            mov r3, v0
            slt r3.w, r3.w, r3.w

            m4x4 r0, -r3, c8        // viewspace normal 

            dp3 r0.w, r0, r0
            rsq r0.w, r0.w
            mul r0, r0, r0.w

            mov oT0, r0

            m4x4 r1, v0, c20       // viewspace position

            dp3 r1.w, r1, r1
            rsq r1.w, r1.w
            mul r1, r1, r1.w

            mov oT1, r1
        };

		PixelShader = 
		asm
        {
            ps.1.1					// version number
		
			tex t0 
            tex t1

            dp3_sat r0, t0_bx2, t1_bx2
            lrp r0, r0, c0, c1
        };

        PixelShaderConstant[ 0 ] = <ToneOne>;
        PixelShaderConstant[ 1 ] = <ToneTwo>;

        CullMode = None;
		Zenable = true;
		ZWriteEnable = true;
		NormalizeNormals = false;
        SpecularEnable = false;

		Texture[0] = <NormalizeTex>;
		MinFilter[0] = Linear;
		MagFilter[0] = Linear;
		AddressU[0] = Clamp;
		AddressV[0] = Clamp;
		//MipFilter[0] = Point;

		Texture[1] = <NormalizeTex>;
		MinFilter[1] = Linear;
		MagFilter[1] = Linear;
		AddressU[1] = Clamp;
		AddressV[1] = Clamp;
		//MipFilter[1] = Point;
	}
}

//////////////

technique FixedFunction <
	string Script = "Pass=p0;";
> {
	pass p0 <
		string Script = "Draw=geometry;";
	> {
        SpecularEnable = false;
        Lighting = false;
        CullMode = None;
		Zenable = true;
		ZWriteEnable = true;
        AlphaBlendEnable = false;

		VertexShaderConstant[0] = <World>; 		// c0-c3 is ModelView matrix
        VertexShaderConstant[4] = <WorldViewProj>;			// c4-c7 is ModelViewProjection matrix
		VertexShaderConstant[8] = <ViewIT>;

		VertexShaderConstant[20] = <View >;

		VertexShaderConstant[12] = {0.1,-1.0,-1.0,1.0};
		VertexShaderConstant[13] = {1.0,0.0,0.0,1.0};

        VertexShaderConstant[ 50 ] = <ToneOne>;
        VertexShaderConstant[ 51 ] = <ToneTwo>;

		VertexShader = 
		asm
        {
            vs.1.1					// version number

            dcl_position v0
            dcl_normal v3
            dcl_texcoord0 v7
            dcl_tangent0 v8
            dcl_binormal0 v9
            dcl_color0 v5
			
			m4x4 oPos, v0, c4		// pos in screen space.

            mov r3, v0
            slt r3.w, r3.w, r3.w

            m4x4 r0, -r3, c8        // viewspace normal 

            dp3 r0.w, r0, r0
            rsq r0.w, r0.w
            mul r0, r0, r0.w

            m4x4 r1, v0, c20       // viewspace position

            dp3 r1.w, r1, r1
            rsq r1.w, r1.w
            mul r1, r1, r1.w

            dp3 r1, r1, r0
            sge r0, r1, c13.y     // force negative result to zero

            mov r0, c50
            mov r2, c51

            mul r0, r1, r0

            sub r1, c13.x, r1

            mad oD0, r1, r2, r0
        };

		ColorArg1[0] = Diffuse;
		ColorArg2[0] = Current;
		ColorOp[0] = SelectArg1;
		AlphaOp[0] = SelectArg1;

        ColorOp[1] = Disable;
        AlphaOp[1] = Disable;
	}
}

////////////////////////////// eof ////
