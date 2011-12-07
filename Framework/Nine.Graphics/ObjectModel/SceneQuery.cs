#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.ParticleEffects;
using Nine.Graphics.ScreenEffects;
#if WINDOWS || XBOX
using Nine.Graphics.Effects.Deferred;
#endif
using Nine.Graphics.Effects;
using EffectMaterial = Nine.Graphics.Effects.EffectMaterial;
#endregion

namespace Nine.Graphics.ObjectModel
{
    #region FlattenedCollectionAdapter<T>
    class FlattenedCollectionAdapter<T> : SpatialQueryCollectionAdapter<ISpatialQueryable> where T : class
    {
        public static FlattenedCollectionAdapter<T> Instance { get { return instance ?? (instance = new FlattenedCollectionAdapter<T>()); } }
        static FlattenedCollectionAdapter<T> instance;

        public ICollection<T> Result;
        private ISpatialQueryable CurrentItem;
        private Func<object, TraverseOptions> Traverser;

        private FlattenedCollectionAdapter()
        {
            Traverser = new Func<object, TraverseOptions>(FindNonSpatialQueryableDescendantTraverser);
        }

        public override void Add(ISpatialQueryable item)
        {
            CurrentItem = item;
            ContainerTraverser.Traverse(item, Traverser);
            CurrentItem = null;
        }

        public void IncludeTopLevelNonSpatialQueryableDesendants(List<object> topLevelObjects)
        {
            for (int i = 0; i < topLevelObjects.Count; i++)
            {
                ContainerTraverser.Traverse(topLevelObjects[i], Traverser);
            }
        }

        private TraverseOptions FindNonSpatialQueryableDescendantTraverser(object item)
        {
            // Skip ISpatialQueryables since they are explicitly added to the scene manager,
            // but only if it doesn't equal to the current T.
            var queryable = item as ISpatialQueryable;
            if (queryable != null)
            {
                if (queryable == CurrentItem)
                {
                    if (queryable is T)
                        Result.Add(queryable as T);
                    return TraverseOptions.Continue;
                }
                return TraverseOptions.Skip;
            }

            if (item is T)
                Result.Add(item as T);
            return TraverseOptions.Continue;
        }
    }
    #endregion

    #region FlattenedQuery
    class FlattenedQuery : ISpatialQuery<object>
    {
        FlattenedCollectionAdapter<object> adapter;
        ISpatialQuery<ISpatialQueryable> sceneManager;
        List<object> topLevelObjects;

        public FlattenedQuery(ISpatialQuery<ISpatialQueryable> sceneManager, List<object> topLevelObjects)
        {
            this.sceneManager = sceneManager;
            this.topLevelObjects = topLevelObjects;
            this.adapter = FlattenedCollectionAdapter<object>.Instance;
        }

        public void FindAll(ref BoundingSphere boundingSphere, ICollection<object> result)
        {
            adapter.Result = result;
            adapter.IncludeTopLevelNonSpatialQueryableDesendants(topLevelObjects);
            sceneManager.FindAll(ref boundingSphere, adapter);
            adapter.Result = null;
        }

        public void FindAll(ref Ray ray, ICollection<object> result)
        {
            adapter.Result = result;
            adapter.IncludeTopLevelNonSpatialQueryableDesendants(topLevelObjects);
            sceneManager.FindAll(ref ray, adapter);
            adapter.Result = null;
        }

        public void FindAll(ref BoundingBox boundingBox, ICollection<object> result)
        {
            adapter.Result = result;
            adapter.IncludeTopLevelNonSpatialQueryableDesendants(topLevelObjects);
            sceneManager.FindAll(ref boundingBox, adapter);
            adapter.Result = null;
        }

        public void FindAll(ref BoundingFrustum boundingFrustum, ICollection<object> result)
        {
            adapter.Result = result;
            adapter.IncludeTopLevelNonSpatialQueryableDesendants(topLevelObjects);
            sceneManager.FindAll(ref boundingFrustum, adapter);
            adapter.Result = null;
        }
    }
    #endregion

    #region DetailedQuery
    class DetailedQuery : ISpatialQuery<FindResult>
    {
        CollectionAdapter adapter;
        ISceneManager<ISpatialQueryable> sceneManager;

        public DetailedQuery(ISceneManager<ISpatialQueryable> sceneManager)
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

        public void FindAll(ref BoundingFrustum boundingFrustum, ICollection<FindResult> result)
        {
            adapter.Result = result;
            sceneManager.FindAll(ref boundingFrustum, adapter);
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
            var transformable = item as Transformable;
            if (transformable != null && transformable.Name == TargetName)
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
            var transformable = item as Transformable;
            if (transformable != null && transformable.Name == TargetName)
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