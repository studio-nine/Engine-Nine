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
using Microsoft.Xna.Framework.Input;
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
        public static float? Intersects(this Ray ray, IGeometry geometry)
        {
            float? result = null;
            float? point = null;

            if (geometry.Transform.HasValue)
            {
                var transform = geometry.Transform.Value;
                Matrix.Invert(ref transform, out transform);
                ray.Transform(ref transform, out ray);
            }

            // Test each triangle
            for (int i = 0; i < geometry.Indices.Length; i += 3)
            {
                Intersects(ray, ref geometry.Positions[geometry.Indices[i]],
                                ref geometry.Positions[geometry.Indices[i + 1]],
                                ref geometry.Positions[geometry.Indices[i + 2]], out point);

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
        public static float? Intersects(this Ray ray, Triangle triangle)
        {
            float? result;
            Intersects(ray, ref triangle.V1, ref triangle.V2, ref triangle.V3, out result);
            return result;
        }

        /// <summary>
        /// Checks whether a ray intersects a triangle.
        /// </summary>
        public static void Intersects(this Ray ray, ref Triangle triangle, out float? result)
        {
            Intersects(ray, ref triangle.V1, ref triangle.V2, ref triangle.V3, out result);
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
        internal static void Intersects(this Ray ray,
                                          ref Vector3 vertex1,
                                          ref Vector3 vertex2,
                                          ref Vector3 vertex3, out float? result)
        {
            const float Epsilon = 1E-10F;

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
            if (determinant > -Epsilon && determinant < Epsilon)
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
            Ray result;
            Transform(input, ref transform, out result);
            return result;
        }

        /// <summary>
        /// Creates a new ray that is the transformed by the input matrix
        /// </summary>
        public static void Transform(this Ray input, ref Matrix transform, out Ray result)
        {
            Vector3.Transform(ref input.Position, ref transform, out result.Position);

            result.Direction = new Vector3();
            result.Direction.X = input.Position.X + input.Direction.X;
            result.Direction.Y = input.Position.Y + input.Direction.Y;
            result.Direction.Z = input.Position.Z + input.Direction.Z;

            Vector3.Transform(ref result.Direction, ref transform, out result.Direction);

            result.Direction.X = result.Direction.X - result.Position.X;
            result.Direction.Y = result.Direction.Y - result.Position.Y;
            result.Direction.Z = result.Direction.Z - result.Position.Z;

            result.Direction.Normalize();
        }
    }
}