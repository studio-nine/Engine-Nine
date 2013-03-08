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
    using Nine.Content;

    public class DynamicPrimitiveTest : ISample
    {
        public Scene CreateScene(GraphicsDevice graphics, ContentLoader content)
        {
            var scene = new Scene();
            var solidPrimitive = new DynamicPrimitive(graphics) { DepthBias = 0 };
            var linePrimitive = new DynamicPrimitive(graphics) { DepthBias = 0.0002f };

            var frustum = new BoundingFrustum(
                            Matrix.CreateLookAt(new Vector3(0, 15, 15), Vector3.Zero, Vector3.Up) *
                            Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1, 1, 10));

            linePrimitive.AddLine(new Vector3[] { Vector3.Zero, Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ }, null, Color.White, 1);

            linePrimitive.AddAxis(Matrix.CreateScale(4) * Matrix.CreateTranslation(-8, 0, 0), 4);

            linePrimitive.AddGrid(8, 16, 16, null, Color.Black, 2);
            linePrimitive.AddGrid(1, 128, 128, null, Color.White * 0.25f, 1);

            linePrimitive.AddSphere(new Vector3(16, 0, 16), 4, 24, null, Color.White, 2);
            solidPrimitive.AddSolidSphere(new Vector3(16, 0, 8), 4, 24, null, new Color(128, 21, 199));

            linePrimitive.AddBox(Vector3.Zero, Vector3.One * 4, null, Color.White, 2);
            solidPrimitive.AddSolidBox(new Vector3(8, 0, 0), Vector3.One * 4, null, new Color(255, 255, 0, 255) * 0.2f);

            linePrimitive.AddCircle(new Vector3(16, 0, -8), 4, 32, null, new Color(255, 255, 0, 255), 2);
            solidPrimitive.AddSolidSphere(new BoundingSphere(Vector3.UnitX * 4, 1), 24, null, new Color(255, 0, 0, 255) * 0.2f);

            linePrimitive.AddFrustum(frustum, null, Color.White, 2);
            solidPrimitive.AddSolidFrustum(frustum, null, new Color(255, 192, 203, 255) * 0.5f);

            linePrimitive.AddCylinder(new Vector3(-16, 0, 8), 2, 1, 12, null, Color.White, 2);
            solidPrimitive.AddSolidCylinder(new Vector3(-5, 0, -6), 2, 1, 24, null, new Color(230, 230, 255, 255) * 0.3f);

            scene.Add(solidPrimitive);
            scene.Add(linePrimitive);
            return scene;
        }
    }
}
