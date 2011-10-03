#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
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
using Nine.Graphics.ScreenEffects;
#endregion

namespace Nine.Graphics.Primitives
{
    public class Sphere : Primitive<VertexPositionNormal>
    {   
        /// <summary>
        /// Constructs a new sphere primitive, using default settings.
        /// </summary>
        public Sphere(GraphicsDevice graphicsDevice)
            : this(graphicsDevice, 1, 16)
        {
        }
        
        /// <summary>
        /// Constructs a new sphere primitive,
        /// with the specified size and tessellation level.
        /// </summary>
        public Sphere(GraphicsDevice graphicsDevice,
                               float radius, int tessellation)
        {
            if (tessellation < 3)
                throw new ArgumentOutOfRangeException("tessellation");

            int verticalSegments = tessellation;
            int horizontalSegments = tessellation * 2;

            // Start with a single vertex at the bottom of the sphere.
            AddVertex(-Vector3.UnitZ * radius, -Vector3.UnitZ);

            // Create rings of vertices at progressively higher latitudes.
            for (int i = 0; i < verticalSegments - 1; i++)
            {
                float latitude = ((i + 1) * MathHelper.Pi /
                                            verticalSegments) - MathHelper.PiOver2;

                float dz = (float)Math.Sin(latitude);
                float dxy = (float)Math.Cos(latitude);

                // Create a single ring of vertices at this latitude.
                for (int j = 0; j < horizontalSegments; j++)
                {
                    float longitude = j * MathHelper.TwoPi / horizontalSegments;

                    float dx = (float)Math.Cos(longitude) * dxy;
                    float dy = (float)Math.Sin(longitude) * dxy;

                    Vector3 normal = new Vector3(dx, dy, dz);

                    AddVertex(normal * radius, normal);
                }
            }

            // Finish with a single vertex at the top of the sphere.
            AddVertex(Vector3.UnitZ * radius, Vector3.UnitZ);

            // Create a fan connecting the bottom vertex to the bottom latitude ring.
            for (int i = 0; i < horizontalSegments; i++)
            {
                AddIndex(0);
                AddIndex(1 + i);
                AddIndex(1 + (i + 1) % horizontalSegments);
            }

            // Fill the sphere body with triangles joining each pair of latitude rings.
            for (int i = 0; i < verticalSegments - 2; i++)
            {
                for (int j = 0; j < horizontalSegments; j++)
                {
                    int nextI = i + 1;
                    int nextJ = (j + 1) % horizontalSegments;

                    AddIndex(1 + i * horizontalSegments + j);
                    AddIndex(1 + nextI * horizontalSegments + j);
                    AddIndex(1 + i * horizontalSegments + nextJ);

                    AddIndex(1 + i * horizontalSegments + nextJ);
                    AddIndex(1 + nextI * horizontalSegments + j);
                    AddIndex(1 + nextI * horizontalSegments + nextJ);
                }
            }

            // Create a fan connecting the top vertex to the top latitude ring.
            for (int i = 0; i < horizontalSegments; i++)
            {
                AddIndex(CurrentVertex - 1);
                AddIndex(CurrentVertex - 2 - i);
                AddIndex(CurrentVertex - 2 - (i + 1) % horizontalSegments);
            }

            InitializePrimitive(graphicsDevice);
        }

        private void AddVertex(Vector3 position, Vector3 normal)
        {
            AddVertex(position, new VertexPositionNormal() { Position = position, Normal = normal });
        }
    }

    class SphereInvert : Sphere 
    {
        public SphereInvert(GraphicsDevice graphics) : base(graphics)
        {
            InvertWindingOrder = true;
        }
    }
}
