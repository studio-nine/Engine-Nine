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
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.ParticleEffects;
using Nine.Graphics.ScreenEffects;
#if !WINDOWS_PHONE
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
    public class Scene : ISceneManager<ISpatialQueryable>, IDisposable, IDrawable
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
        #endregion

        #region Fields
        private SceneManager<ISpatialQueryable, ISpatialQueryable> sceneManager;
        private SpatialQuery<object, IDrawableObject> shadowQuery;
        private SceneQuery<object> sceneQuery;

        private List<ISpatialQueryable> objectsInViewFrustum = new List<ISpatialQueryable>();
        private List<IDrawableObject> drawablesInViewFrustum = new List<IDrawableObject>();
        private List<IDrawableObject> opaqueDrawablesInViewFrustum = new List<IDrawableObject>();
        private List<IDrawableObject> transparentDrawablesInViewFrustum = new List<IDrawableObject>();
        
        private List<Light> lightsInViewFrustum = new List<Light>();
        private List<Light> appliedMultiPassLights = new List<Light>();
        private List<Light> unAppliedMultiPassLights = new List<Light>();
        
        private EffectMaterial cachedEffectMaterial;
        private BitArray lightUsed;

#if !WINDOWS_PHONE
        private GraphicsBuffer graphicsBuffer;
        private DeferredEffect deferredEffect;
