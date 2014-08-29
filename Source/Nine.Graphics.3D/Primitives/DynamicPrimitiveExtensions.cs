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
    public static class DynamicPrimitiveExtensions
    {
        public static void AddGeometry(this DynamicPrimitive dynamicPrimitive, IGeometry geometry, Color color)
        {
            Vector3[] positions;
            ushort[] indices;
            geometry.TryGetTriangles(out positions, out indices);
            var transform = geometry.Transform;

            dynamicPrimitive.BeginPrimitive(PrimitiveType.LineList, null, transform);
            {
                foreach (Vector3 position in positions)
                {
                    dynamicPrimitive.AddVertex(position, color);
                }

                for (int i = 0; i < indices.Length; i += 3)
                {
                    dynamicPrimitive.AddIndex(indices[i + 0]);
                    dynamicPrimitive.AddIndex(indices[i + 1]);
                    dynamicPrimitive.AddIndex(indices[i + 1]);
                    dynamicPrimitive.AddIndex(indices[i + 2]);
                    dynamicPrimitive.AddIndex(indices[i + 2]);
                    dynamicPrimitive.AddIndex(indices[i + 0]);
                }
            }
            dynamicPrimitive.EndPrimitive();
        }

        public static void AddBox(this DynamicPrimitive dynamicPrimitive, Vector3 center, Vector3 size, Matrix? world, Color color, float lineWidth)
        {
            AddBox(dynamicPrimitive, new BoundingBox(center - size / 2, center + size / 2), world, color, lineWidth);
        }

        public static void AddBox(this DynamicPrimitive dynamicPrimitive, BoundingBox boundingBox, Matrix? world, Color color, float lineWidth)
        {
            dynamicPrimitive.BeginPrimitive(PrimitiveType.LineList, null, world, lineWidth);
            {
                Vector3[] corners = boundingBox.GetCorners();

                dynamicPrimitive.AddVertex(corners[0], color);
                dynamicPrimitive.AddVertex(corners[1], color);

                dynamicPrimitive.AddVertex(corners[1], color);
                dynamicPrimitive.AddVertex(corners[2], color);

                dynamicPrimitive.AddVertex(corners[2], color);
                dynamicPrimitive.AddVertex(corners[3], color);

                dynamicPrimitive.AddVertex(corners[3], color);
                dynamicPrimitive.AddVertex(corners[0], color);

                dynamicPrimitive.AddVertex(corners[4], color);
                dynamicPrimitive.AddVertex(corners[5], color);

                dynamicPrimitive.AddVertex(corners[5], color);
                dynamicPrimitive.AddVertex(corners[6], color);

                dynamicPrimitive.AddVertex(corners[6], color);
                dynamicPrimitive.AddVertex(corners[7], color);

                dynamicPrimitive.AddVertex(corners[7], color);
                dynamicPrimitive.AddVertex(corners[4], color);

                dynamicPrimitive.AddVertex(corners[0], color);
                dynamicPrimitive.AddVertex(corners[4], color);

                dynamicPrimitive.AddVertex(corners[1], color);
                dynamicPrimitive.AddVertex(corners[5], color);

                dynamicPrimitive.AddVertex(corners[2], color);
                dynamicPrimitive.AddVertex(corners[6], color);

                dynamicPrimitive.AddVertex(corners[3], color);
                dynamicPrimitive.AddVertex(corners[7], color);
            }
            dynamicPrimitive.EndPrimitive();
        }

        public static void AddSolidBox(this DynamicPrimitive dynamicPrimitive, Vector3 center, Vector3 size, Matrix? world, Color color)
        {
            AddSolidBox(dynamicPrimitive, new BoundingBox(center - size / 2, center + size / 2), world, color);
        }

        public static void AddSolidBox(this DynamicPrimitive dynamicPrimitive, BoundingBox boundingBox, Matrix? world, Color color)
        {
            Vector3[] corners = boundingBox.GetCorners();

            dynamicPrimitive.BeginPrimitive(PrimitiveType.TriangleList, null, world);
            {
                for (int i = 0; i < BoundingBox.CornerCount; ++i)
                    dynamicPrimitive.AddVertex(ref corners[i], color);

                for (int i = 0; i < BoundingBoxExtensions.TriangleIndices.Length; ++i)
                    dynamicPrimitive.AddIndex(BoundingBoxExtensions.TriangleIndices[i]);
            }
            dynamicPrimitive.EndPrimitive();
        }

        public static void AddSphere(this DynamicPrimitive dynamicPrimitive, Vector3 center, float radius, Color color, float lineWidth)
        {
            AddSphere(dynamicPrimitive, new BoundingSphere(center, radius), 4, null, color, lineWidth);
        }

        public static void AddSphere(this DynamicPrimitive dynamicPrimitive, Vector3 center, float radius, int tessellation, Matrix? world, Color color, float lineWidth)
        {
            AddSphere(dynamicPrimitive, new BoundingSphere(center, radius), tessellation, world, color, lineWidth);
        }

        public static void AddSphere(this DynamicPrimitive dynamicPrimitive, BoundingSphere boundingSphere, int tessellation, Matrix? world, Color color, float lineWidth)
        {
            dynamicPrimitive.BeginPrimitive(PrimitiveType.LineList, null, world, lineWidth);
            {
                if (tessellation < 3)
                    throw new ArgumentOutOfRangeException("tessellation");

                int currentVertex = 2;
                int verticalSegments = tessellation;
                int horizontalSegments = tessellation;

                // Start with a single vertex at the bottom of the sphere.
                dynamicPrimitive.AddVertex(Vector3.Down * boundingSphere.Radius + boundingSphere.Center, color);

                // Create rings of vertices at progressively higher latitudes.
                for (int i = 0; i < verticalSegments - 1; ++i)
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

                        dynamicPrimitive.AddVertex(normal * boundingSphere.Radius + boundingSphere.Center, color);
                        currentVertex++;
                    }
                }

                // Finish with a single vertex at the top of the sphere.
                dynamicPrimitive.AddVertex(Vector3.Up * boundingSphere.Radius + boundingSphere.Center, color);

                // Create a fan connecting the bottom vertex to the bottom latitude ring.
                for (int i = 0; i < horizontalSegments; ++i)
                {
                    dynamicPrimitive.AddIndex(0);
                    dynamicPrimitive.AddIndex(1 + (i + 1) % horizontalSegments);
                }

                // Fill the sphere body with triangles joining each pair of latitude rings.
                for (int i = 0; i < verticalSegments - 2; ++i)
                {
                    for (int j = 0; j < horizontalSegments; j++)
                    {
                        int nextI = i + 1;
                        int nextJ = (j + 1) % horizontalSegments;
                        
                        dynamicPrimitive.AddIndex(1 + i * horizontalSegments + j);
                        dynamicPrimitive.AddIndex(1 + i * horizontalSegments + nextJ);

                        dynamicPrimitive.AddIndex(1 + i * horizontalSegments + nextJ);
                        dynamicPrimitive.AddIndex(1 + nextI * horizontalSegments + nextJ);

                        dynamicPrimitive.AddIndex(1 + nextI * horizontalSegments + j);
                        dynamicPrimitive.AddIndex(1 + nextI * horizontalSegments + j);
                    }
                }

                // Create a fan connecting the top vertex to the top latitude ring.
                for (int i = 0; i < horizontalSegments; ++i)
                {
                    dynamicPrimitive.AddIndex(currentVertex - 1);
                    dynamicPrimitive.AddIndex(currentVertex - 2 - (i + 1) % horizontalSegments);
                }
            }
            dynamicPrimitive.EndPrimitive();
        }

        public static void AddSolidSphere(this DynamicPrimitive dynamicPrimitive, Vector3 center, float radius, Color color)
        {
            AddSolidSphere(dynamicPrimitive, new BoundingSphere(center, radius), 8, null, color);
        }

        public static void AddSolidSphere(this DynamicPrimitive dynamicPrimitive, Vector3 center, float radius, int tessellation, Matrix? world, Color color)
        {
            AddSolidSphere(dynamicPrimitive, new BoundingSphere(center, radius), tessellation, world, color);
        }

        public static void AddSolidSphere(this DynamicPrimitive dynamicPrimitive, BoundingSphere boundingSphere, int tessellation, Matrix? world, Color color)
        {
            dynamicPrimitive.BeginPrimitive(PrimitiveType.TriangleList, null, world);
            {
                if (tessellation < 3)
                    throw new ArgumentOutOfRangeException("tessellation");

                int currentVertex = 2;
                int verticalSegments = tessellation;
                int horizontalSegments = tessellation * 2;

                // Start with a single vertex at the bottom of the sphere.
                dynamicPrimitive.AddVertex(Vector3.Down * boundingSphere.Radius + boundingSphere.Center, color);

                // Create rings of vertices at progressively higher latitudes.
                for (int i = 0; i < verticalSegments - 1; ++i)
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

                        dynamicPrimitive.AddVertex(normal * boundingSphere.Radius + boundingSphere.Center, color);
                        currentVertex++;
                    }
                }

                // Finish with a single vertex at the top of the sphere.
                dynamicPrimitive.AddVertex(Vector3.Up * boundingSphere.Radius + boundingSphere.Center, color);

                // Create a fan connecting the bottom vertex to the bottom latitude ring.
                for (int i = 0; i < horizontalSegments; ++i)
                {
                    dynamicPrimitive.AddIndex(0);
                    dynamicPrimitive.AddIndex(1 + (i + 1) % horizontalSegments);
                    dynamicPrimitive.AddIndex(1 + i);
                }

                // Fill the sphere body with triangles joining each pair of latitude rings.
                for (int i = 0; i < verticalSegments - 2; ++i)
                {
                    for (int j = 0; j < horizontalSegments; j++)
                    {
                        int nextI = i + 1;
                        int nextJ = (j + 1) % horizontalSegments;

                        dynamicPrimitive.AddIndex(1 + i * horizontalSegments + j);
                        dynamicPrimitive.AddIndex(1 + i * horizontalSegments + nextJ);
                        dynamicPrimitive.AddIndex(1 + nextI * horizontalSegments + j);

                        dynamicPrimitive.AddIndex(1 + i * horizontalSegments + nextJ);
                        dynamicPrimitive.AddIndex(1 + nextI * horizontalSegments + nextJ);
                        dynamicPrimitive.AddIndex(1 + nextI * horizontalSegments + j);
                    }
                }

                // Create a fan connecting the top vertex to the top latitude ring.
                for (int i = 0; i < horizontalSegments; ++i)
                {
                    dynamicPrimitive.AddIndex(currentVertex - 1);
                    dynamicPrimitive.AddIndex(currentVertex - 2 - (i + 1) % horizontalSegments);
                    dynamicPrimitive.AddIndex(currentVertex - 2 - i);
                }
            }
            dynamicPrimitive.EndPrimitive();
        }

        public static void AddFrustum(this DynamicPrimitive dynamicPrimitive, BoundingFrustum boundingFrustum, Matrix? world, Color color, float lineWidth)
        {
            Vector3[] corners = boundingFrustum.GetCorners();

            // near plane
            dynamicPrimitive.AddLine(corners[0], corners[1], color, lineWidth);
            dynamicPrimitive.AddLine(corners[1], corners[2], color, lineWidth);
            dynamicPrimitive.AddLine(corners[2], corners[3], color, lineWidth);
            dynamicPrimitive.AddLine(corners[3], corners[0], color, lineWidth);

            // connections
            dynamicPrimitive.AddLine(corners[0], corners[4], color, lineWidth);
            dynamicPrimitive.AddLine(corners[1], corners[5], color, lineWidth);
            dynamicPrimitive.AddLine(corners[2], corners[6], color, lineWidth);
            dynamicPrimitive.AddLine(corners[3], corners[7], color, lineWidth);

            // far plane
            dynamicPrimitive.AddLine(corners[4], corners[5], color, lineWidth);
            dynamicPrimitive.AddLine(corners[5], corners[6], color, lineWidth);
            dynamicPrimitive.AddLine(corners[6], corners[7], color, lineWidth);
            dynamicPrimitive.AddLine(corners[7], corners[4], color, lineWidth);
        }

        public static void AddSolidFrustum(this DynamicPrimitive dynamicPrimitive, BoundingFrustum boundingFrustum, Matrix? world, Color color)
        {
            Vector3[] corners = boundingFrustum.GetCorners();

            dynamicPrimitive.BeginPrimitive(PrimitiveType.TriangleList, null, world);
            {
                for (int i = 0; i < BoundingFrustum.CornerCount; ++i)
                    dynamicPrimitive.AddVertex(ref corners[i], color);

                for (int i = 0; i < BoundingBoxExtensions.TriangleIndices.Length; ++i)
                    dynamicPrimitive.AddIndex(BoundingBoxExtensions.TriangleIndices[i]);
            }
            dynamicPrimitive.EndPrimitive();
        }

        public static void AddCone(this DynamicPrimitive dynamicPrimitive, Vector3 position, float height, float radius, int tessellation, Matrix? world, Color color, float lineWidth)
        {
            if (tessellation < 3)
                throw new ArgumentOutOfRangeException("tessellation");

            dynamicPrimitive.BeginPrimitive(PrimitiveType.LineList, null, world, lineWidth);
            {
                dynamicPrimitive.AddVertex(position + Vector3.Up * height, color);

                for (int i = 0; i < tessellation; ++i)
                {
                    float angle = i * MathHelper.TwoPi / tessellation;

                    float dx = (float)Math.Cos(angle);
                    float dz = (float)Math.Sin(angle);

                    Vector3 normal = new Vector3(dx, 0, dz);

                    dynamicPrimitive.AddVertex(position + normal * radius, color);

                    dynamicPrimitive.AddIndex(0);
                    dynamicPrimitive.AddIndex(1 + i);

                    dynamicPrimitive.AddIndex(1 + i);
                    dynamicPrimitive.AddIndex(1 + (i + 1) % tessellation);
                }
            }
            dynamicPrimitive.EndPrimitive();
        }

        public static void AddSolidCone(this DynamicPrimitive dynamicPrimitive, Vector3 position, float height, float radius, int tessellation, Matrix? world, Color color)
        {
            if (tessellation < 3)
                throw new ArgumentOutOfRangeException("tessellation");

            dynamicPrimitive.BeginPrimitive(PrimitiveType.TriangleList, null, world);
            {
                dynamicPrimitive.AddVertex(position + Vector3.Up * height, color);
                dynamicPrimitive.AddVertex(position, color);

                for (int i = 0; i < tessellation; ++i)
                {
                    float angle = i * MathHelper.TwoPi / tessellation;

                    float dx = (float)Math.Cos(angle);
                    float dz = (float)Math.Sin(angle);

                    Vector3 normal = new Vector3(dx, 0, dz);

                    dynamicPrimitive.AddVertex(position + normal * radius, color);

                    dynamicPrimitive.AddIndex(0);
                    dynamicPrimitive.AddIndex(2 + i);
                    dynamicPrimitive.AddIndex(2 + (i + 1) % tessellation);

                    dynamicPrimitive.AddIndex(1);
                    dynamicPrimitive.AddIndex(2 + (i + 1) % tessellation);
                    dynamicPrimitive.AddIndex(2 + i);
                }
            }
            dynamicPrimitive.EndPrimitive();
        }

        public static void AddCylinder(this DynamicPrimitive dynamicPrimitive, Vector3 position, float height, float radius, int tessellation, Matrix? world, Color color, float lineWidth)
        {
            if (tessellation < 3)
                throw new ArgumentOutOfRangeException("tessellation");

            dynamicPrimitive.BeginPrimitive(PrimitiveType.LineList, null, world, lineWidth);
            {
                dynamicPrimitive.AddVertex(position + Vector3.Up * height, color);
                dynamicPrimitive.AddVertex(position, color);

                for (int i = 0; i < tessellation; ++i)
                {
                    float angle = i * MathHelper.TwoPi / tessellation;

                    float dx = (float)Math.Cos(angle);
                    float dz = (float)Math.Sin(angle);

                    Vector3 normal = new Vector3(dx, 0, dz);

                    dynamicPrimitive.AddVertex(position + normal * radius + Vector3.Up * height, color);
                    dynamicPrimitive.AddVertex(position + normal * radius, color);

                    dynamicPrimitive.AddIndex(2 + i * 2);
                    dynamicPrimitive.AddIndex(3 + i * 2);

                    dynamicPrimitive.AddIndex(2 + i * 2);
                    dynamicPrimitive.AddIndex(2 + ((i + 1) % tessellation) * 2);

                    dynamicPrimitive.AddIndex(3 + i * 2);
                    dynamicPrimitive.AddIndex(3 + ((i + 1) % tessellation) * 2);
                }
            }
            dynamicPrimitive.EndPrimitive();
        }

        public static void AddSolidCylinder(this DynamicPrimitive dynamicPrimitive, Vector3 position, float height, float radius, int tessellation, Matrix? world, Color color)
        {
            if (tessellation < 3)
                throw new ArgumentOutOfRangeException("tessellation");

            dynamicPrimitive.BeginPrimitive(PrimitiveType.TriangleList, null, world);
            {
                dynamicPrimitive.AddVertex(position + Vector3.Up * height, color);
                dynamicPrimitive.AddVertex(position, color);

                for (int i = 0; i < tessellation; ++i)
                {
                    float angle = i * MathHelper.TwoPi / tessellation;

                    float dx = (float)Math.Cos(angle);
                    float dz = (float)Math.Sin(angle);

                    Vector3 normal = new Vector3(dx, 0, dz);

                    dynamicPrimitive.AddVertex(position + normal * radius + Vector3.Up * height, color);
                    dynamicPrimitive.AddVertex(position + normal * radius, color);

                    dynamicPrimitive.AddIndex(2 + i * 2);
                    dynamicPrimitive.AddIndex(2 + (i * 2 + 2) % (tessellation * 2));
                    dynamicPrimitive.AddIndex(2 + i * 2 + 1);

                    dynamicPrimitive.AddIndex(2 + i * 2 + 1);
                    dynamicPrimitive.AddIndex(2 + (i * 2 + 2) % (tessellation * 2));
                    dynamicPrimitive.AddIndex(2 + (i * 2 + 3) % (tessellation * 2));

                    dynamicPrimitive.AddIndex(0);
                    dynamicPrimitive.AddIndex(2 + (i * 2 + 2) % (tessellation * 2));
                    dynamicPrimitive.AddIndex(2 + i * 2);

                    dynamicPrimitive.AddIndex(1);
                    dynamicPrimitive.AddIndex(2 + i * 2 + 1);
                    dynamicPrimitive.AddIndex(2 + (i * 2 + 3) % (tessellation * 2));
                }
            }
            dynamicPrimitive.EndPrimitive();
        }

        public static void AddTriangle(this DynamicPrimitive dynamicPrimitive, Triangle triangle, Matrix? world, Color color, float lineWidth)
        {
            dynamicPrimitive.BeginPrimitive(PrimitiveType.LineList, null, world, lineWidth);
            dynamicPrimitive.AddVertex(ref triangle.V1, color);
            dynamicPrimitive.AddVertex(ref triangle.V2, color);
            dynamicPrimitive.AddVertex(ref triangle.V2, color);
            dynamicPrimitive.AddVertex(ref triangle.V3, color);
            dynamicPrimitive.AddVertex(ref triangle.V3, color);
            dynamicPrimitive.AddVertex(ref triangle.V1, color);
            dynamicPrimitive.EndPrimitive();
        }

        public static void AddRay(this DynamicPrimitive dynamicPrimitive, Ray ray, float length, Color color, float lineWidth)
        {
            AddArrow(dynamicPrimitive, ray.Position, ray.Position + ray.Direction * length, color, lineWidth);
        }

        public static void AddArrow(this DynamicPrimitive dynamicPrimitive, Vector3 start, Vector3 end, Color color, float lineWidth)
        {
            const float Ratio = 0.2f;
            var mid = Vector3.Lerp(end, start, Ratio);
            var head = (end - start).Length() * Ratio;

            dynamicPrimitive.AddLine(start, mid, null, color, lineWidth);
            dynamicPrimitive.AddSolidCone(Vector3.Zero, head, head * 0.5f, 24,
                MatrixHelper.CreateRotation(Vector3.Up, Vector3.Normalize(end - start)) *
                Matrix.CreateTranslation(mid), color);
        }

        public static void AddAxis(this DynamicPrimitive dynamicPrimitive, Matrix world, float lineWidth)
        {
            AddArrow(dynamicPrimitive, world.Translation, Vector3.Transform(Vector3.UnitX, world), new Color(255, 0, 0), lineWidth);
            AddArrow(dynamicPrimitive, world.Translation, Vector3.Transform(Vector3.UnitY, world), new Color(0, 255, 0), lineWidth);
            AddArrow(dynamicPrimitive, world.Translation, Vector3.Transform(Vector3.UnitZ, world), new Color(0, 0, 255), lineWidth);
        }

        public static void AddAxis(this DynamicPrimitive dynamicPrimitive, Matrix world, float length, float lineWidth)
        {
            Vector3 scale;
            Vector3 translation;
            Quaternion rotation;

            world.Decompose(out scale, out rotation, out translation);
            world = Matrix.CreateFromQuaternion(rotation);
            world.Translation = translation;

            AddArrow(dynamicPrimitive, world.Translation, Vector3.Transform(Vector3.UnitX * length, world), new Color(255, 0, 0), lineWidth);
            AddArrow(dynamicPrimitive, world.Translation, Vector3.Transform(Vector3.UnitY * length, world), new Color(0, 255, 0), lineWidth);
            AddArrow(dynamicPrimitive, world.Translation, Vector3.Transform(Vector3.UnitZ * length, world), new Color(0, 0, 255), lineWidth);
        }

        public static void AddAxis(this DynamicPrimitive dynamicPrimitive, Matrix world, float length, Color colorX, Color colorY, Color colorZ, float lineWidth)
        {
            Vector3 scale;
            Vector3 translation;
            Quaternion rotation;

            world.Decompose(out scale, out rotation, out translation);
            world = Matrix.CreateFromQuaternion(rotation);
            world.Translation = translation;

            AddArrow(dynamicPrimitive, world.Translation, Vector3.Transform(Vector3.UnitX * length, world), colorX, lineWidth);
            AddArrow(dynamicPrimitive, world.Translation, Vector3.Transform(Vector3.UnitY * length, world), colorY, lineWidth);
            AddArrow(dynamicPrimitive, world.Translation, Vector3.Transform(Vector3.UnitZ * length, world), colorZ, lineWidth);
        }

        public static void AddSkeleton(this DynamicPrimitive dynamicPrimitive, Microsoft.Xna.Framework.Graphics.Model model, Matrix? world, Color color, float lineWidth)
        {
            dynamicPrimitive.BeginPrimitive(PrimitiveType.LineList, null, world, lineWidth);
            {
                AddSkeleton(dynamicPrimitive, model.Root, Matrix.Identity, world, color);
            }
            dynamicPrimitive.EndPrimitive();
        }

        private static void AddSkeleton(this DynamicPrimitive dynamicPrimitive, ModelBone node, Matrix parentTransform, Matrix? world, Color color)
        {
            Matrix start = parentTransform;
            Matrix end = node.Transform * parentTransform;
            
            if (node.Parent != null)
            {
                if (Vector3.Subtract(end.Translation, start.Translation).LengthSquared() > 0)
                {
                    dynamicPrimitive.AddVertex(start.Translation, color);
                    dynamicPrimitive.AddVertex(end.Translation, color);
                }
            }

            foreach (ModelBone child in node.Children)
            {
                AddSkeleton(dynamicPrimitive, child, end, world, color);
            }
        }

        public static void AddSkeleton(this DynamicPrimitive dynamicPrimitive, Skeleton skeleton, Matrix? world, Color color, float lineWidth)
        {
            dynamicPrimitive.BeginPrimitive(PrimitiveType.LineList, null, world, lineWidth);
            {
                AddSkeleton(dynamicPrimitive, skeleton, 0, Matrix.Identity, color);
            }
            dynamicPrimitive.EndPrimitive();
        }

        private static void AddSkeleton(this DynamicPrimitive dynamicPrimitive, Skeleton skeleton, int bone, Matrix parentTransform, Color color)
        {
            Matrix start = parentTransform;
            Matrix end = skeleton.GetBoneTransform(bone) * parentTransform;

            if (Vector3.Subtract(end.Translation, start.Translation).LengthSquared() > 0)
            {
                dynamicPrimitive.AddVertex(start.Translation, color);
                dynamicPrimitive.AddVertex(end.Translation, color);
                
                Vector3.Subtract(end.Translation, start.Translation).Length();
            }

            foreach (int child in skeleton.GetChildBones(bone))
            {
                AddSkeleton(dynamicPrimitive, skeleton, child, end, color);
            }
        }
        
        static Matrix[] bones;

        public static void AddCollision(this DynamicPrimitive dynamicPrimitive, Microsoft.Xna.Framework.Graphics.Model model, Matrix? world, Color color)
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
                
                tree.Traverse(node =>
                {
                    if (!node.hasChildren && node.value)
                        dynamicPrimitive.AddSolidBox(node.bounds, transform, color);
                    return TraverseOptions.Continue;
                });
            }
            // Add collision sphere
            else
            {
                for (int i = 0; i < model.Meshes.Count; ++i)
                {
                    var mesh = model.Meshes[i];
                    dynamicPrimitive.AddSolidSphere(mesh.BoundingSphere, 18, bones[mesh.ParentBone.Index] * transform, color);
                }
            }
        }
    }
}
