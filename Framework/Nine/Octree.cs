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
    public class Octree<T>
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
        /// Gets the max depth of this Octree<T>.
        /// </summary>
        public int MaxDepth { get; internal set; }

        /// <summary>
        /// For serialization.
        /// </summary>
        internal Octree() { }

        /// <summary>
        /// Creates a new Octree<T> with the specified boundary.
        /// </summary>
        public Octree(int maxDepth, BoundingBox bounds)
        {
            Root = new OctreeNode<T>();
            Root.Depth = 0;
            Root.Parent = null;
            Root.Tree = this;

            BoundingBox fixedBounds = new BoundingBox();
            fixedBounds.Min.X = Math.Min(bounds.Min.X, bounds.Max.X);
            fixedBounds.Min.Y = Math.Min(bounds.Min.Y, bounds.Max.Y);
            fixedBounds.Min.Z = Math.Min(bounds.Min.Z, bounds.Max.Z);
            fixedBounds.Max.X = Math.Max(bounds.Max.X, bounds.Max.X);
            fixedBounds.Max.Y = Math.Max(bounds.Max.Y, bounds.Max.Y);
            fixedBounds.Max.Z = Math.Max(bounds.Max.Z, bounds.Max.Z);
            Root.Bounds = fixedBounds;

            Bounds = bounds;

            MaxDepth = maxDepth;
        }

        /// <summary>
        /// Expand the target node of with 8 child nodes.
        /// </summary>
        public bool Expand(OctreeNode<T> node)
        {
            if (node.Tree != this)
                throw new InvalidOperationException("The node must be a child of this Octree<T>.");

            if (node.Depth >= MaxDepth)
                return false;

            if (node.HasChildren)
                return true;

            if (node.childNodes == null || node.childNodes.Length <= 0)
            {
                node.HasChildren = true;
                node.childNodes = new OctreeNode<T>[ChildCount];
                
                Vector3 min = node.Bounds.Min;
                Vector3 max = node.Bounds.Max;
                Vector3 center = (max + min) / 2;

                for (int i = 0; i < node.childNodes.Length; i++)
                {
                    OctreeNode<T> child;
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

            return true;
        }

        /// <summary>
        /// Expand the root node and all its child nodes with the specified predication.
        /// </summary>
        /// <param name="contains">
        /// Wether the bounds of the target OctreeNode<T> contains this value.
        /// </param>
        public void ExpandAll(Predicate<OctreeNode<T>> contains)
        {
            ExpandAll(Root, contains);
        }

        /// <summary>
        /// Expand the target node and all its child nodes with the specified predication.
        /// </summary>
        /// <param name="contains">
        /// Wether the bounds of the target OctreeNode<T> contains this value.
        /// </param>
        public void ExpandAll(OctreeNode<T> target, Predicate<OctreeNode<T>> contains)
        {
            foreach (var node in Traverse(target, (o) => { return contains(o) && Expand(o); })) { }
        }

        /// <summary>
        /// Removes all the child nodes of the target node.
        /// </summary>
        public void Collapse(OctreeNode<T> target)
        {
            stack.Push(target);

            while (stack.Count > 0)
            {
                OctreeNode<T> node = stack.Pop();

                node.HasChildren = false;
                node.Value = default(T);

                foreach (OctreeNode<T> child in node.childNodes)
                {
                    stack.Push(child);
                }
            }
        }

        /// <summary>
        /// Traverses the tree using Depth First Search (DFS). Compare each node 
        /// with the condition to determine whether the traverse should continue.
        /// </summary>
        public IEnumerable<OctreeNode<T>> Traverse(Predicate<OctreeNode<T>> condition)
        {
            return Traverse(Root, condition);
        }

        /// <summary>
        /// Traverses the tree using Depth First Search (DFS) from the target. Compare each node 
        /// with the condition to determine whether the traverse should continue.
        /// </summary>
        public IEnumerable<OctreeNode<T>> Traverse(OctreeNode<T> target, Predicate<OctreeNode<T>> condition)
        {
            if (target.Tree != this)
                throw new InvalidOperationException("The node must be a child of this Octree<T>.");
            
            stack.Clear();
            stack.Push(target);

            while (stack.Count > 0)
            {
                OctreeNode<T> node = stack.Pop();

                if (condition(node))
                {
                    yield return node;

                    foreach (OctreeNode<T> child in node.ChildNodes)
                    {
                        stack.Push(child);
                    }
                }
            }
        }

        /// <summary>
        /// Stack for enumeration.
        /// </summary>
        static Stack<OctreeNode<T>> stack = new Stack<OctreeNode<T>>();
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
        /// Gets or sets the value contained in the node.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Gets a read-only collection of the 8 child nodes.
        /// </summary>
        public ICollection<OctreeNode<T>> ChildNodes
        {
            get { return HasChildren ? childNodes : Empty; }
        }

        static OctreeNode<T>[] Empty = new OctreeNode<T>[0];

        internal OctreeNode<T>[] childNodes;

        internal OctreeNode() { }
    }

    
    internal class OctreeReader<T> : ContentTypeReader<Octree<T>>
    {
        protected override Octree<T> Read(ContentReader input, Octree<T> existingInstance)
        {
            Octree<T> tree = new Octree<T>();

            tree.MaxDepth = input.ReadInt32();
            tree.Bounds = input.ReadObject<BoundingBox>();
            tree.Root = input.ReadRawObject<OctreeNode<T>>(new OctreeNodeReader<T>());

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
            node.Value = input.ReadObject<T>();

            if (node.HasChildren)
                node.childNodes = input.ReadObject<OctreeNode<T>[]>();

            return node;
        }
    }
}