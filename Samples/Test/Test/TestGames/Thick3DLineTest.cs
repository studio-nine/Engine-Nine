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

    public class Thick3DLineTest : ITestGame
    {
        public Scene CreateTestScene(GraphicsDevice graphics, ContentManager content)
        {
            var scene = new Scene();
            var dynamicPrimitive = new DynamicPrimitive(graphics);
            dynamicPrimitive.AddSolidBox(new BoundingBox(Vector3.One * -1, Vector3.One * 1), null, Color.Blue);
            dynamicPrimitive.AddBox(new BoundingBox(Vector3.One * -1, Vector3.One * 1), null, Color.Yellow, 4);
            scene.Add(dynamicPrimitive);
            return scene;
        }
    }
}
