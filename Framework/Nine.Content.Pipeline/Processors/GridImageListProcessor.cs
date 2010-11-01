#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Nine.Graphics;
#endregion

namespace Nine.Content.Pipeline.Processors
{
    /// <summary>
    /// Imports grid based image list
    /// </summary>
    [ContentProcessor(DisplayName = "Image List Processor - Nine")]
    public class GridImageListProcessor : ContentProcessor<Texture2DContent, ImageListContent>
    {
        [DefaultValue(1)]
        public int RowCount { get; set; }

        [DefaultValue(1)]
        public int ColumnCount { get; set; }

        [DefaultValue(typeof(Color), "255, 0, 255, 255")]
        public Color ColorKey { get; set; }
        
        [DefaultValue(true)]
        public bool ColorKeyEnabled { get; set; }

        [DefaultValue(false)]
        public bool GenerateMipmaps { get; set; }

        [DefaultValue(true)]
        public bool PremultiplyAlpha { get; set; }

        [DefaultValue(false)]
        public bool ResizeToPowerOfTwo { get; set; }

        [DefaultValue(TextureProcessorOutputFormat.Color)]
        public TextureProcessorOutputFormat TextureFormat { get; set; }


        public GridImageListProcessor()
        {
            RowCount = 1;
            ColumnCount = 1;

            ColorKey = new Color(255, 0, 255, 255);
            ColorKeyEnabled = true;
            GenerateMipmaps = false;
            PremultiplyAlpha = true;
            ResizeToPowerOfTwo = false;
            TextureFormat = TextureProcessorOutputFormat.Color;
        }

        public override ImageListContent Process(Texture2DContent input, ContentProcessorContext context)
        {
            TextureProcessor processor = new TextureProcessor();

            processor.ColorKeyColor = ColorKey;
            processor.ColorKeyEnabled = ColorKeyEnabled;
            processor.GenerateMipmaps = GenerateMipmaps;
            processor.PremultiplyAlpha = PremultiplyAlpha;
            processor.ResizeToPowerOfTwo = ResizeToPowerOfTwo;
            processor.TextureFormat = TextureFormat;

            Texture2DContent texture = (Texture2DContent)processor.Process(input, context);

            ImageListContent result = new ImageListContent();

            result.Textures.Add(texture);

            int width = input.Mipmaps[0].Width;
            int height = input.Mipmaps[0].Height;
                        
            for (int y = 0; y < RowCount; y++)
            {
                for (int x = 0; x < ColumnCount; x++)
                {
                    result.SpriteTextures.Add(0);
                    result.SpriteRectangles.Add(new Rectangle(
                        x * width / ColumnCount, y * height / RowCount,
                        width / ColumnCount, height / RowCount));
                }
            }

            return result;
        }
    }
}
