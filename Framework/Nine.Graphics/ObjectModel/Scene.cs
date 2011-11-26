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
    /// <summary>
    /// Defines a graphical scene that manages a set of objects, cameras and lights.
    /// </summary>
    public class Scene : ICollection<object>, IDisposable, IDrawable
    {
        #region Properties
        /// <summary>
        /// Gets the graphics settings
        /// </summary>
        public GraphicsSettings Settings { get; private set; }

        /// <summary>
        /// Gets or sets the active camera.
        /// </summary>
        public ICamera Camera
        {
            get { return camera ?? (camera = new TopDownEditorCamera(GraphicsDevice)); }
            set { camera = value; }
        }
        private ICamera camera;

        /// <summary>
        /// Gets or sets the post processing screen effect used by this renderer.
        /// </summary>
        [ContentSerializerIgnore]
        public ScreenEffect ScreenEffect
        {
            get { return screenEffect ?? (screenEffect = new ScreenEffect(GraphicsDevice) { Enabled = false }); }
            set { screenEffect = value; }
        }
        private ScreenEffect screenEffect;

        /// <summary>
        /// Gets the underlying graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets the statistics of this renderer.
        /// </summary>
        public GraphicsStatistics Statistics { get; private set; }

        /// <summary>
        /// Gets or sets the graphics context.
        /// </summary>
        public GraphicsContext GraphicsContext { get; protected set; }

        /// <summary>
        /// For serialization only.
        /// </summary>
        [ContentSerializer()]
        internal List<object> SceneObjects
        {
            get { return topLevelObjects; }
            set 
            {
                if (value != null)
                {
                    for (int i = 0; i < value.Count; i++)
                        Add(value[i]);
                }
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the tag.
        /// </summary>
        public object Tag { get; set; }
        #endregion

        #region Fields
        /// <summary>
        /// This is the underlying scene manager that manages all spatial queryables.
        /// </summary>
        private ISceneManager<ISpatialQueryable> sceneManager;

        /// <summary>
        /// Gets the spatial query that can find all the top level objects.
        /// </summary>
        private ISpatialQuery<FindResult> detailedQuery;

        /// <summary>
        /// Gets the spatial query that can find all the flattened objects.
        /// </summary>
        private ISpatialQuery<object> flattenedQuery;

        /// <summary>
        /// This list contains only the objects added using Scene.Add method.
        /// </summary>
        private List<object> topLevelObjects = new List<object>();

        /// <summary>
        /// This list contains all the drawable objects in the current view frustum.
        /// </summary>
        private List<IDrawableObject> drawablesInViewFrustum = new List<IDrawableObject>();
        
        /// <summary>
        /// This list contains all the flattened objects in the current view frustum.
        /// </summary>
        private List<object> flattenedObjectsInViewFrustum = new List<object>();

        /// <summary>
        /// This list contains all the opaque objects in the current view frustum.
        /// </summary>
        private List<IDrawableObject> opaqueDrawablesInViewFrustum = new List<IDrawableObject>();

        /// <summary>
        /// This list contains all the transparent objects in the current view frustum.
        /// </summary>
        private List<IDrawableObject> transparentDrawablesInViewFrustum = new List<IDrawableObject>();
        
        private List<Light> lightsInViewFrustum = new List<Light>();
        private List<Light> appliedMultiPassLights = new List<Light>();
        private List<Light> unAppliedMultiPassLights = new List<Light>();
        private List<FindResult> rayCastResult = new List<FindResult>();

        private HashSet<ISpatialQueryable> shadowCastersInLightFrustum = new HashSet<ISpatialQueryable>();
        private HashSet<ISpatialQueryable> shadowCastersInViewFrustum = new HashSet<ISpatialQueryable>();

        private bool hasShadowReceiversInViewFrustum;
        private EffectMaterial cachedEffectMaterial;
        private BitArray lightUsed;

#if WINDOWS || XBOX
        private GraphicsBuffer graphicsBuffer;
        private DeferredEffect deferredEffect;
#endif
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
        public Scene(GraphicsDevice graphics, GraphicsSettings settings, ISceneManager<ISpatialQueryable> sceneManager)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            this.GraphicsDevice = graphics;
            this.sceneManager = sceneManager ?? new OctreeSceneManager<ISpatialQueryable>();
            this.flattenedQuery = new FlattenedQuery(this.sceneManager, this.topLevelObjects);
            this.detailedQuery = new DetailedQuery(this.sceneManager);
            this.Settings = settings ?? new GraphicsSettings();
            this.Statistics = new GraphicsStatistics();
            this.GraphicsContext = GraphicsContext ?? new GraphicsContext(graphics, Settings, Statistics);
        }
        #endregion

        #region Collection
        /// <summary>
        /// Adds a new item to the scene.
        /// </summary>
        public void Add(object item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            topLevelObjects.Add(item); 
            InternalAdd(item);
        }

        private void InternalAdd(object item)
        {
            ContainerTraverser.Traverse<object>(item, FlattenedAdd);
        }

        private TraverseOptions FlattenedAdd(object item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            var queryable = item as ISpatialQueryable;
            if (queryable != null)
                sceneManager.Add(queryable);

            var collectionChanged = item as INotifyCollectionChanged<object>;
            if (collectionChanged != null)
            {
                collectionChanged.Added += OnChildAdded;
                collectionChanged.Removed += OnChildRemoved;
            }
            return TraverseOptions.Continue;            
        }

        /// <summary>
        /// Removes an item from the scene.
        /// </summary>
        public bool Remove(object item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            InternalRemove(item);
            return topLevelObjects.Remove(item);
        }

        private void InternalRemove(object item)
        {
            ContainerTraverser.Traverse<object>(item, FlattenedRemove);
        }

        private TraverseOptions FlattenedRemove(object item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            var queryable = item as ISpatialQueryable;
            if (queryable != null)
                sceneManager.Remove(queryable);

            var collectionChanged = item as INotifyCollectionChanged<object>;
            if (collectionChanged != null)
            {
                collectionChanged.Added -= OnChildAdded;
                collectionChanged.Removed -= OnChildRemoved;
            }
            return TraverseOptions.Continue;
        }

        private void OnChildAdded(object sender, NotifyCollectionChangedEventArgs<object> e)
        {
            InternalAdd(e.Value);
        }

        private void OnChildRemoved(object sender, NotifyCollectionChangedEventArgs<object> e)
        {
            InternalRemove(e.Value);
        }

        /// <summary>
        /// Clears all the scene objects.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < topLevelObjects.Count; i++)
            {
                ContainerTraverser.Traverse<INotifyCollectionChanged<object>>(topLevelObjects[i], ClearEvents);
            }

            sceneManager.Clear();
            topLevelObjects.Clear();
        }

        private TraverseOptions ClearEvents(INotifyCollectionChanged<object> collectionChanged)
        {
            if (collectionChanged != null)
            {
                collectionChanged.Added -= OnChildAdded;
                collectionChanged.Removed -= OnChildRemoved;
            }
            return TraverseOptions.Continue;
        }

        /// <summary>
        /// Gets whether the scene contains the target item.
        /// </summary>
        public bool Contains(object item)
        {
            return topLevelObjects.Contains(item);
        }

        void ICollection<object>.CopyTo(object[] array, int arrayIndex)
        {
            topLevelObjects.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of objects managed by this scene.
        /// </summary>
        public int Count 
        {
            get { return topLevelObjects.Count; } 
        }

        bool ICollection<object>.IsReadOnly
        {
            get { return false; } 
        }

        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            return topLevelObjects.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return topLevelObjects.GetEnumerator();
        }
        #endregion

        #region Find
        /// <summary>
        /// Finds the first object of type T with the specified name.
        /// </summary>
        public T Find<T>(string name) where T : class
        {
            if (Name == name && this is T)
                return this as T;

            for (int i = 0; i < topLevelObjects.Count; i++)
            {
                var result = SceneQueryHelper<T>.FindName(topLevelObjects[i], name);
                if (result != null)
                    return result;
            }
            return null;
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
        /// Finds all the objects of type T with the specified name.
        /// </summary>
        public void FindAll<T>(string name, ICollection<T> result) where T : class
        {
            if (Name == name && this is T)
                result.Add(this as T);

            for (int i = 0; i < topLevelObjects.Count; i++)
            {
                SceneQueryHelper<T>.FindAllNames(topLevelObjects[i], name, result);
            }
        }

        /// <summary>
        /// Finds all the objects that is contained by or intersects the specified bounding sphere.
        /// </summary>
        /// <remarks>
        /// This find process will look up the object tree from the leaf nodes for all the occurrences of 
        /// <see cref="ISpatialQueryable"/>. 
        /// If any of object's parent is contained by or intersects the input
        /// bounding volumn, the object will be added to the result list. 
        /// If the <see cref="ISpatialQueryable"/>
        /// is not found, the object will also be added to the result list.
        /// </remarks>
        public void FindAll(ref BoundingSphere boundingSphere, ICollection<object> result)
        {
            flattenedQuery.FindAll(ref boundingSphere, result);
        }
        
        /// <summary>
        /// Finds all the objects that intersects the specified ray.
        /// </summary>
        public void FindAll(ref Ray ray, ICollection<object> result)
        {
            flattenedQuery.FindAll(ref ray, result);
        }

        /// <summary>
        /// Finds all the objects that is contained by or intersects the specified bounding box.
        /// </summary>
        public void FindAll(ref BoundingBox boundingBox, ICollection<object> result)
        {
            flattenedQuery.FindAll(ref boundingBox, result);
        }
        /// <summary>
        /// Finds all the objects that is contained by or intersects the specified bounding frustum.
        /// </summary>
        public void FindAll(ref BoundingFrustum boundingFrustum, ICollection<object> result)
        {
            flattenedQuery.FindAll(ref boundingFrustum, result);
        }

        /// <summary>
        /// Finds all the objects of type T that is contained by or intersects the specified bounding sphere.
        /// </summary>
        public void FindAll<T>(ref BoundingSphere boundingSphere, ICollection<T> result) where T : class
        {
            var adapter = FlattenedCollectionAdapter<T>.Instance;
            adapter.Result = result;
            adapter.IncludeTopLevelNonSpatialQueryableDesendants(topLevelObjects);
            sceneManager.FindAll(ref boundingSphere, adapter);
            adapter.Result = null;
        }

        /// <summary>
        /// Finds all the objects of type T that intersects the specified ray.
        /// </summary>
        public void FindAll<T>(ref Ray ray, ICollection<T> result) where T : class
        {
            var adapter = FlattenedCollectionAdapter<T>.Instance;
            adapter.Result = result;
            adapter.IncludeTopLevelNonSpatialQueryableDesendants(topLevelObjects);
            sceneManager.FindAll(ref ray, adapter);
            adapter.Result = null;
        }

        /// <summary>
        /// Finds all the objects of type T that is contained by or intersects the specified bounding box.
        /// </summary>
        public void FindAll<T>(ref BoundingBox boundingBox, ICollection<T> result) where T : class
        {
            var adapter = FlattenedCollectionAdapter<T>.Instance;
            adapter.Result = result;
            adapter.IncludeTopLevelNonSpatialQueryableDesendants(topLevelObjects);
            sceneManager.FindAll(ref boundingBox, adapter);
            adapter.Result = null;
        }

        /// <summary>
        /// Finds all the objects of type T that is contained by or intersects the specified bounding frustum.
        /// </summary>
        public void FindAll<T>(ref BoundingFrustum boundingFrustum, ICollection<T> result) where T : class
        {
            var adapter = FlattenedCollectionAdapter<T>.Instance;
            adapter.Result = result;
            adapter.IncludeTopLevelNonSpatialQueryableDesendants(topLevelObjects);
            sceneManager.FindAll(ref boundingFrustum, adapter);
            adapter.Result = null;
        }

        /// <summary>
        /// Finds all the scene objects and the original volumn for intersection test that is contained by or
        /// intersects the specified bounding sphere.
        /// </summary>
        /// <remarks>
        /// When an object tree is added to the scene using <see cref="Scene.Add"/>, 
        /// this find method will set the <see cref="FindResult.OriginalTarget"> property to the original 
        /// <see cref="ISpatialQueryable"/> for the intersection test against the input bounding volumn.
        /// It will set the <see cref="FindResult.Target"/> property to the containing object that is added
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

        #region Draw
        /// <summary>
        /// Draws all the object managed by this renderer.
        /// </summary>
        public void Draw(TimeSpan elapsedTime)
        {
            Statistics.Reset();
            
            UpdateVisibleDrawablesAndLightsInViewFrustum();

            Update(elapsedTime);

            GraphicsContext.View = Camera.View;
            GraphicsContext.Projection = Camera.Projection;
            GraphicsContext.ElapsedTime = elapsedTime;
            
            BeginDraw();

            UpdateFog();

            GraphicsDevice.Clear(Settings.BackgroundColor);

            if (Settings.LightingEnabled)
            {
#if WINDOWS_PHONE
                ClearLights(drawablesInViewFrustum);
                UpdateAffectedDrawablesAndAffectingLights(drawablesInViewFrustum, lightsInViewFrustum);

                GraphicsContext.Begin();
                DrawObjects(opaqueDrawablesInViewFrustum);
                GraphicsContext.End();
                
                GraphicsContext.Begin(BlendState.Additive, null, DepthStencilState.DepthRead, null);
                DrawObjects(transparentDrawablesInViewFrustum);
                GraphicsContext.End();
#else
                // Draw opaque objects
                ClearLights(opaqueDrawablesInViewFrustum);
                UpdateAffectedDrawablesAndAffectingLights(drawablesInViewFrustum, lightsInViewFrustum);
#if !SILVERLIGHT
                ApplyDeferredLighting(drawablesInViewFrustum, lightsInViewFrustum);
#endif
                ApplyShadows(drawablesInViewFrustum, lightsInViewFrustum);

                GraphicsDevice.Clear(Settings.BackgroundColor);

                GraphicsContext.Begin();
                DrawObjects(opaqueDrawablesInViewFrustum);
                GraphicsContext.End();

                if (Settings.MultiPassShadowEnabled)
                {
                    GraphicsContext.Begin(BlendState.AlphaBlend, null, DepthStencilState.DepthRead, null);
                    DrawMultiPassShadowMapOverlay(opaqueDrawablesInViewFrustum);
                    GraphicsContext.End();
                }

                if (Settings.MultiPassLightingEnabled)
                {
                    GraphicsContext.Begin(BlendState.Additive, null, DepthStencilState.DepthRead, null);
                    DrawMultiPassLightingOverlay(opaqueDrawablesInViewFrustum);
                    GraphicsContext.End();
                }

                // Draw transparent objects
                ClearLights(transparentDrawablesInViewFrustum);

                GraphicsContext.Begin(BlendState.AlphaBlend, null, null, null);
                DrawObjects(transparentDrawablesInViewFrustum);
                GraphicsContext.End();

                if (Settings.MultiPassLightingEnabled)
                {
                    GraphicsContext.Begin(BlendState.Additive, null, DepthStencilState.DepthRead, null);
                    DrawMultiPassLightingOverlay(transparentDrawablesInViewFrustum);
                    GraphicsContext.End();
                }
#endif
            }
            else
            {
                GraphicsContext.Begin();
                DrawNoLighting(opaqueDrawablesInViewFrustum);
                GraphicsContext.End();

                GraphicsContext.Begin(BlendState.AlphaBlend, null, null, null);
                DrawNoLighting(transparentDrawablesInViewFrustum);
                GraphicsContext.End();
            }

            EndDraw();

            GraphicsContext.Begin();
            DrawDebug(drawablesInViewFrustum, lightsInViewFrustum);
            GraphicsContext.End();
        }

        private void UpdateVisibleDrawablesAndLightsInViewFrustum()
        {
            flattenedObjectsInViewFrustum.Clear();
            drawablesInViewFrustum.Clear();
            opaqueDrawablesInViewFrustum.Clear();
            transparentDrawablesInViewFrustum.Clear();
            lightsInViewFrustum.Clear();

            var viewFrustum = GraphicsContext.ViewFrustum;
            FindAll(ref viewFrustum, flattenedObjectsInViewFrustum);

            hasShadowReceiversInViewFrustum = false;

            for (int i = 0; i < flattenedObjectsInViewFrustum.Count; i++)
            {
                var obj = flattenedObjectsInViewFrustum[i];

                var light = obj as Light;
                if (light != null && light.Enabled)
                {
                    lightsInViewFrustum.Add(light);
                    continue;
                }

                var drawable = obj as IDrawableObject;
                if (drawable != null && drawable.Visible)
                {
                    if (IsTransparent(drawable))
                        transparentDrawablesInViewFrustum.Add(drawable);
                    else
                        opaqueDrawablesInViewFrustum.Add(drawable);

                    if (!hasShadowReceiversInViewFrustum && ReceiveShadow(drawable))
                        hasShadowReceiversInViewFrustum = true;

                    drawablesInViewFrustum.Add(drawable);
                }
            }

            Statistics.VisibleDrawableCount = drawablesInViewFrustum.Count;
            Statistics.VisibleObjectCount = flattenedObjectsInViewFrustum.Count;
            Statistics.VisibleLightCount = lightsInViewFrustum.Count;
        }

        /// <summary>
        /// Updates all the object in the scene.
        /// </summary>
        private void Update(TimeSpan elapsedTime)
        {
            // Camera
            // TODO: Make camera transformable just like lights.
            IUpdateable updateable;
            updateable = camera as IUpdateable;
            if (updateable != null)
                updateable.Update(elapsedTime);

            for (int i = 0; i < flattenedObjectsInViewFrustum.Count; i++)
            {
                updateable = flattenedObjectsInViewFrustum[i] as IUpdateable;
                if (updateable != null)
                    updateable.Update(elapsedTime);
            }
        }

        private void BeginDraw()
        {
            // Xna might complain about floating point texture requires texture filter to be point.
            for (int i = 0; i < 16; i++)
                GraphicsDevice.Textures[i] = null;

            for (int i = 0; i < drawablesInViewFrustum.Count; i++)
            {
                drawablesInViewFrustum[i].BeginDraw(GraphicsContext);
            }
        }

        private void EndDraw()
        {
            for (int i = 0; i < drawablesInViewFrustum.Count; i++)
            {
                drawablesInViewFrustum[i].EndDraw(GraphicsContext);
            }
        }

        private void DrawNoLighting(List<IDrawableObject> drawables)
        {
            for (int i = 0; i < drawables.Count; i++)
            {
                drawables[i].Draw(GraphicsContext);
            }
        }

        #region Deferred
#if WINDOWS || XBOX
        private void ApplyDeferredLighting(List<IDrawableObject> drawables, List<Light> lights)
        {
            bool hasDeferred = false;
            for (int i = 0; i < drawables.Count; i++)
            {
                var drawable = drawables[i];
                var material = drawable.Material;
                if (material != null && material.IsDeferred)
                    hasDeferred = true;
            }

            if (!hasDeferred)
                return;
                    
            if (graphicsBuffer == null)
                graphicsBuffer = new GraphicsBuffer(GraphicsDevice);
            if (deferredEffect == null)
                deferredEffect = new DeferredEffect(GraphicsDevice);

            if (Settings.PreferHighDynamicRangeLighting)
                graphicsBuffer.LightBufferFormat = SurfaceFormat.HdrBlendable;
            else
                graphicsBuffer.LightBufferFormat = SurfaceFormat.Color;
            
            // Draw depth normal buffer.
            graphicsBuffer.Begin();
            GraphicsContext.Begin();
            for (int i = 0; i < drawables.Count; i++)
            {
                var drawable = drawables[i];
                var material = drawable.Material;
                if (material == null || !material.IsDeferred)
                    continue;

                if (material.GraphicsBufferEffect != null)
                {
                    material.Apply();
                    drawable.Draw(GraphicsContext, material.GraphicsBufferEffect);
                }
                else
                {
                    drawable.Draw(GraphicsContext, graphicsBuffer.Effect);
                }
            }
            GraphicsContext.End();
            graphicsBuffer.End();
            
            // Draw light buffer
            GraphicsContext.Begin();
            graphicsBuffer.BeginLights(GraphicsContext.View, GraphicsContext.Projection);
            for (int i = 0; i < lights.Count; i++)
            {
                var light = lights[i] as IDeferredLight;
                if (light != null)
                    graphicsBuffer.DrawLight(light);
            }
            graphicsBuffer.EndLights();
            GraphicsContext.End();

            // Apply light buffer
            for (int i = 0; i < drawables.Count; i++)
            {
                var drawable = drawables[i];
                var material = drawable.Material;
                if (material != null && material.IsDeferred && material.GraphicsBufferEffect != null)
                {
                    IEffectTexture texture = material.Find<IEffectTexture>();
                    if (texture != null)
                        texture.SetTexture(TextureUsage.LightBuffer, graphicsBuffer.LightBuffer);
                }
            }
            deferredEffect.LightTexture = graphicsBuffer.LightBuffer;
        }
#endif
        #endregion

        #region Forward
        private void DrawObjects(List<IDrawableObject> drawables)
        {
            for (int i = 0; i < drawables.Count; i++)
            {
                var drawable = drawables[i];
                var material = drawable.Material;
                var lightable = drawable as ILightable;
                
#if !WINDOWS_PHONE && !SILVERLIGHT
                if (material != null && material.IsDeferred)
                {
                    // Object uses deferred effect
                    if (material.GraphicsBufferEffect != null)
                        drawable.Draw(GraphicsContext);
                    else
                        drawable.Draw(GraphicsContext, deferredEffect);
                }
                else
#endif
                {
                    // Setup light info in drawable materials.
                    if (lightable != null && lightable.LightingEnabled && material != null)
                    {
                        var lightingData = lightable.LightingData as LightingData;
                        if (lightingData != null)
                        {
                            if (lightingData.MultiPassLights != null)
                                lightingData.MultiPassLights.Clear();
                            ApplyLights(lightingData.AffectingLights, material, ref lightingData.MultiPassLights);
                        }
                    }
                    drawable.Draw(GraphicsContext);
                }
            }
        }

        private void DrawMultiPassLightingOverlay(List<IDrawableObject> drawables)
        {
            // Multipass lighting
            foreach (var drawable in drawables)
            {
                var lightable = drawable as ILightable;

                if (lightable != null && lightable.LightingEnabled && lightable.MultiPassLightingEnabled)
                {
                    var lightingData = lightable.LightingData as LightingData;
                    if (lightingData != null && lightingData.AffectingLights != null &&
                        lightingData.MultiPassLights != null && lightingData.MultiPassLights.Count > 0)
                    {
                        unAppliedMultiPassLights.Clear();
                        unAppliedMultiPassLights.AddRange(lightingData.MultiPassLights);

                        while (unAppliedMultiPassLights.Count > 0)
                        {
                            var effect = unAppliedMultiPassLights[0].MultiPassEffect;
                            appliedMultiPassLights.Clear();

                            int countBeforeApply = unAppliedMultiPassLights.Count;
                            ApplyLights(unAppliedMultiPassLights, effect, ref appliedMultiPassLights);
                            int countAfterApply = appliedMultiPassLights.Count;

                            if (countAfterApply >= countBeforeApply)
                            {
                                throw new InvalidOperationException("Light<T>.Effect must implement IEffectLights<T>");
                            }

                            drawable.Draw(GraphicsContext, effect);
                            if (appliedMultiPassLights.Count <= 0)
                                break;

                            // Swap
                            var temp = unAppliedMultiPassLights;
                            unAppliedMultiPassLights = appliedMultiPassLights;
                            appliedMultiPassLights = temp;
                        }
                    }
                }
            }
        }
        #endregion

        #region Debug
        private void DrawDebug(List<IDrawableObject> drawables, List<Light> lights)
        {
            var settings = Settings.Debug;
            var primitiveBatch = GraphicsContext.PrimitiveBatch;
            var spriteBatch = GraphicsContext.SpriteBatch;

            if (settings.ShowStatistics && Settings.DefaultFont != null)
            {
                Statistics.Draw(spriteBatch, Settings.DefaultFont, settings.StatisticsColor);
            }
            if (settings.ShowBoundingBox)
            {
                foreach (var obj in flattenedObjectsInViewFrustum)
                {
                    var boundable = obj as IBoundable;
                    if (boundable != null)
                        primitiveBatch.DrawBox(boundable.BoundingBox, null, settings.BoundingBoxColor);
                }
            }
            if (settings.ShowLightFrustum)
            {
                foreach (var light in lights)
                { 
                    light.DrawFrustum(GraphicsContext);
                    if (light.ShadowFrustum.Matrix.M44 != 0)
                        primitiveBatch.DrawFrustum(light.ShadowFrustum, null, settings.ShadowFrustumColor);
                }
            }
            if (settings.ShowSceneManager)
            {
                DrawSceneManager(sceneManager);
            }
#if WINDOWS || XBOX
            if (settings.ShowDepthBuffer && graphicsBuffer != null)
            {
                spriteBatch.End();
                spriteBatch.Begin(0, null, SamplerState.PointClamp, null, null);
                spriteBatch.Draw(graphicsBuffer.DepthBuffer, Vector2.Zero, Color.White);
                spriteBatch.End();
                spriteBatch.Begin();
            }
            if (settings.ShowNormalBuffer && graphicsBuffer != null)
            {
                spriteBatch.Draw(graphicsBuffer.NormalBuffer, Vector2.Zero, Color.White);
            }
            if (settings.ShowLightBuffer && graphicsBuffer != null)
            {
                spriteBatch.Draw(graphicsBuffer.LightBuffer, Vector2.Zero, Color.White);
            }
            if (settings.ShowShadowMap)
            {
                var light = lightsInViewFrustum.FirstOrDefault(l => l.CastShadow && l.Enabled && l.ShadowMap != null);
                if (light != null)
                {
                    spriteBatch.End();
                    spriteBatch.Begin(0, BlendState.Opaque, SamplerState.PointClamp, null, null);
                    spriteBatch.Draw(light.ShadowMap.Texture, new Rectangle(0, 0, 512, 512), Color.White);
                    spriteBatch.End();
                    spriteBatch.Begin();
                }
            }
#endif
        }

        private void DrawSceneManager(ISceneManager<ISpatialQueryable> sceneManager)
        {
            OctreeSceneManager<ISpatialQueryable> octreeSceneManager = sceneManager as OctreeSceneManager<ISpatialQueryable>;
            if (octreeSceneManager != null)
                DrawOctreeSceneManager(octreeSceneManager);
        }

        private void DrawOctreeSceneManager(OctreeSceneManager<ISpatialQueryable> octreeSceneManager)
        {
            var color = Settings.Debug.SceneManagerColor;
            var primitiveBatch = GraphicsContext.PrimitiveBatch;

            octreeSceneManager.Tree.ForEach(node =>
            {
                if (node.Value != null)
                {
                    primitiveBatch.DrawBox(node.Bounds, null, color * node.Value.Count);
                    node.Value.ForEach(val => primitiveBatch.DrawBox(val.BoundingBox, null, Settings.Debug.BoundingBoxColor));
                }
            });
        }
        #endregion
        #endregion

        #region Lighting
        private void UpdateAffectedDrawablesAndAffectingLights(List<IDrawableObject> drawables, List<Light> lights)
        {
            // Clear affecting lights
            for (var i = 0; i < drawables.Count; i++)
            {
                var obj = drawables[i];
                if (!obj.Visible)
                    continue;

                var lightable = obj as ILightable;
                if (lightable != null && lightable.LightingEnabled)
                {
                    var lightingData = lightable.LightingData as LightingData;
                    if (lightingData != null && lightingData.AffectingLights != null)
                        lightingData.AffectingLights.Clear();
                }
            }

            // Setup lights and drawable relations.
            for (var currentLight = 0; currentLight < lights.Count; currentLight++)
            {
                var light = lights[currentLight];
                if (!light.Enabled)
                    continue;

                if (light.AffectedDrawables == null)
                    light.AffectedDrawables = new List<IDrawableObject>();
                light.AffectedDrawables.Clear();

                light.FindAll(this, drawablesInViewFrustum, light.AffectedDrawables);

                for (int currentDrawable = 0; currentDrawable < light.AffectedDrawables.Count; currentDrawable++)
                {
                    var drawable = light.AffectedDrawables[currentDrawable];
                    var lightable = drawable as ILightable;
                    if (drawable.Visible && lightable != null && lightable.LightingEnabled)
                    {
                        var lightingData = lightable.LightingData as LightingData;
                        if (lightingData == null)
                            lightable.LightingData = (lightingData = new LightingData());
                        if (lightingData.AffectingLights == null)
                            lightingData.AffectingLights = new List<Light>();
                        lightingData.AffectingLights.Add(light);
                    }
                }
            }
        }

        private void ClearLights(List<IDrawableObject> drawables)
        {
            for (int currentDrawable = 0; currentDrawable < drawables.Count; currentDrawable++)
            {
                var drawable = drawables[currentDrawable];
                if (drawable.Material != null)
                    ClearLights(drawable.Material);
            }
        }

        private void ClearLights(Material material)
        {
            if (material == null)
                return;

            var ambientLights = material.Find<IEffectLights<IAmbientLight>>();
            if (ambientLights != null)
            {
                var lights = ambientLights.Lights;
                var count = lights.Count;
                for (int i = 0; i < count; i++)
                    lights[i].AmbientLightColor = Vector3.Zero;
            }
            var directionalLights = material.Find<IEffectLights<IDirectionalLight>>();
            if (directionalLights != null)
            {
                var lights = directionalLights.Lights;
                var count = lights.Count;
                for (int i = 0; i < count; i++)
                    lights[i].DiffuseColor = Vector3.Zero;
            }

            var pointLights = material.Find<IEffectLights<IPointLight>>();
            if (pointLights != null)
            {
                var lights = pointLights.Lights;
                var count = lights.Count;
                for (int i = 0; i < count; i++)
                    lights[i].DiffuseColor = Vector3.Zero;
            }

            var spotLights = material.Find<IEffectLights<ISpotLight>>();
            if (spotLights != null)
            {
                var lights = spotLights.Lights;
                var count = lights.Count;
                for (int i = 0; i < count; i++)
                    lights[i].DiffuseColor = Vector3.Zero;
            }
        }

        private void ApplyLights(List<Light> sourceLights, Effect effect, ref List<Light> unusedLights)
        {
            if (cachedEffectMaterial == null)
                cachedEffectMaterial = new EffectMaterial();
            cachedEffectMaterial.SetEffect(effect);
            ApplyLights(sourceLights, cachedEffectMaterial, ref unusedLights);
        }

        private void ApplyLights(List<Light> sourceLights, Material material, ref List<Light> unusedLights)
        {
            if (sourceLights == null || material == null)
                return;

            int lightCount = sourceLights.Count;
            if (lightUsed == null || lightUsed.Length < lightCount)
                lightUsed = new BitArray(lightCount);

            lightUsed.SetAll(false);
            for (int i = 0; i < lightCount; i++)
            {
                if (lightUsed[i])
                    continue;

                var iLight = 0;
                var light = sourceLights[i];
                if (!light.Apply(material, iLight++, IsLastLightOfType(sourceLights, i)))
                {
                    if (unusedLights == null)
                        unusedLights = new List<Light>();
                    unusedLights.Add(light);
                    continue;
                }

                bool failed = false;
                for (int j = i + 1; j < lightCount; j++)
                {
                    var light2 = sourceLights[j];
                    if (light2.GetType() != light.GetType())
                        continue;

                    lightUsed[j] = true;
                    if (failed || !light2.Apply(material, iLight++, IsLastLightOfType(sourceLights, j)))
                    {
                        if (unusedLights == null)
                            unusedLights = new List<Light>();
                        unusedLights.Add(light2);
                        failed = true;
                    }
                }
            }
        }

        private bool IsLastLightOfType(List<Light> sourceLights, int i)
        {
            int lightCount = sourceLights.Count;
            var type = sourceLights[i].GetType();
            for (int j = i + 1; j < lightCount; j++)
            {
                if (!lightUsed[j] && sourceLights[j].GetType() == type)
                    return false;
            }
            return true;
        }
        #endregion

        #region Shadow
#if !WINDOWS_PHONE
        private void ApplyShadows(List<IDrawableObject> drawables, List<Light> lightsInViewFrustum)
        {
            DrawShadowMaps(lightsInViewFrustum);

            for (int i = 0; i < drawables.Count; i++)
            {
                var drawable = drawables[i];
                var material = drawable.Material;
                var lightable = drawable as ILightable;
                if (lightable == null || material == null || !lightable.ReceiveShadow)
                    continue;

                var lightingData = lightable.LightingData as LightingData;
                if (lightable != null)
                {
                    if (lightingData.MultiPassShadows != null)
                        lightingData.MultiPassShadows.Clear();

                    ApplyShadowMap(lightingData.AffectingLights, material, ref lightingData.MultiPassShadows);
                }
            }
        }

        private void DrawShadowMaps(List<Light> lightsInViewFrustum)
        {
            if (!Settings.ShadowEnabled || !hasShadowReceiversInViewFrustum)
                return;

            for (int currentLight = 0; currentLight < lightsInViewFrustum.Count; currentLight++)
            {
                var light = lightsInViewFrustum[currentLight];
                if (light.Enabled && light.CastShadow)
                {
                    FindShadowCasters(drawablesInViewFrustum, shadowCastersInViewFrustum);
                    FindShadowCasters(light.AffectedDrawables, shadowCastersInLightFrustum);

                    if (shadowCastersInLightFrustum.Count > 0 || shadowCastersInViewFrustum.Count > 0)
                    {
                        light.DrawShadowMap(GraphicsContext, this, shadowCastersInLightFrustum, shadowCastersInViewFrustum);
                    }

                    shadowCastersInViewFrustum.Clear();
                    shadowCastersInLightFrustum.Clear();
                }
            }
        }

        private void FindShadowCasters(List<IDrawableObject> drawables, HashSet<ISpatialQueryable> shadowCasters)
        {
            for (int currentDrawable = 0; currentDrawable < drawables.Count; currentDrawable++)
            {
                var drawable = drawables[currentDrawable];
                if (CastShadow(drawable))
                {
                    var parentSpatialQueryable = ContainerTraverser.FindParentContainer<ISpatialQueryable>(drawable);
                    if (parentSpatialQueryable != null)
                        shadowCasters.Add(parentSpatialQueryable);
                }
            }
        }

        private void DrawMultiPassShadowMapOverlay(List<IDrawableObject> opaqueDrawablesInViewFrustum)
        {

        }
        private void ApplyShadowMap(List<Light> sourceLights, Material material, ref List<Light> unusedLights)
        {
            var effectShadowMap = material.Find<IEffectShadowMap>();

            foreach (var light in sourceLights)
            {
                if (light.CastShadow && light.Enabled && light.ShadowMap != null)
                {
                    if (effectShadowMap != null)
                    {
                        effectShadowMap.ShadowMap = light.ShadowMap.Texture;
                        effectShadowMap.LightViewProjection= light.ShadowFrustum.Matrix;
                        effectShadowMap.DepthBias = Settings.ShadowMapDepthBias;
                        effectShadowMap = null;
                    }
                    else
                    {
                        if (unusedLights == null)
                            unusedLights = new List<Light>();
                        unusedLights.Add(light);
                    }
                }
            }
        }
#endif
        #endregion

        #region Fog
        private void UpdateFog()
        {
            if (!Settings.FogEnable)
                return;

            for (int currentFog = 0; currentFog < flattenedObjectsInViewFrustum.Count; currentFog++)
            {
                var firstFog = flattenedObjectsInViewFrustum[currentFog] as IEffectFog;
                if (firstFog != null)
                {
                    for (int currentDrawable = 0; currentDrawable < drawablesInViewFrustum.Count; currentDrawable++)
                    {
                        ApplyFog(firstFog, drawablesInViewFrustum[currentDrawable].Material);
                    }
                    break;
                }
            }
        }

        private void ApplyFog(IEffectFog sourceFog, Material material)
        {
            if (material != null)
            {
                IEffectFog target = material.Find<IEffectFog>();
                if (target != null)
                {
                    target.FogEnabled = sourceFog.FogEnabled;
                    if (sourceFog.FogEnabled)
                    {
                        target.FogColor = sourceFog.FogColor;
                        target.FogStart = sourceFog.FogStart;
                        target.FogEnd = sourceFog.FogEnd;
                    }
                }
            }
        }
        #endregion

        #region Material
        internal static bool IsTransparent(IDrawableObject drawable)
        {
            return drawable != null && drawable.Material != null && drawable.Material.IsTransparent;
        }

        internal static bool CastShadow(IDrawableObject drawable)
        {
            var lightable = drawable as ILightable;
            return lightable != null && lightable.CastShadow;
        }

        internal static bool ReceiveShadow(IDrawableObject drawable)
        {
            var lightable = drawable as ILightable;
            return lightable != null && lightable.ReceiveShadow;
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