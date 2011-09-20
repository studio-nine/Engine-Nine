#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Linq;
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
    public partial class AmbientLightEffectPartContent : LinkedEffectPartContent
    {
        protected internal override void Validate(ContentProcessorContext context)
        {
            var i = EffectParts.IndexOf(this);
            var begin = EffectParts.IndexOf(EffectParts.FirstOrDefault(part => part is BeginLightEffectPartContent));
            var end = EffectParts.IndexOf(EffectParts.FirstOrDefault(part => part is EndLightEffectPartContent));
            if (begin < 0 || end < 0 || i < begin || i > end)
            {
                throw new InvalidContentException("AmbientLightEffectPartContent must be added between BeginLightEffectPartContent and EndLightEffectPartContent.");
            }
        }

        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.AmbientLight); }
        }
    }

    public partial class BasicTextureEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get 
            {
                string code = Encoding.UTF8.GetString(LinkedEffectParts.BasicTexture);
                if (Contains(typeof(ScreenEffectEffectPartContent)))
                {
                    return code.Replace("{$SAMPLER}", "BasicSampler");
                }
                return code.Replace("{$SAMPLER}", "Texture");
            }
        }
    }

    public partial class DetailTextureEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.DetailTexture); }
        }
    }

    public partial class DeferredLightsEffectPartContent : LinkedEffectPartContent
    {
        protected internal override void Validate(ContentProcessorContext context)
        {
            if (EffectParts.OfType<DeferredLightsEffectPartContent>().Count() != 1)
                throw new InvalidContentException("Only 1 DeferredLightsEffectPartContent can be specified.");

            if (EffectParts.Any(part => part is BeginLightEffectPartContent))
                throw new InvalidContentException("Deferred lights and forward lights cannot be used together.");
        }

        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.DeferredLights); }
        }
    }

    public partial class BeginLightEffectPartContent : LinkedEffectPartContent
    {
        protected internal override void Validate(ContentProcessorContext context)
        {
            if (EffectParts.OfType<BeginLightEffectPartContent>().Count() != 1)
                throw new InvalidContentException("Only 1 BeginLightEffectPartContent can be specified.");

            if (!EffectParts.Any(part => part is EndLightEffectPartContent))
                throw new InvalidContentException("BeginLightEffectPartContent and EndLightEffectPartContent must be used together in pairs.");
        }

        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.BeginLight); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine.Graphics")]
    public partial class EndLightEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.EndLight); }
        }
    }

    public partial class DirectionalLightEffectPartContent : LinkedEffectPartContent
    {
        protected internal override void Validate(ContentProcessorContext context)
        {
            var i = EffectParts.IndexOf(this);
            var begin = EffectParts.IndexOf(EffectParts.FirstOrDefault(part => part is BeginLightEffectPartContent));
            var end = EffectParts.IndexOf(EffectParts.FirstOrDefault(part => part is EndLightEffectPartContent));
            if (begin < 0 || end < 0 || i < begin || i > end)
            {
                throw new InvalidContentException("DirectionalLightEffectPartContent must be added between BeginLightEffectPartContent and EndLightEffectPartContent.");
            }
        }

        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.DirectionalLight); }
        }
    }

    public partial class PointLightEffectPartContent : LinkedEffectPartContent
    {
        protected internal override void Validate(ContentProcessorContext context)
        {
            var i = EffectParts.IndexOf(this);
            var begin = EffectParts.IndexOf(EffectParts.FirstOrDefault(part => part is BeginLightEffectPartContent));
            var end = EffectParts.IndexOf(EffectParts.FirstOrDefault(part => part is EndLightEffectPartContent));
            if (begin < 0 || end < 0 || i < begin || i > end)
            {
                throw new InvalidContentException("PointLightEffectPartContent must be added between BeginLightEffectPartContent and EndLightEffectPartContent.");
            }
        }

        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.PointLight); }
        }
    }

    public partial class SpotLightEffectPartContent : LinkedEffectPartContent
    {
        protected internal override void Validate(ContentProcessorContext context)
        {
            var i = EffectParts.IndexOf(this);
            var begin = EffectParts.IndexOf(EffectParts.FirstOrDefault(part => part is BeginLightEffectPartContent));
            var end = EffectParts.IndexOf(EffectParts.FirstOrDefault(part => part is EndLightEffectPartContent));
            if (begin < 0 || end < 0 || i < begin || i > end)
            {
                throw new InvalidContentException("SpotLightEffectPartContent must be added between BeginLightEffectPartContent and EndLightEffectPartContent.");
            }
        }

        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.SpotLight); }
        }
    }


    public partial class DualTextureEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.DualTexture); }
        }
    }

    public partial class SplatterTextureEffectPartContent : LinkedEffectPartContent
    {
        [ContentSerializer(Optional=true)]
        public bool TextureXEnabled { get; set; }

        [ContentSerializer(Optional = true)]
        public bool TextureYEnabled { get; set; }

        [ContentSerializer(Optional = true)]
        public bool TextureZEnabled { get; set; }

        [ContentSerializer(Optional = true)]
        public bool TextureWEnabled { get; set; }

        [ContentSerializer(Optional = true)]
        public bool NormalMapXEnabled { get; set; }

        [ContentSerializer(Optional = true)]
        public bool NormalMapYEnabled { get; set; }

        [ContentSerializer(Optional = true)]
        public bool NormalMapZEnabled { get; set; }

        [ContentSerializer(Optional = true)]
        public bool NormalMapWEnabled { get; set; }

        partial void OnCreate()
        {
            TextureXEnabled = true;
            TextureYEnabled = true;
            TextureZEnabled = true;
            TextureWEnabled = true;
        }
        
        public override string Code
        {
            get 
            {
                bool normalMapped = NormalMapXEnabled || NormalMapYEnabled || NormalMapZEnabled || NormalMapWEnabled;
                return Encoding.UTF8.GetString(normalMapped ? LinkedEffectParts.NormalMappedSplatterTexture : LinkedEffectParts.SplatterTexture)
                               .Replace("{$HASX}", TextureXEnabled ? "" : "//")
                               .Replace("{$HASY}", TextureYEnabled ? "" : "//")
                               .Replace("{$HASZ}", TextureZEnabled ? "" : "//")
                               .Replace("{$HASW}", TextureWEnabled ? "" : "//")
                               .Replace("{$HASNORMALX}", NormalMapXEnabled ? "" : "//")
                               .Replace("{$HASNORMALY}", NormalMapYEnabled ? "" : "//")
                               .Replace("{$HASNORMALZ}", NormalMapZEnabled ? "" : "//")
                               .Replace("{$HASNORMALW}", NormalMapWEnabled ? "" : "//");
            }
        }
    }

    public partial class FogEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.Fog); }
        }

    }

    public partial class OverlayTextureEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.OverlayTexture); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine.Graphics")]
    public partial class PixelShaderOutputEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.PixelShaderOutput); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine.Graphics")]
    public partial class PositionColorEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.PositionColor); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine.Graphics")]
    public partial class PositionColorNormalTextureEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.PositionColorNormalTexture); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine.Graphics")]
    public partial class PositionColorTextureEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.PositionColorTexture); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine.Graphics")]
    public partial class PositionNormalTextureEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.PositionNormalTexture); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine.Graphics")]
    public partial class PositionTextureEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.PositionTexture); }
        }
    }

    public partial class SkinTransformEffectPartContent : LinkedEffectPartContent
    {
        protected internal override void Validate(ContentProcessorContext context)
        {
            var i = EffectParts.IndexOf(this);
            var vertexTransform = EffectParts.IndexOf(EffectParts.FirstOrDefault(part => part is VertexTransformEffectPartContent));
            if (i >= vertexTransform)
            {
                throw new InvalidContentException("SkinTransformEffectPartContent must be added before VertexTransformEffectPartContent.");
            }
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

    public partial class TextureTransformEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.TextureTransform); }
        }
    }

    public partial class ShadowMapEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.ShadowMap); }
        }
    }

    public partial class EmissiveMapEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.EmissiveMap); }
        }
    }

    public partial class SpecularMapEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.SpecularMap); }
        }
    }

    public partial class NormalMapEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get
            {
                bool isSkinned = EffectParts.Any(p => p is SkinTransformEffectPartContent);
                return Encoding.UTF8.GetString(LinkedEffectParts.NormalMap)
                                    .Replace("{$SKINNED}", isSkinned ? "" : "//")
                                    .Replace("{$SKINNEDIMPORT}", isSkinned ? "import" : "//");
            }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine.Graphics")]
    public partial class VertexShaderOutputEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.VertexShaderOutput); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.EffectParts.VertexTransformEffectPart, Nine.Graphics")]
    public partial class VertexTransformEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.VertexTransform); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine.Graphics")]
    public partial class ScreenEffectEffectPartContent : LinkedEffectPartContent
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
    
    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine.Graphics")]
    public partial class RadicalBlurEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get
            {
                string code = Encoding.UTF8.GetString(LinkedEffectParts.RadicalBlur);
                if (Contains(typeof(ScreenEffectEffectPartContent)))
                {
                    return code.Replace("{$SAMPLER}", "BasicSampler");
                }
                return code.Replace("{$SAMPLER}", "Texture");
            }
        }
    }

    public partial class ColorMatrixEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.ColorMatrix); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine.Graphics")]
    public partial class ThresholdEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.Threshold); }
        }
    }
}
