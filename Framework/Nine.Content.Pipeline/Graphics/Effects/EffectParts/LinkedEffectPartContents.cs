#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
#endregion

namespace Nine.Content.Pipeline.Graphics.Effects.EffectParts
{
    [ContentSerializerRuntimeType("Nine.Graphics.Effects.EffectParts.AmbientLightEffectPart, Nine")]
    public class AmbientLight : LinkedEffectPartContent
    {
        [ContentSerializer(Optional=true)]
        public Vector3 AmbientLightColor { get; set; }

        public AmbientLight()
        {
            AmbientLightColor = Vector3.One * 0.2f;
        }

        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.AmbientLight); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.EffectParts.BasicTextureEffectPart, Nine")]
    public class BasicTexture : LinkedEffectPartContent
    {
        [ContentSerializer(Optional = true)]
        public ContentReference<Texture2DContent> Texture { get; set; }

        public override string Code
        {
            get 
            {
                string code = Encoding.UTF8.GetString(LinkedEffectParts.BasicTexture);
                if (Contains(typeof(ScreenEffect)))
                {
                    return code.Replace("{$SAMPLER}", "BasicSampler");
                }
                return code.Replace("{$SAMPLER}", "Texture");
            }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.EffectParts.LightEffectPart, Nine")]
    public class BeginLight : LinkedEffectPartContent
    {
        [ContentSerializer(Optional = true)]
        public Vector3 DiffuseColor { get; set; }

        [ContentSerializer(Optional = true)]
        public Vector3 EmissiveColor { get; set; }

        [ContentSerializer(Optional = true)]
        public Vector3 SpecularColor { get; set; }

        [ContentSerializer(Optional = true)]
        public float SpecularPower { get; set; }

        public BeginLight()
        {
            DiffuseColor = Vector3.One;
            EmissiveColor = Vector3.Zero;
            SpecularColor = Vector3.One;
            SpecularPower = 32;
        }

        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.BeginLight); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine")]
    public class EndLight : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.EndLight); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.EffectParts.DirectionalLightEffectPart, Nine")]
    public class DirectionalLight : LinkedEffectPartContent
    {
        [ContentSerializer(Optional = true)]
        public Vector3 Direction { get; set; }

        [ContentSerializer(Optional = true)]
        public Vector3 DiffuseColor { get; set; }

        [ContentSerializer(Optional = true)]
        public Vector3 SpecularColor { get; set; }

        public DirectionalLight()
        {
            Direction = new Vector3(0, -0.707107f, -0.707107f);
            DiffuseColor = Vector3.One;
            SpecularColor = Vector3.One;
        }

        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.DirectionalLight); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.EffectParts.PointLightEffectPart, Nine")]
    public class PointLight : LinkedEffectPartContent
    {
        [ContentSerializer(Optional = true)]
        public Vector3 Position { get; set; }

        [ContentSerializer(Optional = true)]
        public Vector3 DiffuseColor { get; set; }

        [ContentSerializer(Optional = true)]
        public Vector3 SpecularColor { get; set; }

        [ContentSerializer(Optional = true)]
        public float Range { get; set; }

        [ContentSerializer(Optional = true)]
        public float Attenuation { get; set; }

        public PointLight()
        {
            DiffuseColor = Vector3.One;
            SpecularColor = Vector3.One;
            Range = 10;
            Attenuation = 1.0f / MathHelper.E;
        }

        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.PointLight); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.EffectParts.SpotLightEffectPart, Nine")]
    public class SpotLight : LinkedEffectPartContent
    {
        [ContentSerializer(Optional = true)]
        public Vector3 Position { get; set; }

        [ContentSerializer(Optional = true)]
        public Vector3 Direction { get; set; }

        [ContentSerializer(Optional = true)]
        public Vector3 DiffuseColor { get; set; }

        [ContentSerializer(Optional = true)]
        public Vector3 SpecularColor { get; set; }

        [ContentSerializer(Optional = true)]
        public float Range { get; set; }

        [ContentSerializer(Optional = true)]
        public float Attenuation { get; set; }

        [ContentSerializer(Optional = true)]
        public float InnerAngle { get; set; }

        [ContentSerializer(Optional = true)]
        public float OuterAngle { get; set; }

        [ContentSerializer(Optional = true)]
        public float Falloff { get; set; }

