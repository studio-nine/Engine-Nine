namespace Nine.Content.Pipeline.Processors
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
    using Nine.Content.Pipeline.Xaml;

    /// <summary>
    /// Processes texture splatter used for terrain rendering.
    /// </summary>
    [ContentProcessor(DisplayName="Splatter Texture - Engine Nine")]
    public class SplatterTextureProcessor : ContentProcessor<Splatter, Texture2DContent>
    {
        /// <summary>
        /// Gets or sets a value indicating whether a base layer should be generated.
        /// </summary>
        [DefaultValue(true)]
        [Description("Determines if a base layer should be generated automatically if the first layer is not specified.")]
        public bool GenerateBaseLayer { get; set; }

        public SplatterTextureProcessor()
        {
            GenerateBaseLayer = true;
        }

        public override Texture2DContent Process(Splatter splatter, ContentProcessorContext context)
        {
            var input = splatter.Layers.Select(layer => layer != null ? layer.Filename : null).ToArray();
            if (input.Length > 4)
            {
                context.Logger.LogWarning(null, null, "SplatterTextureProcessor supports at most 4 textures. Additional textures will be discarded");
            }

            int width = 0;
            int height = 0;

            Texture2DContent texture = null;

            PixelBitmapContent<float> bitmapR = null;
            PixelBitmapContent<float> bitmapG = null;
            PixelBitmapContent<float> bitmapB = null;
            PixelBitmapContent<float> bitmapA = null;

            if (input.Length > 0 && !string.IsNullOrEmpty(input[0]))
            {
                texture = context.BuildAndLoadAsset<TextureContent, Texture2DContent>(
                    new ExternalReference<TextureContent>(input[0]), null);

                texture.ConvertBitmapType(typeof(PixelBitmapContent<float>));
                bitmapR = (PixelBitmapContent<float>)texture.Mipmaps[0];

                width = bitmapR.Width;
                height = bitmapR.Height;
            }

            if (input.Length > 1 && !string.IsNullOrEmpty(input[1]))
            {
                texture = context.BuildAndLoadAsset<TextureContent, Texture2DContent>(
                    new ExternalReference<TextureContent>(input[1]), null);

                texture.ConvertBitmapType(typeof(PixelBitmapContent<float>));
                bitmapG = (PixelBitmapContent<float>)texture.Mipmaps[0];

                width = bitmapG.Width;
                height = bitmapG.Height;
            }

            if (input.Length > 2 && !string.IsNullOrEmpty(input[2]))
            {
                texture = context.BuildAndLoadAsset<TextureContent, Texture2DContent>(
                    new ExternalReference<TextureContent>(input[2]), null);

                texture.ConvertBitmapType(typeof(PixelBitmapContent<float>));
                bitmapB = (PixelBitmapContent<float>)texture.Mipmaps[0];

                width = bitmapB.Width;
                height = bitmapB.Height;
            }

            if (input.Length > 3 && !string.IsNullOrEmpty(input[3]))
            {
                texture = context.BuildAndLoadAsset<TextureContent, Texture2DContent>(
                    new ExternalReference<TextureContent>(input[3]), null);

                texture.ConvertBitmapType(typeof(PixelBitmapContent<float>));
                bitmapA = (PixelBitmapContent<float>)texture.Mipmaps[0];

                width = bitmapA.Width;
                height = bitmapA.Height;
            }

            // In case no alpha texture is specified.
            width = Math.Max(1, width);
            height = Math.Max(1, height);

            PixelBitmapContent<Vector4> bitmap = new PixelBitmapContent<Vector4>(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var color = new Vector4(
                        bitmapR != null ? bitmapR.GetPixel(x, y) : GenerateBaseLayer ? 1 : 0,
                        bitmapG != null ? bitmapG.GetPixel(x, y) : 0,
                        bitmapB != null ? bitmapB.GetPixel(x, y) : 0,
                        bitmapA != null ? bitmapA.GetPixel(x, y) : 0);

                    color.Z = Math.Min(color.Z, 1 - color.W);
                    color.Y = Math.Min(color.Y, 1 - color.W - color.Z);
                    color.X = Math.Min(color.X, 1 - color.W - color.Z - color.Y);

                    bitmap.SetPixel(x, y, color);
                }
            }

            Texture2DContent result = new Texture2DContent();

            result.Mipmaps = new MipmapChain(bitmap);
            result.ConvertBitmapType(typeof(PixelBitmapContent<Color>));
            
            return result;
        }
    }
}
