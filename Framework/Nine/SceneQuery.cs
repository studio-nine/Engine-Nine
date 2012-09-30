namespace Nine
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;

    /// <summary>
    /// Traverses the scene hierarchy and find all the objects that implements T.
    /// </summary>
    class SceneQuery<T> : ISpatialQuery<T> where T : class
    {
        CollectionAdapter adapter;
        List<ISceneManager<ISpatialQueryable>> sceneManagers;
        IList<object> topLevelObjects;

        public SceneQuery(List<ISceneManager<ISpatialQueryable>> sceneManagers, IList<object> topLevelObjects, Predicate<T> condition)
        {
            this.sceneManagers = sceneManagers;
            this.topLevelObjects = topLevelObjects;
            this.adapter = new CollectionAdapter(condition);
        }

        public void FindAll(ref BoundingSphere boundingSphere, ICollection<T> result)
        {
            adapter.Result = result;
            adapter.IncludeTopLevelNonSpatialQueryableDesendants(topLevelObjects);

            var count = sceneManagers.Count;
            for (int i = 0; i < count; ++i)
                sceneManagers[i].FindAll(ref boundingSphere, adapter);

            adapter.Result = null;
        }

        public void FindAll(ref Ray ray, ICollection<T> result)
        {
            adapter.Result = result;
            adapter.IncludeTopLevelNonSpatialQueryableDesendants(topLevelObjects);

            var count = sceneManagers.Count;
            for (int i = 0; i < count; ++i)
                sceneManagers[i].FindAll(ref ray, adapter);

            adapter.Result = null;
        }

        public void FindAll(ref BoundingBox boundingBox, ICollection<T> result)
        {
            adapter.Result = result;
            adapter.IncludeTopLevelNonSpatialQueryableDesendants(topLevelObjects);

            var count = sceneManagers.Count;
            for (int i = 0; i < count; ++i)
                sceneManagers[i].FindAll(ref boundingBox, adapter);

            adapter.Result = null;
        }

        public void FindAll(BoundingFrustum boundingFrustum, ICollection<T> result)
        {
            adapter.Result = result;
            adapter.IncludeTopLevelNonSpatialQueryableDesendants(topLevelObjects);

            var count = sceneManagers.Count;
            for (int i = 0; i < count; ++i)
                sceneManagers[i].FindAll(boundingFrustum, adapter);

            adapter.Result = null;
        }

        class CollectionAdapter : SpatialQueryCollectionAdapter<ISpatialQueryable>
        {
            public ICollection<T> Result;
            private ISpatialQueryable currentItem;
            private Func<object, TraverseOptions> traverser;
            private Predicate<T> condition;

            public CollectionAdapter(Predicate<T> condition)
            {
                this.condition = condition;
                this.traverser = new Func<object, TraverseOptions>(FindNonSpatialQueryableDescendantTraverser);
            }

            public override void Add(ISpatialQueryable item)
            {
                currentItem = item;
                ContainerTraverser.Traverse(item, traverser);
                currentItem = null;
            }

            public void IncludeTopLevelNonSpatialQueryableDesendants(IList<object> topLevelObjects)
            {
                var count = topLevelObjects.Count;
                for (int i = 0; i < count; ++i)
                {
                    ContainerTraverser.Traverse(topLevelObjects[i], traverser);
                }
            }

            private TraverseOptions FindNonSpatialQueryableDescendantTraverser(object item)
            {
                T t;

                // Skip ISpatialQueryables since they are explicitly added to the scene manager,
                // but only if it doesn't equal to the current T.
                var queryable = item as ISpatialQueryable;
                if (queryable != null)
                {
                    if (queryable == currentItem)
                    {
                        t = queryable as T;
                        if (t != null && (condition == null || condition(t)))
                            Result.Add(t);
                        return TraverseOptions.Continue;
                    }
                    return TraverseOptions.Skip;
                }

                t = item as T;
                if (t != null && (condition == null || condition(t)))
                    Result.Add(t);
                return TraverseOptions.Continue;
            }
        }
    }
}