#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
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
    #region PickEngine
    /// <summary>
    /// Generic pick engine.
    /// Currently T can only be generalize to BoundingBox?, BoundingSphere?, BoundingFrustum, IPickable and IGeometry.
    /// 
    /// In order to identify which object has been picked, we use BoundingBox? rather than BoundingBox to keep references.
    /// </summary>
    public class PickEngine : IPickable
    {
        struct Entry 
        {
            public object Object; 
            public Matrix InverseTransform;
            public BoundingSphere? Sphere;
        }

        List<Entry> objects = new List<Entry>();


        public PickEngine() { }
        

        /// <summary>
        /// Add a new object to be picked. 
        /// 
        /// When you are adding an IGeometry instance to the pick engine, you can specify
        /// a BoundingSphere so that the geometry is tested against the sphere before
        /// per triangle intersection test is performed, thus improves performance.
        /// 
        /// However, for objects other than IGeometry, the sphere parameter is ignored and
        /// you can safely pass null.
        /// </summary>
        public void Add(object item, Matrix transform, BoundingSphere? sphere)
        {
            if (!((item.GetType() == typeof(BoundingBox)) || (item is BoundingFrustum) ||
                  (item.GetType() == typeof(BoundingSphere)) || (item is IPickable) || (item is IGeometry)))
            {
                throw new ArgumentException("Pick engine only support BoundingBox, BoundingSphere, BoundingFrustum, IPickable and IGeometry");
            }

            Entry e = new PickEngine.Entry();

            e.Object = item;
            e.InverseTransform = Matrix.Invert(transform);
            e.Sphere = sphere;

            objects.Add(e);
        }


        public void Clear()
        {
            objects.Clear();
        }

        public int Count
        {
            get { return objects.Count; }
        }


        public object Pick(Vector3 point)
        {
            foreach (Entry e in objects)
            {
                object picked = e.Object;
                Vector3 pickPoint = Vector3.Transform(point, e.InverseTransform);


                if (e.Object.GetType() == typeof(BoundingBox) &&
                    (((BoundingBox)e.Object).Contains(pickPoint) == ContainmentType.Contains))
                {
                    return e.Object;
                }

                if (e.Object.GetType() == typeof(BoundingSphere) &&
                    (((BoundingSphere)e.Object).Contains(pickPoint) == ContainmentType.Contains))
                {
                    return e.Object;
                }

                if (e.Object is BoundingFrustum &&
                    ((e.Object as BoundingFrustum).Contains(pickPoint) == ContainmentType.Contains))
                {
                    return e.Object;
                }

                if (e.Object is IPickable && (picked = (e.Object as IPickable).Pick(pickPoint)) != null)
                {
                    return picked;
                }

                if (e.Object is IGeometry &&
                    Intersects(e.Object as IGeometry, pickPoint, e.Sphere))
                {
                    return e.Object;
                }
            }

            return null;
        }


        public object Pick(Ray ray, out float? distance)
        {
            object result = null;
            float? point = null;
            distance = null;


            foreach (Entry e in objects)
            {
                object picked = e.Object;
                Ray pickRay = RayExtensions.Transform(ray, e.InverseTransform);


                if (e.Object.GetType() == typeof(BoundingBox))
                    point = ((BoundingBox)e.Object).Intersects(pickRay);
                else if (e.Object.GetType() == typeof(BoundingSphere))
                    point = ((BoundingSphere)e.Object).Intersects(pickRay);
                else if (e.Object is BoundingFrustum)
                    point = (e.Object as BoundingFrustum).Intersects(pickRay);
                else if (e.Object is IGeometry)
                    point = RayExtensions.Intersects(pickRay, e.Object as IGeometry, e.Sphere);
                else if (e.Object is IPickable)
                    picked = (e.Object as IPickable).Pick(pickRay, out point);


                if (point.HasValue)
                {
                    if (distance == null || point.Value < distance.Value)
                    {
                        result = picked;
                        distance = point.Value;
                    }
                }
            }

            return result;
        }


        public static bool Intersects(IGeometry geometry, Vector3 point, BoundingSphere? sphere)
        {
            // Currently we only perform bounding sphere test
            if (sphere.HasValue && sphere.Value.Contains(point) == ContainmentType.Contains)
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Creates a picking ray from screen position.
        /// </summary>
        public static Ray RayFromScreen(GraphicsDevice graphics, int x, int y, Matrix view, Matrix projection)
        {
            // create 2 positions in screenspace using the cursor position. 0 is as
            // close as possible to the camera, 1 is as far away as possible.
            Vector3 nearSource = new Vector3(x, y, 0f);
            Vector3 farSource = new Vector3(x, y, 1f);

            // use Viewport.Unproject to tell what those two screen space positions
            // would be in world space. we'll need the projection matrix and view
            // matrix, which we have saved as member variables. We also need a world
            // matrix, which can just be identity.
            Vector3 nearPoint = graphics.Viewport.Unproject(nearSource,
                projection, view, Matrix.Identity);

            Vector3 farPoint = graphics.Viewport.Unproject(farSource,
                projection, view, Matrix.Identity);

            // find the direction vector that goes from the nearPoint to the farPoint
            // and normalize it....
            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            // and then create a new ray using nearPoint as the source.
            return new Ray(nearPoint, direction);
        }
    }
    #endregion

    #region RayExtensions
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
        /// Checks whether a ray intersects a triangle. This uses the algorithm
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
        /// </summary>
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
            Vector3 pt2 = Vector3.Transform(input.Position + Vector3.Normalize(input.Direction), transform);

            return new Ray(pt1, Vector3.Subtract(pt2, pt1));
        }
    }
    #endregion
}