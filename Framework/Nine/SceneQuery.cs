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

        public SceneQuery(List<ISceneManager<ISpatialQueryable>> sceneManagers, IList<object> topLevelObjects)
        {
            this.sceneManagers = sceneManagers;
            this.topLevelObjects = topLevelObjects;
            this.adapter = CollectionAdapter.Instance;
        }

        public void FindAll(ref BoundingSphere boundingSphere, ICollection<T> result)
        {
            try
            {
                adapter.Result = result;
                adapter.IncludeTopLevelNonSpatialQueryableDesendants(topLevelObjects);

                var count = sceneManagers.Count;
                for (int i = 0; i < count; i++)
                    sceneManagers[i].FindAll(ref boundingSphere, adapter);
            }
            finally
            {
                adapter.Result = null;
            }
        }

        public void FindAll(ref Ray ray, ICollection<T> result)
        {
            try
            {
                adapter.Result = result;
                adapter.IncludeTopLevelNonSpatialQueryableDesendants(topLevelObjects);

                var count = sceneManagers.Count;
                for (int i = 0; i < count; i++)
                    sceneManagers[i].FindAll(ref ray, adapter);
            }
            finally
            {
                adapter.Result = null;
            }
        }

        public void FindAll(ref BoundingBox boundingBox, ICollection<T> result)
        {
            try
            {
                adapter.Result = result;
                adapter.IncludeTopLevelNonSpatialQueryableDesendants(topLevelObjects);

                var count = sceneManagers.Count;
                for (int i = 0; i < count; i++)
                    sceneManagers[i].FindAll(ref boundingBox, adapter);
            }
            finally
            {
                adapter.Result = null;
            }
        }

        public void FindAll(BoundingFrustum boundingFrustum, ICollection<T> result)
        {
            try
            {
                adapter.Result = result;
                adapter.IncludeTopLevelNonSpatialQueryableDesendants(topLevelObjects);

                var count = sceneManagers.Count;
                for (int i = 0; i < count; i++)
                    sceneManagers[i].FindAll(boundingFrustum, adapter);
            }
            finally
            {
                adapter.Result = null;
            }
        }

        class CollectionAdapter : SpatialQueryCollectionAdapter<ISpatialQueryable>
        {
            public static CollectionAdapter Instance 
            {
                get { return instance ?? (instance = new CollectionAdapter()); } 
            }
            static CollectionAdapter instance;

            public ICollection<T> Result;
            private ISpatialQueryable CurrentItem;
            private Func<object, TraverseOptions> Traverser;

            private CollectionAdapter()
            {
                Traverser = new Func<object, TraverseOptions>(FindNonSpatialQueryableDescendantTraverser);
            }

            public override void Add(ISpatialQueryable item)
            {
                CurrentItem = item;
                ContainerTraverser.Traverse(item, Traverser);
                CurrentItem = null;
            }

            public void IncludeTopLevelNonSpatialQueryableDesendants(IList<object> topLevelObjects)
            {
                var count = topLevelObjects.Count;
                for (int i = 0; i < count; i++)
                {
                    ContainerTraverser.Traverse(topLevelObjects[i], Traverser);
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
                    if (queryable == CurrentItem)
                    {
                        t = queryable as T;
                        if (t != null)
                            Result.Add(t);
                        return TraverseOptions.Continue;
                    }
                    return TraverseOptions.Skip;
                }
                
                t = item as T;
                if (t != null)
                    Result.Add(t);
                return TraverseOptions.Continue;
            }
        }
    }
}