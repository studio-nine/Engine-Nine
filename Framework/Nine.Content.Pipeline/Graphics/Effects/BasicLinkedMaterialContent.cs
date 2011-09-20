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
    /// Defines basic building blocks for materials that supports lighting, shadowing and fog.
    /// </summary>
    public abstract class BasicLinkedMaterialContent
    {
        [DefaultValue(true)]
        [ContentSerializer(Optional = true)]
        public virtual bool AmbientLightEnabled { get; set; }

        [DefaultValue(1)]
        [ContentSerializer(Optional = true)]
        public virtual int DirectionalLightCount { get; set; }

        [ContentSerializer(Optional = true)]
        public virtual bool FogEnabled { get; set; }

        [DefaultValue(true)]
        [ContentSerializer(Optional = true)]
        public virtual bool LightingEnabled { get; set; }

        [ContentSerializer(Optional = true)]
        public virtual int PointLightCount { get; set; }

        [ContentSerializer(Optional = true)]
        public virtual bool PreferDeferredLighting { get; set; }

        [ContentSerializer(Optional=true)]
        public virtual bool ShadowEnabled { get; set; }

        [ContentSerializer(Optional = true)]
        public virtual int SpotLightCount { get; set; }

        [ContentSerializer(Optional = true)]
        public virtual bool VertexColorEnabled { get; set; }

        public BasicLinkedMaterialContent()
        {
            AmbientLightEnabled = true;
            LightingEnabled = true;
            DirectionalLightCount = 1;
        }

        public virtual ExternalReference<LinkedEffectContent> Build(ContentProcessorContext context)
        {
            var effect = new LinkedEffectContent();

            if (VertexColorEnabled)
                effect.EffectParts.Add(new PositionColorNormalTextureEffectPartContent());
            else
                effect.EffectParts.Add(new PositionNormalTextureEffectPartContent());

            PreVertexTransform(effect, context);

            effect.EffectParts.Add(new VertexTransformEffectPartContent());
            
            PostVertexTransform(effect, context);

            effect.EffectParts.Add(new VertexShaderOutputEffectPartContent());

            PreLighting(effect, context);

            if (LightingEnabled)
            {
                if (PreferDeferredLighting && context.TargetProfile == GraphicsProfile.HiDef)
                {
                    effect.EffectParts.Add(new DeferredLightsEffectPartContent());
                }
                else
                {
                    effect.EffectParts.Add(new BeginLightEffectPartContent());
                    if (AmbientLightEnabled)
                        effect.EffectParts.Add(new AmbientLightEffectPartContent());
                    for (int i = 0; i < DirectionalLightCount; i++)
                        effect.EffectParts.Add(new DirectionalLightEffectPartContent());
                    for (int i = 0; i < PointLightCount; i++)
                        effect.EffectParts.Add(new PointLightEffectPartContent());
                    for (int i = 0; i < SpotLightCount; i++)
                        effect.EffectParts.Add(new SpotLightEffectPartContent());
                    effect.EffectParts.Add(new EndLightEffectPartContent());
                }
            }

            PostLighting(effect, context);

            if (ShadowEnabled)
                effect.EffectParts.Add(new ShadowMapEffectPartContent());

            if (FogEnabled)
                effect.EffectParts.Add(new FogEffectPartContent());

            effect.EffectParts.Add(new PixelShaderOutputEffectPartContent());

            return LinkedEffectBuilder.Build(effect, context);
        }

        protected virtual void PreVertexTransform(LinkedEffectContent effect, ContentProcessorContext context) { }
        protected virtual void PostVertexTransform(LinkedEffectContent effect, ContentProcessorContext context) { }

        protected virtual void PreLighting(LinkedEffectContent effect, ContentProcessorContext context) { }
        protected virtual void PostLighting(LinkedEffectContent effect, ContentProcessorContext context) { }
    }
}
