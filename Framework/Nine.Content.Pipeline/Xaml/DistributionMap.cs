namespace Nine.Content.Pipeline.Xaml
{
    using System;
    using System.Collections.Generic;
    using System.Xaml;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
    using Nine.Graphics;

    public class DistributionMap
    {
        /// <summary>
        /// Gets or sets the max number of plants per hectare.
        /// </summary>
        public float Density { get; set; }

        /// <summary>
        /// Gets or sets the length of each pixel in meters.
        /// </summary>
        public float Step { get; set; }

        /// <summary>
        /// Gets or sets the max height of the heightmap.
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// Gets or sets the random seed.
        /// </summary>
        public int Seed { get; set; }

        /// <summary>
        /// Gets or sets the distribution texture.
        /// </summary>
        public ExternalReference<Texture2DContent> Texture { get; set; }

        /// <summary>
        /// Gets or sets the heightmap texture.
        /// </summary>
        public ExternalReference<Texture2DContent> Heightmap { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributionMap"/> class.
        /// </summary>
        public DistributionMap()
        {
            Density = 5;
            Step = 1;
            Seed = 198749;
        }

        private void Apply(InstancedModel model)
        {
            if (model == null || Texture == null)
                return;

            var random = new Random(Seed);
            var transforms = new List<Matrix>();
            var texture = (new PipelineBuilder()).BuildAndLoad<Texture2DContent>(Texture.Filename, (string)null, null, null);            
            
            texture.ConvertBitmapType(typeof(PixelBitmapContent<float>));
            
            var map = (PixelBitmapContent<float>)texture.Mipmaps[0];

            for (int z = 0; z < map.Height; z++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    var probability = map.GetPixel(x, z) * Density * 0.0001 * Step * Step;
                    var count = (int)Math.Floor(probability);
                    if (random.NextDouble() < probability - count)
                        count++;

                    for (int i = 0; i < count; i++)
                    {
                        var xx = random.NextDouble();
                        var zz = random.NextDouble();

                        Matrix transform = Matrix.Identity;

                        transform.M41 = (x * Step) + (float)(xx * Step);
                        transform.M43 = (z * Step) + (float)(zz * Step);

                        transforms.Add(transform);
                    }
                }
            }

            model.SetInstanceTransforms(transforms.ToArray());
        }

        public static void SetDistributionMap(InstancedModel model, DistributionMap value)
        {
            if (value != null)
                value.Apply(model);
        }

        public static DistributionMap GetDistributionMap(InstancedModel model)
        {
            return null;
        }
        private static AttachableMemberIdentifier DistributionMapProperty = new AttachableMemberIdentifier(typeof(DistributionMap), "DistributionMap");
    }
}
