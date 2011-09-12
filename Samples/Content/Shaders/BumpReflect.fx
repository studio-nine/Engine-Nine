/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/BumpReflect.fx#1 $

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
	Vertex shader lighting.
	DX8-class hardware required.

******************************************************************************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?BumpReflect0:ReflectNoPixelShader;";
> = 0.8;

//// un-tweakables ////////

float4x4 worldMatrix : World < string UIWidget="None"; >;	// World or Model matrix
float4x4 mvpMatrix : WorldViewProjection < string UIWidget="None"; >;	// Model*View*Projection
float4x4 worldViewMatrix : WorldView < string UIWidget="None"; >;
float4x4 worldViewMatrixI: WorldViewInverse < string UIWidget="None"; >;
float4x4 viewInverseMatrix : ViewInverseTranspose < string UIWidget="None"; >;
float4x4 viewMatrix : View < string UIWidget="None"; >;

float4x4 worldInverse : WorldInverseTranspose;


//// tweakable 

float4 bumpHeight
<
	string UIWidget = "slider";
> = {0.5, -1.0, -1.0, 1.0};

///////

texture normalMap : NORMAL
<
	string ResourceName = "default_bump_normal.dds";
    string ResourceType = "2D";
>;

texture cubeMap : ENVIRONMENT
<
	string ResourceName = "default_reflection.dds";
	string ResourceType = "Cube";
>;

string t0 = "Vertex Shader Diffuse Lighting";

///////////// techniques //////

technique BumpReflect0 <
	string Script = "Pass=p0;";
> {
	pass p0 <
		string Script = "Draw=geometry;";
	> {
		VertexShaderConstant[0] = <worldMatrix>; 		// c0-c3 is ModelView matrix
        VertexShaderConstant[4] = <mvpMatrix>;			// c4-c7 is ModelViewProjection matrix
		VertexShaderConstant[8] = <viewInverseMatrix>;
		//VertexShaderConstant[11] = {1.0,1.0,1.0,1.0};
		VertexShaderConstant[12] = <bumpHeight>;
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
			
			dp3 r1.w, v8, v8		// normalize tangent
			rsq r1.w, r1.w
			mul r1, v8, r1.w		

			dp3 r2.w, v9, v9		// normalize binormal
			rsq r2.w, r2.w
			mul r2, v9, r2.w
			
			//mul r3, r1.zxyw, r2.yzxw	// cross product (i.e. normal)
			//mad r3, r1.yzxw, r2.zxyw, -r3
			mov r3, v3

			dp3 r3.w, r3, r3		// r3 = normal.
			rsq r3.w, r3.w
			mul r3, v3, r3.w

			mul r1, r1, c12.x
			mul r2, r2, c12.x

			dp3 oT1.x, r1, c0
			dp3 oT1.y, r2, c0
			dp3 oT1.z, r3, c0
			
			dp3 oT2.x, r1, c1
			dp3 oT2.y, r2, c1
			dp3 oT2.z, r3, c1
			
			dp3 oT3.x, r1, c2
			dp3 oT3.y, r2, c2
			dp3 oT3.z, r3, c2

			dp3 r5.x, r1, c2
			dp3 r5.y, r2, c2
			dp3 r5.z, r3, c2
			mov r5.w, c13.x

			m4x4 r4, v0, c0			// pos in world space
			sub r4, c11, r4			// r4 = eye vector in world space.
			//mov r4, c13
			//dp3 r4.w, r4, r4		
			//rsq r4.w, r4.w
			//mul r4, r4, r4.w
			
			mov oT1.w, r4.x
			mov oT2.w, r4.y
			mov oT3.w, r4.z
			
			mov oT0, v7	
			mov oD0, r5	
        };
		
		Zenable = true;
		ZWriteEnable = true;
		CullMode = None;
		NormalizeNormals = true;
		
		Texture[0] = <normalMap>;
		MinFilter[0] = Linear;
		MagFilter[0] = Linear;
		MipFilter[0] = Linear;

		Texture[3] = <cubeMap>;
		MinFilter[3] = Linear;
		MagFilter[3] = Linear;
		MipFilter[3] = Linear;

		ColorOp[0] = Modulate;		// Here for now to force GL to use D3D defaults.
		ColorArg1[0] = Texture;
		ColorArg2[0] = Current;
		AlphaOp[0] = SelectArg1;
		AlphaArg1[0] = Texture;
		AlphaArg2[0] = Current;

		ColorOp[1] = Disable;
		AlphaOp[1] = Disable;

		PixelShader = 
		asm
		{
			ps.1.1
			tex t0
			texm3x3pad t1, t0_bx2
			texm3x3pad t2, t0_bx2		
			texm3x3vspec t3, t0_bx2
			mov r0, t3
		};
		
	}
}

//////////////////////////////////////////

technique ReflectNoPixelShader <
	string Script = "Pass=p0; Pass=p1; Pass=p2;";
> {
    pass p0 <
		string Script = "Draw=geometry;";
    > {
		Zenable = true;
		ZWriteEnable = true;
		CullMode = None;
		NormalizeNormals = true;
		LocalViewer = true;

        TextureFactor = 0x008080ff; // 008080FF == 0,0,1

        TexCoordIndex[ 1 ] = 1 | CameraSpaceReflectionVector;

        TextureTransform[ 1 ] = <worldViewMatrix>;

        TextureTransformFlags[1] = Count3;

		Texture[1] = <cubeMap>;
		MinFilter[1] = Linear;
		MagFilter[1] = Linear;
		MipFilter[1] = Point;

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

        TextureFactor = 0x00ff0080;

        TextureTransform[ 1 ] = <worldViewMatrixI>;
        TexCoordIndex[ 1 ] = 1 | CameraSpaceReflectionVector;

        TextureTransformFlags[1] = Count3;

		Texture[1] = <cubeMap>;
		MinFilter[1] = Linear;
		MagFilter[1] = Linear;
		MipFilter[1] = Point;

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
		AlphaOp[1] = SelectArg2;
		AlphaArg1[1] = Texture;
		AlphaArg2[1] = Current;

        SrcBlend = One;
        DestBlend = InvSrcColor;
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

        TextureFactor = 0x0000ff80; 

        TextureTransform[ 1 ] = <worldViewMatrixI>;
        TexCoordIndex[ 1 ] = 1 | CameraSpaceNormal;

        TextureTransformFlags[1] = Count3;

		Texture[1] = <cubeMap>;
		MinFilter[1] = Linear;
		MagFilter[1] = Linear;
		MipFilter[1] = Point;

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
		AlphaOp[1] = SelectArg2;
		AlphaArg1[1] = Texture;
		AlphaArg2[1] = Current;

        SrcBlend = One;
        DestBlend = InvSrcColor;
        AlphaBlendEnable = true;
    }
}

///// eof 
