/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/DiffuseBump.fx#1 $

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
    string Script = "Technique=Technique?PixelShader:NoPixelShader;";
> = 0.8;

float4x4 worldMatrix : WorldTranspose < string UIWidget="None"; >;	// World or Model matrix
float4x4 worldIMatrix : WorldInverseTranspose < string UIWidget="None"; >;	// World Inverse matrix
float4x4 mvpMatrix : WorldViewProjection < string UIWidget="None"; >;	// Model*View*Projection
float4x4 worldViewMatrix : WorldView < string UIWidget="None"; >;
float4x4 viewInverseMatrix : ViewInverse < string UIWidget="None"; >;

texture diffuseTexture : DIFFUSE
<
	string ResourceName = "default_color.dds";
    string ResourceType = "2D";
>;

texture normalMap : NORMAL
<
	string ResourceName = "default_bump_normal.dds";
    string ResourceType = "2D";
>;

float4 lightPos : POSITION
<
	string Object = "PointLight";
	string Space = "World";
> = { 100.0f, 100.0f, 100.0f, 0.0f };


float4 lightColor : DIFFUSE
<
	string Object = "PointLight";
	string UIWidget = "Color";
> = { 0.8f, 0.8f, 0.8f, 0.0f };



float bumpHeight
<
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 1.0;
	float UIStep = 0.05;
> = 0.5;

float ambientLight
<
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 1.0;
	float UIStep = 0.05;
> = 0.25;

////////////////////////////// techniques

technique PixelShader <
	string Script = "Pass=p0;";
> {
	pass p0 <
		string Script = "Draw=geometry;";
	> {
		VertexShaderConstant[0] = <worldMatrix>; 		// c0-c3 is Model matrix
        VertexShaderConstant[4] = <mvpMatrix>;			// c4-c7 is ModelViewProjection matrix
		VertexShaderConstant[8] = <worldIMatrix>;
		VertexShaderConstant[12] = <lightPos>;
		VertexShaderConstant[13] = {0.5,0.5,0.5,1.0};
        VertexShaderConstant[14] = <lightColor>;
        VertexShaderConstant[15] =  <bumpHeight>; // {bumpHeight, bumpHeight, .5, ambientLight };
        VertexShaderConstant[16] =  <ambientLight>; // {bumpHeight, bumpHeight, .5, ambientLight };

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
			mov oT0, v7				// tex coords for normal map
			mov oT1, v7				// tex coords for diffuse tex.

            mov r0, c12
			dp3 r0.w, r0, r0		// normalize light dir.
			rsq r0.w, r0.w
			mul r0, r0, r0.w	

            // Transform Light to Object space
			dp3 r2.x, c8, r0		// Tangent dot Light
			dp3 r2.y, c9, r0		// Binormal dot Light
			dp3 r2.z, c10, r0		// Normal dot Light

			// Transform Light to tangent space.
			dp3 r1.x, v8, r2		// Tangent dot Light
			dp3 r1.y, v9, r2		// Binormal dot Light
			dp3 r1.z, v3, r2		// Normal dot Light
            sge r1.w, r1.x, r1.x

            add r0.xyz, c15, c15    // *= 2
            mul r0.xyz, r1, r0    // *= bump height

            mad oD0.xyz, r0, c13, c13 // put in diffuse
            mov oD0.w, c16.w
            mov oD1, c14          // light color
        };

        SpecularEnable = false;

		Zenable = true;
		ZWriteEnable = true;
		CullMode = None;
		
		Texture[0] = <normalMap>;
		MinFilter[0] = Linear;
		MagFilter[0] = Linear;
		MipFilter[0] = Linear;

		Texture[1] = <diffuseTexture>;
		MinFilter[1] = Linear;
		MagFilter[1] = Linear;
		MipFilter[1] = Linear;

		PixelShader = 
		asm
		{
			ps.1.1

			tex t0
			tex t1

			dp3_sat r0, t0_bx2, v0_bx2
            mad_sat r0.a, r0.a, 1-v0.a, v0.a        // add smooth dot3 result and ambient factor

            mul r0.rgb, v1, t1           // factor in light and texture color
		};

        AlphaBlendEnable = true;
        SrcBlend = SrcAlpha;
        DestBlend = Zero;
		
	}
}

////////

technique NoPixelShader <
	string Script = "Pass=p0;";
> {
	pass p0 <
		string Script = "Draw=geometry;";
	> {
		VertexShaderConstant[0] = <worldMatrix>; 		// c0-c3 is Model matrix
        VertexShaderConstant[4] = <mvpMatrix>;			// c4-c7 is ModelViewProjection matrix
		VertexShaderConstant[8] = <worldIMatrix>;
		VertexShaderConstant[12] = <lightPos>;
		VertexShaderConstant[13] = {0.5,0.5,0.5,1.0};
        VertexShaderConstant[14] = <lightColor>;
        VertexShaderConstant[15] =  <bumpHeight>;//{bumpHeight, bumpHeight, .5, ambientLight };
        VertexShaderConstant[16] =  <ambientLight>; // {bumpHeight, bumpHeight, .5, ambientLight };

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
			mov oT0, v7				// tex coords for normal map
			mov oT1, v7				// tex coords for diffuse tex.

            mov r0, c12
			dp3 r0.w, r0, r0		// normalize light dir.
			rsq r0.w, r0.w
			mul r0, r0, r0.w	

            // Transform Light to Object space
			dp3 r2.x, c8, r0		// Tangent dot Light
			dp3 r2.y, c9, r0		// Binormal dot Light
			dp3 r2.z, c10, r0		// Normal dot Light

			// Transform Light to tangent space.
			dp3 r1.x, v8, r2		// Tangent dot Light
			dp3 r1.y, v9, r2		// Binormal dot Light
			dp3 r1.z, v3, r2		// Normal dot Light
            sge r1.w, r1.x, r1.x

            add r0, c15, c15    // *= 2
            mul r0, r1, r0    // *= bump height

            mad oD0.xyz, r0, c13, c13 // put in diffuse
            mov oD0.w, c16.w
            mov oD1, c14          // light color
        };

 		Zenable = true;
		ZWriteEnable = true;
		CullMode = None;
		
		Texture[0] = <normalMap>;
		MinFilter[0] = Linear;
		MagFilter[0] = Linear;
		MipFilter[0] = Linear;

		Texture[1] = <diffuseTexture>;
		MinFilter[1] = Linear;
		MagFilter[1] = Linear;
		MipFilter[1] = Linear;

		ColorArg1[0] = Texture;
		ColorOp[0] = DotProduct3;
		ColorArg2[0] = Diffuse;

		AlphaArg1[0] = Texture;
		AlphaOp[0] = SelectArg1;
		AlphaArg2[0] = Diffuse;

		AlphaArg1[1] = Current;
		AlphaOp[1] = AddSmooth;
		AlphaArg2[1] = Diffuse;
 
        ColorArg1[1] = Texture;
        ColorOp[1] = Modulate;
        ColorArg2[1] = Specular;

        SpecularEnable = false;

        AlphaBlendEnable = true;
        SrcBlend = SrcAlpha;
        DestBlend = Zero;
	}
}

///// eof
