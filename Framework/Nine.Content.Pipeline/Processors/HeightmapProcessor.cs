#region Copyright 2009 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 (c) Engine Nine. All Rights Reserved.
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
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Nine.Graphics;
#endregion

namespace Nine.Content.Pipeline.Processors
{
    /// <summary>
    /// Turns a heightmap into a terrain geometry.
    /// </summary>
    [ContentProcessor(DisplayName = "Heightmap Processor - Nine")]
    public class HeightmapProcessor : ContentProcessor<Texture2DContent, Heightmap>
    {
        /// <summary>
        /// Controls the scale of the terrain. This will be the distance between
        /// vertices in the finished terrain mesh.
        /// </summary>
        [DefaultValue(1.0f)]
        [Description("The distance between vertices in the finished terrain mesh.")]
        public float Step { get; set; }


        /// <summary>
        /// Controls the height of the terrain. The heights of the vertices in the
        /// finished mesh will vary between 0 and -Bumpiness.
        /// </summary>
        [DefaultValue(20.0f)]
        [Description("Controls the height of the terrain.")]
        public float Height { get; set; }


        /// <summary>
        /// Creates a new instance fo heightmap processor
        /// </summary>
        public HeightmapProcessor()
        {
            Step = 1.0f;
            Height = 20.0f;
        }


        /// <summary>
        /// Generates a terrain mesh from an input heightfield texture.
        /// </summary>
        public override Heightmap Process(Texture2DContent input, ContentProcessorContext context)
        {
            PixelBitmapContent<Alpha8> heightfield;

            // Convert the input texture to float format, for ease of processing.
            GrayScaleTextureProcessor grayProcessor = new GrayScaleTextureProcessor();
            input = grayProcessor.Process(input, context) as Texture2DContent;

            heightfield = (PixelBitmapContent<Alpha8>)input.Mipmaps[0];

            if (heightfield.Width * heightfield.Height > ushort.MaxValue)
                throw new InvalidContentException("Input texture too large for a heightmap");

            
            // Create the terrain vertices.
            int i = 0;
            int width = heightfield.Width;
            int height = heightfield.Height;

            if (width % 2 == 0)
                width++;
            if (height % 2 == 0)
                height++;

            float[] heightmap = new float[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x < heightfield.Width && y < heightfield.Height)
                        heightmap[i++] = heightfield.GetPixel(x, y).ToAlpha() * Height;
                    else
                        heightmap[i++] = 0;
                }
            }

            return new Heightmap(heightmap, Step, width - 1, height - 1);
        }
    }
}