#endif
        #endregion

        #region Initialization
        /// <summary>
        /// Creates a new instance of <c>Renderer</c>.
        /// </summary>
        public Scene(GraphicsDevice graphics) : this(graphics, null, null)
        {

        }

        /// <summary>
        /// Creates a new instance of <c>Renderer</c>.
        /// </summary>
        public Scene(GraphicsDevice graphics, GraphicsSettings settings, ISceneManager<ISpatialQueryable> sceneManager)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            if (sceneManager == null)
                sceneManager = new OctreeSceneManager<ISpatialQueryable>();

            this.sceneQuery = new SceneQuery<object>(sceneManager);
            this.sceneManager = new SceneManager<ISpatialQueryable, ISpatialQueryable>(sceneManager);
            this.sceneManager.Added += OnAdded;
            this.sceneManager.Removed += OnRemoved;

            this.GraphicsDevice = graphics;
            this.Settings = settings ?? new GraphicsSettings();
            this.Statistics = new GraphicsStatistics();
            this.GraphicsContext = GraphicsContext ?? new GraphicsContext(graphics, Settings, Statistics);
        }
        #endregion

        #region Scene Manager
        /// <summary>
        /// Adds a new item to the scene.
        /// </summary>
        public void Add(ISpatialQueryable item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            sceneManager.Add(item);
        }

        /// <summary>
        /// Removes an item from the scene.
        /// </summary>
        public bool Remove(ISpatialQueryable item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            return sceneManager.Remove(item);
        }

        protected virtual void OnAdded(object sender, NotifyCollectionChangedEventArgs<ISpatialQueryable> e)
        {
            IEnumerable enumerable = e.Value as IEnumerable;
            if (enumerable != null)
            {
                ForEachInEnumerable<object>(enumerable, child =>
                {
                    Transformable transformable = child as Transformable;
                    if (transformable != null)
                        transformable.Parent = enumerable as Transformable;
                    ISpatialQueryable queryable = child as ISpatialQueryable;
                    if (queryable != null)
                        Add(queryable);
                });

                var collectionChanged = e.Value as INotifyCollectionChanged<object>;
                if (collectionChanged != null)
                {
                    collectionChanged.Added += OnChildAdded;
                    collectionChanged.Removed += OnChildRemoved;
                }
            }
        }

        protected virtual void OnRemoved(object sender, NotifyCollectionChangedEventArgs<ISpatialQueryable> e)
        {
            IEnumerable enumerable = e.Value as IEnumerable;
            if (enumerable != null)
            {
                ForEachInEnumerable<object>(enumerable, child =>
                {
                    Transformable transformable = child as Transformable;
                    if (transformable != null)
                    {
                        if (transformable.Parent != enumerable as Transformable)
                        {
                            throw new InvalidOperationException(
                                "Cannot modify the IEnumerable when it is added to a scene, " +
                                "or implement INotifyCollectionChanged<object> if the modification is intended.");
                        }
                        transformable.Parent = null;
                    }
                    ISpatialQueryable queryable = child as ISpatialQueryable;
                    if (queryable != null)
                        Remove(queryable);
                });

                var collectionChanged = e.Value as INotifyCollectionChanged<object>;
                if (collectionChanged != null)
                {
                    collectionChanged.Added -= OnChildAdded;
                    collectionChanged.Removed -= OnChildRemoved;
                }
            }
        }

        private void OnChildAdded(object sender, NotifyCollectionChangedEventArgs<object> e)
        {
            var queryable = e.Value as ISpatialQueryable;
            if (queryable != null)
                Add(queryable);
        }

        private void OnChildRemoved(object sender, NotifyCollectionChangedEventArgs<object> e)
        {
            var queryable = e.Value as ISpatialQueryable;
            if (queryable != null)
                Remove(queryable);
        }

        private void ForEachInEnumerable<T>(IEnumerable enumerable, Action<T> action)
        {
            if (enumerable != null)
            {
                foreach (var child in enumerable)
                {
                    if (child is T)
                        action((T)child);
                }
            }
        }

        private void ForEachInSpatialQueryable<T>(ISpatialQueryable queryable, Action<T> action) where T : class
        {
            if (queryable is T)
                action((T)queryable);

            var enumerable = queryable as IEnumerable;
            if (enumerable != null)
            {
                foreach (var child in SceneQuery<T>.Enumerate(enumerable))
                {
                    action((T)child);
                }
            }
        }

        /// <summary>
        /// Clears all the scene objects.
        /// </summary>
        public void Clear()
        {
            sceneManager.Clear();
        }

        /// <summary>
        /// Gets whether the scene contains the target item.
        /// </summary>
        public bool Contains(ISpatialQueryable item)
        {
            return sceneManager.Contains(item);
        }

        void ICollection<ISpatialQueryable>.CopyTo(ISpatialQueryable[] array, int arrayIndex)
        {
            sceneManager.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of objects managed by this scene.
        /// </summary>
        public int Count 
        {
            get { return sceneManager.Count; } 
        }

        bool ICollection<ISpatialQueryable>.IsReadOnly
        {
            get { return sceneManager.IsReadOnly; } 
        }

        IEnumerator<ISpatialQueryable> IEnumerable<ISpatialQueryable>.GetEnumerator()
        {
            return sceneManager.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return sceneManager.GetEnumerator();
        }

        public IEnumerable<ISpatialQueryable> FindAll(Vector3 position, float radius)
        {
            return sceneManager.FindAll(position, radius);
        }

        public IEnumerable<ISpatialQueryable> FindAll(Ray ray)
        {
            return sceneManager.FindAll(ray);
        }

        public IEnumerable<ISpatialQueryable> FindAll(BoundingBox boundingBox)
        {
            return sceneManager.FindAll(boundingBox);
        }

        public IEnumerable<ISpatialQueryable> FindAll(BoundingFrustum frustum)
        {
            return sceneManager.FindAll(frustum);
        }

        public IEnumerable<T> FindAll<T>(Vector3 position, float radius)
        {
            return sceneQuery.FindAll(position, radius).OfType<T>();
        }

        public IEnumerable<T> FindAll<T>(Ray ray)
        {
            return sceneQuery.FindAll(ray).OfType<T>();
        }

        public IEnumerable<T> FindAll<T>(BoundingBox boundingBox)
        {
            return sceneQuery.FindAll(boundingBox).OfType<T>();
        }

        public IEnumerable<T> FindAll<T>(BoundingFrustum frustum)
        {
            return sceneQuery.FindAll(frustum).OfType<T>();
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

            if (Settings.FogEnable)
            {
                UpdateFog();
            }
            
#if !WINDOWS_PHONE
            if (Settings.ShadowEnabled)
            {
                UpdateAffectedDrawablesAndAffectingLights(drawablesInViewFrustum, lightsInViewFrustum);

                DrawShadowMaps();
            }
#endif

            if (Settings.LightingEnabled)
            {
#if WINDOWS_PHONE
                ClearLights(drawablesInViewFrustum);
                UpdateAffectedDrawablesAndAffectingLights(drawablesInViewFrustum, lightsInViewFrustum);

                GraphicsContext.Begin();
                DrawUsingForwardLighting(opaqueDrawablesInViewFrustum);
                GraphicsContext.End();
                
                GraphicsContext.Begin(BlendState.Additive, null, DepthStencilState.DepthRead, null);
                DrawUsingForwardLighting(transparentDrawablesInViewFrustum);
                GraphicsContext.End();
#else
                {
                    ClearLights(opaqueDrawablesInViewFrustum);

                    if (!Settings.ShadowEnabled)
                    {
                        UpdateAffectedDrawablesAndAffectingLights(opaqueDrawablesInViewFrustum, lightsInViewFrustum);
                    }

                    GraphicsContext.Begin();
                    DrawUsingForwardLighting(opaqueDrawablesInViewFrustum);
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
                }

                ClearLights(transparentDrawablesInViewFrustum);
                if (!Settings.ShadowEnabled)
                    UpdateAffectedDrawablesAndAffectingLights(transparentDrawablesInViewFrustum, lightsInViewFrustum);

                GraphicsContext.Begin(BlendState.AlphaBlend, null, null, null);
                DrawUsingForwardLighting(transparentDrawablesInViewFrustum);
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
            objectsInViewFrustum.Clear();
            drawablesInViewFrustum.Clear();
            opaqueDrawablesInViewFrustum.Clear();
            transparentDrawablesInViewFrustum.Clear();
            lightsInViewFrustum.Clear();

            foreach (var obj in sceneQuery.FindAll(GraphicsContext.ViewFrustum))
            {
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
                    drawablesInViewFrustum.Add(drawable);
                }

                var boundable = obj as ISpatialQueryable;
                if (boundable != null)
                {
                    objectsInViewFrustum.Add(boundable);
                }
            }

            Statistics.VisibleDrawableCount = drawablesInViewFrustum.Count;
            Statistics.VisibleObjectCount = objectsInViewFrustum.Count;
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

            foreach (var up in sceneQuery.OfType<IUpdateable>())
                up.Update(elapsedTime);
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
            drawables.ForEach(d => d.Draw(GraphicsContext));
        }

        #region Deferred
#if !WINDOWS_PHONE
        private void DrawUsingDeferredLighting(List<IDrawableObject> drawables, List<Light> lights)
        {
            if (deferredEffect == null)
                deferredEffect = new DeferredEffect(GraphicsDevice);
            if (graphicsBuffer == null)
                graphicsBuffer = new GraphicsBuffer(GraphicsDevice);

            if (Settings.PreferHighDynamicRangeLighting)
                graphicsBuffer.LightBufferFormat = SurfaceFormat.HdrBlendable;
            else
                graphicsBuffer.LightBufferFormat = SurfaceFormat.Color;

            var lightables = drawables.Where(d => d is ILightable);

            // Draw deferred scene with DepthNormalEffect first.
            graphicsBuffer.Begin();
                GraphicsContext.Begin();
                    lightables.ForEach(d => d.Draw(GraphicsContext, graphicsBuffer.Effect));
                GraphicsContext.End();
            graphicsBuffer.End();
            
            // Draw all the lights
            graphicsBuffer.DrawLights(GraphicsContext.View, GraphicsContext.Projection, lights.OfType<IDeferredLight>());
            deferredEffect.LightTexture = graphicsBuffer.LightBuffer;            

            GraphicsDevice.Clear(Color.DarkSlateGray);

            // 3. Draw the scene using DeferredEffect.
            GraphicsContext.Begin();
                drawables.ForEach(d =>
                {
                    if (d is ILightable)
                        d.Draw(GraphicsContext, deferredEffect);
                    else
                        d.Draw(GraphicsContext);
                });
            GraphicsContext.End();
        }
#endif
        #endregion

        #region Forward
        private void DrawUsingForwardLighting(IList<IDrawableObject> drawables)
        {
            foreach (var drawable in drawables)
            {
                var material = drawable.Material;
                var lightable = drawable as ILightable;

                // Setup light info in drawable materials.
                if (lightable != null && lightable.LightingEnabled && material != null)
                {
                    var lightingData = lightable.LightingData as LightingData;
                    if (lightingData != null)
                    {
                        if (lightingData.MultiPassLights != null)
                            lightingData.MultiPassLights.Clear();

                        ApplyLights(lightingData.AffectingLights, material, light =>
                        {
                            if (lightingData.MultiPassLights == null)
                                lightingData.MultiPassLights = new List<Light>();
                            lightingData.MultiPassLights.Add(light);
                        });

#if !WINDOWS_PHONE
                        // Setup shadow info in drawable materials.
                        if (lightable.ReceiveShadow)
                        {
                            if (lightingData.MultiPassShadows != null)
                                lightingData.MultiPassShadows.Clear();

                            ApplyShadowMap(lightingData.AffectingLights, material, light =>
                            {
                                if (lightingData.MultiPassShadows == null)
                                    lightingData.MultiPassShadows = new List<Light>();
                                lightingData.MultiPassShadows.Add(light);
                            });
                        }     
#endif
                    }
                }
                drawable.Draw(GraphicsContext);
            }
        }

        private void DrawMultiPassLightingOverlay(IList<IDrawableObject> drawables)
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
                            ApplyLights(unAppliedMultiPassLights, effect, light => appliedMultiPassLights.Add(light));
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
        private void DrawDebug(IEnumerable<IDrawableObject> drawables, IEnumerable<Light> lights)
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
                sceneManager.ForEach(d => primitiveBatch.DrawBox(d.BoundingBox, null, settings.BoundingBoxColor));
            }
            if (settings.ShowLightFrustum)
            {
                lights.ForEach(d => 
                { 
                    d.DrawFrustum(GraphicsContext);
                    if (d.ShadowFrustum.Matrix.M44 != 0)
                        primitiveBatch.DrawFrustum(d.ShadowFrustum, null, settings.ShadowFrustumColor);
                });
            }
            if (settings.ShowSceneManager)
            {
                DrawSceneManager(sceneManager);
            }
