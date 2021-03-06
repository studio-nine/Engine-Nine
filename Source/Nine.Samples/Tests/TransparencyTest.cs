namespace Nine.Samples
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

    public class TransparencyTest : Sample
    {
        public override Scene CreateScene(GraphicsDevice graphics, ContentLoader content)
        {
            var scene = new Scene();
            scene.Add(new Nine.Graphics.ModelViewerCamera(graphics) { Center = new Vector3(10, 10, 10) });

            var size = 5;
            var step = 4;
            for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                    for (var z = 0; z < size; z++)
                    {
                        scene.Add(new Box(graphics)
                        {
                            Material = new BasicMaterial(graphics) { Alpha = 0.2f },
                            Transform = Matrix.CreateScale(2) * Matrix.CreateTranslation(x * step, y * step, z * step)
                        });
                    }

            return scene;
        }
    }
}
