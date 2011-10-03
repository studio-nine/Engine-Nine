#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Linq;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Nine.Graphics;
using Nine.Content.Pipeline.Graphics.Effects;
using Nine.Content.Pipeline.Graphics.Effects.EffectParts;
#endregion

namespace Nine.Content.Pipeline.Processors
{
    /// <summary>
    /// This processor generates the appropriate texture and materials for the input terrain.
    /// </summary>
    [DesignTimeVisible(false)]
    [ContentProcessor(DisplayName="Model Material - Engine Nine")]
    public class ModelMaterialProcessor : BasicLinkedMaterialProcessor<ModelMaterialContent>
    {
        public override LinkedMaterialContent Process(ModelMaterialContent input, ContentProcessorContext context)
        {
            var material = base.Process(input, context);
            material.DepthAlphaEnabled = input.DepthAlphaEnabled;
            material.IsTransparent = input.IsTransparent;
            return material;
        }

        protected override void PreVertexTransform(ModelMaterialContent input, LinkedEffectContent effect, LinkedMaterialContent material, ContentProcessorContext context)
        {
            if (input.SkinningEnabled)
                effect.EffectParts.Add(new SkinTransformEffectPartContent());
        }

        protected override void PreLighting(ModelMaterialContent input, LinkedEffectContent effect, LinkedMaterialContent material, ContentProcessorContext context)
        {
            if (input.NormalMappingEnabled)
                effect.EffectParts.Add(new NormalMapEffectPartContent());
            if (input.TextureEnabled)
                effect.EffectParts.Add(new BasicTextureEffectPartContent());
            if (input.SpecularMappingEnabled)
                effect.EffectParts.Add(new SpecularMapEffectPartContent());
            if (input.EmissiveMappingEnabled)
                effect.EffectParts.Add(new EmissiveMapEffectPartContent());
            
            Flush(effect, material);

            effect.EffectParts.Add(new MaterialEffectPartContent());
            material.EffectParts.Add(new MaterialEffectPartContent() 
            {
                Alpha = input.Alpha,
                DiffuseColor = input.DiffuseColor, 
                EmissiveColor = input.EmissiveColor,
                SpecularColor = input.SpecularColor,
                SpecularPower = input.SpecularPower 
            });
        }
    }
}