#if !WINDOWS_PHONE
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
        private void UpdateAffectedDrawablesAndAffectingLights(IList<IDrawableObject> drawables, IList<Light> lights)
        {
            // Clear affecting lights
            foreach (var obj in drawables)
            {
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
            foreach (var light in lights)
            {
                if (!light.Enabled)
                    continue;

                if (light.AffectedDrawables == null)
                    light.AffectedDrawables = new List<IDrawableObject>();
                light.AffectedDrawables.Clear();
                
                if (light.AffectedBoundables == null)
                    light.AffectedBoundables = new List<ISpatialQueryable>();
                light.AffectedBoundables.Clear();

                foreach (var obj in light.Find(sceneManager, objectsInViewFrustum))
                {
                    bool hasChildDrawable = false;
                    ForEachInSpatialQueryable<IDrawableObject>(obj, drawable =>
                    {
                        var lightable = drawable as ILightable;
                        if (drawable.Visible && lightable != null && lightable.LightingEnabled)
                        {
                            var lightingData = lightable.LightingData as LightingData;
                            if (lightingData == null)
                                lightable.LightingData = (lightingData = new LightingData());
                            if (lightingData.AffectingLights == null)
                                lightingData.AffectingLights = new List<Light>();
                            lightingData.AffectingLights.Add(light);
                            light.AffectedDrawables.Add(drawable);
                            hasChildDrawable = true;
                        }
                    });

                    if (hasChildDrawable)
                    {
                        light.AffectedBoundables.Add(obj);
                    }
                }
            }
        }

        private void ClearLights(IList<IDrawableObject> drawables)
        {
            foreach (var drawable in drawables)
            {
                if (drawable.Material != null)
                    ClearLights(drawable.Material);
            }
        }

        private void ClearLights(Material material)
        {
            if (material == null)
                return;

            var ambientLights = material.As<IEffectLights<IAmbientLight>>();
            if (ambientLights != null)
                ambientLights.Lights.ForEach(light => light.AmbientLightColor = Vector3.Zero);

            var directionalLights = material.As<IEffectLights<IDirectionalLight>>();
            if (directionalLights != null)
                directionalLights.Lights.ForEach(light => light.DiffuseColor = Vector3.Zero);

            var pointLights = material.As<IEffectLights<IPointLight>>();
            if (pointLights != null)
                pointLights.Lights.ForEach(light => light.DiffuseColor = Vector3.Zero);

            var spotLights = material.As<IEffectLights<ISpotLight>>();
            if (spotLights != null)
                spotLights.Lights.ForEach(light => light.DiffuseColor = Vector3.Zero);
        }

        private void ApplyLights(IList<Light> sourceLights, Effect effect, Action<Light> onLightNotUsed)
        {
            if (cachedEffectMaterial == null)
                cachedEffectMaterial = new EffectMaterial();
            cachedEffectMaterial.SetEffect(effect);
            ApplyLights(sourceLights, cachedEffectMaterial, onLightNotUsed);
        }

        private void ApplyLights(IList<Light> sourceLights, Material material, Action<Light> onLightNotUsed)
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
                    if (onLightNotUsed != null)
                        onLightNotUsed(light);
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
                        if (onLightNotUsed != null)
                            onLightNotUsed(light2);
                        failed = true;
                    }
                }
            }
        }

        private bool IsLastLightOfType(IList<Light> sourceLights, int i)
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
        private void DrawShadowMaps()
        {
            lightsInViewFrustum.ForEach(light =>
            {
                if (light.Enabled && light.CastShadow)
                {
                    if (shadowQuery == null)
                    {
                        shadowQuery = new SpatialQuery<object, IDrawableObject>(sceneQuery);
                        shadowQuery.Filter = obj =>
                        {
                            var drawable = obj as IDrawableObject;
                            return drawable != null && drawable.Visible && CastShadow(drawable);
                        };
                    }

                    light.DrawShadowMap(GraphicsContext, shadowQuery,
                                        light.AffectedBoundables.Where(boundable => CastShadow(boundable) || ReceiveShadow(boundable)),
                                        objectsInViewFrustum.Where(boundable => CastShadow(boundable) || ReceiveShadow(boundable)));
                }
            });
        }

        private void DrawMultiPassShadowMapOverlay(IList<IDrawableObject> opaqueDrawablesInViewFrustum)
        {

        }

