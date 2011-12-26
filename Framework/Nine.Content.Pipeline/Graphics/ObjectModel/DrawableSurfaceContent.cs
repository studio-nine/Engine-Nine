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
using Nine.Graphics;
using System.Windows.Markup;
#endregion

namespace Nine.Content.Pipeline.Graphics.ObjectModel
{
    [ContentProperty("Material")]
    partial class DrawableSurfaceContent
    {
        [ContentSerializer(Optional = true)]
        public virtual int Width { get; set; }

        [ContentSerializer(Optional = true)]
        public virtual int Height { get; set; }

        [ContentSerializer(Optional = true)]
        public virtual float Step { get; set; }

        [ContentSerializer(Optional = true)]
        public virtual Vector3 Position
        {
            get { return position; }
            set { position = value; Transform = Matrix.CreateTranslation(value); }
        }
        Vector3 position;

        [ContentSerializer(Optional = true)]
        public virtual Vector2 TextureScale
        {
            get { return textureScale; }
            set { textureScale = value; UpdateTextureTransform(); }
        }
        Vector2 textureScale = Vector2.One;

        [ContentSerializer(Optional = true)]
        public virtual Vector2 TextureOffset
        {
            get { return textureOffset; }
            set { textureOffset = value; UpdateTextureTransform(); }
        }
        Vector2 textureOffset = Vector2.Zero;
        
        private void UpdateTextureTransform()
        {
            TextureTransform = Nine.Graphics.TextureTransform.CreateScale(textureScale.X, textureScale.Y) *
                               Nine.Graphics.TextureTransform.CreateTranslation(textureOffset.X, textureOffset.Y);
        }

        [SelfProcess]
        static DrawableSurfaceContent Process(DrawableSurfaceContent input, ContentProcessorContext context)
        {
            // Build a flat heightmap if width & height is specified
            if (input.Heightmap != null && !string.IsNullOrEmpty(input.Heightmap.Filename))
            {
                if (input.Width != 0 && input.Height != 0 && input.Step != 0)
                {
                    context.Logger.LogWarning(null, null,
                        "The width, height and step property will be ignored when a heightmap is specified.");
                }
            }
            else if (input.Width != 0 && input.Height != 0 && input.Step != 0)
            {
                var heightmap = new Heightmap(input.Step, input.Width, input.Height);
                var compiled = context.BuildAsset<object, object>(heightmap, null);
                input.Heightmap = compiled.Filename;
            }
            else
            {
                throw new InvalidContentException(
                    "Either a heightmap or a valid width/height/step pair must be specified for DrawableSurfaceContent.");
            }
            return input;
        }
    }
}
