namespace Test
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

    public class CubeStressTest : ITestGame
    {
        public Scene CreateTestScene(GraphicsDevice graphics, ContentManager content)
        {
            var scene = new Scene();
            //var scene = new Scene(new PassThroughSceneManager());
            //var scene = new Scene(new BruteForceSceneManager());
            var cube = content.Load<Microsoft.Xna.Framework.Graphics.Model>("Models/cube");

            scene.Add(new Nine.Graphics.DirectionalLight(graphics) { DiffuseColor = Color.Red.ToVector3(), Direction = Vector3.UnitX });
            scene.Add(new Nine.Graphics.DirectionalLight(graphics) { DiffuseColor = Color.Green.ToVector3(), Direction = Vector3.UnitY });
            scene.Add(new Nine.Graphics.DirectionalLight(graphics) { DiffuseColor = Color.Blue.ToVector3(), Direction = Vector3.UnitZ });

            var size = 20;
            var step = 2;
            for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                    for (var z = 0; z < size; z++)
                    {
                        //scene.Add(new Nine.Graphics.Model(cube)
                        scene.Add(new Box(graphics)
                        {
                            Material = new BasicMaterial(graphics) { LightingEnabled = true },
                            Transform = Matrix.CreateTranslation(x * step, y * step, z * step)
                        });
                    }

            return scene;
        }
    }
}