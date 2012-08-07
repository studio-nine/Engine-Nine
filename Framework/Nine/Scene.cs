namespace Nine
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    
    /// <summary>
    /// Defines a graphical scene that manages a set of objects, cameras and lights.
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
        private ISpatialQuery<FindResult> detailedQuery;

        /// <summary>
        /// Gets the spatial query that can find all the flattened objects.
        /// </summary>
        private ISpatialQuery<object> flattenedQuery;

        /// <summary>
        /// Used for ray casting.
        /// </summary>
        private List<FindResult> rayCastResult = new List<FindResult>();
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
            this.flattenedQuery = CreateSpatialQuery<object>();
            this.detailedQuery = new DetailedQuery(this.defaultSceneManager);
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
        #endregion

        #region Find
        /// <summary>
        /// Create a spatial query of the specified type from this scene.
        /// </summary>
        public ISpatialQuery<T> CreateSpatialQuery<T>() where T : class
        {
            return new SceneQuery<T>(sceneManagers, Children);
        }

        /// <summary>
        /// Finds the nearest object that intersects the specified ray.
        /// </summary>
        public FindResult Find(Ray ray)
        {
            FindResult result;
            Find(ref ray, out result);
            return result;
        }

        /// <summary>
        /// Finds the nearest object that intersects the specified ray.
        /// </summary>
        public void Find(ref Ray ray, out FindResult result)
        {
            result = new FindResult();
            FindAll(ref ray, rayCastResult);

            float? currentDistance = null;
            for (int i = 0; i < rayCastResult.Count; i++)
            {
                var pickable = ContainerTraverser.FindParentContainer<IPickable>(rayCastResult[i].OriginalTarget);
                if (pickable != null)
                {
                    currentDistance = pickable.Intersects(ray);
                    if (currentDistance.HasValue && (!result.Distance.HasValue || result.Distance.Value > currentDistance.Value))
                    {
                        result.Distance = currentDistance;
                        result.Target = rayCastResult[i].Target;
                        result.OriginalTarget = rayCastResult[i].OriginalTarget;
                    }
                }
            }
            rayCastResult.Clear();
        }

        /// <summary>
        /// Finds all the scene objects and the original volumn for intersection test that is contained by or
        /// intersects the specified bounding sphere.
        /// </summary>
        /// <remarks>
        /// When an object tree is added to the scene using Scene.Add, 
        /// this find method will set the FindResult.OriginalTarget property to the original 
        /// <see cref="ISpatialQueryable"/> for the intersection test against the input bounding volumn.
        /// It will set the FindResult.Target property to the containing object that is added
        /// to the scene.
        /// </remarks>
        public void FindAll(ref BoundingSphere boundingSphere, ICollection<FindResult> result)
        {
            detailedQuery.FindAll(ref boundingSphere, result);
        }

        /// <summary>
        /// Finds all the scene objects and the original volumn for intersection test that intersects the specified ray.
        /// </summary>
        public void FindAll(ref Ray ray, ICollection<FindResult> result)
        {
            detailedQuery.FindAll(ref ray, result);
        }

        /// <summary>
        /// Finds all the scene objects and the original volumn for intersection test that is contained by or
        /// intersects the specified bounding box.
        /// </summary>
        public void FindAll(ref BoundingBox boundingBox, ICollection<FindResult> result)
        {
            detailedQuery.FindAll(ref boundingBox, result);
        }

        /// <summary>
        /// Finds all the scene objects and the original volumn for intersection test that is contained by or
        /// intersects the specified bounding frustum.
        /// </summary>
        public void FindAll(BoundingFrustum boundingFrustum, ICollection<FindResult> result)
        {
            detailedQuery.FindAll(boundingFrustum, result);
        }
        #endregion
    }
}