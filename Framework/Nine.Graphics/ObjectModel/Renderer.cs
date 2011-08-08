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
using Nine.Graphics.Effects.Deferred;
#endregion

namespace Nine.Graphics.ObjectModel
{
    /// <summary>
    /// Defines a renderer that is used to render a list of object using the
    /// specified camera and light settings.
    /// </summary>
    public class Renderer : IDisposable, IDrawable
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
        /// Gets all the lights used by this renderer.
        /// </summary>
        public ISceneManager<Light> Lights { get; private set; }

        /// <summary>
        /// Gets a collection of drawables to be rendered by this renderer.
        /// </summary>
        public ISceneManager<Drawable> Drawables { get; private set; }

        /// <summary>
        /// Gets a collection of render passes used by this renderer.
        /// </summary>
        public ReadOnlyCollection<GraphicsPass> Passes { get; private set; }
        private List<GraphicsPass> passes = new List<GraphicsPass>();

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
        private ISceneManager<ISpatialQueryable> sceneManager;
        private List<Drawable> drawablesInViewFrustum = new List<Drawable>();
        private List<Light> lightsInViewFrustum = new List<Light>();
        private List<Light> appliedMultiPassLights = new List<Light>();
        private List<Light> unAppliedMultiPassLights = new List<Light>();
        private EffectInstance cachedEffectInstance;
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
        public Renderer(GraphicsDevice graphics) : this(graphics, null, null)
        {

        }

        /// <summary>
        /// Creates a new instance of <c>Renderer</c>.
        /// </summary>
        public Renderer(GraphicsDevice graphics, GraphicsSettings settings, ISceneManager<ISpatialQueryable> sceneManager)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            if (sceneManager == null)
                sceneManager = new OctreeSceneManager<ISpatialQueryable>();

            this.sceneManager = sceneManager;
            this.Lights = new SceneManager<ISpatialQueryable, Light>(sceneManager);
            this.Drawables = new SceneManager<ISpatialQueryable, Drawable>(sceneManager);

            this.GraphicsDevice = graphics;
            this.Settings = settings ?? new GraphicsSettings();
            this.Statistics = new GraphicsStatistics();
            this.GraphicsContext = GraphicsContext ?? new GraphicsContext(graphics, this.Settings);
            this.Passes = new ReadOnlyCollection<GraphicsPass>(passes);

