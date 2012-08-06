namespace Nine
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;

    #region SceneQuery
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
    #endregion

    #region DetailedQuery
    class DetailedQuery : ISpatialQuery<FindResult>
    {
        CollectionAdapter adapter;
        ISpatialQuery<ISpatialQueryable> sceneManager;

        public DetailedQuery(ISpatialQuery<ISpatialQueryable> sceneManager)
        {
            this.sceneManager = sceneManager;
            this.adapter = new CollectionAdapter();
        }

        public void FindAll(ref BoundingSphere boundingSphere, ICollection<FindResult> result)
        {
            adapter.Result = result;
            sceneManager.FindAll(ref boundingSphere, adapter);
            adapter.Result = null;
        }

        public void FindAll(ref Ray ray, ICollection<FindResult> result)
        {
            adapter.Result = result;
            sceneManager.FindAll(ref ray, adapter);
            adapter.Result = null;
        }

        public void FindAll(ref BoundingBox boundingBox, ICollection<FindResult> result)
        {
            adapter.Result = result;
            sceneManager.FindAll(ref boundingBox, adapter);
            adapter.Result = null;
        }

        public void FindAll(BoundingFrustum boundingFrustum, ICollection<FindResult> result)
        {
            adapter.Result = result;
            sceneManager.FindAll(boundingFrustum, adapter);
            adapter.Result = null;
        }

        class CollectionAdapter : SpatialQueryCollectionAdapter<ISpatialQueryable>
        {
            public ICollection<FindResult> Result;

            public override void Add(ISpatialQueryable item)
            {
                var findResult = new FindResult();
                findResult.OriginalTarget = item;
                findResult.Target = ContainerTraverser.FindRootContainer(item);
                findResult.Distance = null;
                findResult.ContainmentType = ContainmentType.Disjoint;
                Result.Add(findResult);
            }
        }
    }
    #endregion

    #region SceneQueryHelper
    static class SceneQueryHelper<T> where T : class
    {
        public static ICollection<T> Result;

        private static T ResultObject;
        private static string TargetName;
        private static Func<T, TraverseOptions> NameTraverser = new Func<T, TraverseOptions>(FindNameTraverser);
        private static Func<T, TraverseOptions> AllNamesTraverser = new Func<T, TraverseOptions>(FindAllNamesTraverser);

        public static T FindName(object targetObject, string name)
        {
            TargetName = name;
            ContainerTraverser.Traverse(targetObject, NameTraverser);
            T result = ResultObject;
            ResultObject = null;
            return result;
        }
        
        private static TraverseOptions FindNameTraverser(T item)
        {
            if (item.ToString() == TargetName || TargetName == null)
            {
                ResultObject = item;
                return TraverseOptions.Stop;
            }
            return TraverseOptions.Continue;
        }

        public static void FindAllNames(object targetObject, string name, ICollection<T> result)
        {
            Result = result;
            TargetName = name;
            ContainerTraverser.Traverse(targetObject, AllNamesTraverser);
            Result = null;
        }

        private static TraverseOptions FindAllNamesTraverser(T item)
        {
            if (item.ToString() == TargetName)
            {
                Result.Add(item);
            }
            return TraverseOptions.Continue;
        }

        private static void Output(object value)
        {
            var t = value as T;
            if (t != null)
            {
                if (Result != null)
                    Result.Add(t);
            }
        }
    }
    #endregion
}