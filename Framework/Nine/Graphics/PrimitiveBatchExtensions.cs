#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Statements
using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class PrimitiveBatchExtensions
    {
        public static void DrawGeometry(this PrimitiveBatch primitiveBatch, IGeometry geometry, Matrix? world, Color color)
        {
            primitiveBatch.BeginPrimitive(PrimitiveType.LineList, null, world);
            {
                foreach (Vector3 position in geometry.Positions)
                {
                    primitiveBatch.AddVertex(new VertexPositionColorTexture() { Position = position, Color = color });
                }

                for (int i = 0; i < geometry.Indices.Count; i += 3)
                {
                    primitiveBatch.AddIndex(geometry.Indices[i + 0]);
                    primitiveBatch.AddIndex(geometry.Indices[i + 1]);
                    primitiveBatch.AddIndex(geometry.Indices[i + 1]);
                    primitiveBatch.AddIndex(geometry.Indices[i + 2]);
                    primitiveBatch.AddIndex(geometry.Indices[i + 2]);
                    primitiveBatch.AddIndex(geometry.Indices[i + 0]);
                }
            }
            primitiveBatch.EndPrimitive();
        }

        public static void DrawRectangle(this PrimitiveBatch primitiveBatch, Vector2 min, Vector2 max, Matrix? world, Color color)
        {
            primitiveBatch.BeginPrimitive(PrimitiveType.LineList, null, world);
            {
                primitiveBatch.AddVertex(new Vector3(min.X, min.Y, 0), color);
                primitiveBatch.AddVertex(new Vector3(min.X, max.Y, 0), color);
                primitiveBatch.AddVertex(new Vector3(max.X, max.Y, 0), color);
                primitiveBatch.AddVertex(new Vector3(max.X, min.Y, 0), color);

                primitiveBatch.AddIndex(0);
                primitiveBatch.AddIndex(1);
                primitiveBatch.AddIndex(1);
                primitiveBatch.AddIndex(2);
                primitiveBatch.AddIndex(2);
                primitiveBatch.AddIndex(3);
                primitiveBatch.AddIndex(3);
                primitiveBatch.AddIndex(0);
            }
            primitiveBatch.EndPrimitive();
        }

        public static void DrawRectangle(this PrimitiveBatch primitiveBatch, Vector2 min, Vector2 max, Vector3 up, Matrix? world, Color color)
        {
            Matrix transform = MatrixHelper.CreateRotation(Vector3.UnitZ, up);

            primitiveBatch.BeginPrimitive(PrimitiveType.LineList, null, world);
            {
                primitiveBatch.AddVertex(Vector3.TransformNormal(new Vector3(min.X, min.Y, 0), transform), color);
                primitiveBatch.AddVertex(Vector3.TransformNormal(new Vector3(min.X, max.Y, 0), transform), color);
                primitiveBatch.AddVertex(Vector3.TransformNormal(new Vector3(max.X, max.Y, 0), transform), color);
                primitiveBatch.AddVertex(Vector3.TransformNormal(new Vector3(max.X, min.Y, 0), transform), color);

                primitiveBatch.AddIndex(0);
                primitiveBatch.AddIndex(1);
                primitiveBatch.AddIndex(1);
                primitiveBatch.AddIndex(2);
                primitiveBatch.AddIndex(2);
                primitiveBatch.AddIndex(3);
                primitiveBatch.AddIndex(3);
                primitiveBatch.AddIndex(0);
            }
            primitiveBatch.EndPrimitive();
        }

        public static void DrawBox(this PrimitiveBatch primitiveBatch, Vector3 center, Vector3 size, Matrix? world, Color color)
        {
            DrawBox(primitiveBatch, new BoundingBox(center - size / 2, center + size / 2), world, color);
        }

        public static void DrawBox(this PrimitiveBatch primitiveBatch, BoundingBox boundingBox, Matrix? world, Color color)
        {
            primitiveBatch.BeginPrimitive(PrimitiveType.LineList, null, world);
            {
                // Modified from http://www.roastedamoeba.com/blog/archive/2010/12/10/drawing-an-xna-model-bounding-box
                const float ratio = 5.0f;

                Vector3 xOffset = new Vector3((boundingBox.Max.X - boundingBox.Min.X) / ratio, 0, 0);
                Vector3 yOffset = new Vector3(0, (boundingBox.Max.Y - boundingBox.Min.Y) / ratio, 0);
                Vector3 zOffset = new Vector3(0, 0, (boundingBox.Max.Z - boundingBox.Min.Z) / ratio);
                Vector3[] corners = boundingBox.GetCorners();

                // Corner 1. 
                primitiveBatch.AddVertex(corners[0], color);
                primitiveBatch.AddVertex(corners[0] + xOffset, color);
                primitiveBatch.AddVertex(corners[0], color);
                primitiveBatch.AddVertex(corners[0] - yOffset, color);
                primitiveBatch.AddVertex(corners[0], color);
                primitiveBatch.AddVertex(corners[0] - zOffset, color);

                // Corner 2. 
                primitiveBatch.AddVertex(corners[1], color);
                primitiveBatch.AddVertex(corners[1] - xOffset, color);
                primitiveBatch.AddVertex(corners[1], color);
                primitiveBatch.AddVertex(corners[1] - yOffset, color);
                primitiveBatch.AddVertex(corners[1], color);
                primitiveBatch.AddVertex(corners[1] - zOffset, color);

                // Corner 3. 
                primitiveBatch.AddVertex(corners[2], color);
                primitiveBatch.AddVertex(corners[2] - xOffset, color);
                primitiveBatch.AddVertex(corners[2], color);
                primitiveBatch.AddVertex(corners[2] + yOffset, color);
                primitiveBatch.AddVertex(corners[2], color);
                primitiveBatch.AddVertex(corners[2] - zOffset, color);

                // Corner 4. 
                primitiveBatch.AddVertex(corners[3], color);
                primitiveBatch.AddVertex(corners[3] + xOffset, color);
                primitiveBatch.AddVertex(corners[3], color);
                primitiveBatch.AddVertex(corners[3] + yOffset, color);
                primitiveBatch.AddVertex(corners[3], color);
                primitiveBatch.AddVertex(corners[3] - zOffset, color);

                // Corner 5. 
                primitiveBatch.AddVertex(corners[4], color);
                primitiveBatch.AddVertex(corners[4] + xOffset, color);
                primitiveBatch.AddVertex(corners[4], color);
                primitiveBatch.AddVertex(corners[4] - yOffset, color);
                primitiveBatch.AddVertex(corners[4], color);
                primitiveBatch.AddVertex(corners[4] + zOffset, color);

                // Corner 6. 
                primitiveBatch.AddVertex(corners[5], color);
                primitiveBatch.AddVertex(corners[5] - xOffset, color);
                primitiveBatch.AddVertex(corners[5], color);
                primitiveBatch.AddVertex(corners[5] - yOffset, color);
                primitiveBatch.AddVertex(corners[5], color);
                primitiveBatch.AddVertex(corners[5] + zOffset, color);

                // Corner 7. 
                primitiveBatch.AddVertex(corners[6], color);
                primitiveBatch.AddVertex(corners[6] - xOffset, color);
                primitiveBatch.AddVertex(corners[6], color);
                primitiveBatch.AddVertex(corners[6] + yOffset, color);
                primitiveBatch.AddVertex(corners[6], color);
                primitiveBatch.AddVertex(corners[6] + zOffset, color);

                // Corner 8. 
                primitiveBatch.AddVertex(corners[7], color);
                primitiveBatch.AddVertex(corners[7] + xOffset, color);
                primitiveBatch.AddVertex(corners[7], color);
                primitiveBatch.AddVertex(corners[7] + yOffset, color);
                primitiveBatch.AddVertex(corners[7], color);
                primitiveBatch.AddVertex(corners[7] + zOffset, color); 
            }
            primitiveBatch.EndPrimitive();
        }

        public static void DrawSolidBox(this PrimitiveBatch primitiveBatch, Vector3 center, Vector3 size, Matrix? world, Color color)
        {
            DrawSolidBox(primitiveBatch, new BoundingBox(center - size / 2, center + size / 2), world, color);
        }

        public static void DrawSolidBox(this PrimitiveBatch primitiveBatch, BoundingBox boundingBox, Matrix? world, Color color)
        {
            Matrix transform = Matrix.CreateScale((boundingBox.Max - boundingBox.Min)) *
                               Matrix.CreateTranslation((boundingBox.Min + boundingBox.Max) / 2);

            primitiveBatch.BeginPrimitive(PrimitiveType.TriangleList, null, world);
            {
                float size = 1;

                // A cube has six faces, each one pointing in a different direction.
                Vector3[] normals =
                {
                    new Vector3(0, 0, 1),
                    new Vector3(0, 0, -1),
                    new Vector3(1, 0, 0),
                    new Vector3(-1, 0, 0),
                    new Vector3(0, 1, 0),
                    new Vector3(0, -1, 0),
                };

                int currentVertex = 0;

                // Create each face in turn.
                foreach (Vector3 normal in normals)
                {
                    // Get two vectors perpendicular to the face normal and to each other.
                    Vector3 side1 = new Vector3(normal.Y, normal.Z, normal.X);
                    Vector3 side2 = Vector3.Cross(normal, side1);

                    // Six indices (two triangles) per face.
                    primitiveBatch.AddIndex((ushort)(currentVertex + 0));
                    primitiveBatch.AddIndex((ushort)(currentVertex + 1));
                    primitiveBatch.AddIndex((ushort)(currentVertex + 2));
                    primitiveBatch.AddIndex((ushort)(currentVertex + 0));
                    primitiveBatch.AddIndex((ushort)(currentVertex + 2));
                    primitiveBatch.AddIndex((ushort)(currentVertex + 3));

                    // Four vertices per face.
                    primitiveBatch.AddVertex(Vector3.Transform((normal - side1 - side2) * size / 2, transform), color);
                    primitiveBatch.AddVertex(Vector3.Transform((normal - side1 + side2) * size / 2, transform), color);
                    primitiveBatch.AddVertex(Vector3.Transform((normal + side1 + side2) * size / 2, transform), color);
                    primitiveBatch.AddVertex(Vector3.Transform((normal + side1 - side2) * size / 2, transform), color);

                    currentVertex += 4;
                }
            }
            primitiveBatch.EndPrimitive();
        }

        public static void DrawCircle(this PrimitiveBatch primitiveBatch, Vector3 center, float radius, int tessellation, Matrix? world, Color color)
        {
            primitiveBatch.BeginPrimitive(PrimitiveType.LineStrip, null, world);
            {
                if (tessellation < 3)
                    throw new ArgumentOutOfRangeException("tessellation");

                int horizontalSegments = tessellation;

                // Create a single ring of vertices at this latitude.
                for (int j = 0; j <= horizontalSegments; j++)
                {
                    float longitude = j * MathHelper.TwoPi / horizontalSegments;

                    float dx = (float)Math.Cos(longitude);
                    float dy = (float)Math.Sin(longitude);

                    Vector3 normal = new Vector3(dx, dy, 0);

                    primitiveBatch.AddVertex(normal * radius + center, color);
                }
            }
            primitiveBatch.EndPrimitive();
        }

        public static void DrawCircle(this PrimitiveBatch primitiveBatch, Vector3 center, float radius, Vector3 up, int tessellation, Matrix? world, Color color)
        {
            Matrix transform = Matrix.CreateScale(radius) *
                               MatrixHelper.CreateRotation(Vector3.UnitZ, up) *
                               Matrix.CreateTranslation(center);

            primitiveBatch.BeginPrimitive(PrimitiveType.LineStrip, null, world);
            {
                if (tessellation < 3)
                    throw new ArgumentOutOfRangeException("tessellation");

                int horizontalSegments = tessellation;

                // Create a single ring of vertices at this latitude.
                for (int j = 0; j <= horizontalSegments; j++)
                {
                    float longitude = j * MathHelper.TwoPi / horizontalSegments;

                    float dx = (float)Math.Cos(longitude);
                    float dy = (float)Math.Sin(longitude);

                    Vector3 normal = new Vector3(dx, dy, 0);

                    primitiveBatch.AddVertex(Vector3.Transform(normal, transform), color);
                }
            }
            primitiveBatch.EndPrimitive();
        }

        public static void DrawSphere(this PrimitiveBatch primitiveBatch, Vector3 center, float radius, int tessellation, Matrix? world, Color color)
        {
            DrawSphere(primitiveBatch, new BoundingSphere(center, radius), tessellation, world, color);
        }

        public static void DrawSphere(this PrimitiveBatch primitiveBatch, BoundingSphere boundingSphere, int tessellation, Matrix? world, Color color)
        {
            DrawCircle(primitiveBatch, boundingSphere.Center, boundingSphere.Radius, Vector3.UnitX, tessellation, world, color);
            DrawCircle(primitiveBatch, boundingSphere.Center, boundingSphere.Radius, Vector3.UnitY, tessellation, world, color);
            DrawCircle(primitiveBatch, boundingSphere.Center, boundingSphere.Radius, Vector3.UnitZ, tessellation, world, color);
        }

        public static void DrawSphere(this PrimitiveBatch primitiveBatch, BoundingSphere boundingSphere, int tessellation, Matrix? world, Color colorX, Color colorY, Color colorZ)
        {
            DrawCircle(primitiveBatch, boundingSphere.Center, boundingSphere.Radius, Vector3.UnitX, tessellation, world, colorX);
            DrawCircle(primitiveBatch, boundingSphere.Center, boundingSphere.Radius, Vector3.UnitY, tessellation, world, colorY);
            DrawCircle(primitiveBatch, boundingSphere.Center, boundingSphere.Radius, Vector3.UnitZ, tessellation, world, colorZ);
        }

        public static void DrawSolidSphere(this PrimitiveBatch primitiveBatch, Vector3 center, float radius, int tessellation, Matrix? world, Color color)
        {
            DrawSolidSphere(primitiveBatch, new BoundingSphere(center, radius), tessellation, world, color);
        }

        public static void DrawSolidSphere(this PrimitiveBatch primitiveBatch, BoundingSphere boundingSphere, int tessellation, Matrix? world, Color color)
        {
            primitiveBatch.BeginPrimitive(PrimitiveType.TriangleList, null, world);
            {
                if (tessellation < 3)
                    throw new ArgumentOutOfRangeException("tessellation");

                int currentVertex = 2;
                int verticalSegments = tessellation;
                int horizontalSegments = tessellation * 2;

                // Start with a single vertex at the bottom of the sphere.
                primitiveBatch.AddVertex(Vector3.Down * boundingSphere.Radius + boundingSphere.Center, color);

                // Create rings of vertices at progressively higher latitudes.
                for (int i = 0; i < verticalSegments - 1; i++)
                {
                    float latitude = ((i + 1) * MathHelper.Pi /
                                                verticalSegments) - MathHelper.PiOver2;

                    float dy = (float)Math.Sin(latitude);
                    float dxz = (float)Math.Cos(latitude);

                    // Create a single ring of vertices at this latitude.
                    for (int j = 0; j < horizontalSegments; j++)
                    {
                        float longitude = j * MathHelper.TwoPi / horizontalSegments;

                        float dx = (float)Math.Cos(longitude) * dxz;
                        float dz = (float)Math.Sin(longitude) * dxz;

                        Vector3 normal = new Vector3(dx, dy, dz);

                        primitiveBatch.AddVertex(normal * boundingSphere.Radius + boundingSphere.Center, color);
                        currentVertex++;
                    }
                }

                // Finish with a single vertex at the top of the sphere.
                primitiveBatch.AddVertex(Vector3.Up * boundingSphere.Radius + boundingSphere.Center, color);

                // Create a fan connecting the bottom vertex to the bottom latitude ring.
                for (int i = 0; i < horizontalSegments; i++)
                {
                    primitiveBatch.AddIndex(0);
                    primitiveBatch.AddIndex(1 + (i + 1) % horizontalSegments);
                    primitiveBatch.AddIndex(1 + i);
                }

                // Fill the sphere body with triangles joining each pair of latitude rings.
                for (int i = 0; i < verticalSegments - 2; i++)
                {
                    for (int j = 0; j < horizontalSegments; j++)
                    {
                        int nextI = i + 1;
                        int nextJ = (j + 1) % horizontalSegments;

                        primitiveBatch.AddIndex(1 + i * horizontalSegments + j);
                        primitiveBatch.AddIndex(1 + i * horizontalSegments + nextJ);
                        primitiveBatch.AddIndex(1 + nextI * horizontalSegments + j);

                        primitiveBatch.AddIndex(1 + i * horizontalSegments + nextJ);
                        primitiveBatch.AddIndex(1 + nextI * horizontalSegments + nextJ);
                        primitiveBatch.AddIndex(1 + nextI * horizontalSegments + j);
                    }
                }

                // Create a fan connecting the top vertex to the top latitude ring.
                for (int i = 0; i < horizontalSegments; i++)
                {
                    primitiveBatch.AddIndex(currentVertex - 1);
                    primitiveBatch.AddIndex(currentVertex - 2 - (i + 1) % horizontalSegments);
                    primitiveBatch.AddIndex(currentVertex - 2 - i);
                }
            }
            primitiveBatch.EndPrimitive();
        }

        public static void DrawFrustum(this PrimitiveBatch primitiveBatch, BoundingFrustum boundingFrustum, Matrix? world, Color color)
        {
            Vector3[] corners = boundingFrustum.GetCorners();

            // near plane
            primitiveBatch.DrawLine(corners[0], corners[1], color);
            primitiveBatch.DrawLine(corners[1], corners[2], color);
            primitiveBatch.DrawLine(corners[2], corners[3], color);
            primitiveBatch.DrawLine(corners[3], corners[0], color);

            // connections
            primitiveBatch.DrawLine(corners[0], corners[4], color);
            primitiveBatch.DrawLine(corners[1], corners[5], color);
            primitiveBatch.DrawLine(corners[2], corners[6], color);
            primitiveBatch.DrawLine(corners[3], corners[7], color);

            // far plane
            primitiveBatch.DrawLine(corners[4], corners[5], color);
            primitiveBatch.DrawLine(corners[5], corners[6], color);
            primitiveBatch.DrawLine(corners[6], corners[7], color);
            primitiveBatch.DrawLine(corners[7], corners[4], color);
        }

        public static void DrawSolidFrustum(this PrimitiveBatch primitiveBatch, BoundingFrustum boundingFrustum, Matrix? world, Color color)
        {
            Vector3[] corners = boundingFrustum.GetCorners();

            primitiveBatch.BeginPrimitive(PrimitiveType.TriangleList, null, world);
            {
                primitiveBatch.AddVertex(corners[0], color);
                primitiveBatch.AddVertex(corners[1], color);
                primitiveBatch.AddVertex(corners[2], color);
                primitiveBatch.AddVertex(corners[3], color);
                primitiveBatch.AddVertex(corners[4], color);
                primitiveBatch.AddVertex(corners[5], color);
                primitiveBatch.AddVertex(corners[6], color);
                primitiveBatch.AddVertex(corners[7], color);

                // near plane
                primitiveBatch.AddIndex(0);
                primitiveBatch.AddIndex(1);
                primitiveBatch.AddIndex(2);
                primitiveBatch.AddIndex(3);
                primitiveBatch.AddIndex(0);
                primitiveBatch.AddIndex(2);

                // far plane
                primitiveBatch.AddIndex(4);
                primitiveBatch.AddIndex(6);
                primitiveBatch.AddIndex(5);
                primitiveBatch.AddIndex(4);
                primitiveBatch.AddIndex(7);
                primitiveBatch.AddIndex(6);
                
                primitiveBatch.AddIndex(0);
                primitiveBatch.AddIndex(3);
                primitiveBatch.AddIndex(4);
                primitiveBatch.AddIndex(4);
                primitiveBatch.AddIndex(3);
                primitiveBatch.AddIndex(7);

                primitiveBatch.AddIndex(5);
                primitiveBatch.AddIndex(1);
                primitiveBatch.AddIndex(0);
                primitiveBatch.AddIndex(5);
                primitiveBatch.AddIndex(0);
                primitiveBatch.AddIndex(4);

                primitiveBatch.AddIndex(5);
                primitiveBatch.AddIndex(6);
                primitiveBatch.AddIndex(2);
                primitiveBatch.AddIndex(5);
                primitiveBatch.AddIndex(2);
                primitiveBatch.AddIndex(1);

                primitiveBatch.AddIndex(3);
                primitiveBatch.AddIndex(2);
                primitiveBatch.AddIndex(6);
                primitiveBatch.AddIndex(3);
                primitiveBatch.AddIndex(6);
                primitiveBatch.AddIndex(7);
            }
            primitiveBatch.EndPrimitive();
        }
        
        public static void DrawCentrum(this PrimitiveBatch primitiveBatch, Vector3 position, float height, float radius, int tessellation, Matrix? world, Color color)
        {
            if (tessellation < 3)
                throw new ArgumentOutOfRangeException("tessellation");

            primitiveBatch.BeginPrimitive(PrimitiveType.LineList, null, world);
            {
                primitiveBatch.AddVertex(position + Vector3.UnitZ * height, color);

                for (int i = 0; i < tessellation; i++)
                {
                    float angle = i * MathHelper.TwoPi / tessellation;

                    float dx = (float)Math.Cos(angle);
                    float dy = (float)Math.Sin(angle);

                    Vector3 normal = new Vector3(dx, dy, 0);

                    primitiveBatch.AddVertex(position + normal * radius, color);

                    primitiveBatch.AddIndex(0);
                    primitiveBatch.AddIndex(1 + i);

                    primitiveBatch.AddIndex(1 + i);
                    primitiveBatch.AddIndex(1 + (i + 1) % tessellation);
                }
            }
            primitiveBatch.EndPrimitive();
        }

        public static void DrawSolidCentrum(this PrimitiveBatch primitiveBatch, Vector3 position, float height, float radius, int tessellation, Matrix? world, Color color)
        {
            if (tessellation < 3)
                throw new ArgumentOutOfRangeException("tessellation");

            primitiveBatch.BeginPrimitive(PrimitiveType.TriangleList, null, world);
            {
                primitiveBatch.AddVertex(position + Vector3.UnitZ * height, color);
                primitiveBatch.AddVertex(position, color);

                for (int i = 0; i < tessellation; i++)
                {
                    float angle = i * MathHelper.TwoPi / tessellation;

                    float dx = (float)Math.Cos(angle);
                    float dy = (float)Math.Sin(angle);

                    Vector3 normal = new Vector3(dx, dy, 0);

                    primitiveBatch.AddVertex(position + normal * radius, color);

                    primitiveBatch.AddIndex(0);
                    primitiveBatch.AddIndex(2 + i);
                    primitiveBatch.AddIndex(2 + (i + 1) % tessellation);

                    primitiveBatch.AddIndex(1);
                    primitiveBatch.AddIndex(2 + (i + 1) % tessellation);
                    primitiveBatch.AddIndex(2 + i);
                }
            }
            primitiveBatch.EndPrimitive();
        }
        
        public static void DrawCylinder(this PrimitiveBatch primitiveBatch, Vector3 position, float height, float radius, int tessellation, Matrix? world, Color color)
        {
            if (tessellation < 3)
                throw new ArgumentOutOfRangeException("tessellation");

            primitiveBatch.BeginPrimitive(PrimitiveType.LineList, null, world);
            {
                primitiveBatch.AddVertex(position + Vector3.UnitZ * height, color);
                primitiveBatch.AddVertex(position, color);

                for (int i = 0; i < tessellation; i++)
                {
                    float angle = i * MathHelper.TwoPi / tessellation;

                    float dx = (float)Math.Cos(angle);
                    float dy = (float)Math.Sin(angle);

                    Vector3 normal = new Vector3(dx, dy, 0);

                    primitiveBatch.AddVertex(position + normal * radius + Vector3.UnitZ * height, color);
                    primitiveBatch.AddVertex(position + normal * radius, color);

                    primitiveBatch.AddIndex(2 + i * 2);
                    primitiveBatch.AddIndex(2 + i * 2 + 1);

                    primitiveBatch.AddIndex(2 + i * 2);
                    primitiveBatch.AddIndex(2 + (i * 2 + 2) % (tessellation * 2));

                    primitiveBatch.AddIndex(2 + i * 2 + 1);
                    primitiveBatch.AddIndex(2 + (i * 2 + 3) % (tessellation * 2));
                }
            }
            primitiveBatch.EndPrimitive();
        }

        public static void DrawSolidCylinder(this PrimitiveBatch primitiveBatch, Vector3 position, float height, float radius, int tessellation, Matrix? world, Color color)
        {
            if (tessellation < 3)
                throw new ArgumentOutOfRangeException("tessellation");

            primitiveBatch.BeginPrimitive(PrimitiveType.TriangleList, null, world);
            {
                primitiveBatch.AddVertex(position + Vector3.UnitZ * height, color);
                primitiveBatch.AddVertex(position, color);

                for (int i = 0; i < tessellation; i++)
                {
                    float angle = i * MathHelper.TwoPi / tessellation;

                    float dx = (float)Math.Cos(angle);
                    float dy = (float)Math.Sin(angle);

                    Vector3 normal = new Vector3(dx, dy, 0);

                    primitiveBatch.AddVertex(position + normal * radius + Vector3.UnitZ * height, color);
                    primitiveBatch.AddVertex(position + normal * radius, color);

                    primitiveBatch.AddIndex(2 + i * 2);
                    primitiveBatch.AddIndex(2 + (i * 2 + 2) % (tessellation * 2));
                    primitiveBatch.AddIndex(2 + i * 2 + 1);

                    primitiveBatch.AddIndex(2 + i * 2 + 1);
                    primitiveBatch.AddIndex(2 + (i * 2 + 2) % (tessellation * 2));
                    primitiveBatch.AddIndex(2 + (i * 2 + 3) % (tessellation * 2));

                    primitiveBatch.AddIndex(0);
                    primitiveBatch.AddIndex(2 + (i * 2 + 2) % (tessellation * 2));
                    primitiveBatch.AddIndex(2 + i * 2);

                    primitiveBatch.AddIndex(1);
                    primitiveBatch.AddIndex(2 + i * 2 + 1);
                    primitiveBatch.AddIndex(2 + (i * 2 + 3) % (tessellation * 2));
                }
            }
            primitiveBatch.EndPrimitive();
        }

        public static void DrawPlane(this PrimitiveBatch primitiveBatch, Plane plane, float size, int tessellation, Matrix? world, Color color)
        {
            Matrix transform = MatrixHelper.CreateRotation(Vector3.UnitZ, plane.Normal) * 
                               Matrix.CreateTranslation(plane.Normal * plane.D);

            if (world.HasValue)
                transform *= world.Value;

            DrawGrid(primitiveBatch, size, size, tessellation, tessellation, transform, color);
        }

        public static void DrawGrid(this PrimitiveBatch primitiveBatch, float step, int countX, int countY, Matrix? world, Color color)
        {
            DrawGrid(primitiveBatch, step * countX, step * countY, countX, countY, world, color);
        }

        public static void DrawGrid(this PrimitiveBatch primitiveBatch, float width, float height, int countX, int countY, Matrix? world, Color color)
        {
            primitiveBatch.BeginPrimitive(PrimitiveType.LineList, null, world);
            {
                float incU = width / countX;
                float incV = height / countY;

                for (int u = 0; u <= countX; u++)
                {
                    primitiveBatch.AddVertex(new Vector3(-height / 2, u * incU - width / 2, 0), color);
                    primitiveBatch.AddVertex(new Vector3(height / 2, u * incU - width / 2, 0), color);
                }

                for (int v = 0; v <= countY; v++)
                {
                    primitiveBatch.AddVertex(new Vector3(v * incV - height / 2, -width / 2, 0), color);
                    primitiveBatch.AddVertex(new Vector3(v * incV - height / 2, height / 2, 0), color);
                }            
            }
            primitiveBatch.EndPrimitive();
        }

        public static void DrawGrid(this PrimitiveBatch primitiveBatch, UniformGrid grid, Matrix? world, Color color)
        {
            if (grid.Position == Vector2.Zero)
                DrawGrid(primitiveBatch, grid.Size.X, grid.Size.Y, grid.SegmentCountX, grid.SegmentCountY, world, color);
            else if (world.HasValue)
                DrawGrid(primitiveBatch, grid.Size.X, grid.Size.Y, grid.SegmentCountX, grid.SegmentCountY, Matrix.CreateTranslation(new Vector3(grid.Position, 0)) * world.Value, color);
            else
                DrawGrid(primitiveBatch, grid.Size.X, grid.Size.Y, grid.SegmentCountX, grid.SegmentCountY, Matrix.CreateTranslation(new Vector3(grid.Position, 0)), color);
        }

        public static void DrawRay(this PrimitiveBatch primitiveBatch, Ray ray, float length, Matrix? world, Color color)
        {
            DrawArrow(primitiveBatch, ray.Position, ray.Position + ray.Direction * length, world, color);
        }

        public static void DrawArrow(this PrimitiveBatch primitiveBatch, Vector3 start, Vector3 end, Matrix? world, Color color)
        {           
            const float Ratio = 0.2f;

            float length = Vector3.Subtract(start, end).Length();
            float width = length * Ratio * 0.8f;
            Vector3 mid = Vector3.Lerp(end, start, Ratio);

            primitiveBatch.DrawLine(null, start, mid, length * 0.04f, null, world, color);

            primitiveBatch.BeginPrimitive(PrimitiveType.TriangleList, null, world);
            {
                Vector3 aa = new Vector3();
                Vector3 ab = new Vector3();
                Vector3 ba = new Vector3();
                Vector3 bb = new Vector3();

                primitiveBatch.CreateBillboard(mid, end, width, out aa, out ab, out ba, out bb);
                primitiveBatch.AddVertex((aa + ab) * 0.5f, color);
                primitiveBatch.AddVertex(bb, color);
                primitiveBatch.AddVertex(ba, color);
            }
            primitiveBatch.EndPrimitive();
        }

        public static void DrawAxis(this PrimitiveBatch primitiveBatch, Matrix world)
        {
            DrawArrow(primitiveBatch, world.Translation, Vector3.Transform(Vector3.UnitX, world), null, Color.Red);
            DrawArrow(primitiveBatch, world.Translation, Vector3.Transform(Vector3.UnitY, world), null, Color.Green);
            DrawArrow(primitiveBatch, world.Translation, Vector3.Transform(Vector3.UnitZ, world), null, Color.Blue);
        }

        public static void DrawAxis(this PrimitiveBatch primitiveBatch, Matrix world, float length)
        {
            Vector3 scale;
            Vector3 translation;
            Quaternion rotation;

            world.Decompose(out scale, out rotation, out translation);
            world = Matrix.CreateFromQuaternion(rotation);
            world.Translation = translation;

            DrawArrow(primitiveBatch, world.Translation, Vector3.Transform(Vector3.UnitX * length, world), null, Color.Red);
            DrawArrow(primitiveBatch, world.Translation, Vector3.Transform(Vector3.UnitY * length, world), null, Color.Green);
            DrawArrow(primitiveBatch, world.Translation, Vector3.Transform(Vector3.UnitZ * length, world), null, Color.Blue);
        }

        public static void DrawAxis(this PrimitiveBatch primitiveBatch, Matrix world, float length, Color colorX, Color colorY, Color colorZ)
        {
            Vector3 scale;
            Vector3 translation;
            Quaternion rotation;

            world.Decompose(out scale, out rotation, out translation);
            world = Matrix.CreateFromQuaternion(rotation);
            world.Translation = translation;

            DrawArrow(primitiveBatch, world.Translation, Vector3.Transform(Vector3.UnitX * length, world), null, colorX);
            DrawArrow(primitiveBatch, world.Translation, Vector3.Transform(Vector3.UnitY * length, world), null, colorY);
            DrawArrow(primitiveBatch, world.Translation, Vector3.Transform(Vector3.UnitZ * length, world), null, colorZ);
        }

        public static void DrawSkeleton(this PrimitiveBatch primitiveBatch, Model model, Matrix? world, Color color)
        {
            primitiveBatch.BeginPrimitive(PrimitiveType.LineList, null, world);
            {
                DrawSkeleton(primitiveBatch, model.Root, Matrix.Identity, world, color);
            }
            primitiveBatch.EndPrimitive();
        }

        private static void DrawSkeleton(this PrimitiveBatch primitiveBatch, ModelBone node, Matrix parentTransform, Matrix? world, Color color)
        {
            Matrix start = parentTransform;
            Matrix end = node.Transform * parentTransform;
            
            if (node.Parent != null)
            {
                if (Vector3.Subtract(end.Translation, start.Translation).LengthSquared() > 0)
                {
                    primitiveBatch.AddVertex(start.Translation, color);
                    primitiveBatch.AddVertex(end.Translation, color);
                }
            }

            foreach (ModelBone child in node.Children)
            {
                DrawSkeleton(primitiveBatch, child, end, world, color);
            }
        }
        
        static Matrix[] bones;

        public static void DrawCollision(this PrimitiveBatch primitiveBatch, Model model, Matrix? world, Color color)
        {
            if (bones == null || bones.Length < model.Bones.Count)
                bones = new Matrix[model.Bones.Count];

            model.CopyAbsoluteBoneTransformsTo(bones);

            Matrix transform = world.HasValue ? world.Value : Matrix.Identity;

            ModelTag tag = model.Tag as ModelTag;

            // Draw collision tree
            if (tag != null && tag.Collision != null)
            {
                transform = bones[model.Meshes[0].ParentBone.Index] * transform;
                Octree<bool> tree = tag.Collision.CollisionTree;

                foreach (OctreeNode<bool> node in tree.Traverse((o) => { return true; }))
                {
                    if (!node.HasChildren && node.Value)
                        primitiveBatch.DrawSolidBox(node.Bounds, transform, color);
                }
            }
            // Draw collision sphere
            else
            {
                foreach (ModelMesh mesh in model.Meshes)
                {
                    primitiveBatch.DrawSolidSphere(mesh.BoundingSphere, 18, bones[mesh.ParentBone.Index] * transform, color);
                }
            }
        }
    }
}
