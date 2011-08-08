#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
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
    /// Represents basic a space partition tree structure.
    /// </summary>
    public abstract class SpacePartitionTree<T, TNode> : IEnumerable<TNode> where TNode : SpacePartitionTreeNode<T, TNode>
    {
        /// <summary>
        /// Gets the root SpacePartitionTreeNode of this SpacePartitionTree.
        /// </summary>
        public TNode Root { get; internal set; }

        /// <summary>
        /// Gets the max depth of this SpacePartitionTree.
        /// </summary>
        public int MaxDepth { get; internal set; }

        /// <summary>
        /// For serialization.
        /// </summary>
        internal SpacePartitionTree() { }

        /// <summary>
        /// Creates a new SpacePartitionTree with the specified boundary.
        /// </summary>
        public SpacePartitionTree(TNode root, int maxDepth)
        {
            if (root == null)
                throw new ArgumentNullException("root");
            if (maxDepth < 0)
                throw new ArgumentOutOfRangeException("maxDepth");

            Root = root;
            Root.Depth = 0;
            Root.Parent = null;
            Root.Tree = this;
            MaxDepth = maxDepth;
        }

        /// <summary>
        /// Expand the target node with 8 child nodes.
        /// </summary>
        /// <returns>
        /// If max depth is reached, returns true, otherwise, return false;
        /// </returns>
        public bool Expand(TNode node)
        {
            if (node.Tree != this)
                throw new InvalidOperationException(Strings.NodeMustBeAPartOfTheTree);

            if (node.Depth >= MaxDepth)
                return false;

            if (node.HasChildren)
                return true;

            node.HasChildren = true;
            if (node.childNodes == null || node.childNodes.Length <= 0)
            {
                node.childNodes = ExpandNode(node);
                node.children = new ReadOnlyCollection<TNode>(node.childNodes);

                foreach (var child in node.childNodes)
                {
                    child.Tree = this;
                    child.Depth = node.Depth + 1;
                    child.Parent = node;
                }
            }
            return true;
        }

        /// <summary>
        /// When implemented, return the child nodes of the target node.
        /// </summary>
        protected abstract TNode[] ExpandNode(TNode node);

        /// <summary>
        /// Expand the root node and all its child nodes with the specified predication.
        /// </summary>
        /// <param name="condition">
        /// Wether the bounds of the target SpacePartitionTreeNode contains this value.
        /// </param>
        /// <returns>
        /// Number of node expanded.
        /// </returns>
        public int ExpandAll(Predicate<TNode> condition)
        {
            return ExpandAll(Root, condition);
        }

        /// <summary>
        /// Expand the target node and all its child nodes with the specified predication.
        /// </summary>
        /// <param name="condition">
        /// Whether the bounds of the target SpacePartitionTreeNode contains this value.
        /// </param>
        /// <returns>
        /// Number of node expanded.
        /// </returns>
        public int ExpandAll(TNode target, Predicate<TNode> condition)
        {
            int count = 0;
            foreach (var node in Traverse(target, (o) => { return condition(o) && Expand(o); }))
            {
                count++;
            }
            return count;
        }

        /// <summary>
        /// Removes all the child nodes of the root.
        /// </summary>
        public void Collapse()
        {
            Collapse(Root);
        }

        /// <summary>
        /// Removes all the child nodes of the root.
        /// </summary>
        public void Collapse(TNode target)
        {
            Collapse(target, node => true);
        }

        /// <summary>
        /// Removes all the child nodes of the target node.
        /// </summary>
        /// <returns>
        /// Number of nodes collapsed.
        /// </returns>
        public int Collapse(TNode target, Predicate<TNode> condition)
        {
            if (target.Tree != this)
                throw new InvalidOperationException(Strings.NodeMustBeAPartOfTheTree);

            int count = 1;
            bool collapsedThisNode = true;

            foreach (TNode child in target.Children)
            {
                int childCount = Collapse(child, condition);
                if (childCount <= 0)
                    collapsedThisNode = false;
                else
                    count += childCount;
            }

            if (collapsedThisNode && condition(target))
            {
                target.HasChildren = false;
                target.Value = default(T);
                return count;
            }
            return 0;
        }

        /// <summary>
        /// Traverses the tree using Depth First Search (DFS). Compare each node 
        /// with the condition to determine whether the traverse should continue.
        /// </summary>
        public IEnumerable<TNode> Traverse(Predicate<TNode> condition)
        {
            return Traverse(Root, condition);
        }

        /// <summary>
        /// Traverses the tree using Depth First Search (DFS) from the target. Compare each node 
        /// with the condition to determine whether the traverse should continue.
        /// </summary>
        public IEnumerable<TNode> Traverse(TNode target, Predicate<TNode> condition)
        {
            if (target.Tree != this)
                throw new InvalidOperationException(Strings.NodeMustBeAPartOfTheTree);

            stack.Clear();
            stack.Push(target);

            while (stack.Count > 0)
            {
                TNode node = stack.Pop();

                if (condition(node))
                {
                    yield return node;

                    foreach (TNode child in node.Children)
                    {
                        stack.Push(child);
                    }
                }
            }
        }

        /// <summary>
        /// Stack for enumeration.
        /// </summary>
        static Stack<TNode> stack = new Stack<TNode>();

        public IEnumerator<TNode> GetEnumerator()
        {
            return GetEnumerator(Root).GetEnumerator();
        }

        IEnumerable<TNode> GetEnumerator(TNode node)
        {
            yield return node;
            if (node != null && node.HasChildren)
            {
                foreach (var child in node.Children)
                {
                    foreach (var result in GetEnumerator(child))
                        yield return result;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// Represents a node in SpacePartitionTree.
    /// </summary>
    public abstract class SpacePartitionTreeNode<T, TNode> where TNode : SpacePartitionTreeNode<T, TNode>
    {
        internal object Tree;

        /// <summary>
        /// Gets a value indicating whether the control contains child nodes.
        /// </summary>
        public bool HasChildren { get; internal set; }

        /// <summary>
        /// Gets the parent node of the SpacePartitionTree Node.
        /// </summary>
        public TNode Parent { get; internal set; }

        /// <summary>
        /// Gets the depth of this SpacePartitionTree node.
        /// </summary>
        public int Depth { get; internal set; }

        /// <summary>
        /// Gets or sets the value contained in the node.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Gets a read-only collection of the 8 child nodes.
        /// </summary>
        public ReadOnlyCollection<TNode> Children
        {
            get { return HasChildren ? children : Empty; }
        }

        static ReadOnlyCollection<TNode> Empty = new ReadOnlyCollection<TNode>(new TNode[0]);

        internal ReadOnlyCollection<TNode> children;
        internal TNode[] childNodes;

        protected SpacePartitionTreeNode() { }
    }
}