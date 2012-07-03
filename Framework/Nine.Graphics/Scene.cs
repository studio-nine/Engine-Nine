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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.ParticleEffects;
using Nine.Graphics.PostEffects;
#if WINDOWS || XBOX
using Nine.Graphics.Materials.Deferred;
#endif
using Nine.Graphics.Materials;
#endregion

namespace Nine.Graphics
{
    using Nine.Graphics.ObjectModel;
    using Nine.Graphics.Drawing;

    /// <summary>
    /// Defines a graphical scene that manages a set of objects, cameras and lights.
    /// </summary>
    [ContentSerializable]
    public class Scene : DrawingGroup, ISpatialQuery<FindResult>, IDrawable
    {
        #region Properties
        /// <summary>
        /// Gets the underlying graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets the graphics settings
        /// </summary>
        public Settings Settings { get; private set; }

        /// <summary>
        /// Gets or sets the active camera.
        /// </summary>
        [ContentSerializerIgnore]
        public ICamera Camera
        {
            get { return camera ?? (camera = new TopDownEditorCamera(GraphicsDevice)); }
            set { camera = value; }
        }
        private ICamera camera;

        /// <summary>
        /// Gets the statistics of this renderer.
        /// </summary>
        public Statistics Statistics { get; private set; }

        /// <summary>
        /// Gets or sets the graphics context.
        /// </summary>
        public DrawingContext Context { get; protected set; }
        #endregion

        #region Fields
        /// <summary>
        /// This is the default scene manager that manages all spatial queryables.
        /// </summary>
        private ISceneManager defaultSceneManager;

        /// <summary>
        /// Stores which scene manager to use when adding an object to the scene.
        /// </summary>
        private ISceneManager currentSceneManagerForAddition;

        /// <summary>
        /// This list holds all the scene managers used inside this scene.
        /// Each object in the scene can be managed by a different scene manager.
        /// </summary>
        private List<ISceneManager> sceneManagers;

        /// <summary>
        /// Gets the spatial query that can find all the top level objects.
        /// </summary>
        private ISpatialQuery<FindResult> detailedQuery;

        /// <summary>
        /// Gets the spatial query that can find all the flattened objects.
        /// </summary>
        private ISpatialQuery<object> flattenedQuery;

        /// <summary>
        /// Gets the spatial query that finds all the drawable objects.
        /// </summary>
        private ISpatialQuery<IDrawableObject> drawables;

        /// <summary>
        /// This list contains all the drawable objects in the current view frustum.
        /// </summary>
        private FastList<IDrawableObject> drawablesInViewFrustum = new FastList<IDrawableObject>();
        
        /// <summary>
        /// This list contains all the flattened objects in the current view frustum.
        /// </summary>
        private FastList<object> flattenedObjectsInViewFrustum = new FastList<object>();

        /// <summary>
        /// This list contains all the opaque objects in the current view frustum.
        /// </summary>
        private FastList<IDrawableObject> opaqueDrawablesInViewFrustum = new FastList<IDrawableObject>();

        /// <summary>
        /// This list contains all the transparent objects in the current view frustum.
        /// </summary>
        private FastList<IDrawableObject> transparentDrawablesInViewFrustum = new FastList<IDrawableObject>();
        
        private List<Light> lightsInViewFrustum = new List<Light>();
        private List<Light> appliedMultiPassLights = new List<Light>();
        private List<Light> unAppliedMultiPassLights = new List<Light>();
        private List<FindResult> rayCastResult = new List<FindResult>();
        private List<IUpdateable> updateableObjects = new List<IUpdateable>();
        private List<IDrawableObject> drawableObjects = new List<IDrawableObject>();

        private HashSet<ISpatialQueryable> shadowCastersInLightFrustum = new HashSet<ISpatialQueryable>();
        private HashSet<ISpatialQueryable> shadowCastersInViewFrustum = new HashSet<ISpatialQueryable>();
        