            ((SceneManager<ISpatialQueryable, Drawable>)Drawables).Added += new EventHandler<NotifyCollectionChangedEventArgs<Drawable>>(Drawable_Added);
            ((SceneManager<ISpatialQueryable, Drawable>)Drawables).Removed += new EventHandler<NotifyCollectionChangedEventArgs<Drawable>>(Drawable_Removed);
        }

        internal void Add(object obj)
        {
            if (obj is Light)
                Lights.Add(obj as Light);
            else if (obj is Drawable)
                Drawables.Add(obj as Drawable);
        }

        internal void Remove(object obj)
        {
            if (obj is Light)
                Lights.Remove(obj as Light);
            else if (obj is Drawable)
                Drawables.Remove(obj as Drawable);
        }

        void Drawable_Added(object sender, NotifyCollectionChangedEventArgs<Drawable> e)
        {
            IEnumerable enumerable = e.Value as IEnumerable;
            if (enumerable != null)
            {
                ForEachEnumerable<object>(enumerable, child => 
                {
                    Transformable transformable = child as Transformable;
                    if (transformable != null)
                        transformable.Parent = enumerable as Transformable;
                    Add(child); 
                });

                INotifyCollectionChanged<object> collectionChanged = e.Value as INotifyCollectionChanged<object>;
                if (collectionChanged != null)
                {
                    collectionChanged.Added += DrawableChild_Added;
                    collectionChanged.Removed += DrawableChild_Removed;
                }
            }

            AddPass(e.Value);
        }

        void DrawableChild_Added(object sender, NotifyCollectionChangedEventArgs<object> e)
        {
            Add(e.Value);
        }

        void Drawable_Removed(object sender, NotifyCollectionChangedEventArgs<Drawable> e)
        {
            IEnumerable enumerable = e.Value as IEnumerable;
            if (enumerable != null)
            {
                ForEachEnumerable<object>(enumerable, child =>
                {
                    Transformable transformable = child as Transformable;
                    if (transformable != null)
                    {
                        if (transformable.Parent != enumerable as Drawable)
                        {
                            throw new InvalidOperationException("Cannot modify the IEnumerable when it is adde to a renderer");
                        }
                        transformable.Parent = null;
                    }
                    Remove(child);
                });

                INotifyCollectionChanged<object> collectionChanged = e.Value as INotifyCollectionChanged<object>;
                if (collectionChanged != null)
                {
                    collectionChanged.Added -= DrawableChild_Added;
                    collectionChanged.Removed -= DrawableChild_Removed;
                }
            }

            RemovePass(e.Value);
        }

        void DrawableChild_Removed(object sender, NotifyCollectionChangedEventArgs<object> e)
        {
            Remove(e.Value);
        }

        private void ForEachEnumerable<T>(IEnumerable enumerable, Action<T> action)
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

        private void AddPass(Drawable drawable)
        {
            if (drawable != null && drawable.Passes != null)
            {
                foreach (var passType in drawable.Passes)
                {
                    var existingPass = passes.FirstOrDefault(p => p.GetType() == passType);
                    if (existingPass == null)
                    {
                        var newPass = (GraphicsPass)Activator.CreateInstance(passType);
                        newPass.OnAdded(drawable);
                        newPass.SubscriberCount++;
                        AddPass(newPass, newPass);
                    }
                    else
                    {
                        existingPass.OnAdded(drawable);
                        existingPass.SubscriberCount++;
                    }
                }
            }
        }

        private void AddPass(GraphicsPass pass, GraphicsPass circularDependencyDetector)
        {
            if (pass == null)
                return;

            if (pass.Dependencies != null)
            {
                foreach (var passType in pass.Dependencies)
                {
                    var existingPass = passes.FirstOrDefault(p => p.GetType() == passType);
                    if (existingPass == null)
                    {
                        var newPass = (GraphicsPass)Activator.CreateInstance(passType);
                        newPass.ReferencingPassCount++;
                        AddPass(newPass, circularDependencyDetector);
                    }
                    else
                    {
                        if (existingPass == circularDependencyDetector)
                            throw new InvalidOperationException(Strings.CircularDependency);

                        existingPass.ReferencingPassCount++;
                    }
                    pass.dependentPasses.Add(existingPass);
                }
            }
            passes.Add(pass);
        }

        private void RemovePass(Drawable drawable)
        {
            if (drawable != null && drawable.Passes != null)
            {
                foreach (var passType in drawable.Passes)
                {
                    var existingPass = passes.FirstOrDefault(p => p.GetType() == passType);
                    if (existingPass == null)
                        throw new InvalidOperationException("Cannot modify Drawable.Passes or GraphicsPass.Dependencies.");

                    existingPass.OnRemoved(drawable);
                    existingPass.SubscriberCount--;
                    if (existingPass.SubscriberCount <= 0)
                    {
                        RemovePass(existingPass);
                    }
                }
            }
        }

        private void RemovePass(GraphicsPass pass)
        {
            if (pass == null)
                return;

            if (pass.Dependencies != null)
            {
                foreach (var passType in pass.Dependencies)
                {
                    var existingPass = passes.FirstOrDefault(p => p.GetType() == passType);
                    if (existingPass == null)
                        throw new InvalidOperationException("Cannot modify Drawable.Passes or GraphicsPass.Dependencies.");

                    existingPass.ReferencingPassCount--;
                    if (existingPass.ReferencingPassCount <= 0)
                    {
                        RemovePass(existingPass);
                    }
                }
            }
            pass.dependentPasses.Clear();
            passes.Remove(pass);
        }
        #endregion

        #region Draw
        /// <summary>
        /// Draws all the object managed by this renderer.
        /// </summary>
        public void Draw(TimeSpan elapsedTime)
        {
            Statistics.Reset();

            if (camera is IUpdateable)
            {
                ((IUpdateable)camera).Update(elapsedTime);
            }


            GraphicsContext.Lights = Lights;
            GraphicsContext.Drawables = Drawables;
            GraphicsContext.View = Camera.View;
            GraphicsContext.Projection = Camera.Projection;
            GraphicsContext.ElapsedTime = elapsedTime;


            // Find drawables and lights in the current view frustum
            drawablesInViewFrustum.Clear();
            lightsInViewFrustum.Clear();
            foreach (var obj in sceneManager.FindAll(GraphicsContext.ViewFrustum))
            {
                if (obj is Light)
                    lightsInViewFrustum.Add(obj as Light);
                else if (obj is Drawable)
                    drawablesInViewFrustum.Add(obj as Drawable);
            }
            Statistics.VisibleObjectCount = drawablesInViewFrustum.Count;
            Statistics.VisibleLightCount = lightsInViewFrustum.Count;


            // Update drawables
            drawablesInViewFrustum.ForEach(d => d.Update(elapsedTime));

            ClearLights(drawablesInViewFrustum);

            UpdateAffectedDrawablesAndAffectingLights(drawablesInViewFrustum, lightsInViewFrustum);
            
#if !WINDOWS_PHONE
            if (Settings.PreferDeferredLighting && GraphicsDevice.GraphicsProfile == GraphicsProfile.HiDef)
                DrawUsingDeferredLighting(drawablesInViewFrustum, lightsInViewFrustum, elapsedTime);
            else
#endif
                DrawUsingForwardLighting(drawablesInViewFrustum, lightsInViewFrustum, elapsedTime);


            GraphicsContext.Begin();

            DrawDebug(drawablesInViewFrustum, lightsInViewFrustum);

            foreach (var pass in passes)
            {
                pass.Draw(GraphicsContext);
            }

            GraphicsContext.End();
        }

        #region Deferred
