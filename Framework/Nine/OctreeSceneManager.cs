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
        public OctreeSceneManager() : this(new BoundingBox(new Vector3(-100f, -100f, -100f),
                                                            new Vector3(100f, 100f, 100f)), 5)
        {

        }

        /// <summary>
        /// Creates a new instance of OctreeSceneManager.
        /// </summary>
        public OctreeSceneManager(BoundingBox bounds, int maxDepth)
        {
            Tree = new Octree<List<T>>(bounds, maxDepth);

            add = new Func<OctreeNode<List<T>>, TraverseOptions>(Add);
            findAllRay = new Func<OctreeNode<List<T>>, TraverseOptions>(FindAllRay);
            findAllBoundingBox = new Func<OctreeNode<List<T>>, TraverseOptions>(FindAllBoundingBox);
            findAllBoundingSphere = new Func<OctreeNode<List<T>>, TraverseOptions>(FindAllBoundingSphere);
            findAllBoundingFrustum = new Func<OctreeNode<List<T>>, TraverseOptions>(FindAllBoundingFrustum);
            boundingBoxChanged = new EventHandler<EventArgs>(BoundingBoxChanged);
        }

        private bool needResize;
        private bool addedToNode;
        private T item;
        private Func<OctreeNode<List<T>>, TraverseOptions> add;

        private ICollection<T> result;
        private Ray ray;
        private BoundingBox boundingBox;
        private BoundingFrustum boundingFrustum;
        private BoundingSphere boundingSphere;
        private Func<OctreeNode<List<T>>, TraverseOptions> findAllRay;
        private Func<OctreeNode<List<T>>, TraverseOptions> findAllBoundingBox;
        private Func<OctreeNode<List<T>>, TraverseOptions> findAllBoundingSphere;
        private Func<OctreeNode<List<T>>, TraverseOptions> findAllBoundingFrustum;
        private EventHandler<EventArgs> boundingBoxChanged;
        
        #region ICollection
        public void Add(T item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            if (item.SpatialData != null)
                throw new InvalidOperationException(Strings.AlreadyAddedToASceneManager);

            item.SpatialData = new OctreeSceneManagerSpatialData<T>();

            AddWithResize(Tree.Root, item);

            item.BoundingBoxChanged += boundingBoxChanged;

            Count++;
        }

        private void AddWithResize(OctreeNode<List<T>> treeNode, T item)
        {
            needResize = false;
            Add(treeNode, item);

            if (needResize)
            {
                needResize = false;

                var newBounds = BoundingBox.CreateMerged(Tree.Bounds, item.BoundingBox);
                var extends = 0.5f * ((newBounds.Max - newBounds.Min) - (Tree.Bounds.Max - Tree.Bounds.Min));

                newBounds.Min -= extends;
                newBounds.Max += extends;

                Resize(newBounds);

                Add(Tree.Root, item);

                if (needResize)
                {
                    // Should be able to add when the tree is resized.
                    throw new InvalidOperationException();
                }
            }
        }
        
        private void Add(OctreeNode<List<T>> treeNode, T item)
        {
            addedToNode = false;

            this.item = item;
            Tree.Traverse(treeNode, add);
            this.item = default(T);

            if (!addedToNode && !needResize)
            {
                // Something must be wrong if the node is not added.
                throw new InvalidOperationException();
            }
        }

        private TraverseOptions Add(OctreeNode<List<T>> node)
        {
            ContainmentType containment = node.Bounds.Contains(item.BoundingBox);

            // Expand the tree to root if the object is too large
            if (node == Tree.Root && containment != ContainmentType.Contains)
            {
                needResize = true;
                return TraverseOptions.Stop;
            }

            if (containment == ContainmentType.Disjoint)
                return TraverseOptions.Skip;

            if (containment == ContainmentType.Intersects)
            {
                AddToNode(item, node.Parent);
                return TraverseOptions.Stop;
            }

            if (containment == ContainmentType.Contains && node.Depth == Tree.MaxDepth - 1)
            {
                AddToNode(item, node);
                return TraverseOptions.Stop;
            }
            return Tree.Expand(node) ? TraverseOptions.Continue : TraverseOptions.Skip;
        }

        private void AddToNode(T item, OctreeNode<List<T>> node)
        {
            if (node.Value == null)
                node.Value = new List<T>();
            var data = (OctreeSceneManagerSpatialData<T>)item.SpatialData;
            data.Tree = Tree;
            data.Node = node;
            node.Value.Add(item);
            addedToNode = true;
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
            item.BoundingBoxChanged -= boundingBoxChanged;
            Count--;

            return true;
        }

        private void BoundingBoxChanged(object sender, EventArgs e)
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

                Count--;
                AddWithResize(node, item);
                Count++;
            }
        }

        public void Clear()
        {
            // Clear event handlers
            foreach (var item in this)
            {
                item.SpatialData = null;
                item.BoundingBoxChanged -= boundingBoxChanged;
            }

            Tree.Collapse();
            Count = 0;
        }

        private void Resize(BoundingBox boundingBox)
        {
            var items = new T[Count];

            CopyTo(items, 0);
            Clear();

            Tree = new Octree<List<T>>(boundingBox, MaxDepth);
            foreach (var item in items)
            {
                Add(item);
            }
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
        public void FindAll(ref Ray ray, ICollection<T> result)
        {
            this.result = result;
            this.ray = ray;
            Tree.Traverse(findAllRay);
            this.result = null;
        }

        private TraverseOptions FindAllRay(OctreeNode<List<T>> node)
        {
            float? intersection;
            bool skip = (node != Tree.Root);
            if (skip)
            {
                node.Bounds.Intersects(ref ray, out intersection);
                if (intersection.HasValue)
                {
                    skip = false;
                }
            }

            if (skip)
                return TraverseOptions.Skip;

            if (node.Value != null)
            {
                var count = node.Value.Count;
                for (int i = 0; i < count; i++)
                {
                    var val = node.Value[i];
                    val.BoundingBox.Intersects(ref ray, out intersection);
                    if (intersection.HasValue)
                    {
                        result.Add(val);
                    }
                }
            }
            return TraverseOptions.Continue;
        }

        public void FindAll(ref BoundingBox boundingBox, ICollection<T> result)
        {
            this.result = result;
            this.boundingBox = boundingBox;
            Tree.Traverse(findAllBoundingBox);
            this.result = null;
        }

        private TraverseOptions FindAllBoundingBox(OctreeNode<List<T>> node)
        {
            var nodeContainment = ContainmentType.Intersects;
            boundingBox.Contains(ref node.boundingBox, out nodeContainment);

            if (nodeContainment == ContainmentType.Disjoint)
                return TraverseOptions.Skip;

            if (nodeContainment == ContainmentType.Contains)
            {
                AddAllDesedents(node);
                return TraverseOptions.Skip;
            }
            
            if (node.Value != null)
            {
                var count = node.Value.Count;
                for (int i = 0; i < count; i++)
                {
                    var val = node.Value[i];
                    ContainmentType objectContainment;
                    val.BoundingBox.Contains(ref boundingBox, out objectContainment);
                    if (objectContainment != ContainmentType.Disjoint)
                        result.Add(val);
                }
            }
            return TraverseOptions.Continue;
        }

        public void FindAll(ref BoundingSphere boundingSphere, ICollection<T> result)
        {
            this.result = result;
            this.boundingSphere = boundingSphere;
            Tree.Traverse(findAllBoundingBox);
            this.result = null;
        }

        private TraverseOptions FindAllBoundingSphere(OctreeNode<List<T>> node)
        {
            var nodeContainment = ContainmentType.Intersects;
            boundingSphere.Contains(ref node.boundingBox, out nodeContainment);

            if (nodeContainment == ContainmentType.Disjoint)
                return TraverseOptions.Skip;

            if (nodeContainment == ContainmentType.Contains)
            {
                AddAllDesedents(node);
                return TraverseOptions.Skip;
            }

            if (node.Value != null)
            {
                var count = node.Value.Count;
                for (int i = 0; i < count; i++)
                {
                    var val = node.Value[i];
                    ContainmentType objectContainment;
                    val.BoundingBox.Contains(ref boundingSphere, out objectContainment);
                    if (objectContainment != ContainmentType.Disjoint)
                        result.Add(val);
                }
            }
            return TraverseOptions.Continue;
        }

        public void FindAll(ref BoundingFrustum boundingFrustum, ICollection<T> result)
        {
            this.result = result;
            this.boundingFrustum = boundingFrustum;
            Tree.Traverse(findAllBoundingFrustum);
            this.result = null;
        }

        private TraverseOptions FindAllBoundingFrustum(OctreeNode<List<T>> node)
        {
            var nodeContainment = ContainmentType.Intersects;
            boundingFrustum.Contains(ref node.boundingBox, out nodeContainment);

            if (nodeContainment == ContainmentType.Disjoint)
                return TraverseOptions.Skip;

            if (nodeContainment == ContainmentType.Contains)
            {
                AddAllDesedents(node);
                return TraverseOptions.Skip;
            }

            if (node.Value != null)
            {
                var count = node.Value.Count;
                for (int i = 0; i < count; i++)
                {
                    var val = node.Value[i];
                    if (boundingFrustum.Contains(val.BoundingBox) != ContainmentType.Disjoint)
                    {
                        result.Add(val);
                    }
                }
            }
            return TraverseOptions.Continue;
        }

        private void AddAllDesedents(OctreeNode<List<T>> node)
        {
            DesedentsStack.Push(node);
            
            while (DesedentsStack.Count > 0)
            {
                node = DesedentsStack.Pop();
                if (node.Value != null)
                {
                    var count = node.Value.Count;
                    for (int i = 0; i < count; i++)
                    {
                        result.Add(node.Value[i]);
                    }
                }

                var children = node.Children;
                for (int i = 0; i < children.Count; i++)
                    DesedentsStack.Push(children[i]);
            }
        }        
        static Stack<OctreeNode<List<T>>> DesedentsStack = new Stack<OctreeNode<List<T>>>();
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