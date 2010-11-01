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

namespace Nine.Content.Pipeline.Importers
{
    /// <summary>
    /// Imports grayscale raw texture.
    /// </summary>
    [ContentImporter(".raw", DisplayName = "Raw Texture Importer - Nine")]
    public class RawTextureImporter : ContentImporter<TextureContent>
    {
        public int Width { get; set; }
        public int Height { get; set; }


        public override TextureContent Import(string filename, ContentImporterContext context)
        {
            using (FileStream file = new FileStream(filename, FileMode.Open))
            {
                byte[] bytes = new byte[file.Length];

                file.Read(bytes, 0, (int)file.Length);


                // Figure out file size
                if (Width <= 0 || Height <= 0)
                {
                    Width = (int)Math.Sqrt(file.Length);
                    Height = (int)(file.Length / Width);
                }

                if (Width * Height != file.Length)
                {
                    throw new FormatException(
                        "Input texture not a raw grayscale texture, or the specified width and height do not match.");
                }


                // Create texture
                int i = 0;
                PixelBitmapContent<Alpha8> bitmap = new PixelBitmapContent<Alpha8>(Width, Height);

                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                        bitmap.SetPixel(x, y, new Alpha8(1.0f * bytes[i++] / byte.MaxValue));
                 
                Texture2DContent result = new Texture2DContent();

                result.Mipmaps = new MipmapChain(bitmap);

                return result;
            }
        }
    }
}