        public SpotLight()
        {
            Direction = new Vector3(0, -0.707107f, -0.707107f);
            DiffuseColor = Vector3.One;
            SpecularColor = Vector3.One;
            Range = 100;
            Attenuation = 1.0f / MathHelper.E;
            InnerAngle = MathHelper.PiOver4;
            OuterAngle = MathHelper.PiOver2;
            Falloff = 1;
        }

        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.SpotLight); }
        }
    }


    [ContentSerializerRuntimeType("Nine.Graphics.Effects.EffectParts.DualTextureEffectPart, Nine")]
    public class DualTexture : LinkedEffectPartContent
    {
        [ContentSerializer(Optional = true)]
        public ContentReference<Texture2DContent> Texture { get; set; }

        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.DualTexture); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.EffectParts.SplatterTextureEffectPart, Nine")]
    public class SplatterTexture : LinkedEffectPartContent
    {
        [ContentSerializer(Optional = true)]
        public ContentReference<Texture2DContent> TextureX { get; set; }

        [ContentSerializer(Optional = true)]
        public ContentReference<Texture2DContent> TextureY { get; set; }

        [ContentSerializer(Optional = true)]
        public ContentReference<Texture2DContent> TextureZ { get; set; }

        [ContentSerializer(Optional = true)]
        public ContentReference<Texture2DContent> TextureW { get; set; }

        [ContentSerializer(Optional = true)]
        public ContentReference<Texture2DContent> Splatter { get; set; }

        [ContentSerializer(Optional = true)]
        public Vector2 SplatterTextureScale { get; set; }

        public SplatterTexture()
        {
            SplatterTextureScale = Vector2.One;
        }

        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.SplatterTexture); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.EffectParts.FogEffectPart, Nine")]
    public class Fog : LinkedEffectPartContent
    {
        [ContentSerializer(Optional = true)]
        public Vector3 FogColor { get; set; }

        [ContentSerializer(Optional = true)]
        public float FogStart { get; set; }

        [ContentSerializer(Optional = true)]
        public float FogEnd { get; set; }

        public Fog()
        {
            FogColor = Vector3.One;
            FogStart = 1;
            FogEnd = 100;
        }

        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.Fog); }
        }

    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.EffectParts.OverlayTextureEffectPart, Nine")]
    public class OverlayTexture : LinkedEffectPartContent
    {
        [ContentSerializer(Optional = true)]
        public ContentReference<Texture2DContent> Texture { get; set; }

        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.OverlayTexture); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine")]
    public class PixelShaderOutput : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.PixelShaderOutput); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine")]
    public class PositionColor : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.PositionColor); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine")]
    public class PositionColorNormalTexture : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.PositionColorNormalTexture); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine")]
    public class PositionColorTexture : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.PositionColorTexture); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine")]
    public class PositionNormalTexture : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.PositionNormalTexture); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine")]
    public class PositionTexture : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.PositionTexture); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.EffectParts.SkinTransformEffectPart, Nine")]
    public class SkinTransform : LinkedEffectPartContent
    {
        [ContentSerializer(Optional=true)]
        public int MaxBones { get; set; }

        int weightsPerVertex = 4;

        /// <summary>
        /// Gets or sets the number of skinning weights to evaluate for each vertex (1, 2, or 4).
        /// </summary>
        [ContentSerializer(Optional = true)]
        public int WeightsPerVertex
        {
            get { return weightsPerVertex; }

            set
            {
                if ((value != 1) && (value != 2) && (value != 4))
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                weightsPerVertex = value;
            }
        }

        public SkinTransform()
        {
            MaxBones = 59;
        }

        public override string Code
        {
            get 
            { 
                return Encoding.UTF8.GetString(LinkedEffectParts.SkinTransform)
                    .Replace("{$MAXBONES}", MaxBones.ToString())
                    .Replace("{$BONECOUNT}", WeightsPerVertex.ToString()); 
            }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.EffectParts.TextureTransform, Nine")]
    public class TextureTransform : LinkedEffectPartContent
    {
        [ContentSerializer(Optional = true)]
        public Matrix Transform { get; set; }

        public TextureTransform()
        {
            Transform = Matrix.Identity;
        }

        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.TextureTransform); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.EffectParts.ShadowMapEffectPart, Nine")]
    public class ShadowMap : LinkedEffectPartContent
    {
        [ContentSerializer(Optional = true)]
        public float ShadowIntensity { get; set; }

        [ContentSerializer(Optional = true)]
        public float DepthBias { get; set; }

        public ShadowMap()
        {
            ShadowIntensity = 0.5f;
            DepthBias = 0.0005f;
        }
        
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.ShadowMap); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.EffectParts.NormalMapEffectPart, Nine")]
    public class NormalMap : LinkedEffectPartContent
    {
        [ContentSerializer(Optional = true)]
        public ContentReference<Texture2DContent> Texture { get; set; }

        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.NormalMap); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine")]
    public class VertexShaderOutput : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.VertexShaderOutput); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.EffectParts.VertexTransformEffectPart, Nine")]
    public class VertexTransform : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.VertexTransform); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine")]
    public class ScreenEffect : LinkedEffectPartContent
    {
        protected internal override void Validate(ContentProcessorContext context)
        {
            if (EffectParts[0] != this)
                throw new InvalidContentException("ScreenEffect must be placed first.");
        }

        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.ScreenEffect); }
        }
    }
    
    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine")]
    public class RadicalBlur : LinkedEffectPartContent
    {
        public override string Code
        {
            get
            {
                string code = Encoding.UTF8.GetString(LinkedEffectParts.RadicalBlur);
                if (Contains(typeof(ScreenEffect)))
                {
                    return code.Replace("{$SAMPLER}", "BasicSampler");
                }
                return code.Replace("{$SAMPLER}", "Texture");
            }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.EffectParts.ColorMatrixEffectPart, Nine")]
    public class ColorMatrix : LinkedEffectPartContent
    {
        [ContentSerializer(Optional=true)]
        public Matrix Transform { get; set; }

        public ColorMatrix()
        {
            Transform = Matrix.Identity;
        }

        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.ColorMatrix); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine")]
    public class Threshold : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.Threshold); }
        }
    }
}
