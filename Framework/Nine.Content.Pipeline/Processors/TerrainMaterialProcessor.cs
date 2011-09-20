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
    [ContentProcessor(DisplayName="Terrain Material - Engine Nine")]
    public class TerrainMaterialProcessor : ContentProcessor<TerrainMaterialContent, LinkedMaterialContent>
    {
        public override LinkedMaterialContent Process(TerrainMaterialContent input, ContentProcessorContext context)
        {
            if (input.Layers == null || input.Layers.Count <= 0 || input.Layers.Count > 4)
            {
                throw new InvalidContentException("You must specify 1 ~ 4 layers.");
            }

            var material = new LinkedMaterialContent();
            material.DepthAlphaEnabled = false;
            material.IsTransparent = false;
            material.Effect = new ContentReference<CompiledEffectContent>(input.Build(context).Filename);
            material.EffectParts = new LinkedEffectPartContent[input.SplatEffectPartIndex + 2];
            material.EffectParts[input.SplatEffectPartIndex] = new SplatterTextureEffectPartContent
            {
                SplatterTextureScale = input.SplatterTextureScale,
                SplatterTexture = context.BuildAsset<string[], Texture2DContent>(input.Layers.Select(l => l != null ? (l.Alpha != null ? l.Alpha.Filename : null) : null).ToArray(), "SplatterTextureProcessor"),

                TextureX = input.Layers.Count > 0 ? BuildTexture(context, input.Layers[0].Texture) : null,
                TextureY = input.Layers.Count > 1 ? BuildTexture(context, input.Layers[1].Texture) : null,
                TextureZ = input.Layers.Count > 2 ? BuildTexture(context, input.Layers[2].Texture) : null,
                TextureW = input.Layers.Count > 3 ? BuildTexture(context, input.Layers[3].Texture) : null,

                NormalMapX = input.Layers.Count > 0 ? BuildNormalMap(context, input.Layers[0].NormalMap) : null,
                NormalMapY = input.Layers.Count > 1 ? BuildNormalMap(context, input.Layers[1].NormalMap) : null,
                NormalMapZ = input.Layers.Count > 2 ? BuildNormalMap(context, input.Layers[2].NormalMap) : null,
                NormalMapW = input.Layers.Count > 3 ? BuildNormalMap(context, input.Layers[3].NormalMap) : null,
            };
            if (input.DetailEffectPartIndex >= 0)
            {
                material.EffectParts[input.DetailEffectPartIndex] = new DetailTextureEffectPartContent
                {
                    DetailTexture = BuildTexture(context, input.DetailTexture),
                    DetailTextureScale = input.DetailTextureScale,
                };
            }
            return material;
        }

        private ContentReference<Texture2DContent> BuildTexture(ContentProcessorContext context, ContentReference<Texture2DContent> texture)
        {
            if (texture == null || string.IsNullOrEmpty(texture.Filename))
                return null;

            OpaqueDataDictionary param = new OpaqueDataDictionary();
            param.Add("GenerateMipmaps", true);
            return new ContentReference<Texture2DContent>(context.BuildAsset<TextureContent, TextureContent>(
                new ExternalReference<TextureContent>(texture.Filename), "TextureProcessor", param, null, null).Filename);
        }
        
        private ContentReference<Texture2DContent> BuildNormalMap(ContentProcessorContext context, ContentReference<Texture2DContent> texture)
        {
            if (texture == null || string.IsNullOrEmpty(texture.Filename))
                return null;

            OpaqueDataDictionary param = new OpaqueDataDictionary();
            return new ContentReference<Texture2DContent>(context.BuildAsset<TextureContent, TextureContent>(
                new ExternalReference<TextureContent>(texture.Filename), "NormalTextureProcessor", param, null, null).Filename);
        }
    }
}
