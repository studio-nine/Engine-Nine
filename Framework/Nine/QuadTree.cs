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
    /// Defines three different tree node types to be used in the tree
    /// Find method.
    /// </summary>
    public enum TreeNodeType
    {
        /// <summary>
        /// Defines all tree nodes.
        /// </summary>
        All,

        /// <summary>
        /// Defines only leaf nodes which don't have any children
        /// </summary>
        Leaf,

        /// <summary>
        /// Defines a leaf node whose Depth equals the Depth of the tree.
        /// </summary>
        Bottom,
    }

    /// <summary>
    /// Represents a space partition structure based on Quadtree.
    /// </summary>
    public class QuadTree<T> : IEnumerable<QuadTreeNode<T>>, ICollection<T>
    {
        /// <summary>
        /// Specifies the total number of child nodes (4) in the QuadTree<T>.
        /// </summary>
        public const int ChildCount = 4;

        /// <summary>
        /// Gets the root QuadTreeNode<T> of this QuadTree<T>.
        /// </summary>
        public QuadTreeNode<T> Root { get; internal set; }

        /// <summary>
        /// Gets the bounds of the QuadTree<T> node.
        /// </summary>
        public BoundingRectangle Bounds { get; internal set; }

        /// <summary>
        /// Gets or sets the max depth of this QuadTree<T>.
        /// </summary>
        public int MaxDepth { get; internal set; }

        /// <summary>
        /// Gets current depth of this QuadTree<T>.
        /// </summary>
        public int Depth { get; internal set; }

        /// <summary>
        /// Gets the count of nodes of this QuadTree<T>.
        /// </summary>
        public int Count { get; internal set; }

        /// <summary>
        /// For serialization.
        /// </summary>
        internal QuadTree() { }

        /// <summary>
        /// Creats a new QuadTree<T> with the specified boundary.
        /// </summary>
        public QuadTree(int maxDepth, BoundingRectangle bounds)
        {
            Root = new QuadTreeNode<T>();
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
        /// Adds a new item to the QuadTree<T> with the specified position.
        /// </summary>
        public void Add(T item, Vector2 position)
        {
            Add(item, (o) => { return o.Contains(position); });
        }

        /// <summary>
        /// Adds a new item to the QuadTree<T> with the specified bounds.
        /// </summary>
        public void Add(T item, BoundingRectangle bounds)
        {
            Add(item, (o) => { return o.Contains(bounds) != ContainmentType.Disjoint; });
        }

        /// <summary>
        /// Add a new value to this QuadTree<T> with the specified predication.
        /// </summary>
        /// <param name="contains">
        /// Wether the bounds of the target QuadTreeNode<T> contains this value.
        /// </param>
        public void Add(T value, Predicate<BoundingRectangle> contains)
        {
            Add(value, Root, contains);
        }

        /// <summary>
        /// Add a new value to the target node of this QuadTree<T> with the
        /// specified predication.
        /// </summary>
        /// <param name="node">
        /// The node to add.
        /// </param>
        /// <param name="contains">
        /// Wether the bounds of the target QuadTreeNode<T> contains this value.
        /// </param>
        public void Add(T value, QuadTreeNode<T> target, Predicate<BoundingRectangle> contains)
        {
            if (target.Tree != this)
                throw new InvalidOperationException(
                    "The node must be a child of this QuadTree<T>.");

            if (target.Depth >= MaxDepth)
            {
                target.Add(value);
                return;
            }

            stack.Push(target);

            while (stack.Count > 0)
            {
                QuadTreeNode<T> child;
                QuadTreeNode<T> node = stack.Pop();

                if (contains(node.Bounds))
                {
                    Vector2 min = node.Bounds.Min;
                    Vector2 max = node.Bounds.Max;
                    Vector2 center = (max + min) / 2;

                    bool hasChild = false;

                    if (node.childNodes == null || node.childNodes.Length <= 0)
                    {
                        if (node.childNodes == null)
                            node.childNodes = new QuadTreeNode<T>[ChildCount];
                        
                        for (int i = 0; i < node.childNodes.Length; i++)
                        {
                            child = new QuadTreeNode<T>();
                            child.Tree = this;
                            child.Depth = node.Depth + 1;
                            child.Parent = node;
                            child.Bounds = new BoundingRectangle
                            {
                                Min = new Vector2()
                                {
                                    X = (i % 2 == 0 ? min.X : center.X),
                                    Y = (i < 2 ? min.Y : center.Y),
                                },
                                Max = new Vector2()
                                {
                                    X = (i % 2 == 0 ? center.X : max.X),
                                    Y = (i < 2 ? center.Y : max.Y),
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
                        foreach (QuadTreeNode<T> newChild in node.ChildNodes)
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

            foreach (QuadTreeNode<T> node in this)
            {
                if (node.Depth > Depth)
                    Depth = node.Depth;
            }
        }

        /// <summary>
        /// Removes all the acurance of the item from the QuadTree<T>.
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
        /// Clears the QuadTree.
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
        public QuadTreeNode<T> FindFirst(TreeNodeType nodeType, Predicate<QuadTreeNode<T>> condition)
        {
            return FindFirst(Root, nodeType, condition);
        }

        /// <summary>
        /// Search down the tree, compare each node with the condition to
        /// find the first occurance of the node with the specified TreeNodeType.
        /// </summary>
        public QuadTreeNode<T> FindFirst(QuadTreeNode<T> target, TreeNodeType nodeType, Predicate<QuadTreeNode<T>> condition)
        {
            if (target.Tree != this)
                throw new InvalidOperationException(
                    "The node must be a child of this QuadTree<T>.");

            stack.Push(target);

            while (stack.Count > 0)
            {
                QuadTreeNode<T> node = stack.Pop();

                if (condition(node))
                {
                    if (nodeType == TreeNodeType.All ||
                       (nodeType == TreeNodeType.Leaf && !node.HasChildren) ||
                       (nodeType == TreeNodeType.Bottom && !node.HasChildren && node.Depth == Depth))
                    {
                        stack.Clear();
                        return node;
                    }

                    foreach (QuadTreeNode<T> child in node.ChildNodes)
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
        public IEnumerable<QuadTreeNode<T>> Find(TreeNodeType nodeType, Predicate<QuadTreeNode<T>> condition)
        {
            return Find(Root, nodeType, condition);
        }

        /// <summary>
        /// Search down the tree, compare each node with the condition to
        /// find all the nodes with the specified TreeNodeType.
        /// </summary>
        public IEnumerable<QuadTreeNode<T>> Find(QuadTreeNode<T> target, TreeNodeType nodeType, Predicate<QuadTreeNode<T>> condition)
        {
            if (target.Tree != this)
                throw new InvalidOperationException(
                    "The node must be a child of this QuadTree<T>.");

            stack.Push(target);

            while (stack.Count > 0)
            {
                QuadTreeNode<T> node = stack.Pop();

                if (condition(node))
                {
                    if (nodeType == TreeNodeType.All ||
                       (nodeType == TreeNodeType.Leaf && !node.HasChildren) ||
                       (nodeType == TreeNodeType.Bottom && !node.HasChildren && node.Depth == Depth))
                    {
                        yield return node;
                    }

                    foreach (QuadTreeNode<T> child in node.ChildNodes)
                    {
                        stack.Push(child);
                    }
                }
            }
        }

        public IEnumerator<QuadTreeNode<T>> GetEnumerator()
        {
            stack.Push(Root);

            while (stack.Count > 0)
            {
                QuadTreeNode<T> node = stack.Pop();

                if (node.HasChildren)
                {
                    foreach (QuadTreeNode<T> child in node.ChildNodes)
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
                QuadTreeNode<T> node = stack.Pop();

                if (node.HasChildren)
                {
                    foreach (QuadTreeNode<T> child in node.ChildNodes)
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
            return ((IEnumerable<QuadTreeNode<T>>)this).GetEnumerator();
        }

        /// <summary>
        /// Stack for enumeration.
        /// </summary>
        static Stack<QuadTreeNode<T>> stack = new Stack<QuadTreeNode<T>>();

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
    /// Represents a node in QuadTree<T>.
    /// </summary>
    public sealed class QuadTreeNode<T>
    {
        internal QuadTree<T> Tree;

        /// <summary>
        /// Gets a value indicating whether the control contains child nodes.
        /// </summary>
        public bool HasChildren { get; internal set; }

        /// <summary>
        /// Gets the parent node of the QuadTree<T> Node.
        /// </summary>
        public QuadTreeNode<T> Parent { get; internal set; }

        /// <summary>
        /// Gets the bounds of the QuadTree<T> node.
        /// </summary>
        public BoundingRectangle Bounds { get; internal set; }
        
        /// <summary>
        /// Gets the depth of this QuadTree<T> node.
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
        /// Gets a read-only collection of the 4 child nodes.
        /// </summary>
        public ICollection<QuadTreeNode<T>> ChildNodes 
        {
            get { return HasChildren ? childNodes : null; }
        }

        internal QuadTreeNode<T>[] childNodes;

        internal QuadTreeNode() { }

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


    internal class QuadTreeReader<T> : ContentTypeReader<QuadTree<T>>
    {
        protected override QuadTree<T> Read(ContentReader input, QuadTree<T> existingInstance)
        {
            QuadTree<T> tree = new QuadTree<T>();

            tree.Count = input.ReadInt32();
            tree.Depth = input.ReadInt32();
            tree.MaxDepth = input.ReadInt32();
            tree.Bounds = input.ReadObject<BoundingRectangle>();
            tree.Root = input.ReadRawObject<QuadTreeNode<T>>(new QuadTreeReader<T>());

            // Fix reference
            Stack<QuadTreeNode<T>> stack = new Stack<QuadTreeNode<T>>();

            stack.Push(tree.Root);

            while (stack.Count > 0)
            {
                QuadTreeNode<T> node = stack.Pop();

                node.Tree = tree;

                if (node.HasChildren)
                {
                    foreach (QuadTreeNode<T> child in node.ChildNodes)
                    {
                        child.Parent = node;
                        stack.Push(child);
                    }
                }
            }            

            return tree;
        }
    }

    internal class QuadTreeNodeReader<T> : ContentTypeReader<QuadTreeNode<T>>
    {
        protected override QuadTreeNode<T> Read(ContentReader input, QuadTreeNode<T> existingInstance)
        {
            QuadTreeNode<T> node = new QuadTreeNode<T>();

            node.HasChildren = input.ReadBoolean();
            node.Depth = input.ReadInt32();
            node.Bounds = input.ReadObject<BoundingRectangle>();
            node.Values = input.ReadObject<ICollection<T>>();
            node.Tag = input.ReadObject<object>();

            if (node.HasChildren)
                node.childNodes = input.ReadObject<QuadTreeNode<T>[]>();

            return node;
        }
    }
}