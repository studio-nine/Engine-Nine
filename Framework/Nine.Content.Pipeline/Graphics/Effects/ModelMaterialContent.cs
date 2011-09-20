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
    /// Defines an effect for models that supports normal mapping, shadow mapping, etc.
    /// </summary>
    [DefaultProcessor(typeof(ModelMaterialProcessor))]
    public class ModelMaterialContent : BasicLinkedMaterialContent
    {
        [ContentSerializer(Optional = true)]
        public virtual bool DepthAlphaEnabled { get; set; }

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

        [ContentSerializer(Optional = true)]
        public virtual bool SpecularMappingEnabled { get; set; }

        [DefaultValue(true)]
        [ContentSerializer(Optional = true)]
        public virtual bool TextureEnabled { get; set; }

        public ModelMaterialContent()
        {
            TextureEnabled = true;
        }

        protected override void PreVertexTransform(LinkedEffectContent effect, ContentProcessorContext context)
        {
            if (SkinningEnabled)
                effect.EffectParts.Add(new SkinTransformEffectPartContent());
        }

        protected override void PreLighting(LinkedEffectContent effect, ContentProcessorContext context)
        {
            if (NormalMappingEnabled)
                effect.EffectParts.Add(new NormalMapEffectPartContent());
            if (TextureEnabled)
                effect.EffectParts.Add(new BasicTextureEffectPartContent());
            if (SpecularMappingEnabled)
                effect.EffectParts.Add(new SpecularMapEffectPartContent());
            if (EmissiveMappingEnabled)
                effect.EffectParts.Add(new EmissiveMapEffectPartContent());
        }
    }
}
