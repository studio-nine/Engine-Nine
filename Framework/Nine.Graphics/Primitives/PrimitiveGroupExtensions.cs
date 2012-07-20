namespace Nine.Graphics.Primitives
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Contains extension method for <c>PrimitiveBatch</c>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class PrimitiveGroupExtensions
    {
        public static void AddGeometry(this PrimitiveGroup primitiveBatch, IGeometry geometry, Matrix? world, Color color)
        {
            var transform = geometry.Transform;
            if (world.HasValue)
            {
                if (transform.HasValue)
                    transform = transform.Value * world.Value;
                else
                    transform = world;
            }

            primitiveBatch.BeginPrimitive(PrimitiveType.LineList, null, transform);
            {
                foreach (Vector3 position in geometry.Positions)
                {
                    primitiveBatch.AddVertex(position, color);
                }

                for (int i = 0; i < geometry.Indices.Length; i += 3)
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

        public static void AddRectangle(this PrimitiveGroup primitiveBatch, Vector2 min, Vector2 max, Matrix? world, Color color)
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

        public static void AddRectangle(this PrimitiveGroup primitiveBatch, Vector2 min, Vector2 max, Vector3 up, Matrix? world, Color color)
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

        public static void AddBox(this PrimitiveGroup primitiveBatch, Vector3 center, Vector3 size, Matrix? world, Color color)
        {
            AddBox(primitiveBatch, new BoundingBox(center - size / 2, center + size / 2), world, color);
        }

        public static void AddBox(this PrimitiveGroup primitiveBatch, BoundingBox boundingBox, Matrix? world, Color color)
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

        public static void AddSolidBox(this PrimitiveGroup primitiveBatch, Vector3 center, Vector3 size, Matrix? world, Color color)
        {
            AddSolidBox(primitiveBatch, new BoundingBox(center - size / 2, center + size / 2), world, color);
        }

        public static void AddSolidBox(this PrimitiveGroup primitiveBatch, BoundingBox boundingBox, Matrix? world, Color color)
        {
            Vector3[] corners = boundingBox.GetCorners();

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

                primitiveBatch.AddIndex(0);
                primitiveBatch.AddIndex(1);
                primitiveBatch.AddIndex(2);
                primitiveBatch.AddIndex(3);
                primitiveBatch.AddIndex(0);
                primitiveBatch.AddIndex(2);

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

        public static void AddCircle(this PrimitiveGroup primitiveBatch, Vector3 center, float radius, int tessellation, Matrix? world, Color color)
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

        public static void AddCircle(this PrimitiveGroup primitiveBatch, Vector3 center, float radius, Vector3 up, int tessellation, Matrix? world, Color color)
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

        public static void AddSphere(this PrimitiveGroup primitiveBatch, Vector3 center, float radius, int tessellation, Matrix? world, Color color)
        {
            AddSphere(primitiveBatch, new BoundingSphere(center, radius), tessellation, world, color);
        }

        public static void AddSphere(this PrimitiveGroup primitiveBatch, BoundingSphere boundingSphere, int tessellation, Matrix? world, Color color)
        {
            primitiveBatch.BeginPrimitive(PrimitiveType.LineList, null, world);
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

                        primitiveBatch.AddIndex(1 + i * horizontalSegments + nextJ);
                        primitiveBatch.AddIndex(1 + nextI * horizontalSegments + nextJ);

                        primitiveBatch.AddIndex(1 + nextI * horizontalSegments + j);
                        primitiveBatch.AddIndex(1 + nextI * horizontalSegments + j);
                    }
                }

                // Create a fan connecting the top vertex to the top latitude ring.
                for (int i = 0; i < horizontalSegments; i++)
                {
                    primitiveBatch.AddIndex(currentVertex - 1);
                    primitiveBatch.AddIndex(currentVertex - 2 - (i + 1) % horizontalSegments);
                }
            }
            primitiveBatch.EndPrimitive();
        }

        public static void AddSolidSphere(this PrimitiveGroup primitiveBatch, Vector3 center, float radius, int tessellation, Matrix? world, Color color)
        {
            AddSolidSphere(primitiveBatch, new BoundingSphere(center, radius), tessellation, world, color);
        }

        public static void AddSolidSphere(this PrimitiveGroup primitiveBatch, BoundingSphere boundingSphere, int tessellation, Matrix? world, Color color)
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

        public static void AddFrustum(this PrimitiveGroup primitiveBatch, BoundingFrustum boundingFrustum, Matrix? world, Color color)
        {
            Vector3[] corners = boundingFrustum.GetCorners();

            // near plane
            primitiveBatch.AddLine(corners[0], corners[1], color);
            primitiveBatch.AddLine(corners[1], corners[2], color);
            primitiveBatch.AddLine(corners[2], corners[3], color);
            primitiveBatch.AddLine(corners[3], corners[0], color);

            // connections
            primitiveBatch.AddLine(corners[0], corners[4], color);
            primitiveBatch.AddLine(corners[1], corners[5], color);
            primitiveBatch.AddLine(corners[2], corners[6], color);
            primitiveBatch.AddLine(corners[3], corners[7], color);

            // far plane
            primitiveBatch.AddLine(corners[4], corners[5], color);
            primitiveBatch.AddLine(corners[5], corners[6], color);
            primitiveBatch.AddLine(corners[6], corners[7], color);
            primitiveBatch.AddLine(corners[7], corners[4], color);
        }

        public static void AddSolidFrustum(this PrimitiveGroup primitiveBatch, BoundingFrustum boundingFrustum, Matrix? world, Color color)
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
        
        public static void AddCentrum(this PrimitiveGroup primitiveBatch, Vector3 position, float height, float radius, int tessellation, Matrix? world, Color color)
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

        public static void AddSolidCentrum(this PrimitiveGroup primitiveBatch, Vector3 position, float height, float radius, int tessellation, Matrix? world, Color color)
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
        
        public static void AddCylinder(this PrimitiveGroup primitiveBatch, Vector3 position, float height, float radius, int tessellation, Matrix? world, Color color)
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

        public static void AddSolidCylinder(this PrimitiveGroup primitiveBatch, Vector3 position, float height, float radius, int tessellation, Matrix? world, Color color)
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

        public static void AddPlane(this PrimitiveGroup primitiveBatch, Microsoft.Xna.Framework.Plane plane, float size, int tessellation, Matrix? world, Color color)
        {
            Matrix transform = MatrixHelper.CreateRotation(Vector3.UnitZ, plane.Normal) * 
                               Matrix.CreateTranslation(plane.Normal * plane.D);

            if (world.HasValue)
                transform *= world.Value;

            AddGrid(primitiveBatch, 0, 0, 0, size, size, tessellation, tessellation, transform, color);
        }

        public static void AddGrid(this PrimitiveGroup primitiveBatch, float step, int countX, int countY, Matrix? world, Color color)
        {
            AddGrid(primitiveBatch, -step * countX * 0.5f, -step * countY * 0.5f, 0, step * countX, step * countY, countX, countY, world, color);
        }

        public static void AddGrid(this PrimitiveGroup primitiveBatch, float x, float y, float z, float step, int countX, int countY, Matrix? world, Color color)
        {
            AddGrid(primitiveBatch, x, y, z, step * countX, step * countY, countX, countY, world, color);
        }

        public static void AddGrid(this PrimitiveGroup primitiveBatch, float x, float y, float z, float width, float height, int countX, int countY, Matrix? world, Color color)
        {
            primitiveBatch.BeginPrimitive(PrimitiveType.LineList, null, world);
            {
                float incU = width / countX;
                float incV = height / countY;

                for (int u = 0; u <= countX; u++)
                {
                    primitiveBatch.AddVertex(new Vector3(x + 0, y + u * incU, z), color);
                    primitiveBatch.AddVertex(new Vector3(x + height, y + u * incU, z), color);
                }

                for (int v = 0; v <= countY; v++)
                {
                    primitiveBatch.AddVertex(new Vector3(x + v * incV, y + 0, z), color);
                    primitiveBatch.AddVertex(new Vector3(x + v * incV, y + width, z), color);
                }
            }
            primitiveBatch.EndPrimitive();
        }

        public static void AddGrid(this PrimitiveGroup primitiveBatch, UniformGrid grid, Matrix? world, Color color)
        {
            AddGrid(primitiveBatch, grid.Position.X, grid.Position.Y, 0, grid.Size.X, grid.Size.Y, grid.SegmentCountX, grid.SegmentCountY, world, color);
        }

        public static void AddRay(this PrimitiveGroup primitiveBatch, Ray ray, float length, Matrix? world, Color color, Vector3 cameraPosition)
        {
            AddArrow(primitiveBatch, ray.Position, ray.Position + ray.Direction * length, world, color, cameraPosition);
        }

        public static void AddArrow(this PrimitiveGroup primitiveBatch, Vector3 start, Vector3 end, Matrix? world, Color color, Vector3 cameraPosition)
        {           
            const float Ratio = 0.2f;

            float length = Vector3.Subtract(start, end).Length();
            float width = length * Ratio * 0.8f;
            Vector3 mid = Vector3.Lerp(end, start, Ratio);

            primitiveBatch.AddConstrainedBillboard(null, start, mid, length * 0.04f, null, world, color, cameraPosition);

            primitiveBatch.BeginPrimitive(PrimitiveType.TriangleList, null, world);
            {
                Vector3 aa = new Vector3();
                Vector3 ab = new Vector3();
                Vector3 ba = new Vector3();
                Vector3 bb = new Vector3();

                CreateBillboard(mid, end, width, cameraPosition, out aa, out ab, out ba, out bb);
                primitiveBatch.AddVertex((aa + ab) * 0.5f, color);
                primitiveBatch.AddVertex(bb, color);
                primitiveBatch.AddVertex(ba, color);
            }
            primitiveBatch.EndPrimitive();
        }

        public static void AddAxis(this PrimitiveGroup primitiveBatch, Matrix world, Vector3 cameraPosition)
        {
            AddArrow(primitiveBatch, world.Translation, Vector3.Transform(Vector3.UnitX, world), null, new Color(255, 0, 0), cameraPosition);
            AddArrow(primitiveBatch, world.Translation, Vector3.Transform(Vector3.UnitY, world), null, new Color(0, 255, 0), cameraPosition);
            AddArrow(primitiveBatch, world.Translation, Vector3.Transform(Vector3.UnitZ, world), null, new Color(0, 0, 255), cameraPosition);
        }

        public static void AddAxis(this PrimitiveGroup primitiveBatch, Matrix world, float length, Vector3 cameraPosition)
        {
            Vector3 scale;
            Vector3 translation;
            Quaternion rotation;

            world.Decompose(out scale, out rotation, out translation);
            world = Matrix.CreateFromQuaternion(rotation);
            world.Translation = translation;

            AddArrow(primitiveBatch, world.Translation, Vector3.Transform(Vector3.UnitX * length, world), null, new Color(255, 0, 0), cameraPosition);
            AddArrow(primitiveBatch, world.Translation, Vector3.Transform(Vector3.UnitY * length, world), null, new Color(0, 255, 0), cameraPosition);
            AddArrow(primitiveBatch, world.Translation, Vector3.Transform(Vector3.UnitZ * length, world), null, new Color(0, 0, 255), cameraPosition);
        }

        public static void AddAxis(this PrimitiveGroup primitiveBatch, Matrix world, float length, Color colorX, Color colorY, Color colorZ, Vector3 cameraPosition)
        {
            Vector3 scale;
            Vector3 translation;
            Quaternion rotation;

            world.Decompose(out scale, out rotation, out translation);
            world = Matrix.CreateFromQuaternion(rotation);
            world.Translation = translation;

            AddArrow(primitiveBatch, world.Translation, Vector3.Transform(Vector3.UnitX * length, world), null, colorX, cameraPosition);
            AddArrow(primitiveBatch, world.Translation, Vector3.Transform(Vector3.UnitY * length, world), null, colorY, cameraPosition);
            AddArrow(primitiveBatch, world.Translation, Vector3.Transform(Vector3.UnitZ * length, world), null, colorZ, cameraPosition);
        }

        public static void AddSkeleton(this PrimitiveGroup primitiveBatch, Microsoft.Xna.Framework.Graphics.Model model, Matrix? world, Color color)
        {
            primitiveBatch.BeginPrimitive(PrimitiveType.LineList, null, world);
            {
                AddSkeleton(primitiveBatch, model.Root, Matrix.Identity, world, color);
            }
            primitiveBatch.EndPrimitive();
        }

        private static void AddSkeleton(this PrimitiveGroup primitiveBatch, ModelBone node, Matrix parentTransform, Matrix? world, Color color)
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
                AddSkeleton(primitiveBatch, child, end, world, color);
            }
        }

        public static void AddSkeleton(this PrimitiveGroup primitiveBatch, Skeleton skeleton, Matrix? world, Color color)
        {
            primitiveBatch.BeginPrimitive(PrimitiveType.LineList, null, world);
            {
                AddSkeleton(primitiveBatch, skeleton, 0, Matrix.Identity, color);
            }
            primitiveBatch.EndPrimitive();
        }

        private static void AddSkeleton(this PrimitiveGroup primitiveBatch, Skeleton skeleton, int bone, Matrix parentTransform, Color color)
        {
            Matrix start = parentTransform;
            Matrix end = skeleton.GetBoneTransform(bone) * parentTransform;

            if (Vector3.Subtract(end.Translation, start.Translation).LengthSquared() > 0)
            {
                primitiveBatch.AddVertex(start.Translation, color);
                primitiveBatch.AddVertex(end.Translation, color);
                
                Vector3.Subtract(end.Translation, start.Translation).Length();
            }

            foreach (int child in skeleton.GetChildBones(bone))
            {
                AddSkeleton(primitiveBatch, skeleton, child, end, color);
            }
        }


        static Matrix[] bones;

        public static void AddCollision(this PrimitiveGroup primitiveBatch, Microsoft.Xna.Framework.Graphics.Model model, Matrix? world, Color color)
        {
            if (bones == null || bones.Length < model.Bones.Count)
                bones = new Matrix[model.Bones.Count];

            model.CopyAbsoluteBoneTransformsTo(bones);

            Matrix transform = world.HasValue ? world.Value : Matrix.Identity;

            ModelTag tag = model.Tag as ModelTag;

            // Add collision tree
            if (tag != null && tag.Collision != null)
            {
                transform = bones[model.Meshes[0].ParentBone.Index] * transform;
                Octree<bool> tree = tag.Collision.CollisionTree;

                // TODO: This delegate will generate garbage.
                //       But as this method is generally used for debug Adding, it shouldn't matter if it is not optimized.
                tree.Traverse(node =>
                {
                    if (!node.HasChildren && node.Value)
                        primitiveBatch.AddSolidBox(node.Bounds, transform, color);
                    return TraverseOptions.Continue;
                });
            }
            // Add collision sphere
            else
            {
                for (int i = 0; i < model.Meshes.Count; i++)
                {
                    var mesh = model.Meshes[i];
                    primitiveBatch.AddSolidSphere(mesh.BoundingSphere, 18, bones[mesh.ParentBone.Index] * transform, color);
                }
            }
        }

        public static void AddLineSegment(this PrimitiveGroup primitiveBatch, LineSegment line, float z, Matrix? world, Color color)
        {
            primitiveBatch.AddLine(new Vector3(line.Start, z), new Vector3(line.End, z), world, color);
        }

        public static void AddLineSegment(this PrimitiveGroup primitiveBatch, LineSegment line, float z, float arrowLength, Matrix? world, Color color, Vector3 cameraPosition)
        {
            primitiveBatch.AddLine(new Vector3(line.Start, z), new Vector3(line.End, z), world, color);
            primitiveBatch.AddArrow(new Vector3(line.Center, z), new Vector3(line.Center + line.Normal * arrowLength, z), world, color, cameraPosition);
        }

        public static void AddBillboard(this PrimitiveGroup primitiveBatch, Texture2D texture, Vector3 position, float size, Color color, Vector3 cameraPosition)
        {
            AddBillboard(primitiveBatch, texture, ref position, size, size, 0, null, null, color, ref cameraPosition);
        }

        public static void AddBillboard(this PrimitiveGroup primitiveBatch, Texture2D texture, ref Vector3 position, float width, float height, float rotation, Matrix? textureTransform, Matrix? world, Color color, ref Vector3 cameraPosition)
        {
            //      aa --- ab
            //       |     |
            //       |     |
            //      ba --- bb
            VertexPositionColorTexture aa;
            VertexPositionColorTexture ab;
            VertexPositionColorTexture ba;
            VertexPositionColorTexture bb;

            Vector3 up = Vector3.Up;
            Matrix transform = new Matrix();

            if (rotation != 0)
            {
                Matrix billboard = new Matrix();
                Matrix.CreateRotationZ(rotation, out transform);
                Matrix.CreateBillboard(ref position, ref cameraPosition, ref up, null, out billboard);
                Matrix.Multiply(ref transform, ref billboard, out transform);
            }
            else
            {
                Matrix.CreateBillboard(ref position, ref cameraPosition, ref up, null, out transform);
            }

            if (float.IsNaN(transform.M11))
            {
                transform = Matrix.Identity;
                transform.M11 = -1;
            }

            transform.M41 = position.X;
            transform.M42 = position.Y;
            transform.M43 = position.Z;

            aa.Position.X = +width * 0.5f; aa.Position.Y = +height * 0.5f; aa.Position.Z = 0;
            ab.Position.X = -width * 0.5f; ab.Position.Y = +height * 0.5f; ab.Position.Z = 0;
            ba.Position.X = +width * 0.5f; ba.Position.Y = -height * 0.5f; ba.Position.Z = 0;
            bb.Position.X = -width * 0.5f; bb.Position.Y = -height * 0.5f; bb.Position.Z = 0;

#if XBOX || WINDOWS_PHONE
            aa.Position = new Vector3();
            ab.Position = new Vector3();
            ba.Position = new Vector3();
            bb.Position = new Vector3();
#endif
            Vector3.Transform(ref aa.Position, ref transform, out aa.Position);
            Vector3.Transform(ref ab.Position, ref transform, out ab.Position);
            Vector3.Transform(ref ba.Position, ref transform, out ba.Position);
            Vector3.Transform(ref bb.Position, ref transform, out bb.Position);

            if (textureTransform != null)
            {
                aa.TextureCoordinate = TextureTransform.Transform(textureTransform.Value, Vector2.Zero);
                ab.TextureCoordinate = TextureTransform.Transform(textureTransform.Value, Vector2.UnitX);
                ba.TextureCoordinate = TextureTransform.Transform(textureTransform.Value, Vector2.UnitY);
                bb.TextureCoordinate = TextureTransform.Transform(textureTransform.Value, Vector2.One);
            }
            else
            {
                aa.TextureCoordinate = Vector2.Zero;
                ab.TextureCoordinate = Vector2.UnitX;
                ba.TextureCoordinate = Vector2.UnitY;
                bb.TextureCoordinate = Vector2.One;
            }

            aa.Color = ab.Color =
            ba.Color = bb.Color = color;

            primitiveBatch.BeginPrimitive(PrimitiveType.TriangleList, texture, world);
            {
                // Add new vertices and indices
                primitiveBatch.AddIndex((ushort)(0));
                primitiveBatch.AddIndex((ushort)(1));
                primitiveBatch.AddIndex((ushort)(2));
                primitiveBatch.AddIndex((ushort)(1));
                primitiveBatch.AddIndex((ushort)(3));
                primitiveBatch.AddIndex((ushort)(2));

                primitiveBatch.AddVertex(ref aa);
                primitiveBatch.AddVertex(ref ab);
                primitiveBatch.AddVertex(ref ba);
                primitiveBatch.AddVertex(ref bb);
            }
            primitiveBatch.EndPrimitive();
        }

        public static void AddBillboard(this PrimitiveGroup primitiveBatch, Texture2D texture, ref Vector3 position, float width, float height, float rotation, Matrix? textureTransform, Matrix? world, Color color, ref Matrix viewInverse)
        {
            // http://www.flipcode.com/archives/Really_Fast_Billboarding_Alignment_Without_VP_Tricks.shtml

            //      aa --- ab
            //       |     |
            //       |     |
            //      ba --- bb
            VertexPositionColorTexture aa;
            VertexPositionColorTexture ab;
            VertexPositionColorTexture ba;
            VertexPositionColorTexture bb;

            aa.Position.X = +width * 0.5f; aa.Position.Y = +height * 0.5f; aa.Position.Z = 0;
            ab.Position.X = -width * 0.5f; ab.Position.Y = +height * 0.5f; ab.Position.Z = 0;
            ba.Position.X = +width * 0.5f; ba.Position.Y = -height * 0.5f; ba.Position.Z = 0;
            bb.Position.X = -width * 0.5f; bb.Position.Y = -height * 0.5f; bb.Position.Z = 0;

            if (rotation != null)
            {
                float x, y;
                float cos = (float)Math.Cos(rotation);
                float sin = (float)Math.Sin(rotation);

                x = aa.Position.X; y = aa.Position.Y;
                aa.Position.X = x * cos + y * sin;
                aa.Position.Y = y * cos - x * sin;

                x = ab.Position.X; y = ab.Position.Y;
                ab.Position.X = x * cos + y * sin;
                ab.Position.Y = y * cos - x * sin;

                x = ba.Position.X; y = ba.Position.Y;
                ba.Position.X = x * cos + y * sin;
                ba.Position.Y = y * cos - x * sin;

                x = bb.Position.X; y = bb.Position.Y;
                bb.Position.X = x * cos + y * sin;
                bb.Position.Y = y * cos - x * sin;
            }

#if XBOX || WINDOWS_PHONE
            aa.Position = new Vector3();
            ab.Position = new Vector3();
            ba.Position = new Vector3();
            bb.Position = new Vector3();
#endif
            Vector3.TransformNormal(ref aa.Position, ref viewInverse, out aa.Position);
            Vector3.TransformNormal(ref ab.Position, ref viewInverse, out ab.Position);
            Vector3.TransformNormal(ref ba.Position, ref viewInverse, out ba.Position);
            Vector3.TransformNormal(ref bb.Position, ref viewInverse, out bb.Position);

            aa.Position += position;
            ab.Position += position;
            ba.Position += position;
            bb.Position += position;

            if (textureTransform != null)
            {
                aa.TextureCoordinate = TextureTransform.Transform(textureTransform.Value, Vector2.Zero);
                ab.TextureCoordinate = TextureTransform.Transform(textureTransform.Value, Vector2.UnitX);
                ba.TextureCoordinate = TextureTransform.Transform(textureTransform.Value, Vector2.UnitY);
                bb.TextureCoordinate = TextureTransform.Transform(textureTransform.Value, Vector2.One);
            }
            else
            {
                aa.TextureCoordinate = Vector2.Zero;
                ab.TextureCoordinate = Vector2.UnitX;
                ba.TextureCoordinate = Vector2.UnitY;
                bb.TextureCoordinate = Vector2.One;
            }

            aa.Color = ab.Color =
            ba.Color = bb.Color = color;

            primitiveBatch.BeginPrimitive(PrimitiveType.TriangleList, texture, world);
            {
                // Add new vertices and indices
                primitiveBatch.AddIndex((ushort)(0));
                primitiveBatch.AddIndex((ushort)(2));
                primitiveBatch.AddIndex((ushort)(1));
                primitiveBatch.AddIndex((ushort)(1));
                primitiveBatch.AddIndex((ushort)(2));
                primitiveBatch.AddIndex((ushort)(3));

                primitiveBatch.AddVertex(ref aa);
                primitiveBatch.AddVertex(ref ab);
                primitiveBatch.AddVertex(ref ba);
                primitiveBatch.AddVertex(ref bb);
            }
            primitiveBatch.EndPrimitive();
        }

        public static void AddLine(this PrimitiveGroup primitiveBatch, Vector3 v1, Vector3 v2, Color color)
        {
            AddLine(primitiveBatch, v1, v2, null, color);
        }

        public static void AddLine(this PrimitiveGroup primitiveBatch, Vector3 v1, Vector3 v2, Matrix? world, Color color)
        {
            primitiveBatch.BeginPrimitive(PrimitiveType.LineList, null, world);
            {
                primitiveBatch.AddVertex(v1, color);
                primitiveBatch.AddVertex(v2, color);
            }
            primitiveBatch.EndPrimitive();
        }

        public static void AddLine(this PrimitiveGroup primitiveBatch, IEnumerable<Vector3> lineStrip, Matrix? world, Color color)
        {
            primitiveBatch.BeginPrimitive(PrimitiveType.LineList, null, world);
            {
                foreach (Vector3 position in lineStrip)
                {
                    primitiveBatch.AddVertex(position, color);
                }
            }
            primitiveBatch.EndPrimitive();
        }

        public static void AddConstrainedBillboard(this PrimitiveGroup primitiveBatch, Texture2D texture, Vector3 start, Vector3 end, float width, Matrix? textureTransform, Matrix? world, Color color, Vector3 cameraPosition)
        {
            //      aa --- ab
            //       |     |
            //       |     |
            //      ba --- bb
            VertexPositionColorTexture aa;
            VertexPositionColorTexture ab;
            VertexPositionColorTexture ba;
            VertexPositionColorTexture bb;

            CreateBillboard(start, end, width, cameraPosition, out aa.Position, out ab.Position,
                                                               out ba.Position, out bb.Position);

            if (textureTransform != null)
            {
                aa.TextureCoordinate = TextureTransform.Transform(textureTransform.Value, Vector2.One);
                ab.TextureCoordinate = TextureTransform.Transform(textureTransform.Value, Vector2.UnitX);
                ba.TextureCoordinate = TextureTransform.Transform(textureTransform.Value, Vector2.UnitY);
                bb.TextureCoordinate = TextureTransform.Transform(textureTransform.Value, Vector2.Zero);
            }
            else
            {
                aa.TextureCoordinate = Vector2.One;
                ab.TextureCoordinate = Vector2.UnitX;
                ba.TextureCoordinate = Vector2.UnitY;
                bb.TextureCoordinate = Vector2.Zero;
            }

            aa.Color = ab.Color =
            ba.Color = bb.Color = color;

            primitiveBatch.BeginPrimitive(PrimitiveType.TriangleList, texture, world);
            {
                // Add new vertices and indices
                primitiveBatch.AddIndex((ushort)(0));
                primitiveBatch.AddIndex((ushort)(1));
                primitiveBatch.AddIndex((ushort)(2));
                primitiveBatch.AddIndex((ushort)(1));
                primitiveBatch.AddIndex((ushort)(3));
                primitiveBatch.AddIndex((ushort)(2));

                primitiveBatch.AddVertex(ref aa);
                primitiveBatch.AddVertex(ref ab);
                primitiveBatch.AddVertex(ref ba);
                primitiveBatch.AddVertex(ref bb);
            }
            primitiveBatch.EndPrimitive();
        }

        public static void AddConstrainedBillboard(this PrimitiveGroup primitiveBatch, Texture2D texture, IEnumerable<Vector3> lineStrip, float width, Matrix? textureTransform, Matrix? world, Color color, Vector3 cameraPosition)
        {
            if (lineStrip == null)
                throw new ArgumentNullException("lineStrip");


            //      aa --- ab
            //       |     |
            //       |     |
            //      ba --- bb
            VertexPositionColorTexture aa;
            VertexPositionColorTexture ab;
            VertexPositionColorTexture ba;
            VertexPositionColorTexture bb;

            aa.Color = ab.Color =
            ba.Color = bb.Color = color;

            aa.Position = ab.Position =
            ba.Position = bb.Position = Vector3.Zero;


            // We want the texture to uniformly distribute on the line
            // even if each line segment may have different length.
            int i = 0;
            float totalLength = 0;
            float percentage = 0;

            IEnumerator<Vector3> enumerator = lineStrip.GetEnumerator();
            enumerator.Reset();
            enumerator.MoveNext();
            Vector3 previous = enumerator.Current;
            while (enumerator.MoveNext())
            {
                totalLength += Vector3.Subtract(enumerator.Current, previous).Length();
                previous = enumerator.Current;
            }

            primitiveBatch.BeginPrimitive(PrimitiveType.TriangleList, texture, world);
            {
                enumerator.Reset();
                enumerator.MoveNext();

                int vertexCount = 0;
                Vector3 start = enumerator.Current;
                Vector3 lastSegment1 = Vector3.Zero;
                Vector3 lastSegment2 = Vector3.Zero;

                while (enumerator.MoveNext())
                {
                    CreateBillboard(start, enumerator.Current, width, cameraPosition, out aa.Position, out ab.Position,
                                                                                      out ba.Position, out bb.Position);

                    if (textureTransform != null)
                    {
                        ba.TextureCoordinate = TextureTransform.Transform(textureTransform.Value, new Vector2(percentage, 1));
                        bb.TextureCoordinate = TextureTransform.Transform(textureTransform.Value, new Vector2(percentage, 0));
                    }
                    else
                    {
                        ba.TextureCoordinate = new Vector2(percentage, 1);
                        bb.TextureCoordinate = new Vector2(percentage, 0);
                    }

                    percentage += Vector3.Subtract(enumerator.Current, start).Length() / totalLength;

                    if (i > 1)
                    {
                        // Connect adjacent segments
                        ba.Position = (ba.Position + lastSegment1) / 2;
                        bb.Position = (bb.Position + lastSegment2) / 2;

                        // Adjust the connection points to the specified width
                        Vector3 append = Vector3.Subtract(bb.Position, ba.Position);

                        append.Normalize();
                        append *= width / 2;

                        ba.Position = start - append;
                        bb.Position = start + append;
                    }

                    lastSegment1 = aa.Position;
                    lastSegment2 = ab.Position;

                    int startIndex = vertexCount;

                    primitiveBatch.AddIndex((ushort)(startIndex + 0));
                    primitiveBatch.AddIndex((ushort)(startIndex + 3));
                    primitiveBatch.AddIndex((ushort)(startIndex + 1));
                    primitiveBatch.AddIndex((ushort)(startIndex + 0));
                    primitiveBatch.AddIndex((ushort)(startIndex + 2));
                    primitiveBatch.AddIndex((ushort)(startIndex + 3));

                    primitiveBatch.AddVertex(ref ba);
                    primitiveBatch.AddVertex(ref bb);

                    vertexCount += 2;

                    i++;

                    start = enumerator.Current;
                }

                // Last segment
                if (textureTransform != null)
                {
                    aa.TextureCoordinate = TextureTransform.Transform(textureTransform.Value, Vector2.One);
                    ab.TextureCoordinate = TextureTransform.Transform(textureTransform.Value, Vector2.UnitX);
                }
                else
                {
                    aa.TextureCoordinate = Vector2.One;
                    ab.TextureCoordinate = Vector2.UnitX;
                }

                primitiveBatch.AddVertex(ref aa);
                primitiveBatch.AddVertex(ref ab);
            }
            primitiveBatch.EndPrimitive();
        }

        public static void Add(this PrimitiveGroup primitiveBatch, IEnumerable<Vector3> vertices, IEnumerable<int> indices, Matrix? world, Color color)
        {
            if (vertices == null)
                throw new ArgumentNullException("vertices");

            primitiveBatch.BeginPrimitive(PrimitiveType.TriangleList, null, world);
            {
                foreach (Vector3 position in vertices)
                    primitiveBatch.AddVertex(position, color);

                if (indices != null)
                {
                    foreach (var index in indices)
                        primitiveBatch.AddIndex(index);
                }
            }
            primitiveBatch.EndPrimitive();
        }

        public static void Add(this PrimitiveGroup primitiveBatch, PrimitiveType primitiveType, Texture2D texture, IEnumerable<VertexPositionColorTexture> vertices, IEnumerable<ushort> indices, Matrix? world)
        {
            if (vertices == null)
                throw new ArgumentNullException("vertices");

            primitiveBatch.BeginPrimitive(primitiveType, texture, world);
            {
                foreach (var vertex in vertices)
                    ;// TODO: primitiveBatch.AddVertex(ref vertex);

                if (indices != null)
                {
                    foreach (var index in indices)
                        primitiveBatch.AddIndex(index);
                }
            }
            primitiveBatch.EndPrimitive();
        }

        /// <summary>
        /// Matrix.CreateConstraintBillboard has a sudden change effect that is not
        /// desirable, so rolling out our own version.
        /// </summary>
        internal static void CreateBillboard(Vector3 start, Vector3 end, float width, Vector3 cameraPosition,
                                               out Vector3 aa, out Vector3 ab,
                                               out Vector3 ba, out Vector3 bb)
        {
            // Compute billboard facing
            Vector3 v1 = Vector3.Subtract(end, start);
            Vector3 v2 = Vector3.Subtract(cameraPosition, start);

            Vector3 right = Vector3.Cross(v1, v2);

            right.Normalize();
            right *= width / 2;

            // Compute destination vertices
            aa = end - right;
            ab = end + right;
            ba = start - right;
            bb = start + right;
        }
    }
}
