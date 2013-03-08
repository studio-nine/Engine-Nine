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
        public static void AddRectangle(this DynamicPrimitive dynamicPrimitive, Vector2 min, Vector2 max, Matrix? world, Color color, float lineWidth)
        {
            dynamicPrimitive.BeginPrimitive(PrimitiveType.LineList, null, world, lineWidth);
            {
                dynamicPrimitive.AddVertex(new Vector3(min.X, min.Y, 0), color);
                dynamicPrimitive.AddVertex(new Vector3(min.X, max.Y, 0), color);
                dynamicPrimitive.AddVertex(new Vector3(max.X, max.Y, 0), color);
                dynamicPrimitive.AddVertex(new Vector3(max.X, min.Y, 0), color);

                dynamicPrimitive.AddIndex(0);
                dynamicPrimitive.AddIndex(1);
                dynamicPrimitive.AddIndex(1);
                dynamicPrimitive.AddIndex(2);
                dynamicPrimitive.AddIndex(2);
                dynamicPrimitive.AddIndex(3);
                dynamicPrimitive.AddIndex(3);
                dynamicPrimitive.AddIndex(0);
            }
            dynamicPrimitive.EndPrimitive();
        }

        public static void AddRectangle(this DynamicPrimitive dynamicPrimitive, Vector2 min, Vector2 max, Vector3 up, Matrix? world, Color color, float lineWidth)
        {
            Matrix transform = MatrixHelper.CreateRotation(Vector3.Up, up);

            dynamicPrimitive.BeginPrimitive(PrimitiveType.LineList, null, world, lineWidth);
            {
                dynamicPrimitive.AddVertex(Vector3.TransformNormal(new Vector3(min.X, min.Y, 0), transform), color);
                dynamicPrimitive.AddVertex(Vector3.TransformNormal(new Vector3(min.X, max.Y, 0), transform), color);
                dynamicPrimitive.AddVertex(Vector3.TransformNormal(new Vector3(max.X, max.Y, 0), transform), color);
                dynamicPrimitive.AddVertex(Vector3.TransformNormal(new Vector3(max.X, min.Y, 0), transform), color);

                dynamicPrimitive.AddIndex(0);
                dynamicPrimitive.AddIndex(1);
                dynamicPrimitive.AddIndex(1);
                dynamicPrimitive.AddIndex(2);
                dynamicPrimitive.AddIndex(2);
                dynamicPrimitive.AddIndex(3);
                dynamicPrimitive.AddIndex(3);
                dynamicPrimitive.AddIndex(0);
            }
            dynamicPrimitive.EndPrimitive();
        }

        public static void AddCircle(this DynamicPrimitive dynamicPrimitive, Vector3 center, float radius, int tessellation, Matrix? world, Color color, float lineWidth)
        {
            dynamicPrimitive.BeginPrimitive(PrimitiveType.LineStrip, null, world, lineWidth);
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

                    dynamicPrimitive.AddVertex(normal * radius + center, color);
                }
            }
            dynamicPrimitive.EndPrimitive();
        }

        public static void AddCircle(this DynamicPrimitive dynamicPrimitive, Vector3 center, float radius, Vector3 up, int tessellation, Matrix? world, Color color, float lineWidth)
        {
            Matrix transform = Matrix.CreateScale(radius) *
                               MatrixHelper.CreateRotation(Vector3.Up, up) *
                               Matrix.CreateTranslation(center);

            dynamicPrimitive.BeginPrimitive(PrimitiveType.LineStrip, null, world, lineWidth);
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

                    dynamicPrimitive.AddVertex(Vector3.Transform(normal, transform), color);
                }
            }
            dynamicPrimitive.EndPrimitive();
        }

        public static void AddGrid(this DynamicPrimitive dynamicPrimitive, float step, int countX, int countZ, Matrix? world, Color color, float lineWidth)
        {
            AddGrid(dynamicPrimitive, -step * countX * 0.5f, 0, -step * countZ * 0.5f, step * countX, step * countZ, countX, countZ, world, color, lineWidth);
        }

        public static void AddGrid(this DynamicPrimitive dynamicPrimitive, float x, float y, float z, float step, int countX, int countZ, Matrix? world, Color color, float lineWidth)
        {
            AddGrid(dynamicPrimitive, x, y, z, step * countX, step * countZ, countX, countZ, world, color, lineWidth);
        }

        public static void AddGrid(this DynamicPrimitive dynamicPrimitive, float x, float y, float z, float width, float height, int countX, int countZ, Matrix? world, Color color, float lineWidth)
        {
            dynamicPrimitive.BeginPrimitive(PrimitiveType.LineList, null, world, lineWidth);
            {
                float incU = width / countX;
                float incV = height / countZ;

                for (int u = 0; u <= countX; u++)
                {
                    dynamicPrimitive.AddVertex(new Vector3(x + 0, y, z + u * incU), color);
                    dynamicPrimitive.AddVertex(new Vector3(x + height, y, z + u * incU), color);
                }

                for (int v = 0; v <= countZ; v++)
                {
                    dynamicPrimitive.AddVertex(new Vector3(x + v * incV, y, z + 0), color);
                    dynamicPrimitive.AddVertex(new Vector3(x + v * incV, y, z + width), color);
                }
            }
            dynamicPrimitive.EndPrimitive();
        }

        public static void AddGrid(this DynamicPrimitive dynamicPrimitive, UniformGrid grid, Matrix? world, Color color, float lineWidth)
        {
            AddGrid(dynamicPrimitive, grid.Position.X, grid.Position.Y, 0, grid.Size.X, grid.Size.Y, grid.SegmentCountX, grid.SegmentCountY, world, color, lineWidth);
        }

        public static void AddPlane(this DynamicPrimitive dynamicPrimitive, Microsoft.Xna.Framework.Plane plane, float size, int tessellation, Matrix? world, Color color, float lineWidth)
        {
            Matrix transform = MatrixHelper.CreateRotation(Vector3.Up, plane.Normal) *
                               Matrix.CreateTranslation(plane.Normal * plane.D);

            if (world.HasValue)
                transform *= world.Value;

            AddGrid(dynamicPrimitive, 0, 0, 0, size, size, tessellation, tessellation, transform, color, lineWidth);
        }

        public static void AddLineSegment(this DynamicPrimitive dynamicPrimitive, LineSegment line, float z, Matrix? world, Color color, float lineWidth)
        {
            dynamicPrimitive.AddLine(new Vector3(line.Start, z), new Vector3(line.End, z), world, color, lineWidth);
        }

        public static void AddBillboard(this DynamicPrimitive dynamicPrimitive, Texture2D texture, Vector3 position, float size, Color color, Vector3 cameraPosition)
        {
            AddBillboard(dynamicPrimitive, texture, ref position, size, size, 0, null, null, color, ref cameraPosition);
        }

        public static void AddBillboard(this DynamicPrimitive dynamicPrimitive, Texture2D texture, ref Vector3 position, float width, float height, float rotation, Matrix? textureTransform, Matrix? world, Color color, ref Vector3 cameraPosition)
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

