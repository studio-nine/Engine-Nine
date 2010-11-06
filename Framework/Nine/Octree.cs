#region Copyright 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Nine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine
{
    /// <summary>
    /// Represents a space partition structure based on Quadtree.
    /// </summary>
    public class Octree<T> : IEnumerable<OctreeNode<T>>, ICollection<T>
    {
        /// <summary>
        /// Specifies the total number of child nodes (8) in the Octree<T>.
        /// </summary>
        public const int ChildCount = 8;

        /// <summary>
        /// Gets the root OctreeNode<T> of this Octree<T>.
        /// </summary>
        public OctreeNode<T> Root { get; internal set; }

        /// <summary>
        /// Gets the bounds of the Octree<T> node.
        /// </summary>
        public BoundingBox Bounds { get; internal set; }

        /// <summary>
        /// Gets or sets the max depth of this Octree<T>.
        /// </summary>
        public int MaxDepth { get; internal set; }

        /// <summary>
        /// Gets current depth of this Octree<T>.
        /// </summary>
        public int Depth { get; internal set; }

        /// <summary>
        /// Gets the count of nodes of this Octree<T>.
        /// </summary>
        public int Count { get; internal set; }

        /// <summary>
        /// For serialization.
        /// </summary>
        internal Octree() { }

        /// <summary>
        /// Creats a new Octree<T> with the specified boundary.
        /// </summary>
        public Octree(int maxDepth, BoundingBox bounds)
        {
            Root = new OctreeNode<T>();
            Root.Bounds = bounds;
            Root.Depth = 0;
            Root.Parent = null;
            Root.Tree = this;

            Bounds = bounds;

            Count = 0;
            Depth = 0;
            MaxDepth = maxDepth;
        }

        /// <summary>
        /// Adds a new item to the Octree<T> with the specified position.
        /// </summary>
        public void Add(T item, Vector3 position)
        {
            Add(item, (o) => { return o.Contains(position) == ContainmentType.Contains; });
        }

        /// <summary>
        /// Adds a new item to the Octree<T> with the specified bounds.
        /// </summary>
        public void Add(T item, BoundingBox bounds)
        {
            Add(item, (o) => { return o.Contains(bounds) != ContainmentType.Disjoint; });
        }

        /// <summary>
        /// Add a new value to this Octree<T> with the specified predication.
        /// </summary>
        /// <param name="contains">
        /// Wether the bounds of the target OctreeNode<T> contains this value.
        /// </param>
        public void Add(T value, Predicate<BoundingBox> contains)
        {
            Add(value, Root, contains);
        }

        /// <summary>
        /// Add a new value to the target node of this Octree<T> with the
        /// specified predication.
        /// </summary>
        /// <param name="node">
        /// The node to add.
        /// </param>
        /// <param name="contains">
        /// Wether the bounds of the target OctreeNode<T> contains this value.
        /// </param>
        public void Add(T value, OctreeNode<T> target, Predicate<BoundingBox> contains)
        {
            if (target.Tree != this)
                throw new InvalidOperationException(
                    "The node must be a child of this Octree<T>.");

            if (target.Depth >= MaxDepth)
            {
                target.Add(value);
                return;
            }

            stack.Push(target);

            while (stack.Count > 0)
            {
                OctreeNode<T> child;
                OctreeNode<T> node = stack.Pop();

                if (contains(node.Bounds))
                {
                    Vector3 min = node.Bounds.Min;
                    Vector3 max = node.Bounds.Max;
                    Vector3 center = (max + min) / 2;

                    bool hasChild = false;

                    if (node.childNodes == null || node.childNodes.Length <= 0)
                    {
                        if (node.childNodes == null)
                            node.childNodes = new OctreeNode<T>[ChildCount];

                        for (int i = 0; i < node.childNodes.Length; i++)
                        {
                            child = new OctreeNode<T>();
                            child.Tree = this;
                            child.Depth = node.Depth + 1;
                            child.Parent = node; 
                            child.Bounds = new BoundingBox
                            {
                                Min = new Vector3()
                                {
                                    X = (i % 2 == 0 ? min.X : center.X),
                                    Y = ((i < 2 || i == 4 || i == 5) ? min.Y : center.Y),
                                    Z = (i < 4 ? min.Z : center.Z),
                                },
                                Max = new Vector3()
                                {
                                    X = (i % 2 == 0 ? center.X : max.X),
                                    Y = ((i < 2 || i == 4 || i == 5) ? center.Y : max.Y),
                                    Z = (i < 4 ? center.Z : max.Z),
                                },
                            };

                            node.childNodes[i] = child;
                        }
                    }

                    for (int i = 0; i < node.childNodes.Length; i++)
                    {
                        child = node.childNodes[i];

                        if (child.Depth > MaxDepth || !contains(child.Bounds))
                            continue;

                        hasChild = true;
                    }

                    node.HasChildren = hasChild;

                    if (hasChild)
                    {
                        foreach (OctreeNode<T> newChild in node.ChildNodes)
                        {
                            stack.Push(newChild);
                        }
                    }
                    else
                    {
                        Count++;
                        node.Add(value);
                    }
                }
            }

            CalculateDepth();
        }

        private void CalculateDepth()
        {
            Depth = 0;

            foreach (OctreeNode<T> node in this)
            {
                if (node.Depth > Depth)
                    Depth = node.Depth;
            }
        }

        /// <summary>
        /// Removes all the acurance of the item from the Octree<T>.
        /// </summary>
        public bool Remove(T item)
        {
            bool result = false;

            foreach (var node in Find(TreeNodeType.Leaf, (o) =>
            {
                return o.HasChildren || o.Values.Contains(item);
            }))
            {
                result = true;
                Count -= node.Remove(item);
            }

            return result;
        }

        /// <summary>
        /// Clears the Octree.
        /// </summary>
        public void Clear()
        {
            foreach (var node in Find(TreeNodeType.Leaf, (o) =>
            {
                return true;
            }))
            {
                node.Clear();
            }

            Count = 0;
            Depth = 0;
            Root.HasChildren = false;
        }

        /// <summary>
        /// Search down the tree, compare each node with the condition to
        /// find the first occurance of the node with the specified TreeNodeType.
        /// </summary>
        public OctreeNode<T> FindFirst(TreeNodeType nodeType, Predicate<OctreeNode<T>> condition)
        {
            return FindFirst(Root, nodeType, condition);
        }

        /// <summary>
        /// Search down the tree, compare each node with the condition to
        /// find the first occurance of the node with the specified TreeNodeType.
        /// </summary>
        public OctreeNode<T> FindFirst(OctreeNode<T> target, TreeNodeType nodeType, Predicate<OctreeNode<T>> condition)
        {
            if (target.Tree != this)
                throw new InvalidOperationException(
                    "The node must be a child of this Octree<T>.");

            stack.Push(target);

            while (stack.Count > 0)
            {
                OctreeNode<T> node = stack.Pop();

                if (condition(node))
                {
                    if (nodeType == TreeNodeType.All ||
                       (nodeType == TreeNodeType.Leaf && !node.HasChildren) ||
                       (nodeType == TreeNodeType.Bottom && !node.HasChildren && node.Depth == Depth))
                    {
                        stack.Clear();
                        return node;
                    }

                    foreach (OctreeNode<T> child in node.ChildNodes)
                    {
                        stack.Push(child);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Search down the tree, compare each node with the condition to
        /// find all the nodes with the specified TreeNodeType.
        /// </summary>
        public IEnumerable<OctreeNode<T>> Find(TreeNodeType nodeType, Predicate<OctreeNode<T>> condition)
        {
            return Find(Root, nodeType, condition);
        }

        /// <summary>
        /// Search down the tree, compare each node with the condition to
        /// find all the nodes with the specified TreeNodeType.
        /// </summary>
        public IEnumerable<OctreeNode<T>> Find(OctreeNode<T> target, TreeNodeType nodeType, Predicate<OctreeNode<T>> condition)
        {
            if (target.Tree != this)
                throw new InvalidOperationException(
                    "The node must be a child of this Octree<T>.");

            stack.Push(target);

            while (stack.Count > 0)
            {
                OctreeNode<T> node = stack.Pop();

                if (condition(node))
                {
                    if (nodeType == TreeNodeType.All ||
                       (nodeType == TreeNodeType.Leaf && !node.HasChildren) ||
                       (nodeType == TreeNodeType.Bottom && !node.HasChildren && node.Depth == Depth))
                    {
                        yield return node;
                    }

                    foreach (OctreeNode<T> child in node.ChildNodes)
                    {
                        stack.Push(child);
                    }
                }
            }
        }

        public IEnumerator<OctreeNode<T>> GetEnumerator()
        {
            stack.Push(Root);

            while (stack.Count > 0)
            {
                OctreeNode<T> node = stack.Pop();

                if (node.HasChildren)
                {
                    foreach (OctreeNode<T> child in node.ChildNodes)
                    {
                        stack.Push(child);
                    }
                }

                yield return node;
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            stack.Push(Root);

            while (stack.Count > 0)
            {
                OctreeNode<T> node = stack.Pop();

                if (node.HasChildren)
                {
                    foreach (OctreeNode<T> child in node.ChildNodes)
                    {
                        stack.Push(child);
                    }
                }

                foreach (T value in node.Values)
                    yield return value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<OctreeNode<T>>)this).GetEnumerator();
        }

        /// <summary>
        /// Stack for enumeration.
        /// </summary>
        static Stack<OctreeNode<T>> stack = new Stack<OctreeNode<T>>();

        void ICollection<T>.Add(T item)
        {
            throw new InvalidOperationException(
                "Cannot add to the tree, position info needs to be provided.");
        }

        public bool Contains(T item)
        {
            foreach (object value in (IEnumerable)this)
            {
                if (item.Equals(value))
                    return true;
            }

            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (T value in (IEnumerable)this)
            {
                array[arrayIndex++] = value;
            }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }
    }

    /// <summary>
    /// Represents a node in Octree<T>.
    /// </summary>
    public sealed class OctreeNode<T>
    {
        internal Octree<T> Tree;

        /// <summary>
        /// Gets a value indicating whether the control contains child nodes.
        /// </summary>
        public bool HasChildren { get; internal set; }

        /// <summary>
        /// Gets the parent node of the Octree<T> Node.
        /// </summary>
        public OctreeNode<T> Parent { get; internal set; }

        /// <summary>
        /// Gets the bounds of the Octree<T> node.
        /// </summary>
        public BoundingBox Bounds { get; internal set; }

        /// <summary>
        /// Gets the depth of this Octree<T> node.
        /// </summary>
        public int Depth { get; internal set; }

        /// <summary>
        /// Gets or sets a user defined tag.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Gets the values contained in the node.
        /// </summary>
        public ICollection<T> Values { get; internal set; }

        /// <summary>
        /// Gets a read-only collection of the 8 child nodes.
        /// </summary>
        public ICollection<OctreeNode<T>> ChildNodes
        {
            get { return HasChildren ? childNodes : null; }
        }

        internal OctreeNode<T>[] childNodes;

        internal OctreeNode() { }

        internal void Add(T value)
        {
            if (Values == null)
                Values = new InternalReadOnlyCollection<T>();

            if (!Values.Contains(value))
                ((InternalReadOnlyCollection<T>)Values).InternalItems.Add(value);
        }

        internal int Remove(T value)
        {
            int num = 0;

            if (Values != null)
            {
                IList<T> list = ((InternalReadOnlyCollection<T>)Values).InternalItems;
                while (list.Remove(value)) { num++; }
            }

            return num;
        }

        internal void Clear()
        {
            if (Values != null)
                Values.Clear();
        }
    }


    internal class OctreeReader<T> : ContentTypeReader<Octree<T>>
    {
        protected override Octree<T> Read(ContentReader input, Octree<T> existingInstance)
        {
            Octree<T> tree = new Octree<T>();

            tree.Count = input.ReadInt32();
            tree.Depth = input.ReadInt32();
            tree.MaxDepth = input.ReadInt32();
            tree.Bounds = input.ReadObject<BoundingBox>();
            tree.Root = input.ReadRawObject<OctreeNode<T>>(new OctreeReader<T>());

            // Fix reference
            Stack<OctreeNode<T>> stack = new Stack<OctreeNode<T>>();

            stack.Push(tree.Root);

            while (stack.Count > 0)
            {
                OctreeNode<T> node = stack.Pop();

                node.Tree = tree;

                if (node.HasChildren)
                {
                    foreach (OctreeNode<T> child in node.ChildNodes)
                    {
                        child.Parent = node;
                        stack.Push(child);
                    }
                }
            }

            return tree;
        }
    }

    internal class OctreeNodeReader<T> : ContentTypeReader<OctreeNode<T>>
    {
        protected override OctreeNode<T> Read(ContentReader input, OctreeNode<T> existingInstance)
        {
            OctreeNode<T> node = new OctreeNode<T>();

            node.HasChildren = input.ReadBoolean();
            node.Depth = input.ReadInt32();
            node.Bounds = input.ReadObject<BoundingBox>();
            node.Values = input.ReadObject<ICollection<T>>();
            node.Tag = input.ReadObject<object>();

            if (node.HasChildren)
                node.childNodes = input.ReadObject<OctreeNode<T>[]>();

            return node;
        }
    }
}