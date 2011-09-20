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
using Microsoft.Xna.Framework;
using Nine.Content.Pipeline.Graphics.Effects.EffectParts;
#endregion

namespace Nine.Content.Pipeline.Graphics.Effects
{
    /// <summary>
    /// Represents a layer in the terrain.
    /// </summary>
    public class TerrainLayerContent
    {
        [ContentSerializer(Optional=true)]
        public virtual ContentReference<Texture2DContent> Alpha { get; set; }

        [ContentSerializer]
        public virtual ContentReference<Texture2DContent> Texture { get; set; }

        [ContentSerializer(Optional = true)]
        public virtual ContentReference<Texture2DContent> NormalMap { get; set; }
    }

    /// <summary>
    /// Content model for terrain.
    /// </summary>
    [DefaultProcessor(typeof(TerrainMaterialProcessor))]
    public class TerrainMaterialContent : BasicLinkedMaterialContent
    {
        public TerrainMaterialContent()
        {
            ShadowEnabled = true;
            SplatterTextureScale = Vector2.One;
            DetailTextureScale = Vector2.One;
            Layers = new List<TerrainLayerContent>();
        }

        [ContentSerializer(Optional = true)]
        public virtual ContentReference<Texture2DContent> DetailTexture { get; set; }

        [DefaultValue("1, 1")]
        [ContentSerializer(Optional = true)]
        public virtual Vector2 DetailTextureScale { get; set; }

        [DefaultValue("1, 1")]
        [ContentSerializer(Optional = true)]
        public virtual Vector2 SplatterTextureScale { get; set; }

        [ContentSerializer]
        public virtual List<TerrainLayerContent> Layers { get; set; }

        internal int SplatEffectPartIndex = -1;
        internal int DetailEffectPartIndex = -1;

        protected override void PreLighting(LinkedEffectContent effect, ContentProcessorContext context)
        {
            SplatEffectPartIndex = effect.EffectParts.Count;
            effect.EffectParts.Add(new SplatterTextureEffectPartContent()
            {
                TextureXEnabled = Layers != null && Layers.Count > 0 && Layers[0].Texture != null && !string.IsNullOrEmpty(Layers[0].Texture.Filename),
                TextureYEnabled = Layers != null && Layers.Count > 1 && Layers[1].Texture != null && !string.IsNullOrEmpty(Layers[1].Texture.Filename),
                TextureZEnabled = Layers != null && Layers.Count > 2 && Layers[2].Texture != null && !string.IsNullOrEmpty(Layers[2].Texture.Filename),
                TextureWEnabled = Layers != null && Layers.Count > 3 && Layers[3].Texture != null && !string.IsNullOrEmpty(Layers[3].Texture.Filename),

                NormalMapXEnabled = Layers != null && Layers.Count > 0 && Layers[0].NormalMap != null && !string.IsNullOrEmpty(Layers[0].NormalMap.Filename),
                NormalMapYEnabled = Layers != null && Layers.Count > 1 && Layers[1].NormalMap != null && !string.IsNullOrEmpty(Layers[1].NormalMap.Filename),
                NormalMapZEnabled = Layers != null && Layers.Count > 2 && Layers[2].NormalMap != null && !string.IsNullOrEmpty(Layers[2].NormalMap.Filename),
                NormalMapWEnabled = Layers != null && Layers.Count > 3 && Layers[3].NormalMap != null && !string.IsNullOrEmpty(Layers[3].NormalMap.Filename),
            });

            if (DetailTexture != null && !string.IsNullOrEmpty(DetailTexture.Filename))
            {
                DetailEffectPartIndex = effect.EffectParts.Count;
                effect.EffectParts.Add(new DetailTextureEffectPartContent());
            }
        }
    }
}
