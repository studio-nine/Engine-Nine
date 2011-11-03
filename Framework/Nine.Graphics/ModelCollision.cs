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
        private Ray ray;
        private Vector3 point;
        private bool contains;
        private float? distance;
        private float? neareastDistance;
        private Func<OctreeNode<bool>, TraverseOptions> traverseContains;
        private Func<OctreeNode<bool>, TraverseOptions> traverseIntersects;

        internal ModelCollision() 
        {
            traverseContains = new Func<OctreeNode<bool>, TraverseOptions>(TraverseContains);
            traverseIntersects = new Func<OctreeNode<bool>, TraverseOptions>(TraverseIntersects);
        }

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
            this.point = point;
            this.contains = false;
            CollisionTree.Traverse(traverseContains);
            return contains;
        }

        private TraverseOptions TraverseContains(OctreeNode<bool> node)
        {
            if (node.Value && node.Bounds.Contains(point) == ContainmentType.Contains)
            {
                if (!node.HasChildren)
                {
                    contains = true;
                    return TraverseOptions.Stop;
                }
                return TraverseOptions.Continue;
            }
            return TraverseOptions.Skip;
        }

        /// <summary>
        /// Gets the nearest intersection point from the specifed picking ray.
        /// </summary>
        /// <returns>Distance to the start of the ray.</returns>
        public float? Intersects(Ray ray)
        {
            this.ray = ray;
            this.distance = null;
            this.neareastDistance = null;
            CollisionTree.Traverse(traverseIntersects);
            return neareastDistance;
        }

        private TraverseOptions TraverseIntersects(OctreeNode<bool> node)
        {
            if (node.Value && (distance = node.Bounds.Intersects(ray)) != null)
            {
                if (neareastDistance == null)
                    neareastDistance = distance.Value;
                else if (neareastDistance.Value > distance.Value)
                    neareastDistance = distance.Value;
                return TraverseOptions.Continue;
            }
            return TraverseOptions.Skip;
        }
    }
}
