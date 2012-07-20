namespace Nine
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;


    /// <summary>
    /// Determines how to traverse the next node when traversing a space partition tree.
    /// </summary>
    public enum TraverseOptions
    {
        /// <summary>
        /// The traverse operation should continue to visit the next node.
        /// </summary>
        Continue,

        /// <summary>
        /// The traverse operation should skip the current node and its child nodes.
        /// </summary>
        Skip,

        /// <summary>
        /// The traverse operation should stop visiting nodes.
        /// </summary>
        Stop,
    }

    /// <summary>
    /// Represents a basic space partition tree structure.
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

        private int nodeExpanded;
        private Predicate<TNode> condition;
        private Func<TNode, TraverseOptions> expandAllPredicate;

        /// <summary>
        /// For serialization.
        /// </summary>
        internal SpacePartitionTree() 
        {
            expandAllPredicate = new Func<TNode, TraverseOptions>(ExpandAllPredicate);
        }

        /// <summary>
        /// Creates a new SpacePartitionTree with the specified boundary.
        /// </summary>
        public SpacePartitionTree(TNode root, int maxDepth) : this()
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
            this.nodeExpanded = 0;
            this.condition = condition;
            
            Traverse(target, expandAllPredicate);

            this.condition = null;
            return nodeExpanded;
        }

        private TraverseOptions ExpandAllPredicate(TNode node)
        {
            nodeExpanded++;
            return condition(node) && Expand(node) ? TraverseOptions.Continue : TraverseOptions.Skip;
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

            for (int i = 0; i < target.Children.Count; i++)
            {
                var child = target.Children[i];
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
        /// <param name="result">
        /// Returns true when the trverse should continue.
        /// </param>
        public void Traverse(Func<TNode, TraverseOptions> result)
        {
            Traverse(Root, result);
        }

        /// <summary>
        /// Traverses the tree using Depth First Search (DFS) from the target. Compare each node 

        /// with the condition to determine whether the traverse should continue.
        /// </summary>
        /// <param name="result">
        /// Returns true when the trverse should continue.
        /// </param>
        public void Traverse(TNode target, Func<TNode, TraverseOptions> result)
        {
            if (target.Tree != this)
                throw new InvalidOperationException(Strings.NodeMustBeAPartOfTheTree);

            stack.Clear();
            stack.Push(target);

            while (stack.Count > 0)
            {
                TNode node = stack.Pop();

                var traverseOptions = result(node);
                
                if (traverseOptions == TraverseOptions.Stop)
                    break;

                if (traverseOptions == TraverseOptions.Continue)
                {
                    for (int i = 0; i < node.Children.Count; i++)
                    {
                        stack.Push(node.Children[i]);
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
        /// Gets or sets the value contained in the node.
        /// </summary>
        public T Value;

        /// <summary>
        /// Gets a value indicating whether this node contains any child nodes.
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