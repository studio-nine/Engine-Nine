namespace Nine.Serialization.Importers
{
    using System;
    using System.IO;
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
    
    /// <summary>
    /// Imports grayscale raw texture.
    /// </summary>
    [ContentImporter(".raw", DisplayName = "Raw Texture Importer - Engine Nine")]
    public class RawTextureImporter : ContentImporter<TextureContent>
    {
        public int Width { get; set; }
        public int Height { get; set; }


        public override TextureContent Import(string filename, ContentImporterContext context)
        {
            using (Stream file =
#if PCL
                Microsoft.Xna.Framework.TitleContainer.OpenStream(filename))
#else
                new FileStream(filename, FileMode.Open)) // TODO: Can we use TitleContainer?
#endif
            {
                byte[] bytes = new byte[file.Length];

                file.Read(bytes, 0, (int)file.Length);


                bool bit16 = false;

                // Figure out file size
                if (Width <= 0 || Height <= 0)
                {
                    Width = (int)Math.Sqrt(file.Length);
                    Height = (int)(file.Length / Width);

                    if (Width * Height != file.Length)
                    {
                        Width = (int)Math.Sqrt(file.Length / 2);
                        Height = (int)(file.Length / 2 / Width);

                        bit16 = true;
                    }

                    if (Width * Height * 2 == file.Length)
                    {
                        context.Logger.LogWarning(null, new ContentIdentity(filename),
                            "Input texture not a raw grayscale texture, or the specified width and height do not match.");
                    }
                }

                // Create texture
                int i = 0;
                PixelBitmapContent<float> bitmap = new PixelBitmapContent<float>(Width, Height);

                for (int y = 0; y < Height; ++y)
                {
                    for (int x = 0; x < Width; ++x)
                    {
                        if (bit16)
                        {
                            bitmap.SetPixel(x, y, 1.0f * (ushort)((bytes[i++] | bytes[i++] << 8)) / ushort.MaxValue);
                        }
                        else
                        {
                            bitmap.SetPixel(x, y, 1.0f * bytes[i++] / byte.MaxValue);
                        }
                    }
                }
                 
                Texture2DContent result = new Texture2DContent();

                result.Mipmaps = new MipmapChain(bitmap);

                return result;
            }
        }
    }
}
