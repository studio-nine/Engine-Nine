/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/bumplocallight.fx#1 $

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
	DX8 bump surface

******************************************************************************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?PixelShader:BumpNoPixelShader;";
> = 0.8;

float4x4 worldMatrix : World < string UIWidget="None"; >;	// World or Model matrix
float4x4 mvpMatrix : WorldViewProjection < string UIWidget="None"; >;	// Model*View*Projection
float4x4 worldViewMatrix : WorldView < string UIWidget="None"; >;
float4x4 worldViewInverseMatrix : WorldViewInverse < string UIWidget="None"; >;
float4x4 viewMatrix : View < string UIWidget="None"; >;
float4x4 worldViewInverseTransposeMatrix : WorldViewInverse < string UIWidget="None"; >;

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

float4 lightPos : Position
<
	string Object = "PointLight";
	string Space = "World";
> = { 0.0f, 1.0f, -1.0f, 0.0f };

texture lightCubeMap
<
	 string ResourceName = "lightcubestar.dds";
	 string ResourceType = "Cube";
>;

string t0 = "Vertex Shader Bump Local Lighting";

technique PixelShader <
	string Script = "Pass=p0; Pass=p1;";
> {
	pass p0 <
		string Script = "Draw=geometry;";
	> {
		VertexShaderConstant[0] = <mvpMatrix>;
        VertexShaderConstant[4] = <worldViewMatrix>;
		VertexShaderConstant[8] = <worldViewInverseTransposeMatrix>;
		VertexShaderConstant[12] = <worldViewInverseMatrix>;
        VertexShaderConstant[16] = <viewMatrix>;
		VertexShaderConstant[92] = {.2,.2,.2,1.0};
		VertexShaderConstant[93] = {0.0, 0.0, 1.0, 1.0};
		VertexShaderConstant[94] = <lightPos>;
		VertexShaderConstant[95] = {1.0,1.0,1.0,0.5};
		VertexShader = 
		asm
        {
			vs.1.1

			// v[0] == position
			// v[2] == tangent => v8
			// v[3] == binormal => v9
			// v[4] == normal => v3
			// v[7] == (s,t)
            dcl_position v0
            dcl_normal v3
            dcl_texcoord0 v7
            dcl_tangent0 v8
            dcl_binormal0 v9
            dcl_color0 v5

			// c[ 0.. 3] == MODELVIEW_PROJECTION
			// c[ 4.. 7] == MODELVIEW
			// c[ 8..11] == MODELVIEW INVERSE_TRANSPOSE
			// c[12..15] == MODELVIEW INVERSE
			// c[16..19] == VIEW

			// c[93]     == [0 0 1 1]
			// c[94]     == light position (in eye space)
			// c[95].xyz == texture scale
			// c[95].w   == bump scale

			m4x4 oPos, v0, c[0]

			// compute light vector 
			//
			// R11 = eye space vertex position
			m4x4 r11, v0, c[4]

            // transform world light pos into view space
            mov r10, c[94]
            m4x4 r9, r10, c[16]

			// R1 = eye space light position - eye space vertex position
			sub r1, r9, r11

			// R1 = light vector = normalize(R1)
			dp3 r1.w, r1, r1
			rsq r1.w, r1.w
			mul r1.xyz, r1, r1.w

			// R2 = compute eye space normal
			m3x3 r2, v3, c[8]

			// color = L dot N
			dp3 r11.w, r1, r2;
			mul oD0, r11.w, c[92];
			// mov oD0, r1


			//                            | Tx Bx Nx | | b  0  0 |     | b * v[2].x    b * v[3].x   v[4].x |
			// | R3 R4 v[4] | = (S)(B) =  | Ty By Ny | | 0  b  0 |  =  | b * v[2].y    b * v[3].y   v[4].y |
			//                            | Tz Bz Nz | | 0  0  1 |     | b * v[2].z    b * v[3].z   v[4].z |

			mul r3, c[95].w, v8;
			mul r4, c[95].w, v9;


			//
			//  Compute matrix (H) that rotates L into [0 0 1]
			//           such that H^1 rotates [0 1 0] into a y' where (y' dot L) = 0
			//
			//      | R5|   | R | <- U cross L
			//  H = | R6| = | U | <- normalize(L cross [0 0 1])
			//      | R1|   | L | <- light vector

			// R6 == U == L cross [0 0 1]
			mul   r6, r1.zxyw, c[93].yzxw;
			mad   r6, r1.yzxw, c[93].zxyw, -r6;

			// normalize U (or R6)
			dp3   r6.w, r6, r6;

			// check for special case of L == [0 0 1]:
			// if R6.w == 0, then set R6 = [0 1 0 1]
            sge r2.w, c[93].x, r6.w;
            mul r7, r2.w, c[93].xzxz;
            add r2.w, c[93].z, -r2.w;
            mad r6, r6, r2.w, r7;


			rsq   r6.w, r6.w;
			mul   r6.xyz, r6, r6.w;


			// R5 == R == U cross L
			mul   r5, r6.zxyw, r1.yzxw;
			mad   r5, r6.yzxw, r1.zxyw, -r5;

			//         | R7 |
			// compute | R8 | = H * M^-T
			//         | R9 |

			// Note that we actually use the MODELVIEW INVERSE because we need it column-major
			dp3  r7.x,  r5, c[12];        
			dp3  r7.y,  r5, c[13];       
			dp3  r7.z,  r5, c[14];
			dp3  r8.x,  r6, c[12];        
			dp3  r8.y,  r6, c[13];       
			dp3  r8.z,  r6, c[14];
			dp3  r9.x,  r1, c[12];        
			dp3  r9.y,  r1, c[13];       
			dp3  r9.z,  r1, c[14];


			// compute texel matrix  (H * M^-T) * (S * B)
			dp3  oT1.x, r7, r3;       
			dp3 oT1.y, r7, r4;       
			dp3 oT1.z, r7, v3;
			dp3  oT2.x, r8, r3;       
			dp3 oT2.y, r8, r4;       
			dp3 oT2.z, r8, v3;
			dp3  oT3.x, r9, r3;       
			dp3 oT3.y, r9, r4;       
			dp3 oT3.z, r9, v3;

			// rotate eye space eye vector into light space (via H) 
			dp3  oT1.w, r5, -r11;
			dp3  oT2.w, r6, -r11;
			dp3  oT3.w, r1, -r11;

			// pass thru texture coordinates for normal map
			mul oT0.xyz, v7, c[95]; 
			mov oT0.w,  c[93].z;
        };
		
		Zenable = true;
		ZWriteEnable = true;
		CullMode = None;
		
		Texture[0] = <normalMap>;
		MinFilter[0] = Linear;
		MagFilter[0] = Linear;
		MipFilter[0] = Linear;

		Texture[3] = <lightCubeMap>;
		MinFilter[3] = Linear;
		MagFilter[3] = Linear;
		MipFilter[3] = None;
		
		PixelShaderConstant[0] = {1,1,1,1};

		PixelShader = 
		asm
		{
			ps.1.1
			tex t0
			
			texm3x3pad t1, t0_bx2
			texm3x3pad t2, t0_bx2		
			texm3x3vspec t3, t0_bx2

			add r0, t3, v0
		};
			
	}


	pass p1 <
	string Script = "Draw=geometry;";
> {
		VertexShaderConstant[0] = <mvpMatrix>;
		VertexShader = 
		asm
        {
			vs.1.1

            dcl_position v0
            dcl_texcoord0 v7

			m4x4 oPos, v0, c[0];
			mov oT0, v7;
		};

		Zenable = true;
		ZWriteEnable = false;
		ZFunc = LessEqual;
		CullMode = None;

		alphablendenable = true;
		srcblend = DESTCOLOR;
		destblend = ONE;
		Texture[0] = <diffuseTexture>;
		MinFilter[0] = Linear;
		MagFilter[0] = Linear;
		MipFilter[0] = Linear;

		ColorOp[0] = SelectArg1;		// Here for now to force GL to use D3D defaults.
		ColorArg1[0] = Texture;
		ColorArg2[0] = Current;
		AlphaOp[0] = SelectArg1;
		AlphaArg1[0] = Texture;
		AlphaArg2[0] = Current;

		ColorOp[1] = Disable;
		AlphaOp[1] = Disable;

	}
	

}

