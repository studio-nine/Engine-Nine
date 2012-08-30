namespace Test
{
    using System;
    using System.Linq;
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
            scene.Add(new Nine.Graphics.Model(tank) { Transform = Matrix.CreateTranslation(8, 0, 1) });
            scene.Add(new Sphere(graphics) { Transform = Matrix.CreateTranslation(1, 1, 1) });
            scene.Add(new Surface(graphics, 1, 256, 256, 8) { Visible = false });
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
        private Vector3[] intersections = new Vector3[128];
        private BoundingBox pickBox = new BoundingBox(new Vector3(-1, -1, -1), new Vector3(1, 1, 1));
        private BoundingFrustum testFrustum = new BoundingFrustum(
                Matrix.CreateLookAt(new Vector3(0, 10, 0), new Vector3(0, 0, 10), Vector3.Up) *
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 4.0f / 3, 1.0f, 10.0f));

        public void OnAddedToView(DrawingContext context)
        {

        }

        public float GetDistanceToCamera(Vector3 cameraPosition)
        {
            return 0;
        }

        public void Draw(DrawingContext context, Material material)
        {
            Vector3[] positions;
            ushort[] indices;

            if (geometryQuery == null)
                geometryQuery = context.CreateSpatialQuery<IGeometry>(null);   
         
            if (primitive == null)
                primitive = new DynamicPrimitive(context.GraphicsDevice);
            
            // Ray picking
            var mouseState = Mouse.GetState();
            var pickRay = context.GraphicsDevice.Viewport.CreatePickRay(mouseState.X, mouseState.Y, context.View, context.Projection);

            geometryQuery.FindAll(ref pickRay, geometry =>
            {
                if (geometry.TryGetTriangles(out positions, out indices))
                {
                    var rayInGeometrySpace = pickRay.Transform(Matrix.Invert(geometry.Transform));
                    for (int i = 0; i < indices.Length; i += 3)
                    {
                        var triangle = new Triangle(positions[indices[i]], positions[indices[i + 1]], positions[indices[i + 2]]);
                        if (triangle.Intersects(rayInGeometrySpace).HasValue)
                        {
                            primitive.AddTriangle(triangle, geometry.Transform, new Color(255, 255, 0), 2);
                        }
                    }
                }
            });

            // Box picking            
            var speed = 0.01f;
            var keyboard = Keyboard.GetState();
            if (keyboard.IsKeyDown(Keys.Left))
            {
                pickBox.Min += Vector3.Left * speed;
                pickBox.Max += Vector3.Left * speed;
            }
            if (keyboard.IsKeyDown(Keys.Right))
            {
                pickBox.Min += Vector3.Right * speed;
                pickBox.Max += Vector3.Right * speed;
            }
            if (keyboard.IsKeyDown(Keys.Up))
            {
                pickBox.Min += Vector3.Forward * speed;
                pickBox.Max += Vector3.Forward * speed;
            }
            if (keyboard.IsKeyDown(Keys.Down))
            {
                pickBox.Min += Vector3.Backward * speed;
                pickBox.Max += Vector3.Backward * speed;
            }
            if (keyboard.IsKeyDown(Keys.PageUp))
            {
                pickBox.Min += Vector3.Up * speed;
                pickBox.Max += Vector3.Up * speed;
            }
            if (keyboard.IsKeyDown(Keys.PageDown))
            {
                pickBox.Min += Vector3.Down * speed;
                pickBox.Max += Vector3.Down * speed;
            }
            
            primitive.AddBox(pickBox, null, Color.Black, 2);            

            geometryQuery.FindAll(ref pickBox, geometry =>
            {
                Matrix transform = geometry.Transform;
                if (geometry.TryGetTriangles(out positions, out indices))
                {
                    for (int i = 0; i < indices.Length; i += 3)
                    {
                        var triangle = new Triangle(positions[indices[i]], positions[indices[i + 1]], positions[indices[i + 2]]);

                        Vector3.Transform(ref triangle.V1, ref transform, out triangle.V1);
                        Vector3.Transform(ref triangle.V2, ref transform, out triangle.V2);
                        Vector3.Transform(ref triangle.V3, ref transform, out triangle.V3);

                        var count = triangle.Intersects(ref pickBox, intersections, 0);
                        if (count > 2)
                        {
                            intersections[count++] = intersections[0];
                            primitive.AddLine(intersections.Take(count), null, new Color(255, 0, 0), 1);
                        }
                    }
                }
            });         
   

            // Box Frustum test
            primitive.AddFrustum(testFrustum, null, new Color(0, 128, 0) * 0.2f, 4);
            var intersectionCount = pickBox.Intersects(testFrustum, intersections, 0);
            if (intersectionCount > 2)
            {
                intersections[intersectionCount++] = intersections[0];
                primitive.AddLine(intersections.Take(intersectionCount), null, new Color(0, 0, 255), 4);
            }

            primitive.Draw(context, null);
            primitive.Clear();
        }
    }
}