#if !WINDOWS_PHONE
        private void DrawUsingDeferredLighting(List<Drawable> drawables, List<Light> lights, TimeSpan elapsedTime)
        {
            if (deferredEffect == null)
                deferredEffect = new DeferredEffect(GraphicsDevice);
            if (graphicsBuffer == null)
                graphicsBuffer = new GraphicsBuffer(GraphicsDevice);

            if (Settings.PreferHighDynamicRangeLighting)
                graphicsBuffer.LightBufferFormat = SurfaceFormat.HdrBlendable;
            else
                graphicsBuffer.LightBufferFormat = SurfaceFormat.Color;

            var lightables = drawables.Where(d => IsLightable(d));

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
                    if (IsLightable(d))
                        d.Draw(GraphicsContext, deferredEffect);
                    else
                        d.Draw(GraphicsContext);
                });
            GraphicsContext.End();

            // Draw non lightable objects

        }
#endif
        #endregion

        #region Forward
        private void DrawUsingForwardLighting(IList<Drawable> drawables, IList<Light> lights, TimeSpan elapsedTime)
        {
            GraphicsContext.Begin();

            foreach (var drawable in drawables)
            {
                IMaterial material = drawable as IMaterial;
                IEffectInstance materialEffect = material != null ? material.Effect : null;

                // Setup light info in drawable materials.
                if (IsLightable(drawable) && drawable.AffectingLights != null && materialEffect != null)
                {
                    if (drawable.MultiPassLights == null)
                        drawable.MultiPassLights = new List<Light>();
                    else
                        drawable.MultiPassLights.Clear();
                    ApplyLights(drawable.AffectingLights, materialEffect, light => drawable.MultiPassLights.Add(light));
                }

                drawable.Draw(GraphicsContext);
            }


            GraphicsContext.End();
            GraphicsContext.Begin(BlendState.Additive, null, DepthStencilState.DepthRead, null);

            // Multipass lighting
            foreach (var drawable in drawables)
            {
                IMaterial material = drawable as IMaterial;
                IEffectInstance materialEffect = material != null ? material.Effect : null;

                if (IsLightable(drawable) && drawable.AffectingLights != null && drawable.MultiPassLights != null &&
                    drawable.MultiPassLights.Count > 0 && materialEffect != null)
                {
                    unAppliedMultiPassLights.Clear();
                    unAppliedMultiPassLights.AddRange(drawable.MultiPassLights);

                    while (unAppliedMultiPassLights.Count > 0)
                    {
                        var effect = unAppliedMultiPassLights[0].Effect;
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

            GraphicsContext.End();
        }
        #endregion

        #region Debug
        private void DrawDebug(IEnumerable<Drawable> drawables, IEnumerable<Light> lights)
        {
            var settings = Settings.Debug;
            var primitiveBatch = GraphicsContext.PrimitiveBatch;
            var spriteBatch = GraphicsContext.SpriteBatch;

            if (settings.ShowBoundingBox)
            {
                drawables.ForEach(d => primitiveBatch.DrawBox(d.BoundingBox, null, settings.BoundingBoxColor));
                lights.ForEach(d => primitiveBatch.DrawBox(d.BoundingBox, null, settings.BoundingBoxColor));
            }
            if (settings.ShowLightFrustum)
            {
                lights.ForEach(d => d.DrawFrustum(GraphicsContext));
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
        private void UpdateAffectedDrawablesAndAffectingLights(IList<Drawable> drawables, IList<Light> lights)
        {
            // Clear affecting lights
            foreach (var obj in drawables)
            {
                if (obj.AffectingLights != null)
                    obj.AffectingLights.Clear();
            }

            // Setup lights and drawable relations.
            foreach (var light in lights)
            {
                if (light.AffectedDrawables == null)
                    light.AffectedDrawables = new List<Drawable>();
                light.AffectedDrawables.Clear();

                foreach (var obj in light.FindAffectedDrawables(Drawables, drawables))
                {
                    light.AffectedDrawables.Add(obj);
                    if (obj.AffectingLights == null)
                        obj.AffectingLights = new List<Light>();
                    obj.AffectingLights.Add(light);
                }
            }
        }

        private void ClearLights(IList<Drawable> drawables)
        {
            foreach (var drawable in drawables)
            {
                IMaterial material = drawable as IMaterial;
                IEffectInstance materialEffect = material != null ? material.Effect : null;

                if (materialEffect != null)
                    ClearLights(materialEffect);
            }
        }

        private void ClearLights(IEffectInstance effect)
        {
            if (effect == null)
                return;

            var ambientLights = effect.As<IEffectLights<IAmbientLight>>();
            if (ambientLights != null)
                ambientLights.Lights.ForEach(light => light.AmbientLightColor = Vector3.Zero);

            var directionalLights = effect.As<IEffectLights<IDirectionalLight>>();
            if (directionalLights != null)
                directionalLights.Lights.ForEach(light => light.DiffuseColor = Vector3.Zero);

            var pointLights = effect.As<IEffectLights<IPointLight>>();
            if (pointLights != null)
                pointLights.Lights.ForEach(light => light.DiffuseColor = Vector3.Zero);

            var spotLights = effect.As<IEffectLights<ISpotLight>>();
            if (spotLights != null)
                spotLights.Lights.ForEach(light => light.DiffuseColor = Vector3.Zero);
        }

        internal void ApplyLights(IList<Light> sourceLights, Effect effect, Action<Light> onFailed)
        {
            if (cachedEffectInstance == null)
                cachedEffectInstance = new EffectInstance(effect);
            else
                cachedEffectInstance.Effect = effect;
            ApplyLights(sourceLights, cachedEffectInstance, onFailed);
        }

        private void ApplyLights(IList<Light> sourceLights, IEffectInstance effect, Action<Light> onFailed)
        {
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
                if (!light.Apply(effect, iLight++, IsLastLightOfType(sourceLights, i)))
                {
                    if (onFailed != null)
                        onFailed(light);
                    continue;
                }

                bool failed = false;
                for (int j = i + 1; j < lightCount; j++)
                {
                    var light2 = sourceLights[j];
                    if (light2.GetType() != light.GetType())
                        continue;

                    lightUsed[j] = true;
                    if (failed || !light2.Apply(effect, iLight++, IsLastLightOfType(sourceLights, j)))
                    {
                        if (onFailed != null)
                            onFailed(light2);
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

        #region Find
        /// <summary>
        /// Finds all the visible drawable objects within the bounding frustum.
        /// </summary>
        private IEnumerable<Drawable> FindDrawables(ISpatialQuery<object> drawables, BoundingFrustum frustum)
        {
            Drawable drawable;
            foreach (var obj in drawables.FindAll(frustum))
            {
                drawable = obj as Drawable;
                if (drawable != null)
                    yield return drawable;
                IEnumerable enumerable = drawable as IEnumerable;
                if (enumerable != null)
                {
                    foreach (var child in enumerable)
                    {
                        drawable = obj as Drawable;
                        if (drawable != null)
                            yield return drawable;
                    }
                }
            }
        }

        /// <summary>
        /// Finds all the enabled lights within the bounding frustum.
        /// </summary>
        private IEnumerable<Light> FindLights(ISpatialQuery<object> drawables, BoundingFrustum frustum)
        {
            return drawables.FindAll(frustum).OfType<Light>().Where(light => light.Enabled).OrderBy(light => light.Order);
        }

        /// <summary>
        /// Finds all the affected objects within the frustum of the light.
        /// </summary>
        private IEnumerable<Drawable> FindAffectedDrawables(ISpatialQuery<object> drawables, Light light)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Material
        private bool IsTransparent(Drawable drawable)
        {
            IMaterial material = drawable as IMaterial;
            return material != null && material.IsTransparent;
        }

        private bool IsLightable(Drawable drawable)
        {
            ILightable lightable = drawable as ILightable;
            return lightable != null;
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