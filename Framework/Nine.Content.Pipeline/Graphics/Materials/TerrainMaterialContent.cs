#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Markup;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Nine.Content.Pipeline.Design;
using Nine.Content.Pipeline.Processors;
#endregion

namespace Nine.Content.Pipeline.Graphics.Effects
{
    /// <summary>
    /// Represents a layer in the terrain.
    /// </summary>
    public class TerrainLayerContent
    {
        [ContentSerializer(Optional=true)]
        [TypeConverter(typeof(ContentReferenceConverter))]
        public virtual ExternalReference<Texture2DContent> Alpha { get; set; }

        [ContentSerializer]
        [TypeConverter(typeof(ContentReferenceConverter))]
        public virtual ContentReference<Texture2DContent> Texture { get; set; }

        [ContentSerializer(Optional = true)]
        [TypeConverter(typeof(ContentReferenceConverter))]
        public virtual ContentReference<Texture2DContent> NormalMap { get; set; }

        [DefaultValue("1, 1, 1")]
        [ContentSerializer(Optional = true)]
        public virtual Vector3 DiffuseColor { get; set; }

        [ContentSerializer(Optional = true)]
        public virtual Vector3 SpecularColor { get; set; }

        [DefaultValue("16")]
        [ContentSerializer(Optional = true)]
        public virtual float SpecularPower { get; set; }

        public TerrainLayerContent()
        {
            DiffuseColor = Vector3.One;
            SpecularPower = 16;
        }
    }

    /// <summary>
    /// Content model for terrain.
    /// </summary>
    [ContentProperty("Layers")]
    [DefaultContentProcessor(typeof(TerrainMaterialProcessor))]
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
        [TypeConverter(typeof(ContentReferenceConverter))]
        public virtual ContentReference<Texture2DContent> DetailTexture { get; set; }

        [DefaultValue("1, 1")]
        [ContentSerializer(Optional = true)]
        public virtual Vector2 DetailTextureScale { get; set; }

        [DefaultValue("1, 1")]
        [ContentSerializer(Optional = true)]
        public virtual Vector2 SplatterTextureScale { get; set; }

        [ContentSerializer]
        public virtual List<TerrainLayerContent> Layers { get; set; }
    }
}
