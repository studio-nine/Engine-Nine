namespace Nine
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    
    /// <summary>
    /// Defines a scene that manages an object hierarchy.
    /// </summary>
    public class Scene : Group, ISpatialQuery
    {
        #region Fields
        /// <summary>
        /// This is the default scene manager that manages all spatial queryables.
        /// </summary>
        private ISceneManager<ISpatialQueryable> defaultSceneManager;

        /// <summary>
        /// Stores which scene manager to use when adding an object to the scene.
        /// </summary>
        private ISceneManager<ISpatialQueryable> currentSceneManagerForAddition;

        /// <summary>
        /// This list holds all the scene managers used inside this scene.
        /// Each object in the scene can be managed by a different scene manager.
        /// </summary>
        private List<ISceneManager<ISpatialQueryable>> sceneManagers;

        /// <summary>
        /// Keeps track of all the objects, only used under debug mode.
        /// </summary>
        private List<object> objectTracker;

        /// <summary>
        /// When objects are added or removed, they are added to this temporary queue.
        /// </summary>
        private List<INotifyCollectionChanged<object>> workingExpandables = new List<INotifyCollectionChanged<object>>();
        private int workingExpandablesDepth = 0;
        #endregion

        #region Events
        /// <summary>
        /// Occurs when any of the descendant node is removed from the scene either
        /// directly or through the removal from a subtree.
        /// </summary>
        public event Action<object> RemovedFromScene;

        /// <summary>
        /// Occurs when any of the descendant node is added to the scene either
        /// directly or through the addition of a subtree.
        /// </summary>
        public event Action<object> AddedToScene;

        /// <summary>
        /// Occurs when the scene is starting to update.
        /// </summary>
        public event Action<float> Updating;

        /// <summary>
        /// Occurs when the scene has finished updating.
        /// </summary>
        public event Action<float> Updated;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="Scene"/> class.
        /// </summary>
        public Scene() : this(null, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Scene"/> class.
        /// </summary>
        public Scene(IServiceProvider serviceProvider) : this(null, serviceProvider)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Scene"/> class.
        /// </summary>
        public Scene(ISceneManager<ISpatialQueryable> defaultSceneManager, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this.defaultSceneManager = defaultSceneManager ?? new OctreeSceneManager();
            this.sceneManagers = new List<ISceneManager<ISpatialQueryable>>();
            this.sceneManagers.Add(this.defaultSceneManager);
        }

        /// <summary>
        /// Updates the internal state of the object based on game time.
        /// </summary>
        public override void Update(float elapsedTime)
        {
            var updating = Updating;
            if (updating != null)
                updating(elapsedTime);

            base.Update(elapsedTime);

            var updated = Updated;
            if (updated != null)
                updated(elapsedTime);         
        }
        #endregion

        #region Collection
        /// <summary>
        /// Adds a new item to the scene using the specified scene manager.
        /// All child objects are also added using that scene manager unless stated
        /// explicitly with a SceneManger attached property.
        /// The default scene manager is used if no scene manager is specified.
        /// </summary>
        public void Add(object item, ISceneManager<ISpatialQueryable> sceneManager)
        {
            if (currentSceneManagerForAddition != null)
                throw new InvalidOperationException();

            if (sceneManager != null && !sceneManagers.Contains(sceneManager))
                sceneManagers.Add(sceneManager);

            currentSceneManagerForAddition = sceneManager;
            Add(item);

            currentSceneManagerForAddition = null;
        }

        /// <summary>
        /// Called when a child object is added directly to this drawing group.
        /// </summary>
        protected override void OnAdded(object child)
        {
            ++workingExpandablesDepth;
            ContainerTraverser.Traverse<object>(child, InternalAdd);
            if (--workingExpandablesDepth == 0)
            {
                var count = workingExpandables.Count;
                for (var i = 0; i < count; ++i)
                {
                    var collectionChanged = workingExpandables[i];
                    if (collectionChanged != null)
                    {
                        collectionChanged.Added += OnDesecendantAdded;
                        collectionChanged.Removed += OnDesecendantRemoved;
                    }      
                }
                workingExpandables.Clear();
            }
        }

        private TraverseOptions InternalAdd(object descendant)
        {
            if (descendant == null)
                throw new ArgumentNullException("descendant");

            var collectionChanged = descendant as INotifyCollectionChanged<object>;
            if (collectionChanged != null)
                workingExpandables.Add(collectionChanged);

            var queryable = descendant as ISpatialQueryable;
            if (queryable != null)
            {
                // Choose the right scene manager for the target object
                if (currentSceneManagerForAddition != null)
                    currentSceneManagerForAddition.Add(queryable);
                else
                    defaultSceneManager.Add(queryable);
            }

            TrackObject(descendant);
            OnAddedToScene(descendant);

            var addedToScene = AddedToScene;
            if (addedToScene != null)
                addedToScene(descendant);

            return TraverseOptions.Continue;
        }

        /// <summary>
        /// Called when any of the descendant node is added to the scene either
        /// directly or through the addition of a sub tree.
        /// </summary>
        protected virtual void OnAddedToScene(object descendant)
        {

        }

        /// <summary>
        /// Called when a child object is removed directly from this drawing group.
        /// </summary>
        /// <param name="child"></param>
        protected override void OnRemoved(object child)
        {
            ++workingExpandablesDepth;
            ContainerTraverser.Traverse<object>(child, InternalRemove);
            if (--workingExpandablesDepth == 0)
            {
                var count = workingExpandables.Count;
                for (var i = 0; i < count; ++i)
                {
                    var collectionChanged = workingExpandables[i];
                    if (collectionChanged != null)
                    {
                        collectionChanged.Added -= OnDesecendantAdded;
                        collectionChanged.Removed -= OnDesecendantRemoved;
                    }
                }
                workingExpandables.Clear();
            }
        }

        private TraverseOptions InternalRemove(object descendant)
        {
            if (descendant == null)
                throw new ArgumentNullException("descendant");

            var queryable = descendant as ISpatialQueryable;
            if (queryable != null)
                defaultSceneManager.Remove(queryable);

            var collectionChanged = descendant as INotifyCollectionChanged<object>;
            if (collectionChanged != null)
                workingExpandables.Add(collectionChanged);

            UnTrackObject(descendant);
            OnRemovedFromScene(descendant);

            var removedFromScene = RemovedFromScene;
            if (removedFromScene != null)
                removedFromScene(descendant);

            return TraverseOptions.Continue;
        }


        /// <summary>
        /// Called when any of the descendant node is added to the scene either
        /// directly or through the addition of a sub tree.
        /// </summary>
        protected virtual void OnRemovedFromScene(object descendant)
        {

        }
        
        /// <summary>
        /// Ensures OnAddedToScene is called when a descendant is added to a sub tree.
        /// </summary>
        private void OnDesecendantAdded(object value)
        {
            InternalAdd(value);
        }

        /// <summary>
        /// Ensures OnRemovedFromScene is called when a descendant is removed from a sub tree.
        /// </summary>
        private void OnDesecendantRemoved(object value)
        {
            InternalRemove(value);
        }

        [Conditional("DEBUG")]
        private void TrackObject(object value)
        {
            if (objectTracker == null)
                objectTracker = new List<object>();
            if (objectTracker.Contains(value))
                throw new InvalidOperationException();
            objectTracker.Add(value);
        }

        [Conditional("DEBUG")]
        private void UnTrackObject(object value)
        {
            objectTracker.Remove(value);
        }
        #endregion

        #region Find
        /// <summary>
        /// Create a spatial query of the specified type from this scene.
        /// </summary>
        public ISpatialQuery<T> CreateSpatialQuery<T>(Predicate<T> condition) where T : class
        {
            return new SceneQuery<T>(sceneManagers, Children, condition);
        }

        /// <summary>
        /// Finds all the scene objects and the original volume for intersection test that is contained by or
        /// intersects the specified bounding sphere.
        /// </summary>
        public void FindAll<T>(ref BoundingSphere boundingSphere, Predicate<T> condition, ICollection<T> result) where T : class
        {
            CreateSpatialQuery<T>(condition).FindAll(ref boundingSphere, result);
        }

        /// <summary>
        /// Finds all the scene objects and the original volume for intersection test that intersects the specified ray.
        /// </summary>
        public void FindAll<T>(ref Ray ray, Predicate<T> condition, ICollection<T> result) where T : class
        {
            CreateSpatialQuery<T>(condition).FindAll(ref ray, result);
        }

        /// <summary>
        /// Finds all the scene objects and the original volume for intersection test that is contained by or
        /// intersects the specified bounding box.
        /// </summary>
        public void FindAll<T>(ref BoundingBox boundingBox, Predicate<T> condition, ICollection<T> result) where T : class
        {
            CreateSpatialQuery<T>(condition).FindAll(ref boundingBox, result);
        }

        /// <summary>
        /// Finds all the scene objects and the original volume for intersection test that is contained by or
        /// intersects the specified bounding frustum.
        /// </summary>
        public void FindAll<T>(BoundingFrustum boundingFrustum, Predicate<T> condition, ICollection<T> result) where T : class
        {
            CreateSpatialQuery<T>(condition).FindAll(boundingFrustum, result);
        }
        #endregion
    }
}