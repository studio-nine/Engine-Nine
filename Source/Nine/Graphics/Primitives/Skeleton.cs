#region File Description
//-----------------------------------------------------------------------------
// GeometricPrimitive.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics.Primitives
{
    internal class Skeleton : IDisposable
    {
        Sphere sphere;
        Cube cube;
        float cubeSize;


        #region Initialization
        public Skeleton(GraphicsDevice graphicsDevice) : this(graphicsDevice, 2, 0.2f, 12) { }

        public Skeleton(GraphicsDevice graphicsDevice, float nodeSize, float cubeSize, int tessellation)
        {
            this.cubeSize = cubeSize;

            sphere = new Sphere(graphicsDevice, nodeSize, tessellation);
            cube = new Cube(graphicsDevice, 1);
        }


        /// <summary>
        /// Finalizer.
        /// </summary>
        ~Skeleton()
        {
            Dispose(false);
        }


        /// <summary>
        /// Frees resources used by this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Frees resources used by this object.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (sphere != null)
                    sphere.Dispose();

                if (cube != null)
                    cube.Dispose();
            }
        }


        #endregion

        #region Draw


        public void Draw(ModelBone node, Matrix world, Matrix view, Matrix projection, Color color)
        {
            if (node == null)
                return;

            
            sphere.Draw(node.Transform * world, view, projection, color);


            Matrix start = world;
            Matrix end = node.Transform * world;

            if (node.Parent != null)
            {
                Vector3 direction = end.Translation - start.Translation;

                if (direction.LengthSquared() > 0)
                {
                    Matrix cubeTransform = Matrix.CreateTranslation(Vector3.Up * 0.5f);

                    Vector3 axis = Vector3.Normalize(Vector3.Cross(Vector3.Up, direction));
                    float angle = (float)Math.Acos(Vector3.Dot(Vector3.Up, Vector3.Normalize(direction)));

                    cubeTransform = cubeTransform * Matrix.CreateScale(new Vector3(cubeSize, direction.Length(), cubeSize));

                    if (axis.LengthSquared() > 0 && angle != 0)
                        cubeTransform = cubeTransform * Matrix.CreateFromAxisAngle(axis, angle);

                    cubeTransform = cubeTransform * Matrix.CreateTranslation(start.Translation);

                    cube.Draw(cubeTransform, view, projection, color);
                }
            }


            foreach (ModelBone child in node.Children)
            {
                Draw(child, end, view, projection, color);
            }
        }


        #endregion
    }
}
