#region File Description
//-----------------------------------------------------------------------------
// CylinderPrimitive.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics.Primitives
{
    /// <summary>
    /// Geometric primitive class for drawing cylinders.
    /// </summary>
    [ContentSerializable]
    public class Cylinder : Primitive<VertexPositionNormal>
    {
        /// <summary>
        /// Gets or sets the tessellation of this primitive.
        /// </summary>
        public int Tessellation
        {
            get { return tessellation; }
            set
            {
                if (tessellation != value)
                {
                    if (value < 3)
                        throw new ArgumentOutOfRangeException("tessellation");
                    tessellation = value;
                    Invalidate();
                }
            }
        }
        private int tessellation = 32;

        /// <summary>
        /// Constructs a new cylinder primitive, using default settings.
        /// </summary>
        public Cylinder(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {

        }

        protected override bool CanShareBufferWith(Primitive<VertexPositionNormal> primitive)
        {
            return primitive is Cylinder && ((Cylinder)primitive).tessellation == tessellation;
        }

        protected override void  OnBuild()
        {
            // Create a ring of triangles around the outside of the cylinder.
            for (int i = 0; i < tessellation; i++)
            {
                Vector3 normal = GetCircleVector(i, tessellation);

                AddVertex(normal + Vector3.UnitZ, normal);
                AddVertex(normal, normal);

                AddIndex(i * 2);
                AddIndex((i * 2 + 2) % (tessellation * 2));
                AddIndex(i * 2 + 1);

                AddIndex(i * 2 + 1);
                AddIndex((i * 2 + 2) % (tessellation * 2));
                AddIndex((i * 2 + 3) % (tessellation * 2));
            }

            // Create flat triangle fan caps to seal the top and bottom.
            CreateCap(tessellation, 1, 1, Vector3.UnitZ);
            CreateCap(tessellation, 1, 1, -Vector3.UnitZ);
        }


        /// <summary>
        /// Helper method creates a triangle fan to close the ends of the cylinder.
        /// </summary>
        void CreateCap(int tessellation, float height, float radius, Vector3 normal)
        {
            // Create cap indices.
            for (int i = 0; i < tessellation - 2; i++)
            {
                if (normal.Z > 0)
                {
                    AddIndex(CurrentVertex);
                    AddIndex(CurrentVertex + (i + 2) % tessellation);
                    AddIndex(CurrentVertex + (i + 1) % tessellation);
                }
                else
                {
                    AddIndex(CurrentVertex);
                    AddIndex(CurrentVertex + (i + 1) % tessellation);
                    AddIndex(CurrentVertex + (i + 2) % tessellation);
                }
            }

            // Create cap vertices.
            for (int i = 0; i < tessellation; i++)
            {
                Vector3 position = GetCircleVector(i, tessellation) * radius;
                if (normal.Z > 0)
                    position += normal * height;

                AddVertex(position, normal);
            }
        }


        /// <summary>
        /// Helper method computes a point on a circle.
        /// </summary>
        static Vector3 GetCircleVector(int i, int tessellation)
        {
            float angle = i * MathHelper.TwoPi / tessellation;

            float dx = (float)Math.Cos(angle);
            float dy = (float)Math.Sin(angle);

            return new Vector3(dx, dy, 0);
        }

        private void AddVertex(Vector3 position, Vector3 normal)
        {
            AddVertex(position, new VertexPositionNormal() { Position = position, Normal = normal });
        }
    }
}
