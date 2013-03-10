namespace Samples
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine;
    using Nine.Components;
    using Nine.Graphics;
    using Nine.Graphics.Materials;
    using Nine.Graphics.Primitives;
    using Nine.Serialization;

    public class ReallyHugePrimitive : Primitive<VertexPositionNormal>
    {
        public ReallyHugePrimitive(GraphicsDevice graphicsDevice)
            : base(graphicsDevice) 
        {

        }

        protected override void OnBuild()
        {
            var random = new Random(1);
            for (int x = 0; x < 34; x++)
            {
                for (int z = 0; z < 34; z++)
                {
                    var position = new Vector3
                    {
                        X = x * 2 + (float)random.NextDouble() * 2,
                        Y = (float)random.NextDouble(),
                        Z = z * 2 + (float)random.NextDouble() * 2
                    };

                    AddVertex(position, new VertexPositionNormal
                    {
                        Position = position,
                        Normal = Vector3.Up,
                    });
                }
            }
        }
    }

    public class PrimitiveStressTest : Sample
    {
        public override Scene CreateScene(GraphicsDevice graphics, ContentLoader content)
        {
            var scene = new Scene();
            scene.Add(new Nine.Graphics.DirectionalLight(graphics) 
            {
                Direction = Vector3.Down,
                CastShadow = false,
            });

            for (int i = 0; i < 169; i++)
            {
                scene.Add(new ReallyHugePrimitive(graphics)
                {
                    CastShadow = false,
                    Material = new BasicMaterial(graphics) { LightingEnabled = true } 
                });
            }
            return scene;
        }
    }
}
