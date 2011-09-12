/*********************************************************************NVMH3****
File:  $Id: //sw/devtools/FXComposer1.6/SDK/MEDIA/HLSL/DiffuseBumpHLSL.fx#1 $

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
    Diffuse Per-Pixel Lighting

******************************************************************************/

float Script : STANDARDSGLOBAL <
    string UIWidget = "none";
    string ScriptClass = "object";
    string ScriptOrder = "standard";
    string ScriptOutput = "color";
    string Script = "Technique=Technique?DX9Technique:NoPixelShader;";
> = 0.8;

float4x4 WorldIMatrix : WorldInverse < string UIWidget="None"; >;	// World Inverse or Model Inverse matrix
float4x4 WorldViewProj : WorldViewProjection < string UIWidget="None"; >;	// Model*View*Projection

/////////

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

sampler2D diffuseSampler = sampler_state
{
	Texture = <diffuseTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

sampler2D normalSampler = sampler_state 
{
	Texture = <normalMap>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

/////////////////

float4 BumpHeight
<
> = { 1.0, 1.0, 0.5, 0.0};

float4 LightPos : Position
<
	string Object = "PointLight";
	string Space = "World";
> = { 100.0f, 100.0f, 100.0f, 0.0f };

////////////////////////////////

struct a2v {
	float4 Position : POSITION; //in object space
	float3 Normal : NORMAL; //in object space
	float3 TexCoord : TEXCOORD0;
	float3 T : TANGENT0; //in object space
	float3 B : BINORMAL0; //in object space
};

struct v2f {
	float4 Position : POSITION; //in projection space
	float4 TexCoord0 : TEXCOORD0;
	float4 TexCoord1 : TEXCOORD1;
	float4 LightVector : TEXCOORD2;
};

/////////////////////// vertex shader ///////

v2f DiffuseBumpVS(a2v IN)
{
	v2f OUT;
	OUT.TexCoord0 = float4(IN.TexCoord.xyz, 1);	// diffuse map
	OUT.TexCoord1 = float4(IN.TexCoord.xyz, 1);	// normal map
	// compute the 3x3 tranform from tangent space to object space
	float3x3 objToTangentSpace;
	objToTangentSpace[0] = IN.T;
	objToTangentSpace[1] = IN.B;
	objToTangentSpace[2] = IN.Normal;
	// transform normal from object space to tangent space and pass it as a color
	//OUT.Normal.xyz = 0.5 * mul(IN.Normal, objToTangentSpace) + 0.5.xxx;
	float4 normLightVec = normalize(LightPos);
    float4 objnormLightVec = mul(normLightVec, WorldIMatrix);
	// transform light vector from object space to tangent space and pass it as a color 
	OUT.LightVector.xyz = 0.5 * mul(objnormLightVec.xyz, objToTangentSpace) + 0.5.xxx;
	OUT.LightVector.w = 1.0f;
	// transform position to projection space
	OUT.Position = mul(IN.Position, WorldViewProj);
	return OUT;
}

/////////////////////// pixel shader ///////

float4 DiffuseBumpPS(v2f IN) : COLOR
{
	float4 surfCol = tex2D(diffuseSampler, IN.TexCoord0);
	//fetch bump normal
//	float4 bumpNormal = expand(tex2D(normalSampler, IN.TexCoord1)) * BumpHeight;
	float4 bumpNormal = tex2D(normalSampler, IN.TexCoord1) * BumpHeight;
	//expand iterated light vector to [-1,1]
//	float4 lightVector = expand(passthrough(IN.LightVector));
	float4 lightVector = float4(IN.LightVector.xyz, 1);
	//compute final color (diffuse + ambient)
//	float4 bump = uclamp(dot3_rgba(bumpNormal.xyz,lightVector.xyz));
	float4 bump = bumpNormal * lightVector;
	return (surfCol * bump);
}

////////////////////////// Techniques ///

technique DX9Technique <
	string Script = "Pass=p0;";
> {
	pass p0 <
		string Script = "Draw=geometry;";
	> {
		ZEnable = true;
		ZWriteEnable = true;
		CullMode = None;
		VertexShader = compile vs_1_1 DiffuseBumpVS();
        PixelShader = compile ps_1_1 DiffuseBumpPS();
	}
}

/////////////

technique NoPixelShader <
	string Script = "Pass=p0;";
> {
	pass p0 <
		string Script = "Draw=geometry;";
	> {
		VertexShaderConstant[0] = <WorldIMatrix>; 		// c0-c3 is Model matrix
        VertexShaderConstant[4] = <WorldViewProj>;			// c4-c7 is ModelViewProjection matrix
		VertexShaderConstant[8] = <WorldIMatrix>;
		VertexShaderConstant[12] = <LightPos>;
		VertexShaderConstant[13] = {0.5, 0.5, 0.5, 1.0};
        VertexShaderConstant[14] = <BumpHeight>;

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

            mov r0, c14
            add r0, r0, r0          // *= 2
            mul r0, r1, r0          // *= bump height

            mad oD0, r0, c13, c13   // put in diffuse
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

		ColorOp[0] = DotProduct3;
		AlphaOp[0] = SelectArg1;
		ColorArg1[0] = Texture;
		ColorArg2[0] = Diffuse;
		AlphaArg1[0] = Texture;
		AlphaArg2[0] = Diffuse;

        ColorOp[1] = Modulate;
        ColorArg1[1] = Current;
        ColorArg2[1] = Texture;

        AlphaOp[1] = SelectArg1;
	}
}

////////////////////////// eof ///

