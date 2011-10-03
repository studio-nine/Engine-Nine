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
    public class TerrainMaterialProcessor : BasicLinkedMaterialProcessor<TerrainMaterialContent>
    {
        public override LinkedMaterialContent Process(TerrainMaterialContent input, ContentProcessorContext context)
        {
            if (input.Layers == null || input.Layers.Count <= 0 || input.Layers.Count > 4)
            {
                throw new InvalidContentException("You must specify 1 ~ 4 layers.");
            }
            return base.Process(input, context);
        }

        protected override void PreLighting(TerrainMaterialContent input, LinkedEffectContent effect, LinkedMaterialContent material, ContentProcessorContext context)
        {
            effect.EffectParts.Add(new SplatterTextureEffectPartContent()
            {
                TextureXEnabled = input.Layers != null && input.Layers.Count > 0 && input.Layers[0].Texture != null && !string.IsNullOrEmpty(input.Layers[0].Texture.Filename),
                TextureYEnabled = input.Layers != null && input.Layers.Count > 1 && input.Layers[1].Texture != null && !string.IsNullOrEmpty(input.Layers[1].Texture.Filename),
                TextureZEnabled = input.Layers != null && input.Layers.Count > 2 && input.Layers[2].Texture != null && !string.IsNullOrEmpty(input.Layers[2].Texture.Filename),
                TextureWEnabled = input.Layers != null && input.Layers.Count > 3 && input.Layers[3].Texture != null && !string.IsNullOrEmpty(input.Layers[3].Texture.Filename),

                NormalMapXEnabled = input.Layers != null && input.Layers.Count > 0 && input.Layers[0].NormalMap != null && !string.IsNullOrEmpty(input.Layers[0].NormalMap.Filename),
                NormalMapYEnabled = input.Layers != null && input.Layers.Count > 1 && input.Layers[1].NormalMap != null && !string.IsNullOrEmpty(input.Layers[1].NormalMap.Filename),
                NormalMapZEnabled = input.Layers != null && input.Layers.Count > 2 && input.Layers[2].NormalMap != null && !string.IsNullOrEmpty(input.Layers[2].NormalMap.Filename),
                NormalMapWEnabled = input.Layers != null && input.Layers.Count > 3 && input.Layers[3].NormalMap != null && !string.IsNullOrEmpty(input.Layers[3].NormalMap.Filename),

                SpecularXEnabled = input.Layers != null && input.Layers.Count > 0 && input.Layers[0].SpecularColor != Vector3.Zero,
                SpecularYEnabled = input.Layers != null && input.Layers.Count > 1 && input.Layers[1].SpecularColor != Vector3.Zero,
                SpecularZEnabled = input.Layers != null && input.Layers.Count > 2 && input.Layers[2].SpecularColor != Vector3.Zero,
                SpecularWEnabled = input.Layers != null && input.Layers.Count > 3 && input.Layers[3].SpecularColor != Vector3.Zero,
            });
            material.EffectParts.Add(new SplatterTextureEffectPartContent
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

                DiffuseColorX = input.Layers.Count > 0 ? input.Layers[0].DiffuseColor : Vector3.One,
                DiffuseColorY = input.Layers.Count > 1 ? input.Layers[1].DiffuseColor : Vector3.One,
                DiffuseColorZ = input.Layers.Count > 2 ? input.Layers[2].DiffuseColor : Vector3.One,
                DiffuseColorW = input.Layers.Count > 3 ? input.Layers[3].DiffuseColor : Vector3.One,

                SpecularColorX = input.Layers.Count > 0 ? input.Layers[0].SpecularColor : Vector3.Zero,
                SpecularColorY = input.Layers.Count > 1 ? input.Layers[1].SpecularColor : Vector3.Zero,
                SpecularColorZ = input.Layers.Count > 2 ? input.Layers[2].SpecularColor : Vector3.Zero,
                SpecularColorW = input.Layers.Count > 3 ? input.Layers[3].SpecularColor : Vector3.Zero,

                SpecularPowerX = input.Layers.Count > 0 ? input.Layers[0].SpecularPower : 0,
                SpecularPowerY = input.Layers.Count > 1 ? input.Layers[1].SpecularPower : 0,
                SpecularPowerZ = input.Layers.Count > 2 ? input.Layers[2].SpecularPower : 0,
                SpecularPowerW = input.Layers.Count > 3 ? input.Layers[3].SpecularPower : 0,
            });

            if (input.DetailTexture != null && !string.IsNullOrEmpty(input.DetailTexture.Filename))
            {
                effect.EffectParts.Add(new DetailTextureEffectPartContent());
                material.EffectParts.Add(new DetailTextureEffectPartContent
                {
                    DetailTexture = BuildTexture(context, input.DetailTexture),
                    DetailTextureScale = input.DetailTextureScale,
                });
            }
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
