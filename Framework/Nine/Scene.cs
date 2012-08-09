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
        /// Gets the spatial query that can find all the top level objects.
        /// </summary>
        private Dictionary<Type, object> typedQueries;

        /// <summary>
        /// Keeps track of all the objects, only used under debug mode.
        /// </summary>
        private List<object> objectTracker;
        #endregion

        #region Events
        /// <summary>
        /// Occurs when any of the desecendant node is removed from the scene either
        /// directly or through the removal from a subtree.
        /// </summary>
        public event Action<object> RemovedFromScene;

        /// <summary>
        /// Occurs when any of the desecendant node is added to the scene either
        /// directly or through the addition of a subtree.
        /// </summary>
        public event Action<object> AddedToScene;

        /// <summary>
        /// Occurs when the scene is starting to update.
        /// </summary>
        public event Action<TimeSpan> Updating;

        /// <summary>
        /// Occurs when the scene has finished updating.
        /// </summary>
        public event Action<TimeSpan> Updated;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="Scene"/> class.
        /// </summary>
        public Scene() : this(null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Scene"/> class.
        /// </summary>
        public Scene(ISceneManager<ISpatialQueryable> defaultSceneManager)
        {
            this.defaultSceneManager = defaultSceneManager ?? new OctreeSceneManager();
            this.sceneManagers = new List<ISceneManager<ISpatialQueryable>>();
            this.sceneManagers.Add(this.defaultSceneManager);
        }

        /// <summary>
        /// Updates the internal state of the object based on game time.
        /// </summary>
        public override void Update(TimeSpan elapsedTime)
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

            try
            {
                if (sceneManager != null && !sceneManagers.Contains(sceneManager))
                    sceneManagers.Add(sceneManager);

                currentSceneManagerForAddition = sceneManager;
                Add(item);
            }
            finally
            {
                currentSceneManagerForAddition = null;
            }
        }

        /// <summary>
        /// Called when a child object is added directly to this drawing group.
        /// </summary>
        protected override void OnAdded(object child)
        {
            ContainerTraverser.Traverse<object>(child, InternalAdd);
        }

        private TraverseOptions InternalAdd(object desecendant)
        {
            if (desecendant == null)
                throw new ArgumentNullException("desecendant");
            
            var queryable = desecendant as ISpatialQueryable;
            if (queryable != null)
            {
                // Choose the right scene manager for the target object
                if (currentSceneManagerForAddition != null)
                    currentSceneManagerForAddition.Add(queryable);
                else
                    defaultSceneManager.Add(queryable);
            }

            var collectionChanged = desecendant as INotifyCollectionChanged<object>;
            if (collectionChanged != null)
            {
                collectionChanged.Added += OnDesecendantAdded;
                collectionChanged.Removed += OnDesecendantRemoved;
            }

            TrackObject(desecendant);
            OnAddedToScene(desecendant);

            var addedToScene = AddedToScene;
            if (addedToScene != null)
                addedToScene(desecendant);

            return TraverseOptions.Continue;
        }

        /// <summary>
        /// Called when any of the desecendant node is added to the scene either
        /// directly or through the addition of a subtree.
        /// </summary>
        protected virtual void OnAddedToScene(object desecendant)
        {

        }

        /// <summary>
        /// Called when a child object is removed directly from this drawing group.
        /// </summary>
        /// <param name="child"></param>
        protected override void OnRemoved(object child)
        {
            ContainerTraverser.Traverse<object>(child, InternalRemove);
        }

        private TraverseOptions InternalRemove(object desecendant)
        {
            if (desecendant == null)
                throw new ArgumentNullException("desecendant");

            var queryable = desecendant as ISpatialQueryable;
            if (queryable != null)
                defaultSceneManager.Remove(queryable);

            var collectionChanged = desecendant as INotifyCollectionChanged<object>;
            if (collectionChanged != null)
            {
                collectionChanged.Added -= OnDesecendantAdded;
                collectionChanged.Removed -= OnDesecendantRemoved;
            }

            UnTrackObject(desecendant);
            OnRemovedFromScene(desecendant);

            var removedFromScene = RemovedFromScene;
            if (removedFromScene != null)
                removedFromScene(desecendant);

            return TraverseOptions.Continue;
        }


        /// <summary>
        /// Called when any of the desecendant node is added to the scene either
        /// directly or through the addition of a subtree.
        /// </summary>
        protected virtual void OnRemovedFromScene(object desecendant)
        {

        }
        
        /// <summary>
        /// Ensures OnAddedToScene is called when a desecendant is added to a subtree.
        /// </summary>
        private void OnDesecendantAdded(object value)
        {
            InternalAdd(value);
        }

        /// <summary>
        /// Ensures OnRemovedFromScene is called when a desecendant is removed from a subtree.
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
        public ISpatialQuery<T> CreateSpatialQuery<T>() where T : class
        {
            object result = null;
            if (typedQueries != null && typedQueries.TryGetValue(typeof(T), out result))
                return (ISpatialQuery<T>)result;
            if (typedQueries == null)
                typedQueries = new Dictionary<Type, object>();
            var query = new SceneQuery<T>(sceneManagers, Children);
            typedQueries[typeof(T)] = query;
            return query;
        }

        /// <summary>
        /// Finds all the scene objects and the original volumn for intersection test that is contained by or
        /// intersects the specified bounding sphere.
        /// </summary>
        public void FindAll<T>(ref BoundingSphere boundingSphere, ICollection<T> result) where T : class
        {
            CreateSpatialQuery<T>().FindAll(ref boundingSphere, result);
        }

        /// <summary>
        /// Finds all the scene objects and the original volumn for intersection test that intersects the specified ray.
        /// </summary>
        public void FindAll<T>(ref Ray ray, ICollection<T> result) where T : class
        {
            CreateSpatialQuery<T>().FindAll(ref ray, result);
        }

        /// <summary>
        /// Finds all the scene objects and the original volumn for intersection test that is contained by or
        /// intersects the specified bounding box.
        /// </summary>
        public void FindAll<T>(ref BoundingBox boundingBox, ICollection<T> result) where T : class
        {
            CreateSpatialQuery<T>().FindAll(ref boundingBox, result);
        }

        /// <summary>
        /// Finds all the scene objects and the original volumn for intersection test that is contained by or
        /// intersects the specified bounding frustum.
        /// </summary>
        public void FindAll<T>(BoundingFrustum boundingFrustum, ICollection<T> result) where T : class
        {
            CreateSpatialQuery<T>().FindAll(boundingFrustum, result);
        }
        #endregion
    }
}