#if XBOX || WINDOWS_PHONE
            aa.Position = new Vector3();
            ab.Position = new Vector3();
            ba.Position = new Vector3();
            bb.Position = new Vector3();
#endif
            aa.Position.X = +width * 0.5f; aa.Position.Y = +height * 0.5f; aa.Position.Z = 0;
            ab.Position.X = -width * 0.5f; ab.Position.Y = +height * 0.5f; ab.Position.Z = 0;
            ba.Position.X = +width * 0.5f; ba.Position.Y = -height * 0.5f; ba.Position.Z = 0;
            bb.Position.X = -width * 0.5f; bb.Position.Y = -height * 0.5f; bb.Position.Z = 0;

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

            dynamicPrimitive.BeginPrimitive(PrimitiveType.TriangleList, texture, world);
            {
                // Add new vertices and indices
                dynamicPrimitive.AddIndex((ushort)(0));
                dynamicPrimitive.AddIndex((ushort)(1));
                dynamicPrimitive.AddIndex((ushort)(2));
                dynamicPrimitive.AddIndex((ushort)(1));
                dynamicPrimitive.AddIndex((ushort)(3));
                dynamicPrimitive.AddIndex((ushort)(2));

                dynamicPrimitive.AddVertex(ref aa);
                dynamicPrimitive.AddVertex(ref ab);
                dynamicPrimitive.AddVertex(ref ba);
                dynamicPrimitive.AddVertex(ref bb);
            }
            dynamicPrimitive.EndPrimitive();
        }

        public static void AddBillboard(this DynamicPrimitive dynamicPrimitive, Texture2D texture, ref Vector3 position, float width, float height, float rotation, Matrix? textureTransform, Matrix? world, Color color, ref Matrix viewInverse)
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

#if XBOX || WINDOWS_PHONE
            aa.Position = new Vector3();
            ab.Position = new Vector3();
            ba.Position = new Vector3();
            bb.Position = new Vector3();
