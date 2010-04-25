#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion


namespace Isles.Graphics
{
    using Isles.Graphics.Primitives;

    public static class DebugVisual
    {
        public static float Alpha { get; set; }
        public static Matrix View { get; set; }
        public static Matrix Projection { get; set; }
        public static bool LightingEnabled { get; set; }


        static DebugVisual() 
        {
            Alpha = 0.5f; 
            Projection = Matrix.Identity; 
            LightingEnabled = true; 
        }


        static Cube cube;
        static Sphere sphere;
        static Axis axis;
        static Arrow arrow;
        static Cylinder cylinder;
        static Grid grid;
        static GeometryVisualizer geometry;
        static IGeometry currentGeometry;


        public static void DrawBox(GraphicsDevice graphics, BoundingBox box, Matrix world, Color color)
        {
            if (cube == null)
                cube = new Cube(graphics);

            color.A = (byte)(color.A * Alpha);
            cube.BasicEffect.LightingEnabled = LightingEnabled;

            Begin(graphics);

            Matrix m = Matrix.CreateScale((box.Max - box.Min)) *
                       Matrix.CreateTranslation((box.Min + box.Max) / 2);

            cube.Draw(m * world, View, Projection, color);

            End(graphics);
        }

        public static void DrawBox(GraphicsDevice graphics, Vector3 center, Vector3 size, Matrix world, Color color)
        {
            if (cube == null)
                cube = new Cube(graphics);

            color.A = (byte)(color.A * Alpha);
            cube.BasicEffect.LightingEnabled = LightingEnabled;

            Begin(graphics);

            cube.Draw(Matrix.CreateScale(size) *
                      Matrix.CreateTranslation(center) * world, View, Projection, color);

            End(graphics);
        }


        public static void DrawSphere(GraphicsDevice graphics, BoundingSphere region, Color color)
        {
            if (sphere == null)
                sphere = new Sphere(graphics);

            color.A = (byte)(color.A * Alpha);
            sphere.BasicEffect.LightingEnabled = LightingEnabled;

            Begin(graphics);

            sphere.Draw(region, View, Projection, color);

            End(graphics);
        }

        public static void DrawAxis(GraphicsDevice graphics, Matrix transform)
        {
            if (axis == null)
                axis = new Axis(graphics);

            Begin(graphics);

            axis.Draw(transform, View, Projection);

            End(graphics);
        }

        public static void DrawPoint(GraphicsDevice graphics, Vector3 point, Color color, float size)
        {
            if (sphere == null)
                sphere = new Sphere(graphics);

            color.A = (byte)(color.A * Alpha);
            sphere.BasicEffect.LightingEnabled = LightingEnabled;

            Begin(graphics);

            sphere.Draw(Matrix.CreateScale(size) * Matrix.CreateTranslation(point), View, Projection, color);

            End(graphics);
        }

        public static void DrawArrow(GraphicsDevice graphics, Vector3 position, Vector3 target, float scale, Color color)
        {
            if (arrow == null)
                arrow = new Arrow(graphics);

            color.A = (byte)(color.A * Alpha);

            Begin(graphics);

            Matrix mx = Matrix.CreateScale(scale, (target - position).Length(), scale) *
                        Matrix.CreateRotationX(-MathHelper.PiOver2) *
                        Matrix.CreateWorld(position, Vector3.Normalize(target - position), Vector3.Up);

            arrow.Draw(mx, View, Projection, color);

            End(graphics);            
        }

        public static void DrawGrid(GraphicsDevice graphics, Vector3 position, float step, int x, int y, Color color)
        {
            if (grid == null || grid.ResolutionU != x || grid.ResolutionV != y)
                grid = new Grid(graphics, step * x, step * y, x, y);

            color.A = (byte)(color.A * Alpha);

            Begin(graphics);

            grid.Draw(Matrix.CreateTranslation(position), View, Projection, color);

            End(graphics);     
        }

        public static void DrawLine(GraphicsDevice graphics, Vector3 bottom, Vector3 up, float radius, Color color)
        {
            if (cylinder == null)
                cylinder = new Cylinder(graphics);

            color.A = (byte)(color.A * Alpha);
            cylinder.BasicEffect.LightingEnabled = LightingEnabled;

            Begin(graphics);

            Matrix world = Matrix.CreateTranslation(0, 0.5f, 0) *
                           Matrix.CreateScale(radius * 2, (up - bottom).Length(), radius * 2) *
                           Matrix.CreateRotationX(-MathHelper.PiOver2);

            if (up.Y == bottom.Y)
                world *= Matrix.CreateWorld(bottom, Vector3.Normalize(up - bottom), Vector3.Up);
            else
                world *= Matrix.CreateWorld(bottom, Vector3.Normalize(up - bottom), Vector3.UnitZ);

            cylinder.Draw(world, View, Projection, color);

            End(graphics);     
        }

        public static void DrawGeometry(GraphicsDevice graphics, IGeometry geom, Matrix world, Color faceColor, Color borderColor)
        {
            if (geometry == null && currentGeometry != geom)
                geometry = new GeometryVisualizer(graphics, geom);


            faceColor.A = (byte)(faceColor.A * Alpha);
            borderColor.A = (byte)(borderColor.A * Alpha);
            geometry.BasicEffect.LightingEnabled = LightingEnabled;

            Begin(graphics);

            geometry.Draw(world, View, Projection, faceColor, borderColor);

            End(graphics);
        }


        static void Begin(GraphicsDevice graphics)
        {
            if (Projection == Matrix.Identity)
                throw new InvalidOperationException("Have you setup view and projection matrix?");

            graphics.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            graphics.RenderState.DepthBufferEnable = true;
            graphics.RenderState.DepthBufferWriteEnable = true;
            graphics.RenderState.FillMode = FillMode.Solid;
        }

        static void End(GraphicsDevice graphics)
        {
            graphics.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            graphics.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            graphics.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
        }
    }
}
