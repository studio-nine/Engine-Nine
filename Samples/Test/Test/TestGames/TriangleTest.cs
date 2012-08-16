namespace Test
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine;
    using Nine.Components;
    using Nine.Graphics;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.Materials;
    using Nine.Graphics.Primitives;
    using Microsoft.Xna.Framework.Input;

    public class TriangleTest : ITestGame
    {
        public Scene CreateTestScene(GraphicsDevice graphics, ContentManager content)
        {
            var scene = new Scene();
            var tank = content.Load<Microsoft.Xna.Framework.Graphics.Model>("Models/AlphaPalm");

            scene.Add(new Nine.Graphics.DirectionalLight(graphics) { Direction = -Vector3.One });
            scene.Add(new Nine.Graphics.Model(tank) { Transform = Matrix.CreateTranslation(20, 0, 20) });
            scene.Add(new Sphere(graphics) { Transform = Matrix.CreateTranslation(1, 1, 1) });
            scene.Add(new Surface(graphics, 1, 256, 256, 8));
            scene.Add(new TrianglePicker());

            return scene;
        }
    }

    public class TrianglePicker : Component, IDrawableObject
    {
        public bool Visible
        {
            get { return true; }
        }

        public Material Material
        {
            get { return null; }
        }

        private DynamicPrimitive primitive;
        private ISpatialQuery<IGeometry> geometryQuery;
        private List<IGeometry> geometries = new List<IGeometry>();

        public void Draw(DrawingContext context, Material material)
        {
            if (geometryQuery == null)
                geometryQuery = context.CreateSpatialQuery<IGeometry>(null);

            var mouseState = Mouse.GetState();
            var pickRay = context.GraphicsDevice.Viewport.CreatePickRay(mouseState.X, mouseState.Y, context.View, context.Projection);
            
            geometries.Clear();
            geometryQuery.FindAll(ref pickRay, geometries);

            if (primitive == null)
                primitive = new DynamicPrimitive(context.GraphicsDevice);
            primitive.Clear();

            foreach (var geometry in geometries)
            {
                Vector3[] positions;
                ushort[] indices;
                geometry.GetTriangles(out positions, out indices);

                var ray = pickRay.Transform(Matrix.Invert(geometry.Transform));
                for (int i = 0; i < indices.Length; i += 3)
                {
                    var triangle = new Triangle(positions[indices[i]], positions[indices[i + 1]], positions[indices[i + 2]]);
                    if (triangle.Intersects(ray).HasValue)
                    {
                        primitive.AddTriangle(triangle, geometry.Transform, Color.Yellow, 2);
                    }
                }
            }
            primitive.Draw(context, null);
        }

        public void BeginDraw(DrawingContext context) { }
        public void EndDraw(DrawingContext context) { }
    }
}
