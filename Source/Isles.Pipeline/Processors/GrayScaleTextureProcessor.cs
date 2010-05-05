#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Isles.Graphics;
#endregion

namespace Isles.Pipeline.Processors
{
    /// <summary>
    /// This processor gray scales the input texture and produces an output texture with 8bit per pixel.
    /// </summary>
    [ContentProcessor(DisplayName="Gray Scale Texture Processor - Isles")]
    public class GrayScaleTextureProcessor : ContentProcessor<TextureContent, TextureContent>
    {
        /// <summary>
        /// Process converts the encoded normals to the NormalizedByte4 format and 
        /// generates mipmaps.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override TextureContent Process(TextureContent input, ContentProcessorContext context)
        {
            Texture2DContent result = new Texture2DContent();

            if (input.Faces[0][0] is PixelBitmapContent<Alpha8>)
                return input;

            input.ConvertBitmapType(typeof(PixelBitmapContent<Vector3>));

            PixelBitmapContent<Vector3> source = input.Faces[0][0] as PixelBitmapContent<Vector3>;
            PixelBitmapContent<Alpha8> bitmap = new PixelBitmapContent<Alpha8>(source.Width, source.Height);

            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    Vector3 src = source.GetPixel(x, y);

                    bitmap.SetPixel(x, y, new Alpha8(
                        0.3f * src.X + 0.59f * src.Y + 0.11f * src.Z));
                }
            }

            result.Mipmaps.Add(bitmap);
            
            return result;
        }
    }
}