        private HashSet<ParticleEffect> particleEffects = new HashSet<ParticleEffect>();

#if WINDOWS || XBOX
        private GraphicsBuffer graphicsBuffer;
        private DeferredEffect deferredEffect;
#endif
        private PostEffect screenEffect;

        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="Scene"/> class.
        /// </summary>
        public Scene(GraphicsDevice graphics) : this(graphics, null, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Scene"/> class.
        /// </summary>
        public Scene(GraphicsDevice graphics, Settings settings, ISceneManager defaultSceneManager)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            this.GraphicsDevice = graphics;
            this.defaultSceneManager = defaultSceneManager ?? new OctreeSceneManager();
            this.sceneManagers = new List<ISceneManager>();
            this.sceneManagers.Add(this.defaultSceneManager);
            this.flattenedQuery = CreateQuery<object>();
            this.detailedQuery = new DetailedQuery(this.defaultSceneManager);
            this.drawables = CreateQuery<IDrawableObject>();
            this.Settings = settings ?? new Settings();
            this.Statistics = new Statistics();
            this.Context = Context ?? new DrawingContext(graphics, Settings);
        }
        #endregion

        #region Collection
        /// <summary>
        /// Adds a new item to the scene using the specified scene manager.
        /// All child objects are also added using that scene manager unless stated
        /// explicitly with a SceneManger attached property.
        /// The default scene manager is used if no scene manager is specified.
        /// </summary>
        public void Add(object item, ISceneManager sceneManager)
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

            var sceneObject = desecendant as ISceneObject;
            if (sceneObject != null)
                sceneObject.OnAdded(Context);

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

            if (AddedToScene != null)
                AddedToScene(this, new NotifyCollectionChangedEventArgs<object>(0, desecendant));

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
        /// Occurs when any of the desecendant node is added to the scene either
        /// directly or through the addition of a subtree.
        /// </summary>
        public event EventHandler<NotifyCollectionChangedEventArgs<object>> AddedToScene;

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

            var sceneObject = desecendant as ISceneObject;
            if (sceneObject != null)
                sceneObject.OnRemoved(Context);

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

            if (RemovedFromScene != null)
                RemovedFromScene(this, new NotifyCollectionChangedEventArgs<object>(0, desecendant));

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
        /// Occurs when any of the desecendant node is removed from the scene either
        /// directly or through the removal from a subtree.
        /// </summary>
        public event EventHandler<NotifyCollectionChangedEventArgs<object>> RemovedFromScene;
        
        /// <summary>
        /// Ensures OnAddedToScene is called when a desecendant is added to a subtree.
        /// </summary>
        private void OnDesecendantAdded(object sender, NotifyCollectionChangedEventArgs<object> e)
        {
            InternalAdd(e.Value);
        }

        /// <summary>
        /// Ensures OnRemovedFromScene is called when a desecendant is removed from a subtree.
        /// </summary>
        private void OnDesecendantRemoved(object sender, NotifyCollectionChangedEventArgs<object> e)
        {
            InternalRemove(e.Value);
        }
        #endregion

        #region Find
        /// <summary>
        /// Create a spatial query of the specified type from this scene.
        /// </summary>
        public ISpatialQuery<T> CreateQuery<T>() where T : class
        {
            return new SceneQuery<T>(sceneManagers, Children);
        }

