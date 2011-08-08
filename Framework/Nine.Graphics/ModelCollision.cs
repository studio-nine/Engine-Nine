#region Copyright 2008 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2008 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics
{
    /// <summary>
    /// Defines a octree base model collision detection.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ModelCollision : IPickable
    {
        internal ModelCollision() { }

        /// <summary>
        /// Gets the collision tree.
        /// </summary>
        [ContentSerializer]
        public Octree<bool> CollisionTree { get; internal set; }

        /// <summary>
        /// Gets wether the object contains the given point.
        /// </summary>
        public bool Contains(Vector3 point)
        {
            IEnumerable<OctreeNode<bool>> nodes = CollisionTree.Traverse((o) =>
            {
                return o.Value && o.Bounds.Contains(point) == ContainmentType.Contains;
            });

            foreach (OctreeNode<bool> node in nodes)
            {
                if (!node.HasChildren)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the nearest intersection point from the specifed picking ray.
        /// </summary>
        /// <returns>Distance to the start of the ray.</returns>
        public float? Intersects(Ray ray)
        {
            float? currentDistance = null;

            IEnumerable<OctreeNode<bool>> nodes = CollisionTree.Traverse((o) =>
            {
                return o.Value && (currentDistance = o.Bounds.Intersects(ray)) != null;
            });

            foreach (OctreeNode<bool> node in nodes)
            {
                if (!node.HasChildren)
                    return currentDistance;
            }

            return null;
        }
    }
}
