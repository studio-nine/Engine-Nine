namespace Nine.Content.Pipeline.Processors
{
    using System;
    using System.ComponentModel;
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
    using Nine.Graphics;

    /// <summary>
    /// Turns a heightmap into a terrain geometry.
    /// </summary>
    [ContentProcessor(DisplayName = "Heightmap - Engine Nine")]
    public class HeightmapProcessor : ContentProcessor<Texture2DContent, Heightmap>
    {
        /// <summary>
        /// Controls the scale of the terrain. This will be the distance between
        /// vertices in the finished terrain mesh.
        /// </summary>
        public float Step { get; set; }


        /// <summary>
        /// Controls the height of the terrain. The heights of the vertices in the
        /// finished mesh will vary between 0 and -Bumpiness.
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// Determines whether the positions will be flipped on X axis.
        /// </summary>
        public bool FlipX { get; set; }

        /// <summary>
        /// Determines whether the positions will be flipped on X axis.
        /// </summary>
        public bool FlipY { get; set; }


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
            PixelBitmapContent<float> heightfield;

            // Convert the input texture to float format, for ease of processing.
            input.ConvertBitmapType(typeof(PixelBitmapContent<float>));

            heightfield = (PixelBitmapContent<float>)input.Mipmaps[0];

            // Create the terrain vertices.
            int i = 0;
            int width = heightfield.Width;
            int height = heightfield.Height;

            if (width % 2 == 0)
                width++;
            if (height % 2 == 0)
                height++;

            float[] heightmap = new float[width * height];

            for (int y = 0; y < height; ++y)
            {
                var yy = FlipY ? (height - y - 1) : y;

                for (int x = 0; x < width; ++x)
                {
                    var xx = FlipX ? (width - x - 1) : x;

                    if (xx < heightfield.Width && yy < heightfield.Height)
                        heightmap[i++] = heightfield.GetPixel(xx, yy) * Height;
                    else
                        heightmap[i++] = heightfield.GetPixel(Math.Min(xx, heightfield.Width - 1),
                                                              Math.Min(yy, heightfield.Height - 1)) * Height;
                }
            }

            return new Heightmap(heightmap, Step, width - 1, height - 1);
        }
    }
}
