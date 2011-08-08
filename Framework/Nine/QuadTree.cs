#region Copyright 2010 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2010 - 2011 (c) Engine Nine. All Rights Reserved.
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
    public class QuadTree<T> : SpacePartitionTree<T, QuadTreeNode<T>>
    {
        /// <summary>
        /// Specifies the total number of child nodes (4) in the QuadTree.
        /// </summary>
        const int ChildCount = 4;

        /// <summary>
        /// Gets the bounds of the QuadTree node.
        /// </summary>
        public BoundingRectangle Bounds { get { return Root.Bounds; } }

        /// <summary>
        /// For serialization.
        /// </summary>
        internal QuadTree() { }

        /// <summary>
        /// Creates a new Octree with the specified boundary.
        /// </summary>
        public QuadTree(BoundingRectangle bounds, int maxDepth)
            : base(new QuadTreeNode<T>() { Bounds = bounds }, maxDepth)
        {

        }

        protected override QuadTreeNode<T>[] ExpandNode(QuadTreeNode<T> node)
        {
            var childNodes = new QuadTreeNode<T>[ChildCount];
            QuadTreeNode<T> quadTreeNode = (QuadTreeNode<T>)node;

            Vector2 center;
            Vector2 min = quadTreeNode.Bounds.Min;
            Vector2 max = quadTreeNode.Bounds.Max;
            Vector2.Add(ref min, ref max, out center);
            Vector2.Multiply(ref center, 0.5f, out center);

            for (int i = 0; i < ChildCount; i++)
            {
                var child = new QuadTreeNode<T>();
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
                childNodes[i] = child;
            }
            return childNodes;
        }
    }

    /// <summary>
    /// Represents a node in QuadTree.
    /// </summary>
    public sealed class QuadTreeNode<T> : SpacePartitionTreeNode<T, QuadTreeNode<T>>
    {
        /// <summary>
        /// Gets the bounds of the QuadTree node.
        /// </summary>
        public BoundingRectangle Bounds { get; internal set; }

        internal QuadTreeNode() { }
    }

    internal class QuadTreeReader<T> : ContentTypeReader<QuadTree<T>>
    {
        protected override QuadTree<T> Read(ContentReader input, QuadTree<T> existingInstance)
        {
            if (existingInstance == null)
                existingInstance = new QuadTree<T>();

            existingInstance.MaxDepth = input.ReadInt32();
            existingInstance.Root = input.ReadRawObject<QuadTreeNode<T>>(new QuadTreeNodeReader<T>());

            // Fix reference
            Stack<QuadTreeNode<T>> stack = new Stack<QuadTreeNode<T>>();

            stack.Push(existingInstance.Root);

            while (stack.Count > 0)
            {
                QuadTreeNode<T> node = stack.Pop();

                node.Tree = existingInstance;

                if (node.HasChildren)
                {
                    foreach (QuadTreeNode<T> child in node.Children)
                    {
                        child.Parent = node;
                        stack.Push(child);
                    }
                }
            }

            return existingInstance;
        }
    }

    internal class QuadTreeNodeReader<T> : ContentTypeReader<QuadTreeNode<T>>
    {
        protected override QuadTreeNode<T> Read(ContentReader input, QuadTreeNode<T> existingInstance)
        {
            if (existingInstance == null)
                existingInstance = new QuadTreeNode<T>();

            existingInstance.HasChildren = input.ReadBoolean();
            existingInstance.Depth = input.ReadInt32();
            existingInstance.Bounds = input.ReadObject<BoundingRectangle>();
            existingInstance.Value = input.ReadObject<T>();

            if (existingInstance.HasChildren)
            {
                existingInstance.childNodes = input.ReadObject<QuadTreeNode<T>[]>();
                existingInstance.children = new ReadOnlyCollection<QuadTreeNode<T>>(existingInstance.childNodes);
            }
            return existingInstance;
        }
    }
}