        /// <summary>
        /// Creates a pick ray based on the current camera.
        /// </summary>
        public Ray CreatePickRay(int x, int y)
        {
            if (Camera.Viewport == null)
                return GraphicsDevice.Viewport.CreatePickRay(x, y, Camera.View, Camera.Projection);
            return Camera.Viewport.Value.CreatePickRay(x, y, Camera.View, Camera.Projection);
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
        public void FindAll(ref BoundingFrustum boundingFrustum, ICollection<FindResult> result)
        {
            detailedQuery.FindAll(ref boundingFrustum, result);
        }
        #endregion

        #region Bounds
        /*
        /// <summary>
        /// Computes the bounds of the scene.
        /// </summary>
        public BoundingBox ComputeBounds()
        {
            var boundableObjects = new List<IBoundable>(topLevelNonSpatialQueryables.Count);
            FindAll(boundableObjects);
            return BoundingBoxExtensions.CreateMerged(boundableObjects.Select(obj => obj.BoundingBox));
        }

        /// <summary>
        /// Computes the bounds of all the object of type T in the scene.
        /// </summary>
        public BoundingBox ComputeBounds<T>() where T : class
        {
            return ComputeBounds<T>(null);
        }

        /// <summary>
        /// Computes the bounds of all the object of that matches the specified predicate in the scene.
        /// </summary>
        public BoundingBox ComputeBounds<T>(Predicate<T> predicate) where T : class
        {
            if (predicate == null)
                predicate = obj => true;

            var boundableObjects = new List<T>(topLevelNonSpatialQueryables.Count);
            FindAll<T>(boundableObjects);

            return BoundingBoxExtensions.CreateMerged(
                    boundableObjects.Where(obj => predicate(obj)).Select(
                        obj => ContainerTraverser.FindParentContainer<IBoundable>(obj))
                            .OfType<IBoundable>().Select(b => b.BoundingBox));
        }
         */
        #endregion

        #region Decal
        /// <summary>
        /// Projects a decal onto the scene.
        /// </summary>
        [Obsolete("Not Implemented")]
        public Decal ProjectDecal(Ray ray, Texture2D texture, float width, float height, float rotation)
        {
            return ProjectDecal(ray, texture, null, width, height, 0, rotation, null);
        }

        /// <summary>
        /// Projects a decal onto the scene.
        /// </summary>
        /// <param name="ray">The ray to project.</param>
        /// <param name="texture">The decal texture.</param>
        /// <param name="normalMap">The decal normal map or null if normal mapping is disabled.</param>
        /// <param name="width">The width of the decal texture in world space.</param>
        /// <param name="height">The height of the decal texture in world space.</param>
        /// <param name="depth">The depth of the decal bounds or 0 to use the average of width and height.</param>
        /// <param name="rotation">The rotation of the decal texture.</param>
        /// <param name="filter">A predicate determines whether the target object should show the decal.</param>
        /// <returns> The decal object added to the scene or null if nothing is projected.</returns>
        [Obsolete("Not Implemented")]
        public Decal ProjectDecal(Ray ray, Texture2D texture, Texture2D normalMap, float width, float height, float depth, float rotation, Predicate<IGeometry> filter)
        {
            FindResult rayCastResult = Find(ray);
            if (!rayCastResult.Distance.HasValue)
                return null;

            var decalSize = new Vector3(width, height, depth > 0 ? depth : (width + height) * 0.5f);
            var decalPosition = ray.Position + ray.Direction * rayCastResult.Distance.Value;
            var decalTransform = Matrix.CreateWorld(decalPosition, -ray.Direction, Vector3.Up);

            Decal decal = new Decal(GraphicsDevice);
            decal.Texture = texture;
            decal.NormalMap = normalMap;
            decal.Transform = decalTransform;
            decal.Size = decalSize;

            Add(decal);
            return decal;
        }
        #endregion

        #region Update & Draw
        /*
        public void Update(TimeSpan elapsedTime)
        {
            ContainerTraverser.Traverse<IUpdateable>(this, delegate(IUpdateable updateable)
            {
                updateable.Update(elapsedTime);
                return TraverseOptions.Continue;
            });
        }
        */

        /// <summary>
        /// Updates and draws the scene under the current camera and graphics setting.
        /// </summary>
        public void Draw(TimeSpan elapsedTime)
        {
            UpdateCamera(elapsedTime);
            Draw(elapsedTime, Camera.View, Camera.Projection);
        }

        /// <summary>
        /// Updates and draws the scene with the specified camera settings.
        /// </summary>
        public void Draw(TimeSpan elapsedTime, Matrix view, Matrix projection)
        {
            Context.Draw(elapsedTime, drawables, view, projection, Settings);
        }

        /// <summary>
        /// Updates and draws all the drawable objects in the scene with the specified camera setting using the
        /// specified material.
        /// </summary>
        public void Draw(TimeSpan elapsedTime, Matrix view, Matrix projection, Material material)
        {
            Context.Draw(elapsedTime, drawables, view, projection, Settings);
        }

        private void UpdateCamera(TimeSpan elapsedTime)
        {
            IUpdateable updateable;
            updateable = camera as IUpdateable;
            if (updateable != null)
                updateable.Update(elapsedTime);
        }

        /// <summary>
        /// Called when an object is added to view frustum each frame.
        /// </summary>
        protected virtual void OnAddedToViewFrustum(object item)
        {

        }

        /// <summary>
        /// Updates all the object in the scene.
        /// </summary>
        private void UpdateObjects(TimeSpan elapsedTime)
        {
            updateableObjects.Clear();
            //FindAll(updateableObjects);
            for (int i = 0; i < updateableObjects.Count; i++)
                updateableObjects[i].Update(elapsedTime);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing) 
        {
            foreach (var disposable in this.OfType<IDisposable>())
            {
                disposable.Dispose();
            }
        }
        #endregion
    }
}