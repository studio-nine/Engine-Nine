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
    public partial class AmbientLightEffectPartContent : LinkedEffectPartContent
    {
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

    public partial class BeginLightEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.BeginLight); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine")]
    public partial class EndLightEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.EndLight); }
        }
    }

    public partial class DirectionalLightEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.DirectionalLight); }
        }
    }

    public partial class PointLightEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.PointLight); }
        }
    }

    public partial class SpotLightEffectPartContent : LinkedEffectPartContent
    {
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
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.SplatterTexture); }
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

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine")]
    public partial class PixelShaderOutputEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.PixelShaderOutput); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine")]
    public partial class PositionColorEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.PositionColor); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine")]
    public partial class PositionColorNormalTextureEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.PositionColorNormalTexture); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine")]
    public partial class PositionColorTextureEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.PositionColorTexture); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine")]
    public partial class PositionNormalTextureEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.PositionNormalTexture); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine")]
    public partial class PositionTextureEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.PositionTexture); }
        }
    }

    public partial class SkinTransformEffectPartContent : LinkedEffectPartContent
    {
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

    public partial class NormalMapEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.NormalMap); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine")]
    public partial class VertexShaderOutputEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.VertexShaderOutput); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.EffectParts.VertexTransformEffectPart, Nine")]
    public partial class VertexTransformEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.VertexTransform); }
        }
    }

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine")]
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
    
    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine")]
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

    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine")]
    public partial class ThresholdEffectPartContent : LinkedEffectPartContent
    {
        public override string Code
        {
            get { return Encoding.UTF8.GetString(LinkedEffectParts.Threshold); }
        }
    }
}
