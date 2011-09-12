/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/Cellophane.fx#1 $

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
		DX8-class hardware

******************************************************************************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Cellophane;";
> = 0.8;

float4x4 worldMatrix : World < string UIWidget="None"; >;	// World or Model matrix
float4x4 mvpMatrix : WorldViewProjection < string UIWidget="None"; >;	// Model*View*Projection
float4x4 worldViewMatrix : WorldView < string UIWidget="None"; >;
float4x4 viewInverseMatrix : ViewInverseTranspose < string UIWidget="None"; >;
float4x4 viewMatrix : View < string UIWidget="None"; >;

texture diffuseTexture : DIFFUSE
<
	string ResourceName = "default_color.dds";
    string ResourceType = "2D";
>;

string t0 = "Vertex Shader Cellophane";

technique Cellophane <
	string Script = "Pass=p0;";
> {
	pass p0 <
		string Script = "Draw=geometry;";
	> {
		VertexShaderConstant[0] = <worldMatrix>; 		// c0-c3 is ModelView matrix
        VertexShaderConstant[4] = <mvpMatrix>;			// c4-c7 is ModelViewProjection matrix
		VertexShaderConstant[8] = <viewInverseMatrix>;

		VertexShaderConstant[20] = <viewMatrix >;
		VertexShaderConstant[12] = {0.1,-1.0,-1.0,1.0};
		VertexShaderConstant[13] = {1.0,0.0,0.0,0.5};
		VertexShaderConstant[14] = { 0.0f, 0.7f, 0.7f, 0.0f };

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

            m4x4 r1, v0, c20       // viewspace position

            dp3 r1.w, r1, r1
            rsq r1.w, r1.w
            mul r1, r1, r1.w

            mov r2, v3
            slt r2.w, r2.w, r2.w

            m4x4 r3, r2, c20      // viewspace normal

            dp3 r2.w, r3, r3
            rsq r2.w, r2.w
            mul r2, r3, r2.w

            dp3 r0, r1, r2
            mov oD0, r0

            mov oT1, v7     // texture coordinates

            add oT0.xyz, r1, r2    // reflection coordinates
        };

		Zenable = true;
		ZWriteEnable = true;
		CullMode = None;

		Texture[0] = <diffuseTexture>;
		MinFilter[0] = Linear;
		MagFilter[0] = Linear;
		MipFilter[0] = Linear;

		Texture[1] = <diffuseTexture>;
		MinFilter[1] = Linear;
		MagFilter[1] = Linear;
		MipFilter[1] = Linear;

        ColorArg1[0] = Texture;
        ColorOp[0] = Modulate;
        ColorArg2[0] = Texture;

        ColorArg1[1] = Texture;
        ColorOp[1] = Modulate;
        ColorArg2[1] = Current | Complement;

	}
}