technique BumpNoPixelShader <
	string Script = "Pass=p0; Pass=p1; Pass=p2; Pass=p3;";
> {
    pass p0 <
		string Script = "Draw=geometry;";
    > {
		Zenable = true;
		ZWriteEnable = true;
		CullMode = None;
		NormalizeNormals = true;
		LocalViewer = true;

        TextureFactor = 0x008080ff; // 808080FF == 0,0,0,1

        TexCoordIndex[ 1 ] = 1 | CameraSpaceReflectionVector;

        TextureTransform[ 1 ] = <worldViewInverseMatrix>;

        TextureTransformFlags[1] = Count3;

		Texture[1] = <lightCubeMap>;
		MinFilter[1] = Linear;
		MagFilter[1] = Linear;
		MipFilter[1] = None;

		Texture[0] = <normalMap>;
		MinFilter[0] = Linear;
		MagFilter[0] = Linear;
		MipFilter[0] = Point;

		ColorOp[0] = DotProduct3; 
		ColorArg1[0] = Texture;
		ColorArg2[0] = TFactor;
		AlphaOp[0] = SelectArg1;
		AlphaArg1[0] = Texture;
		AlphaArg2[0] = Current;

		ColorOp[1] = Modulate;
		ColorArg1[1] = Texture;
		ColorArg2[1] = Current;
		AlphaOp[1] = SelectArg1;
		AlphaArg1[1] = Current;
		AlphaArg2[1] = Current;
    }

    pass p1 <
		string Script = "Draw=geometry;";
    > {
		Zenable = true;
		ZWriteEnable = false;
		CullMode = None;
		NormalizeNormals = true;
		LocalViewer = true;

        TextureFactor = 0x0000ff80;

        TextureTransform[ 1 ] = <worldViewMatrix>;
        TexCoordIndex[ 1 ] = 1 | CameraSpaceReflectionVector;

        TextureTransformFlags[1] = Count3;

		Texture[1] = <lightCubeMap>;
		MinFilter[1] = Linear;
		MagFilter[1] = Linear;
		MipFilter[1] = None;

		Texture[0] = <normalMap>;
		MinFilter[0] = Linear;
		MagFilter[0] = Linear;
		MipFilter[0] = Point;

		ColorOp[0] = DotProduct3; 
		ColorArg1[0] = Texture;
		ColorArg2[0] = TFactor;
		AlphaOp[0] = SelectArg1;
		AlphaArg1[0] = Texture;
		AlphaArg2[0] = Current;

		ColorOp[1] = Modulate;
		ColorArg1[1] = Texture;
		ColorArg2[1] = Current;
		AlphaOp[1] = SelectArg1;
		AlphaArg1[1] = Texture;
		AlphaArg2[1] = Current;

        SrcBlend = One;
        DestBlend = One;
        AlphaBlendEnable = true;
    }

    pass p2 <
		string Script = "Draw=geometry;";
    > {
		Zenable = true;
		ZWriteEnable = false;
		CullMode = None;
		NormalizeNormals = true;
		LocalViewer = true;

        TextureFactor = 0x00ff0080; 

        TextureTransform[ 1 ] = <worldViewMatrix>;
        TexCoordIndex[ 1 ] = 1 | CameraSpaceReflectionVector;

        TextureTransformFlags[1] = Count3;

		Texture[1] = <lightCubeMap>;
		MinFilter[1] = Linear;
		MagFilter[1] = Linear;
		MipFilter[1] = None;

		Texture[0] = <normalMap>;
		MinFilter[0] = Linear;
		MagFilter[0] = Linear;
		MipFilter[0] = Point;

		ColorOp[0] = DotProduct3; 
		ColorArg1[0] = Texture;
		ColorArg2[0] = TFactor;
		AlphaOp[0] = SelectArg1;
		AlphaArg1[0] = Texture;
		AlphaArg2[0] = Current;

		ColorOp[1] = Modulate;
		ColorArg1[1] = Texture;
		ColorArg2[1] = Current;
		AlphaOp[1] = SelectArg1;
		AlphaArg1[1] = Texture;
		AlphaArg2[1] = Current;

        SrcBlend = One;
        DestBlend = One;
        AlphaBlendEnable = true;
    }

    pass p3 <
		string Script = "Draw=geometry;";
    > {
		Zenable = true;
		ZWriteEnable = false;
		CullMode = None;

		Texture[0] = <diffuseTexture>;
		MinFilter[0] = Linear;
		MagFilter[0] = Linear;
		MipFilter[0] = Linear;

	    ColorOp[0] = SelectArg1; 
		ColorArg1[0] = Texture;
		ColorArg2[0] = TFactor;
		AlphaOp[0] = SelectArg1;
		AlphaArg1[0] = Texture;
		AlphaArg2[0] = Current;

        SrcBlend = DESTCOLOR;
        DestBlend = One;
        AlphaBlendEnable = true;
    }

}
