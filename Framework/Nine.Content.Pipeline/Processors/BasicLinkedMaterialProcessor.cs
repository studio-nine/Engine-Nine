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
    public abstract class BasicLinkedMaterialProcessor<T> : ContentProcessor<T, LinkedMaterialContent> where T : BasicLinkedMaterialContent
    {
        public override LinkedMaterialContent Process(T input, ContentProcessorContext context)
        {
            var material = new LinkedMaterialContent();
            var effect = new LinkedEffectContent();

            if (input.VertexColorEnabled)
                effect.EffectParts.Add(new PositionColorNormalTextureEffectPartContent());
            else
                effect.EffectParts.Add(new PositionNormalTextureEffectPartContent());

            Flush(effect, material);
            PreVertexTransform(input, effect, material, context);

            effect.EffectParts.Add(new VertexTransformEffectPartContent());

            Flush(effect, material);
            PostVertexTransform(input, effect, material, context);

            effect.EffectParts.Add(new VertexShaderOutputEffectPartContent());

            Flush(effect, material);
            PreLighting(input, effect, material, context);

            if (input.LightingEnabled)
            {
                if (input.PreferDeferredLighting && context.TargetProfile == GraphicsProfile.HiDef)
                {
                    effect.EffectParts.Add(new DeferredLightsEffectPartContent());
                }
                else
                {
                    effect.EffectParts.Add(new BeginLightEffectPartContent());
                    if (input.AmbientLightEnabled)
                        effect.EffectParts.Add(new AmbientLightEffectPartContent());
                    for (int i = 0; i < input.DirectionalLightCount; i++)
                        effect.EffectParts.Add(new DirectionalLightEffectPartContent());
                    for (int i = 0; i < input.PointLightCount; i++)
                        effect.EffectParts.Add(new PointLightEffectPartContent());
                    for (int i = 0; i < input.SpotLightCount; i++)
                        effect.EffectParts.Add(new SpotLightEffectPartContent());
                    effect.EffectParts.Add(new EndLightEffectPartContent());
                }
            }

            Flush(effect, material);
            PostLighting(input, effect, material, context);

            if (input.ShadowEnabled)
                effect.EffectParts.Add(new ShadowMapEffectPartContent());

            if (input.FogEnabled)
                effect.EffectParts.Add(new FogEffectPartContent());

            effect.EffectParts.Add(new PixelShaderOutputEffectPartContent());

            material.Effect = new ContentReference<CompiledEffectContent>(LinkedEffectBuilder.Build(effect, context).Filename);
            return material;
        }

        protected void Flush(LinkedEffectContent effect, LinkedMaterialContent material)
        {
            if (material.EffectParts.Count > effect.EffectParts.Count)
                throw new InvalidOperationException();

            while (material.EffectParts.Count < effect.EffectParts.Count)
                material.EffectParts.Add(null);
        }

        protected virtual void PreVertexTransform(T input, LinkedEffectContent effect, LinkedMaterialContent material, ContentProcessorContext context) { }
        protected virtual void PostVertexTransform(T input, LinkedEffectContent effect, LinkedMaterialContent material, ContentProcessorContext context) { }

        protected virtual void PreLighting(T input, LinkedEffectContent effect, LinkedMaterialContent material, ContentProcessorContext context) { }
        protected virtual void PostLighting(T input, LinkedEffectContent effect, LinkedMaterialContent material, ContentProcessorContext context) { }
    }
}
