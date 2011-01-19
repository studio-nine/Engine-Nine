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
    /// Represents a space partition structure based on QuadTree.
    /// </summary>
    public class QuadTree<T>
    {
        /// <summary>
        /// Specifies the total number of child nodes (4) in the QuadTree.
        /// </summary>
        const int ChildCount = 4;

        /// <summary>
        /// Gets the root QuadTreeNode of this QuadTree.
        /// </summary>
        public QuadTreeNode<T> Root { get; internal set; }

        /// <summary>
        /// Gets the bounds of the QuadTree node.
        /// </summary>
        public BoundingRectangle Bounds { get; internal set; }

        /// <summary>
        /// Gets the max depth of this QuadTree.
        /// </summary>
        public int MaxDepth { get; internal set; }

        /// <summary>
        /// For serialization.
        /// </summary>
        internal QuadTree() { }

        /// <summary>
        /// Creates a new QuadTree with the specified boundary.
        /// </summary>
        public QuadTree(BoundingRectangle bounds, int maxDepth)
        {
            Root = new QuadTreeNode<T>();
            Root.Depth = 0;
            Root.Parent = null;
            Root.Tree = this;

            BoundingRectangle fixedBounds = new BoundingRectangle();
            fixedBounds.Min.X = Math.Min(bounds.Min.X, bounds.Max.X);
            fixedBounds.Min.Y = Math.Min(bounds.Min.Y, bounds.Max.Y);
            fixedBounds.Max.X = Math.Max(bounds.Max.X, bounds.Max.X);
            fixedBounds.Max.Y = Math.Max(bounds.Max.Y, bounds.Max.Y);
            Root.Bounds = fixedBounds;

            Bounds = bounds;

            MaxDepth = maxDepth;
        }

        /// <summary>
        /// Expand the target node of with 4 child nodes.
        /// </summary>
        public bool Expand(QuadTreeNode<T> node)
        {
            if (node.Tree != this)
                throw new InvalidOperationException("The node must be a child of this QuadTree<T>.");

            if (node.Depth >= MaxDepth)
                return false;

            if (node.HasChildren)
                return true;

            if (node.childNodes == null || node.childNodes.Length <= 0)
            {
                node.HasChildren = true;
                node.childNodes = new QuadTreeNode<T>[ChildCount];

                Vector2 min = node.Bounds.Min;
                Vector2 max = node.Bounds.Max;
                Vector2 center = (max + min) / 2;

                for (int i = 0; i < node.childNodes.Length; i++)
                {
                    QuadTreeNode<T> child;
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

            return true;
        }

        /// <summary>
        /// Expand the root node and all its child nodes with the specified predication.
        /// </summary>
        /// <param name="contains">
        /// Whether the bounds of the target QuadTreeNode contains this value.
        /// </param>
        public void ExpandAll(Predicate<QuadTreeNode<T>> contains)
        {
            ExpandAll(Root, contains);
        }

        /// <summary>
        /// Expand the target node and all its child nodes with the specified predication.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="contains">
        /// Whether the bounds of the target QuadTreeNode contains this value.
        /// </param>
        public void ExpandAll(QuadTreeNode<T> target, Predicate<QuadTreeNode<T>> contains)
        {
            foreach (var node in Traverse(target, (o) => { return contains(o) && Expand(o); })) { }
        }

        /// <summary>
        /// Removes all the child nodes of the target node.
        /// </summary>
        public void Collapse(QuadTreeNode<T> target)
        {
            stack.Push(target);

            while (stack.Count > 0)
            {
                QuadTreeNode<T> node = stack.Pop();

                node.HasChildren = false;
                node.Value = default(T);

                foreach (QuadTreeNode<T> child in node.childNodes)
                {
                    stack.Push(child);
                }
            }
        }

        /// <summary>
        /// Traverses the tree using Depth First Search (DFS). Compare each node 
        /// with the condition to determine whether the traverse should continue.
        /// </summary>
        public IEnumerable<QuadTreeNode<T>> Traverse(Predicate<QuadTreeNode<T>> condition)
        {
            return Traverse(Root, condition);
        }

        /// <summary>
        /// Traverses the tree using Depth First Search (DFS) from the target. Compare each node 
        /// with the condition to determine whether the traverse should continue.
        /// </summary>
        public IEnumerable<QuadTreeNode<T>> Traverse(QuadTreeNode<T> target, Predicate<QuadTreeNode<T>> condition)
        {
            if (target.Tree != this)
                throw new InvalidOperationException("The node must be a child of this QuadTree<T>.");

            stack.Clear();
            stack.Push(target);

            while (stack.Count > 0)
            {
                QuadTreeNode<T> node = stack.Pop();

                if (condition(node))
                {
                    yield return node;

                    foreach (QuadTreeNode<T> child in node.Children)
                    {
                        stack.Push(child);
                    }
                }
            }
        }

        /// <summary>
        /// Stack for enumeration.
        /// </summary>
        static Stack<QuadTreeNode<T>> stack = new Stack<QuadTreeNode<T>>();
    }

    /// <summary>
    /// Represents a node in QuadTree.
    /// </summary>
    public sealed class QuadTreeNode<T>
    {
        internal QuadTree<T> Tree;

        /// <summary>
        /// Gets a value indicating whether the control contains child nodes.
        /// </summary>
        public bool HasChildren { get; internal set; }

        /// <summary>
        /// Gets the parent node of the QuadTree Node.
        /// </summary>
        public QuadTreeNode<T> Parent { get; internal set; }

        /// <summary>
        /// Gets the bounds of the QuadTree node.
        /// </summary>
        public BoundingRectangle Bounds { get; internal set; }

        /// <summary>
        /// Gets the depth of this QuadTree node.
        /// </summary>
        public int Depth { get; internal set; }

        /// <summary>
        /// Gets or sets the value contained in the node.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Gets a read-only collection of the 4 child nodes.
        /// </summary>
        public ICollection<QuadTreeNode<T>> Children
        {
            get { return HasChildren ? childNodes : Empty; }
        }

        static QuadTreeNode<T>[] Empty = new QuadTreeNode<T>[0];

        internal QuadTreeNode<T>[] childNodes;

        internal QuadTreeNode() { }
    }


    internal class QuadTreeReader<T> : ContentTypeReader<QuadTree<T>>
    {
        protected override QuadTree<T> Read(ContentReader input, QuadTree<T> existingInstance)
        {
            QuadTree<T> tree = new QuadTree<T>();

            tree.MaxDepth = input.ReadInt32();
            tree.Bounds = input.ReadObject<BoundingRectangle>();
            tree.Root = input.ReadRawObject<QuadTreeNode<T>>(new QuadTreeNodeReader<T>());

            // Fix reference
            Stack<QuadTreeNode<T>> stack = new Stack<QuadTreeNode<T>>();

            stack.Push(tree.Root);

            while (stack.Count > 0)
            {
                QuadTreeNode<T> node = stack.Pop();

                node.Tree = tree;

                if (node.HasChildren)
                {
                    foreach (QuadTreeNode<T> child in node.Children)
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
            node.Value = input.ReadObject<T>();

            if (node.HasChildren)
                node.childNodes = input.ReadObject<QuadTreeNode<T>[]>();

            return node;
        }
    }
}