#endif

            aa.Position.X = +width * 0.5f; aa.Position.Y = +height * 0.5f; aa.Position.Z = 0;
            ab.Position.X = -width * 0.5f; ab.Position.Y = +height * 0.5f; ab.Position.Z = 0;
            ba.Position.X = +width * 0.5f; ba.Position.Y = -height * 0.5f; ba.Position.Z = 0;
            bb.Position.X = -width * 0.5f; bb.Position.Y = -height * 0.5f; bb.Position.Z = 0;

            if (Math.Abs(rotation) > 1E-6F)
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

            dynamicPrimitive.BeginPrimitive(PrimitiveType.TriangleList, texture, world);
            {
                // Add new vertices and indices
                dynamicPrimitive.AddIndex((ushort)(0));
                dynamicPrimitive.AddIndex((ushort)(2));
                dynamicPrimitive.AddIndex((ushort)(1));
                dynamicPrimitive.AddIndex((ushort)(1));
                dynamicPrimitive.AddIndex((ushort)(2));
                dynamicPrimitive.AddIndex((ushort)(3));

                dynamicPrimitive.AddVertex(ref aa);
                dynamicPrimitive.AddVertex(ref ab);
                dynamicPrimitive.AddVertex(ref ba);
                dynamicPrimitive.AddVertex(ref bb);
            }
            dynamicPrimitive.EndPrimitive();
        }

        public static void AddLine(this DynamicPrimitive dynamicPrimitive, Vector3 v1, Vector3 v2, Color color, float lineWidth)
        {
            AddLine(dynamicPrimitive, v1, v2, null, color, lineWidth);
        }

        public static void AddLine(this DynamicPrimitive dynamicPrimitive, Vector3 v1, Vector3 v2, Matrix? world, Color color, float lineWidth)
        {
            dynamicPrimitive.BeginPrimitive(PrimitiveType.LineList, null, world, lineWidth);
            {
                dynamicPrimitive.AddVertex(v1, color);
                dynamicPrimitive.AddVertex(v2, color);
            }
            dynamicPrimitive.EndPrimitive();
        }

        public static void AddLine(this DynamicPrimitive dynamicPrimitive, IEnumerable<Vector3> lineStrip, Matrix? world, Color color, float lineWidth)
        {
            dynamicPrimitive.BeginPrimitive(PrimitiveType.LineStrip, null, world, lineWidth);
            {
                foreach (Vector3 position in lineStrip)
                {
                    dynamicPrimitive.AddVertex(position, color);
                }
            }
            dynamicPrimitive.EndPrimitive();
        }

        public static void AddConstrainedBillboard(this DynamicPrimitive dynamicPrimitive, Texture2D texture, Vector3 start, Vector3 end, float width, Matrix? textureTransform, Matrix? world, Color color, Vector3 cameraPosition)
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

            dynamicPrimitive.BeginPrimitive(PrimitiveType.TriangleList, texture, world);
            {
                // Add new vertices and indices
                dynamicPrimitive.AddIndex((ushort)(0));
                dynamicPrimitive.AddIndex((ushort)(1));
                dynamicPrimitive.AddIndex((ushort)(2));
                dynamicPrimitive.AddIndex((ushort)(1));
                dynamicPrimitive.AddIndex((ushort)(3));
                dynamicPrimitive.AddIndex((ushort)(2));

                dynamicPrimitive.AddVertex(ref aa);
                dynamicPrimitive.AddVertex(ref ab);
                dynamicPrimitive.AddVertex(ref ba);
                dynamicPrimitive.AddVertex(ref bb);
            }
            dynamicPrimitive.EndPrimitive();
        }

        public static void AddConstrainedBillboard(this DynamicPrimitive dynamicPrimitive, Texture2D texture, IEnumerable<Vector3> lineStrip, float width, Matrix? textureTransform, Matrix? world, Color color, Vector3 cameraPosition)
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

            dynamicPrimitive.BeginPrimitive(PrimitiveType.TriangleList, texture, world);
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

                    dynamicPrimitive.AddIndex((ushort)(startIndex + 0));
                    dynamicPrimitive.AddIndex((ushort)(startIndex + 3));
                    dynamicPrimitive.AddIndex((ushort)(startIndex + 1));
                    dynamicPrimitive.AddIndex((ushort)(startIndex + 0));
                    dynamicPrimitive.AddIndex((ushort)(startIndex + 2));
                    dynamicPrimitive.AddIndex((ushort)(startIndex + 3));

                    dynamicPrimitive.AddVertex(ref ba);
                    dynamicPrimitive.AddVertex(ref bb);

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

                dynamicPrimitive.AddVertex(ref aa);
                dynamicPrimitive.AddVertex(ref ab);
            }
            dynamicPrimitive.EndPrimitive();
        }

        /// <summary>
        /// Matrix.CreateConstraintBillboard has a sudden change effect that is not
        /// desirable, so rolling out our own version.
        /// </summary>
        private static void CreateBillboard(Vector3 start, Vector3 end, float width, Vector3 cameraPosition,
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
