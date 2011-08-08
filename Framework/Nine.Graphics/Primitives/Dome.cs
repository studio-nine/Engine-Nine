#region File Description
//-----------------------------------------------------------------------------
// SpherePrimitive.cs
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
    /// Geometric primitive class for drawing spheres.
    /// </summary>
    public class Dome : Primitive<VertexPositionNormalTexture>
    {
        /// <summary>
        /// Constructs a new sphere primitive, using default settings.
        /// </summary>
        public Dome(GraphicsDevice graphicsDevice) : this(graphicsDevice, 1, 0.2f, 16) { }


        /// <summary>
        /// Constructs a new sphere primitive,
        /// with the specified size and tessellation level.
        /// </summary>
        public Dome(GraphicsDevice graphicsDevice, float size, float height, int tessellation)
        {
            if (tessellation < 3)
                throw new ArgumentOutOfRangeException("tessellation");

            if (height > size || height < 0 || size <= 0)
                throw new ArgumentOutOfRangeException();

            float angle = (float)Math.Atan2(height, size / 2);
            angle = MathHelper.PiOver2 - angle * 2;
            float radius = (float)(size * 0.5f / Math.Cos(angle));
            float percentage = (MathHelper.PiOver2 - angle) / MathHelper.Pi;
            Vector3 sink = -Vector3.Up * (radius - height);


            int verticalSegments = tessellation / 2;
            int horizontalSegments = tessellation * 2;

            // Start with a single vertex at the bottom of the sphere.
            AddVertex(Vector3.UnitZ * radius + sink, Vector3.UnitZ, Vector2.One * 0.5f);

            // Create rings of vertices at progressively higher latitudes.
            for (int i = 0; i < verticalSegments; i++)
            {
                float latitude = ((i + 1) * MathHelper.Pi / verticalSegments) * percentage + MathHelper.PiOver2;

                float dz = (float)Math.Sin(latitude);
                float dxy = (float)Math.Cos(latitude);

                // Create a single ring of vertices at this latitude.
                for (int j = 0; j < horizontalSegments; j++)
                {
                    float longitude = j * MathHelper.TwoPi / horizontalSegments;

                    float dx = (float)Math.Cos(longitude) * dxy;
                    float dy = (float)Math.Sin(longitude) * dxy;

                    Vector3 normal = new Vector3(dx, dy, dz);

                    Vector2 uv = new Vector2();

                    uv.X = dx * radius / size + 0.5f;
                    uv.Y = dz * radius / size + 0.5f;

                    AddVertex(normal * radius + sink, normal, uv);
                }
            }


            // Create a fan connecting the bottom vertex to the bottom latitude ring.
            for (int i = 0; i < horizontalSegments; i++)
            {
                AddIndex(0);
                AddIndex(1 + (i + 1) % horizontalSegments);
                AddIndex(1 + i);
            }

            // Fill the sphere body with triangles joining each pair of latitude rings.
            for (int i = 0; i < verticalSegments - 1; i++)
            {
                for (int j = 0; j < horizontalSegments; j++)
                {
                    int nextI = i + 1;
                    int nextJ = (j + 1) % horizontalSegments;

                    AddIndex(1 + i * horizontalSegments + j);
                    AddIndex(1 + i * horizontalSegments + nextJ);
                    AddIndex(1 + nextI * horizontalSegments + j);

                    AddIndex(1 + i * horizontalSegments + nextJ);
                    AddIndex(1 + nextI * horizontalSegments + nextJ);
                    AddIndex(1 + nextI * horizontalSegments + j);
                }
            }


            InitializePrimitive(graphicsDevice);
        }

        private void AddVertex(Vector3 position, Vector3 normal, Vector2 uv)
        {
            AddVertex(position, new VertexPositionNormalTexture() { Position = position, Normal = normal, TextureCoordinate = uv });
        }
    }
}

