#region Copyright 2009 - 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Windows.Markup;
using System.Xaml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Nine.Content.Pipeline.Processors;
using Nine.Graphics.ObjectModel;
#endregion

namespace Nine.Content.Pipeline.Xaml
{
    public class DistributionMap
    {
        public float Density { get; set; }
        public float Step { get; set; }
        public int Seed { get; set; }
        public ExternalReference<Texture2DContent> Texture { get; set; }

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

            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    var count = (int)Math.Round(map.GetPixel(x, y) * Density);
                    for (int i = 0; i < count; i++)
                    {
                        var xx = random.NextDouble();
                        var yy = random.NextDouble();

                        Matrix transform = Matrix.Identity;

                        transform.M41 = (x * Step) + (float)(xx * Step);
                        transform.M42 = (y * Step) + (float)(yy * Step);

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
