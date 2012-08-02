namespace Nine
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Xna.Framework;


    #region QuadTreeSceneManager
    /// <summary>
    /// Manages a collection of objects using quad tree.

    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    public class QuadTreeSceneManager : ISceneManager
    {
        internal QuadTree<QuadTreeSceneManagerNodeData<ISpatialQueryable>> Tree;

        /// <summary>
        /// Gets the bounds of this QuadTreeSceneManager.
        /// </summary>
        public BoundingBox Bounds 
        {
            get 
            {
                return new BoundingBox(new Vector3(Tree.Bounds.Min, Tree.Root.Value.MinHeight),
                                       new Vector3(Tree.Bounds.Max, Tree.Root.Value.MaxHeight)); 
            } 
        }

        /// <summary>
        /// Gets the max depth of this QuadTreeSceneManager.
        /// </summary>
        public int MaxDepth { get { return Tree.MaxDepth; } }

        /// <summary>
        /// Creates a new instance of QuadTreeSceneManager.
        /// </summary>
        public QuadTreeSceneManager() : this(new BoundingRectangle(new Vector2(-100f, -100f),
                                                                   new Vector2(100f, 100f)), 5)
        {

        }

        /// <summary>
        /// Creates a new instance of QuadTreeSceneManager.
        /// </summary>
        public QuadTreeSceneManager(BoundingRectangle bounds, int maxDepth)
        {
            Tree = new QuadTree<QuadTreeSceneManagerNodeData<ISpatialQueryable>>(bounds, maxDepth);

            add = new Func<QuadTreeNode<QuadTreeSceneManagerNodeData<ISpatialQueryable>>, TraverseOptions>(Add);
            findAllRay = new Func<QuadTreeNode<QuadTreeSceneManagerNodeData<ISpatialQueryable>>, TraverseOptions>(FindAllRay);
            findAllBoundingBox = new Func<QuadTreeNode<QuadTreeSceneManagerNodeData<ISpatialQueryable>>, TraverseOptions>(FindAllBoundingBox);
            findAllBoundingSphere = new Func<QuadTreeNode<QuadTreeSceneManagerNodeData<ISpatialQueryable>>, TraverseOptions>(FindAllBoundingSphere);
            findAllBoundingFrustum = new Func<QuadTreeNode<QuadTreeSceneManagerNodeData<ISpatialQueryable>>, TraverseOptions>(FindAllBoundingFrustum);
            boundingBoxChanged = new EventHandler<EventArgs>(BoundingBoxChanged);
        }

        private bool needResize;
        private bool addedToNode;
        private ISpatialQueryable item;
        private Func<QuadTreeNode<QuadTreeSceneManagerNodeData<ISpatialQueryable>>, TraverseOptions> add;

        private ICollection<ISpatialQueryable> result;
        private Ray ray;
        private BoundingBox boundingBox;
        private BoundingFrustum boundingFrustum;
        private BoundingSphere boundingSphere;
        private Func<QuadTreeNode<QuadTreeSceneManagerNodeData<ISpatialQueryable>>, TraverseOptions> findAllRay;
        private Func<QuadTreeNode<QuadTreeSceneManagerNodeData<ISpatialQueryable>>, TraverseOptions> findAllBoundingBox;
        private Func<QuadTreeNode<QuadTreeSceneManagerNodeData<ISpatialQueryable>>, TraverseOptions> findAllBoundingSphere;
        private Func<QuadTreeNode<QuadTreeSceneManagerNodeData<ISpatialQueryable>>, TraverseOptions> findAllBoundingFrustum;
        private EventHandler<EventArgs> boundingBoxChanged;
        
        #region ICollection
        public void Add(ISpatialQueryable item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            if (item.SpatialData != null)
                throw new InvalidOperationException(Strings.AlreadyAddedToASceneManager);

            item.SpatialData = new QuadTreeSceneManagerSpatialData<ISpatialQueryable>();

            AddWithResize(Tree.Root, item);

            item.BoundingBoxChanged += boundingBoxChanged;

            Count++;
        }

        private void AddWithResize(QuadTreeNode<QuadTreeSceneManagerNodeData<ISpatialQueryable>> treeNode, ISpatialQueryable item)
        {
            needResize = false;
            Add(treeNode, item);

            if (needResize)
            {
                needResize = false;

                var newBounds = BoundingRectangle.CreateMerged(Tree.Bounds, new BoundingRectangle(item.BoundingBox));
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

        private void Add(QuadTreeNode<QuadTreeSceneManagerNodeData<ISpatialQueryable>> treeNode, ISpatialQueryable item)
        {
            addedToNode = false;

            this.item = item;
            Tree.Traverse(treeNode, add);
            this.item = default(ISpatialQueryable);

            if (!addedToNode && !needResize)
            {
                // Something must be wrong if the node is not added.
                throw new InvalidOperationException();
            }
        }

        private TraverseOptions Add(QuadTreeNode<QuadTreeSceneManagerNodeData<ISpatialQueryable>> node)
        {
            ContainmentType containment = node.Bounds.Contains(new BoundingRectangle(item.BoundingBox));

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

        private void AddToNode(ISpatialQueryable item, QuadTreeNode<QuadTreeSceneManagerNodeData<ISpatialQueryable>> node)
        {
            if (node.Value.List == null)
            {
                node.Value.List = new List<ISpatialQueryable>();
            }

            var data = (QuadTreeSceneManagerSpatialData<ISpatialQueryable>)item.SpatialData;
            data.Tree = Tree;
            data.Node = node;
            node.Value.List.Add(item);
            addedToNode = true;

            // Bubble up the tree to adjust the node bounds accordingly.
            //
            // TODO: Adjust node bounds when objects removed from nodes.
            while (node != null)
            {
                if (node.Value.Initialized)
                {
                    if (item.BoundingBox.Min.Z < node.Value.MinHeight)
                        node.Value.MinHeight = item.BoundingBox.Min.Z;
                    if (item.BoundingBox.Max.Z > node.Value.MaxHeight)
                        node.Value.MaxHeight = item.BoundingBox.Max.Z;
                }
                else
                {
                    node.Value.MinHeight = item.BoundingBox.Min.Z;
                    node.Value.MaxHeight = item.BoundingBox.Max.Z;
                    node.Value.Initialized = true;
                }
                node = node.Parent;
            }
        }

        public bool Remove(ISpatialQueryable item)
        {
            QuadTreeSceneManagerSpatialData<ISpatialQueryable> data = item.SpatialData as QuadTreeSceneManagerSpatialData<ISpatialQueryable>;
            if (data == null || data.Tree != Tree)
                return false;

            var node = data.Node;
            if (!node.Value.List.Remove(item))
            {
                // Something must be wrong if we cannot remove it.
                throw new InvalidOperationException();
            }

            while (node.Value.List == null || node.Value.List.Count <= 0)
            {
                if (node.Parent == null)
                    break;
                node = node.Parent;
            }

            Tree.Collapse(node, n => n.Value.List == null || n.Value.List.Count <= 0);

            item.SpatialData = null;
            item.BoundingBoxChanged -= boundingBoxChanged;
            Count--;

            return true;
        }

        private void BoundingBoxChanged(object sender, EventArgs e)
        {
            ISpatialQueryable item = (ISpatialQueryable)sender;

            if (item == null)
                throw new ArgumentNullException("item");

            var data = (QuadTreeSceneManagerSpatialData<ISpatialQueryable>)item.SpatialData;
            if (data == null)
                throw new InvalidOperationException();

            // Bubble up the tree to find the node that fit the size of the object
            var node = data.Node;
            while (node.Bounds.Contains(new BoundingRectangle(item.BoundingBox)) != ContainmentType.Contains)
            {
                if (node.Parent == null)
                    break;
                node = node.Parent;
            }

            if (node != data.Node)
            {
                if (!data.Node.Value.List.Remove(item))
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

        private void Resize(BoundingRectangle boundingRectangle)
        {
            var items = new ISpatialQueryable[Count];

            CopyTo(items, 0);
            Clear();

            Tree = new QuadTree<QuadTreeSceneManagerNodeData<ISpatialQueryable>>(boundingRectangle, MaxDepth);
            foreach (var item in items)
            {
                Add(item);
            }
        }

        public bool Contains(ISpatialQueryable item)
        {
            foreach (var val in this)
            {
                if (val.Equals(item))
                    return true;
            }
            return false;
        }

        public void CopyTo(ISpatialQueryable[] array, int arrayIndex)
        {
            foreach (var val in this)
            {
                array[arrayIndex++] = val;
            }
        }

        public int Count { get; private set; }
        public bool IsReadOnly { get { return false; } }

        public IEnumerator<ISpatialQueryable> GetEnumerator()
        {
            foreach (var node in Tree)
            {
                if (node.Value.List != null)
                {
                    foreach (var val in node.Value.List)
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
        public void FindAll(ref Ray ray, ICollection<ISpatialQueryable> result)
        {
            this.result = result;
            this.ray = ray;
            Tree.Traverse(findAllRay);
            this.result = null;
        }

        private TraverseOptions FindAllRay(QuadTreeNode<QuadTreeSceneManagerNodeData<ISpatialQueryable>> node)
        {
            if (!node.Value.Initialized)
                return TraverseOptions.Skip;

            float? intersection;
            bool skip = (node != Tree.Root);
            if (skip)
            {
                var nodeBounds = new BoundingBox(new Vector3(node.Bounds.Min, node.Value.MinHeight),
                                                 new Vector3(node.Bounds.Max, node.Value.MaxHeight));

                nodeBounds.Intersects(ref ray, out intersection);
                if (intersection.HasValue)
                {
                    skip = false;
                }
            }

            if (skip)
                return TraverseOptions.Skip;

            if (node.Value.List != null)
            {
                var count = node.Value.List.Count;
                for (int i = 0; i < count; i++)
                {
                    var val = node.Value.List[i];
                    val.BoundingBox.Intersects(ref ray, out intersection);
                    if (intersection.HasValue)
                    {
                        result.Add(val);
                    }
                }
            }
            return TraverseOptions.Continue;
        }

        public void FindAll(ref BoundingBox boundingBox, ICollection<ISpatialQueryable> result)
        {
            this.result = result;
            this.boundingBox = boundingBox;
            Tree.Traverse(findAllBoundingBox);
            this.result = null;
        }

        private TraverseOptions FindAllBoundingBox(QuadTreeNode<QuadTreeSceneManagerNodeData<ISpatialQueryable>> node)
        {
            if (!node.Value.Initialized)
                return TraverseOptions.Skip;

            var nodeContainment = ContainmentType.Intersects;
            var nodeBounds = new BoundingBox(new Vector3(node.Bounds.Min, node.Value.MinHeight),
                                             new Vector3(node.Bounds.Max, node.Value.MaxHeight));

            boundingBox.Contains(ref nodeBounds, out nodeContainment);

            if (nodeContainment == ContainmentType.Disjoint)
                return TraverseOptions.Skip;

            if (nodeContainment == ContainmentType.Contains)
            {
                AddAllDesedents(node);
                return TraverseOptions.Skip;
            }

            if (node.Value.List != null)
            {
                var count = node.Value.List.Count;
                for (int i = 0; i < count; i++)
                {
                    var val = node.Value.List[i];
                    ContainmentType objectContainment;
                    val.BoundingBox.Contains(ref boundingBox, out objectContainment);
                    if (objectContainment != ContainmentType.Disjoint)
                        result.Add(val);
                }
            }
            return TraverseOptions.Continue;
        }

        public void FindAll(ref BoundingSphere boundingSphere, ICollection<ISpatialQueryable> result)
        {
            this.result = result;
            this.boundingSphere = boundingSphere;
            Tree.Traverse(findAllBoundingBox);
            this.result = null;
        }

        private TraverseOptions FindAllBoundingSphere(QuadTreeNode<QuadTreeSceneManagerNodeData<ISpatialQueryable>> node)
        {
            if (!node.Value.Initialized)
                return TraverseOptions.Skip;

            var nodeContainment = ContainmentType.Intersects;
            var boundingBox = new BoundingBox(new Vector3(node.Bounds.Min, node.Value.MinHeight),
                                              new Vector3(node.Bounds.Max, node.Value.MaxHeight));

            boundingSphere.Contains(ref boundingBox, out nodeContainment);

            if (nodeContainment == ContainmentType.Disjoint)
                return TraverseOptions.Skip;

            if (nodeContainment == ContainmentType.Contains)
            {
                AddAllDesedents(node);
                return TraverseOptions.Skip;
            }

            if (node.Value.List != null)
            {
                var count = node.Value.List.Count;
                for (int i = 0; i < count; i++)
                {
                    var val = node.Value.List[i];
                    ContainmentType objectContainment;
                    val.BoundingBox.Contains(ref boundingSphere, out objectContainment);
                    if (objectContainment != ContainmentType.Disjoint)
                        result.Add(val);
                }
            }
            return TraverseOptions.Continue;
        }

        public void FindAll(BoundingFrustum boundingFrustum, ICollection<ISpatialQueryable> result)
        {
            this.result = result;
            this.boundingFrustum = boundingFrustum;
            Tree.Traverse(findAllBoundingFrustum);
            this.result = null;
        }

        private TraverseOptions FindAllBoundingFrustum(QuadTreeNode<QuadTreeSceneManagerNodeData<ISpatialQueryable>> node)
        {
            if (!node.Value.Initialized)
                return TraverseOptions.Skip;

            var nodeContainment = ContainmentType.Intersects;
            var boundingBox = new BoundingBox(new Vector3(node.Bounds.Min, node.Value.MinHeight),
                                              new Vector3(node.Bounds.Max, node.Value.MaxHeight));

            boundingFrustum.Contains(ref boundingBox, out nodeContainment);

            if (nodeContainment == ContainmentType.Disjoint)
                return TraverseOptions.Skip;

            if (nodeContainment == ContainmentType.Contains)
            {
                AddAllDesedents(node);
                return TraverseOptions.Skip;
            }

            if (node.Value.List != null)
            {
                var count = node.Value.List.Count;
                for (int i = 0; i < count; i++)
                {
                    var val = node.Value.List[i];
                    if (boundingFrustum.Contains(val.BoundingBox) != ContainmentType.Disjoint)
                    {
                        result.Add(val);
                    }
                }
            }
            return TraverseOptions.Continue;
        }

        private void AddAllDesedents(QuadTreeNode<QuadTreeSceneManagerNodeData<ISpatialQueryable>> node)
        {
            DesedentsStack.Push(node);
            
            while (DesedentsStack.Count > 0)
            {
                node = DesedentsStack.Pop();
                if (node.Value.List != null)
                {
                    var count = node.Value.List.Count;
                    for (int i = 0; i < count; i++)
                    {
                        result.Add(node.Value.List[i]);
                    }
                }

                var children = node.Children;
                for (int i = 0; i < children.Count; i++)
                    DesedentsStack.Push(children[i]);
            }
        }
        static Stack<QuadTreeNode<QuadTreeSceneManagerNodeData<ISpatialQueryable>>> DesedentsStack = new Stack<QuadTreeNode<QuadTreeSceneManagerNodeData<ISpatialQueryable>>>();
        #endregion
    }
    #endregion

    #region QuadTreeSceneManagerNodeData
    struct QuadTreeSceneManagerNodeData<ISpatialQueryable>
    {
        public List<ISpatialQueryable> List;
        public float MinHeight;
        public float MaxHeight;

        // Flag for whether the bounding box of the node has initialized.
        public bool Initialized;
    }
    #endregion

    #region QuadTreeSceneManagerSpatialData
    class QuadTreeSceneManagerSpatialData<ISpatialQueryable>
    {
        public QuadTree<QuadTreeSceneManagerNodeData<ISpatialQueryable>> Tree;
        public QuadTreeNode<QuadTreeSceneManagerNodeData<ISpatialQueryable>> Node;
    }
    #endregion
}