#if !WINDOWS_PHONE
        private void ApplyShadowMap(IList<Light> sourceLights, Material material, Action<Light> onLightNotUsed)
        {
            var effectShadowMap = material.As<IEffectShadowMap>();

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
                    else if (onLightNotUsed != null)
                    {
                        onLightNotUsed(light);
                    }
                }
            }
        }
#endif
        #endregion

        #region Fog
        private void UpdateFog()
        {
            var firstFog = objectsInViewFrustum.OfType<IEffectFog>().FirstOrDefault();
            if (firstFog != null)
            {
                foreach (var drawable in drawablesInViewFrustum)
                {
                    ApplyFog(firstFog, drawable.Material);
                }
            }
        }

        private void ApplyFog(IEffectFog sourceFog, Material material)
        {
            if (material != null)
            {
                IEffectFog target = material.As<IEffectFog>();
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
        private bool IsTransparent(IDrawableObject drawable)
        {
            return drawable != null && drawable.Material != null && drawable.Material.IsTransparent;
        }

        private bool CastShadow(IDrawableObject drawable)
        {
            var lightable = drawable as ILightable;
            return lightable != null && lightable.CastShadow;
        }

        private bool ReceiveShadow(IDrawableObject drawable)
        {
            var lightable = drawable as ILightable;
            return lightable != null && lightable.ReceiveShadow;
        }

        private bool CastShadow(ISpatialQueryable boundable)
        {
            bool castShadow = false;
            ForEachInSpatialQueryable<IDrawableObject>(boundable, drawable => castShadow |= CastShadow(drawable));
            return castShadow;
        }

        private bool ReceiveShadow(ISpatialQueryable boundable)
        {
            bool receiveShadow = false;
            ForEachInSpatialQueryable<IDrawableObject>(boundable, drawable => receiveShadow |= ReceiveShadow(drawable));
            return receiveShadow;
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) { }
        #endregion
    }
}