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

    public class ___CubeStressTest : ISample
    {
        public Scene CreateScene(GraphicsDevice graphics, ContentLoader content)
        {
            var scene = new Scene();
            //scene = new Scene(new PassThroughSceneManager());
            //scene = new Scene(new BruteForceSceneManager());
            var cube = content.Load<Microsoft.Xna.Framework.Graphics.Model>("Models/cube/cube");

            scene.Add(new Nine.Graphics.DirectionalLight(graphics) { DiffuseColor = Vector3.UnitX, Direction = -Vector3.UnitX });
            scene.Add(new Nine.Graphics.DirectionalLight(graphics) { DiffuseColor = Vector3.UnitY, Direction = -Vector3.UnitY });
            scene.Add(new Nine.Graphics.DirectionalLight(graphics) { DiffuseColor = Vector3.UnitZ, Direction = -Vector3.UnitZ });

#if WINDOWS_PHONE
            var size = 4;
#else
            var size = 20;
#endif
            var step = 2;
            for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                    for (var z = 0; z < size; z++)
                    {
                        scene.Add(new Nine.Graphics.Model(cube)
                        //scene.Add(new Box(graphics)
                        {
                            Material = new BasicMaterial(graphics) { LightingEnabled = true },
                            Transform = Matrix.CreateTranslation(x * step, y * step, z * step)
                        });
                    }

            return scene;
        }
    }
}
