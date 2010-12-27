#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine
{
    /// <summary>
    /// Interface for an object that can be picked by a given ray.
    /// </summary>
    public interface IPickable
    {
        /// <summary>
        /// Gets wether the object contains the given point.
        /// </summary>
        bool Contains(Vector3 point);
        
        /// <summary>
        /// Gets the nearest intersection point from the specifed picking ray.
        /// </summary>
        /// <returns>Distance to the start of the ray.</returns>
        float? Intersects(Ray ray);
    }
    
    /// <summary>
    /// Contains extension method for Viewport.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ViewportExtensions
    {
        /// <summary>
        /// Creates a picking ray from screen position.
        /// </summary>
        public static Ray CreatePickRay(this Viewport viewport, int x, int y, Matrix view, Matrix projection)
        {
            // create 2 positions in screen space using the cursor position. 0 is as
            // close as possible to the camera, 1 is as far away as possible.
            Vector3 nearSource = new Vector3(x, y, 0f);
            Vector3 farSource = new Vector3(x, y, 1f);

            // use Viewport.Unproject to tell what those two screen space positions
            // would be in world space. we'll need the projection matrix and view
            // matrix, which we have saved as member variables. We also need a world
            // matrix, which can just be identity.
            Vector3 nearPoint = viewport.Unproject(nearSource,
                projection, view, Matrix.Identity);

            Vector3 farPoint = viewport.Unproject(farSource,
                projection, view, Matrix.Identity);

            // find the direction vector that goes from the nearPoint to the farPoint
            // and normalize it....
            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            // and then create a new ray using nearPoint as the source.
            return new Ray(nearPoint, direction);
        }

        /// <summary>
        /// Creates a picking frustum from screen rectangle.
        /// </summary>
        public static BoundingFrustum CreatePickFrustum(this Viewport viewport, Point point1, Point point2, Matrix view, Matrix projection)
        {
            Rectangle rectangle = new Rectangle();

            rectangle.X = Math.Min(point1.X, point2.X);
            rectangle.Y = Math.Min(point1.Y, point2.Y);
            rectangle.Width = Math.Abs(point1.X - point2.X);
            rectangle.Height = Math.Abs(point1.Y - point2.Y);

            return CreatePickFrustum(viewport, rectangle, view, projection);
        }

        /// <summary>
        /// Creates a picking frustum from screen rectangle.
        /// </summary>
        public static BoundingFrustum CreatePickFrustum(this Viewport viewport, Rectangle rectangle, Matrix view, Matrix projection)
        {
            rectangle.X -= viewport.X;
            rectangle.Y -= viewport.Y;

            // Select multiple objects
            Matrix innerProject;

            float left = (float)(2 * rectangle.Left - viewport.Width) / viewport.Width;
            float right = (float)(2 * rectangle.Right - viewport.Width) / viewport.Width;
            float bottom = (float)(2 * rectangle.Top - viewport.Height) / viewport.Height;
            float top = (float)(2 * rectangle.Bottom - viewport.Height) / viewport.Height;

            float farClip = Math.Abs(projection.M43 / (Math.Abs(projection.M33) - 1));
            float nearClip = Math.Abs(projection.M43 / projection.M33);

            Matrix projectionInverse = Matrix.Invert(projection);

            Vector3 max = Vector3.Transform(new Vector3(1, 1, 0), projectionInverse) * nearClip;
            Vector3 min = Vector3.Transform(new Vector3(-1, -1, 0), projectionInverse) * -nearClip;

            Matrix.CreatePerspectiveOffCenter(
                left * min.X, right * max.X, bottom * min.Y, top * max.Y, nearClip, farClip, out innerProject);

            return new BoundingFrustum(view * innerProject);
        }
    }

    /// <summary>
    /// Contains extension method for Ray.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class RayExtensions
    {
        /// <summary>
        /// Tests to see if a geometry intersects with the specified ray.
        /// If a bounding sphere is provided, the algorithm will perform bounding sphere
        /// intersection test before per triangle test.
        /// 
        /// The geometry and bounding sphere will be transformed by the specified
        /// transformation matrix before and intersection tests.
        /// </summary>
        public static float? Intersects(this Ray ray, IGeometry geometry, BoundingSphere? sphere)
        {
            // Bounding sphere test
            if (sphere.HasValue && !sphere.Value.Intersects(ray).HasValue)
            {
                return null;
            }


            float? result = null;
            float? point = null;
            Vector3 v1, v2, v3;


            // Test each triangle
            for (int i = 0; i < geometry.Indices.Count; i += 3)
            {
                v1 = geometry.Positions[geometry.Indices[i]];
                v2 = geometry.Positions[geometry.Indices[i + 1]];
                v3 = geometry.Positions[geometry.Indices[i + 2]];


                Intersects(ray, ref v1, ref v2, ref v3, out point);


                if (point.HasValue)
                {
                    if (!result.HasValue || point.Value < result.Value)
                        result = point.Value;
                }
            }

            return result;
        }


        /// <summary>
        /// Checks whether a ray intersects a triangle.
        /// </summary>
        /// <remarks> 
        /// This uses the algorithm
        /// developed by Tomas Moller and Ben Trumbore, which was published in the
        /// Journal of Graphics Tools, volume 2, "Fast, Minimum Storage Ray-Triangle
        /// Intersection".
        /// 
        /// This method is implemented using the pass-by-reference versions of the
        /// XNA math functions. Using these overloads is generally not recommended,
        /// because they make the code less readable than the normal pass-by-value
        /// versions. This method can be called very frequently in a tight inner loop,
        /// however, so in this particular case the performance benefits from passing
        /// everything by reference outweigh the loss of readability.
        /// </remarks>
        public static void Intersects(this Ray ray,
                                          ref Vector3 vertex1,
                                          ref Vector3 vertex2,
                                          ref Vector3 vertex3, out float? result)
        {
            // Compute vectors along two edges of the triangle.
            Vector3 edge1, edge2;

            Vector3.Subtract(ref vertex2, ref vertex1, out edge1);
            Vector3.Subtract(ref vertex3, ref vertex1, out edge2);

            // Compute the determinant.
            Vector3 directionCrossEdge2;
            Vector3.Cross(ref ray.Direction, ref edge2, out directionCrossEdge2);

            float determinant;
            Vector3.Dot(ref edge1, ref directionCrossEdge2, out determinant);

            // If the ray is parallel to the triangle plane, there is no collision.
            if (determinant > -float.Epsilon && determinant < float.Epsilon)
            {
                result = null;
                return;
            }

            float inverseDeterminant = 1.0f / determinant;

            // Calculate the U parameter of the intersection point.
            Vector3 distanceVector;
            Vector3.Subtract(ref ray.Position, ref vertex1, out distanceVector);

            float triangleU;
            Vector3.Dot(ref distanceVector, ref directionCrossEdge2, out triangleU);
            triangleU *= inverseDeterminant;

            // Make sure it is inside the triangle.
            if (triangleU < 0 || triangleU > 1)
            {
                result = null;
                return;
            }

            // Calculate the V parameter of the intersection point.
            Vector3 distanceCrossEdge1;
            Vector3.Cross(ref distanceVector, ref edge1, out distanceCrossEdge1);

            float triangleV;
            Vector3.Dot(ref ray.Direction, ref distanceCrossEdge1, out triangleV);
            triangleV *= inverseDeterminant;

            // Make sure it is inside the triangle.
            if (triangleV < 0 || triangleU + triangleV > 1)
            {
                result = null;
                return;
            }

            // Compute the distance along the ray to the triangle.
            float rayDistance;
            Vector3.Dot(ref edge2, ref distanceCrossEdge1, out rayDistance);
            rayDistance *= inverseDeterminant;

            // Is the triangle behind the ray origin?
            if (rayDistance < 0)
            {
                result = null;
                return;
            }

            result = rayDistance;
        }


        /// <summary>
        /// Creates a new ray that is the transformed by the input matrix
        /// </summary>
        public static Ray Transform(this Ray input, Matrix transform)
        {
            Vector3 pt1 = Vector3.Transform(input.Position, transform);
            Vector3 pt2 = Vector3.Transform(input.Position + input.Direction, transform);

            return new Ray(pt1, Vector3.Normalize(Vector3.Subtract(pt2, pt1)));
        }
    }
}