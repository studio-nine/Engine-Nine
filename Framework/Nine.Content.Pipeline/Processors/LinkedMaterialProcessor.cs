#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.ComponentModel;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics;
using Nine.Content.Pipeline.Graphics.Effects;
using Nine.Content.Pipeline.Graphics.Effects.EffectParts;
#endregion

namespace Nine.Content.Pipeline.Processors
{
    [DesignTimeVisible(false)]
    [ContentProcessor(DisplayName = "Linked Material - Engine Nine")]
    public class LinkedMaterialProcessor : ContentProcessor<LinkedMaterialContent, LinkedMaterialContent>
    {
        public override LinkedMaterialContent Process(LinkedMaterialContent input, ContentProcessorContext context)
        {
            var effect = new LinkedEffectContent();
            effect.EffectParts.AddRange(input.EffectParts);
            input.Effect = new ContentReference<CompiledEffectContent>(LinkedEffectBuilder.Build(effect, context).Filename);
            return input;
        }
    }

    public abstract class LinkedMaterialProcessor<T> : ContentProcessor<T, LinkedMaterialContent> where T : BasicLinkedMaterialContent
    {
        public override LinkedMaterialContent Process(T input, ContentProcessorContext context)
        {
            var material = new LinkedMaterialContent();

            if (input.VertexColorEnabled)
                material.EffectParts.Add(new PositionColorNormalTextureEffectPartContent());
            else
                material.EffectParts.Add(new PositionNormalTextureEffectPartContent());

            PreVertexTransform(input, material, context);

            material.EffectParts.Add(new VertexTransformEffectPartContent());

            PostVertexTransform(input, material, context);

            material.EffectParts.Add(new VertexShaderOutputEffectPartContent());

            PreLighting(input, material, context);

            if (input.LightingEnabled)
            {
                if (input.PreferDeferredLighting && context.TargetProfile == GraphicsProfile.HiDef)
                {
                    material.EffectParts.Add(new DeferredLightsEffectPartContent());
                }
                else
                {
                    material.EffectParts.Add(new BeginLightEffectPartContent());
                    if (input.AmbientLightEnabled)
                        material.EffectParts.Add(new AmbientLightEffectPartContent());
                    for (int i = 0; i < input.DirectionalLightCount; i++)
                        material.EffectParts.Add(new DirectionalLightEffectPartContent());
                    for (int i = 0; i < input.PointLightCount; i++)
                        material.EffectParts.Add(new PointLightEffectPartContent());
                    for (int i = 0; i < input.SpotLightCount; i++)
                        material.EffectParts.Add(new SpotLightEffectPartContent());
                    material.EffectParts.Add(new EndLightEffectPartContent());
                }
            }

            PostLighting(input, material, context);

            if (input.ShadowEnabled)
                material.EffectParts.Add(new ShadowMapEffectPartContent()
                {
                    SampleCount = input.ShadowMapSampleCount,
                    ShadowColor = input.ShadowColor,
                    DepthBias = input.ShadowBias,
                });

            if (input.FogEnabled)
                material.EffectParts.Add(new FogEffectPartContent());

            material.EffectParts.Add(new PixelShaderOutputEffectPartContent());

            return new LinkedMaterialProcessor().Process(material, context);
        }

        protected virtual void PreVertexTransform(T input, LinkedMaterialContent material, ContentProcessorContext context) { }
        protected virtual void PostVertexTransform(T input, LinkedMaterialContent material, ContentProcessorContext context) { }

        protected virtual void PreLighting(T input, LinkedMaterialContent material, ContentProcessorContext context) { }
        protected virtual void PostLighting(T input, LinkedMaterialContent material, ContentProcessorContext context) { }
    }
}
