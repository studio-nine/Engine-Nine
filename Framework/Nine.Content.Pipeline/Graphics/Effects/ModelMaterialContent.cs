#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Nine.Content.Pipeline.Processors;
using Nine.Content.Pipeline.Graphics.Effects.EffectParts;
#endregion

namespace Nine.Content.Pipeline.Graphics.Effects
{
    /// <summary>
    /// Determines how the alpha channel of the diffuse texture will be used.
    /// </summary>
    public enum TextureAlphaUsage
    {
        /// <summary>
        /// The alpha channel will determine the opaque of the object.
        /// </summary>
        Opaque,

        /// <summary>
        /// The alpha channel will be replaced with the specified team color.
        /// </summary>
        TeamColor,

        /// <summary>
        /// The alpha channle will be used as the specular map.
        /// </summary>
        SpecularColor,
    }

    /// <summary>
    /// Defines an effect for models that supports normal mapping, shadow mapping, etc.
    /// </summary>
    [DefaultProcessor(typeof(ModelMaterialProcessor))]
    public class ModelMaterialContent : BasicLinkedMaterialContent
    {
        [DefaultValue("1")]
        [ContentSerializer(Optional = true)]
        public virtual float Alpha { get; set; }

        [ContentSerializer(Optional = true)]
        public virtual bool DepthAlphaEnabled { get; set; }

        [DefaultValue("1, 1, 1")]
        [ContentSerializer(Optional = true)]
        public virtual Vector3 DiffuseColor { get; set; }

        [DefaultValue("0, 0, 0")]
        [ContentSerializer(Optional = true)]
        public virtual Vector3 EmissiveColor { get; set; }

        [ContentSerializer(Optional = true)]
        public virtual bool EmissiveMappingEnabled { get; set; }

        [ContentSerializer(Optional = true)]
        public virtual bool IsTransparent { get; set; }

        [ContentSerializer(Optional = true)]
        public virtual bool NormalMappingEnabled { get; set; }

        [ContentSerializer(Optional = true)]
        public virtual bool ParallaxMappingEnabled { get; set; }

        [ContentSerializer(Optional = true)]
        public virtual bool SkinningEnabled { get; set; }

        [DefaultValue("0, 0, 0")]
        [ContentSerializer(Optional = true)]
        public virtual Vector3 SpecularColor { get; set; }

        [ContentSerializer(Optional = true)]
        public virtual bool SpecularMappingEnabled { get; set; }

        [DefaultValue(16)]
        [ContentSerializer(Optional = true)]
        public virtual float SpecularPower { get; set; }

        [DefaultValue(true)]
        [ContentSerializer(Optional = true)]
        public virtual bool TextureEnabled { get; set; }

        [ContentSerializer(Optional = true)]
        public virtual TextureAlphaUsage TextureAlphaUsage { get; set; }

        [DefaultValue("1, 1, 1")]
        [ContentSerializer(Optional = true)]
        public virtual Vector3 TeamColor { get; set; }

        public ModelMaterialContent()
        {
            TextureEnabled = true;
            TeamColor = Vector3.One;
            DiffuseColor = Vector3.One;
            SpecularPower = 16;
            Alpha = 1;
        }
    }
}
