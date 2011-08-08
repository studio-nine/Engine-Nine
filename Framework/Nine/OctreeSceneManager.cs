#region Copyright 2010 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2010 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Linq;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine
{
    #region OctreeSceneManager
    /// <summary>
    /// Manages a collection of objects using quad tree.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    public class OctreeSceneManager<T> : ISceneManager<T> where T : ISpatialQueryable
    {
        internal Octree<List<T>> Tree;

        /// <summary>
        /// Gets the bounds of this OctreeSceneManager.
        /// </summary>
        public BoundingBox Bounds { get { return Tree.Bounds; } }

        /// <summary>
        /// Gets the max depth of this OctreeSceneManager.
        /// </summary>
        public int MaxDepth { get { return Tree.MaxDepth; } }

        /// <summary>
        /// Creates a new instance of OctreeSceneManager.
        /// </summary>
        public OctreeSceneManager() : this(new BoundingBox(new Vector3(-500f, -500f, -500f),
                                                            new Vector3(500f, 500f, 500f)), 5)
        {

        }

        /// <summary>
        /// Creates a new instance of OctreeSceneManager.
        /// </summary>
        public OctreeSceneManager(BoundingBox bounds, int maxDepth)
        {
            Tree = new Octree<List<T>>(bounds, maxDepth);
        }

        #region ICollection
        public void Add(T item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            if (item.SpatialData != null)
                throw new InvalidOperationException(Strings.AlreadyAddedToASceneManager);

            item.SpatialData = new OctreeSceneManagerSpatialData<T>();

            Add(Tree.Root, item);

            item.BoundingBoxChanged += new EventHandler<EventArgs>(BoundingBoxChanged);

            Count++;
        }

        private void Add(OctreeNode<List<T>> treeNode, T item)
        {
            bool hasResult = false;

            var nodes = Tree.Traverse(treeNode, node =>
            {
                ContainmentType containment = node.Bounds.Contains(item.BoundingBox);

                    // Add to root if the object is too large
                if (node == Tree.Root && containment != ContainmentType.Contains)
                {
                    AddToNode(item, node);
                    hasResult = true;
                    return true;
                }

                if (containment == ContainmentType.Disjoint)
                    return false;

                if (containment == ContainmentType.Intersects)
                {
                    AddToNode(item, node.Parent);
                    hasResult = true;
                    return true;
                }

                if (containment == ContainmentType.Contains && node.Depth == Tree.MaxDepth - 1)
                {
                    AddToNode(item, node);
                    hasResult = true;
                    return true;
                }
                return Tree.Expand(node);
            });

            foreach (var node in nodes)
            {
                if (hasResult)
                    break;
            }

            if (!hasResult)
            {
                // Something must be wrong if the node is not added.
                throw new InvalidOperationException();
            }
        }

        private void AddToNode(T item, OctreeNode<List<T>> node)
        {
            if (node.Value == null)
                node.Value = new List<T>();
            var data = (OctreeSceneManagerSpatialData<T>)item.SpatialData;
            data.Tree = Tree;
            data.Node = node;
            node.Value.Add(item);
        }

        public bool Remove(T item)
        {
            OctreeSceneManagerSpatialData<T> data = item.SpatialData as OctreeSceneManagerSpatialData<T>;
            if (data == null || data.Tree != Tree)
                return false;

            var node = data.Node;
            if (!node.Value.Remove(item))
            {
                // Something must be wrong if we cannot remove it.
                throw new InvalidOperationException();
            }

            while (node.Value == null || node.Value.Count <= 0)
            {
                if (node.Parent == null)
                    break;
                node = node.Parent;
            }

            Tree.Collapse(node, n => n.Value == null || n.Value.Count <= 0);

            item.SpatialData = null;
            item.BoundingBoxChanged -= new EventHandler<EventArgs>(BoundingBoxChanged);
            Count--;

            return true;
        }

        void BoundingBoxChanged(object sender, EventArgs e)
        {
            T item = (T)sender;

            if (item == null)
                throw new ArgumentNullException("item");

            var data = (OctreeSceneManagerSpatialData<T>)item.SpatialData;
            if (data == null)
                throw new InvalidOperationException();

            // Bubble up the tree to find the node that fit the size of the object
            var node = data.Node;
            while (node.Bounds.Contains(item.BoundingBox) != ContainmentType.Contains)
            {
                if (node.Parent == null)
                    break;
                node = node.Parent;
            }

            if (node != data.Node)
            {
                if (!data.Node.Value.Remove(item))
                {
                    // Something must be wrong if we cannot remove it.
                    throw new InvalidOperationException();
                }
                Add(node, item);
            }
        }

        public void Clear()
        {
            Tree.Collapse();
            Count = 0;
        }

        public bool Contains(T item)
        {
            foreach (var val in this)
            {
                if (val.Equals(item))
                    return true;
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (var val in this)
            {
                array[arrayIndex++] = val;
            }
        }

        public int Count { get; private set; }
        public bool IsReadOnly { get { return false; } }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var node in Tree)
            {
                if (node.Value != null)
                {
                    foreach (var val in node.Value)
                        yield return val;
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region ISpatialQuery
        public IEnumerable<T> FindAll(Vector3 position, float radius)
        {
            var sphere = new BoundingSphere(position, radius);

            foreach (var node in Tree.Traverse(node => node == Tree.Root || // Objects outside the tree bounds are stored in the root node.
                                                       node.Bounds.Contains(sphere) != ContainmentType.Disjoint ||
                                                       sphere.Contains(node.Bounds) != ContainmentType.Disjoint))
                if (node.Value != null)
                    foreach (var val in node.Value)
                        if (val.BoundingBox.Contains(sphere) != ContainmentType.Disjoint ||
                            sphere.Contains(val.BoundingBox) != ContainmentType.Disjoint)
                            yield return val;
        }

        public IEnumerable<T> FindAll(Ray ray)
        {
            foreach (var node in Tree.Traverse(node => node == Tree.Root ||
                                                       node.Bounds.Intersects(ray).HasValue))
                if (node.Value != null)
                    foreach (var val in node.Value)
                        if (val.BoundingBox.Intersects(ray).HasValue)
                            yield return val;
        }

        public IEnumerable<T> FindAll(BoundingBox boundingBox)
        {
            foreach (var node in Tree.Traverse(node => node == Tree.Root ||
                                                       node.Bounds.Contains(boundingBox) != ContainmentType.Disjoint ||
                                                       boundingBox.Contains(node.Bounds) != ContainmentType.Disjoint))
                if (node.Value != null)
                    foreach (var val in node.Value)
                        if (val.BoundingBox.Contains(boundingBox) != ContainmentType.Disjoint ||
                            boundingBox.Contains(val.BoundingBox) != ContainmentType.Disjoint)
                            yield return val;
        }

        public IEnumerable<T> FindAll(BoundingFrustum frustum)
        {
            foreach (var node in Tree.Traverse(node => node == Tree.Root ||
                                                       node.Bounds.Contains(frustum) != ContainmentType.Disjoint ||
                                                       frustum.Contains(node.Bounds) != ContainmentType.Disjoint))
                if (node.Value != null)
                    foreach (var val in node.Value)
                        if (val.BoundingBox.Contains(frustum) != ContainmentType.Disjoint ||
                            frustum.Contains(val.BoundingBox) != ContainmentType.Disjoint)
                            yield return val;
        }
        #endregion
    }
    #endregion

    #region OctreeSceneManagerSpatialData
    class OctreeSceneManagerSpatialData<T>
    {
        public Octree<List<T>> Tree;
        public OctreeNode<List<T>> Node;
    }
    #endregion
}