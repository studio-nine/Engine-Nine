#region Copyright 2009 - 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.Materials;
using Nine.Graphics.ObjectModel;
using Nine.Graphics.Primitives;
using Nine.Graphics.Drawing;
#endregion

namespace Nine.Graphics.ParticleEffects
{
    /// <summary>
    /// Defines how each particle should be rendered.
    /// </summary>
    public enum ParticleType
    {
        /// <summary>
        /// The particle will be rendered as 3D billboard that always faces the camera.
        /// </summary>
        Billboard,

        /// <summary>
        /// The particle will be rendered as 3D constrained billboard that is constrained
        /// by the forward moving axis while still faces the camera.
        /// </summary>
        ConstrainedBillboard,

        /// <summary>
        /// The particle will be rendered as 3D constrained billboard that is constrained
        /// by the specified axis while still faces the camera.
        /// </summary>
        ConstrainedBillboardUp,
    }

    /// <summary>
    /// Action for particles.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public delegate void ParticleAction(ref Particle particle);

    /// <summary>
    /// Defines a special visual effect made up of particles.
    /// </summary>
    [ContentSerializable]
    public class ParticleEffect : Transformable, ISpatialQueryable, IDrawableObject, IUpdateable, IDisposable
    {
        #region Properties
        /// <summary>
        /// Gets the graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets or sets the type of each particle.
        /// </summary>
        public ParticleType ParticleType
        {
            get { return particleType; }
            set { particleType = value; }
        }
        private ParticleType particleType;

        /// <summary>
        /// Gets whether this object is visible.
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Gets a value indicating whether this primitive resides inside the view frustum last frame.
        /// </summary>
        public bool InsideViewFrustum { get; internal set; }

        /// <summary>
        /// Gets or sets whether this particle effect is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets a scale factor along the forward axis when drawing this
        /// particle effect using constrained billboard.
        /// </summary>
        public float Stretch { get; set; }

        /// <summary>
        /// Gets or sets the texture used by this particle effect.
        /// </summary>
        public Texture2D Texture { get; set; }

        /// <summary>
        /// Gets or sets the source rectangle in the texture.
        /// </summary>
        public Rectangle? SourceRectangle { get; set; }

        /// <summary>
        /// Gets or sets whether each particles with be blended using additive blending.
        /// </summary>
        public bool IsAdditive
        {
            get { return isAdditive; }
            set
            {
                if (isAdditive != value)
                {
                    isAdditive = value;
                    UpdateMaterial();
                }
            }
        }
        private bool isAdditive = true;

        /// <summary>
        /// Gets or sets a value indicating whether particles should softly blends with other opaque scene objects.
        /// </summary>
        public bool SoftParticleEnabled
        {
            get { return softParticleEnabled; }
            set
            {
                if (softParticleEnabled != value)
                {
                    softParticleEnabled = value;
                    UpdateMaterial();
                }
            }
        }
        private bool softParticleEnabled;

        /// <summary>
        /// Gets or sets a fade factor whether when soft particle is enabled.
        /// </summary>
        public float SoftParticleFade
        {
            get { return softParticleFade; }
            set
            {
                softParticleFade = value;
                SoftParticleMaterial spm = material as SoftParticleMaterial;
                if (spm != null)
                    spm.DepthFade = softParticleFade;
            }
        }
        private float softParticleFade = MaterialConstants.SoftParticleFade;

        /// <summary>
        /// Gets or sets a value indicating whether this particle effect will be updated asynchronously.
        /// </summary>
        public bool IsAsync
        {
            get { return isAsync; }
            set 
            {
                // TODO: You should never modify this value after the particle system is updated or drawed.
                isAsync = value;
            }
        }
        private bool isAsync = true;

        /// <summary>
        /// Gets the absolute position of this particle effect.
        /// </summary>
        public Vector3 AbsolutePosition
        {
            get { return AbsoluteTransform.Translation; }
        }

        /// <summary>
        /// Gets the axis aligned bounding box in world space.
        /// </summary>
        public BoundingBox BoundingBox
        {
            get { return boundingBox; }
        }
        private BoundingBox boundingBox;

        /// <summary>
        /// Gets or sets the data used for spatial query.
        /// </summary>
        object ISpatialQueryable.SpatialData { get; set; }

        /// <summary>
        /// Gets the material of the object.
        /// </summary>
        Material IDrawableObject.Material
        {
            get { return material; }
        }
        private Material material;

        /// <summary>
        /// Gets or sets the emitter prototype of this particle effect.
        /// </summary>
        public IParticleEmitter Emitter
        {
            get { return emitter; }
            set
            {
                if (value != emitter)
                {
                    emitter = (value ?? new PointEmitter());
                    OnEmitterChanged();
                }
            }
        }
        IParticleEmitter emitter;

        /// <summary>
        /// Gets a collection of controllers that defines the visual of this particle effect.
        /// </summary>
        public ParticleControllerCollection Controllers { get; private set; }

        /// <summary>
        /// Gets the approximate particle count.
        /// </summary>
        public int ParticleCount { get; private set; }
        #endregion

        #region Events
        /// <summary>
        /// Occurs when a particle is about to die.
        /// </summary>
        public event ParticleAction ParticleEmitted;

        /// <summary>
        /// Occurs when a particle is about to die.
        /// </summary>
        public event ParticleAction ParticleRetired;

        /// <summary>
        /// Occurs when the bounding box changed.
        /// </summary>
        public event EventHandler<EventArgs> BoundingBoxChanged;
        #endregion

        #region Fields
        // An array of particles, treated as a circular queue.
        internal int MaxParticleCount = 0;
        internal int CurrentParticle = 0;

        private Particle[] particles;
        private int firstParticle = 0;
        private int lastParticle = 0;

        private int CurrentController = 0;
        private Random random = new Random();

        private PrimitiveGroup primitive;
        private float elapsedSeconds;
        private Vector3 eyePosition;
        private Matrix viewInverse;

        private int toBeRemoved;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ParticleEffect"/> class.
        /// </summary>
        public ParticleEffect(GraphicsDevice graphics)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            this.Enabled = true;
            this.Visible = true;
            this.Stretch = 1;
            this.SoftParticleEnabled = false;
            this.GraphicsDevice = graphics;
            this.Emitter = new PointEmitter();

            this.Controllers = new ParticleControllerCollection();
            this.Controllers.ParticleEffect = this;
        }
        #endregion

        #region Methods
        private void EnsureParticlesInitialized()
        {
            if (particles == null)
            {
                this.MaxParticleCount = EstimateMaxParticleCount();
                this.particles = new Particle[MaxParticleCount];
                this.primitive = new PrimitiveGroup(GraphicsDevice, MaxParticleCount * 6, 32768);
                UpdateMaterial();
            }
        }

        private void UpdateMaterial()
        {
#if !WINDOWS_PHONE
            if (softParticleEnabled)
                material = new SoftParticleMaterial(GraphicsDevice) { Texture = Texture, DepthFade = softParticleFade, IsTransparent = true, IsAdditive = isAdditive };
            else
#endif
                material = new BasicMaterial(GraphicsDevice) { Texture = Texture, LightingEnabled = false, VertexColorEnabled = true, IsTransparent = true, IsAdditive = isAdditive };
        }

        /// <summary>
        /// Estimates the max particle count base on emitter properties.
        /// </summary>
        private int EstimateMaxParticleCount()
        {
            var particleEmitter = emitter as ParticleEmitter;
            if (particleEmitter == null)
                return 32;
            return UtilityExtensions.UpperPowerOfTwo((int)(
                particleEmitter.Emission * (particleEmitter.Duration.Max + particleEmitter.Duration.Min) * 0.8f));
        }

        /// <summary>
        /// Called when local or absolute transform changed.
        /// </summary>
        protected override void OnTransformChanged()
        {
            var absoluteTransform = AbsoluteTransform;
            var absolutePosition = absoluteTransform.Translation;

            emitter.Position = absolutePosition;
            emitter.Direction = absoluteTransform.Forward;

            var emitterBounds = emitter.BoundingBox;
            boundingBox.Min = emitterBounds.Min + absolutePosition;
            boundingBox.Max = emitterBounds.Max + absolutePosition;

            if (BoundingBoxChanged != null)
                BoundingBoxChanged(this, EventArgs.Empty);
        }

        private void OnEmitterChanged()
        {
            OnTransformChanged();
        }
        #endregion

        #region Update
        /// <summary>
        /// Traverses all active particles.
        /// </summary>
        public void ForEach(ParticleAction action)
        {
            try
            {
                if (IsAsync)
                    canDraw.WaitOne();

                if (ParticleCount > 0)
                {
                    if (firstParticle < lastParticle)
                    {
                        // ParticleConstroller<T>.Update requires the CurrentParticle to be the correct index.
                        for (CurrentParticle = firstParticle; CurrentParticle < lastParticle; CurrentParticle++)
                            if (particles[CurrentParticle].Age <= 1)
                                action(ref particles[CurrentParticle]);
                    }
                    else
                    {
                        // UpdateParticles requires the enumeration to always start from firstParticle.
                        for (CurrentParticle = firstParticle; CurrentParticle < MaxParticleCount; CurrentParticle++)
                            if (particles[CurrentParticle].Age <= 1)
                                action(ref particles[CurrentParticle]);
                        for (CurrentParticle = 0; CurrentParticle < lastParticle; CurrentParticle++)
                            if (particles[CurrentParticle].Age <= 1)
                                action(ref particles[CurrentParticle]);
                    }
                }
            }
            finally
            {
                if (isAsync)
                    canUpdate.Set();
            }
        }

        /// <summary>
        /// Updates the particle system.
        /// </summary>
        public void Update(TimeSpan elapsedTime)
        {
            if (toBeRemoved > 0)
            {
                // Remove this particle system from the parent container
                var container = Parent as Nine.Graphics.ObjectModel.IContainer;
                if (container != null && container.Children != null)
                    container.Children.Remove(this);
                return;
            }

            EnsureParticlesInitialized();
            elapsedSeconds = (float)elapsedTime.TotalSeconds;

            numFramesBehind++;

            if (!isAsync)
                Update();

            InsideViewFrustum = false;
        }

        /// <summary>
        /// Updates using the elapsed time saved from last frame.
        /// </summary>
        private void Update()
        {
            try
            {
                if (isAsync)
                    canUpdate.WaitOne();

                var dt = Math.Min(1.0f / 30, elapsedSeconds);

                UpdateEmitter(dt);
                UpdateParticles(dt);
                RetireParticles();
                UpdateParticlePrimitive();
            }
            finally
            {
                if (isAsync)
                    canDraw.Set();
            }
        }

        private void UpdateParticles(float elapsedSeconds)
        {
            if (ParticleCount > 0)
            {
                if (firstParticle < lastParticle)
                {
                    // ParticleConstroller<T>.Update requires the CurrentParticle to be the correct index.
                    for (CurrentParticle = firstParticle; CurrentParticle < lastParticle; CurrentParticle++)
                        UpdateParticle(elapsedSeconds, ref particles[CurrentParticle]);
                }
                else
                {
                    // UpdateParticles requires the enumeration to always start from firstParticle.
                    for (CurrentParticle = firstParticle; CurrentParticle < MaxParticleCount; CurrentParticle++)
                        UpdateParticle(elapsedSeconds, ref particles[CurrentParticle]);
                    for (CurrentParticle = 0; CurrentParticle < lastParticle; CurrentParticle++)
                        UpdateParticle(elapsedSeconds, ref particles[CurrentParticle]);
                }
            }
        }

        private void UpdateParticle(float elapsedSeconds, ref Particle particle)
        {
            for (CurrentController = 0; CurrentController < Controllers.Count; CurrentController++)
                Controllers[CurrentController].Update(elapsedSeconds, ref particles[CurrentParticle]);

            particle.Update(elapsedSeconds);

            if (particle.Age > 1 && particle.Age < float.MaxValue)
            {
                if (ParticleRetired != null)
                    ParticleRetired(ref particle);
                particle.Age = float.MaxValue;
            }
        }

        private void UpdateEmitter(float elapsedSeconds)
        {
            if (Emitter != null && Emitter.Update(this, elapsedSeconds))
                Interlocked.Exchange(ref toBeRemoved, 1);
        }

        /// <summary>
        /// Emits a new particle.
        /// </summary>
        public void Emit(ref Particle particle)
        {
            CurrentParticle = lastParticle;

            particle.Age = 0;
            particle.ElapsedTime = 0;

            particles[CurrentParticle] = particle;

            for (int currentController = 0; currentController < Controllers.Count; currentController++)
            {
                Controllers[currentController].Reset(ref particle);
            }

            ParticleCount++;

            if (ParticleEmitted != null)
                ParticleEmitted(ref particles[CurrentParticle]);

            // Expand storage when the queue is full.
            if (ParticleCount >= MaxParticleCount)
            {
                var currentLength = MaxParticleCount;
                Array.Resize(ref particles, MaxParticleCount = MaxParticleCount * 2);
                if (lastParticle < firstParticle)
                {
                    currentLength -= firstParticle;
                    Array.Copy(particles, firstParticle, particles, MaxParticleCount - currentLength, currentLength);
                    firstParticle = MaxParticleCount - currentLength;
                }
            }

            lastParticle = (lastParticle + 1) % MaxParticleCount;
        }
        
        private void RetireParticles()
        {
            while (ParticleCount > 0 && particles[firstParticle].Age > 1)
            {
                firstParticle = (firstParticle + 1) % MaxParticleCount;
                ParticleCount--;
            }
        }

        private void UpdateParticlePrimitive()
        {
            primitive.Clear();

            if (ParticleCount > 0 && Texture != null)
            {
                if (firstParticle < lastParticle)
                {
                    // ParticleConstroller<T>.Update requires the CurrentParticle to be the correct index.
                    for (CurrentParticle = firstParticle; CurrentParticle < lastParticle; CurrentParticle++)
                        AddParticlePrimitive(ref particles[CurrentParticle]);
                }
                else
                {
                    // UpdateParticles requires the enumeration to always start from firstParticle.
                    for (CurrentParticle = firstParticle; CurrentParticle < MaxParticleCount; CurrentParticle++)
                        AddParticlePrimitive(ref particles[CurrentParticle]);
                    for (CurrentParticle = 0; CurrentParticle < lastParticle; CurrentParticle++)
                        AddParticlePrimitive(ref particles[CurrentParticle]);
                }
            }
        }

        private void AddParticlePrimitive(ref Particle particle)
        {
            if (particle.Age <= 1)
            {
                if (ParticleType == ParticleType.Billboard)
                {
                    primitive.AddBillboard(Texture,
                             ref particle.Position,
                             particle.Size,
                             particle.Size,
                             particle.Rotation,
                             null, null,
                             particle.Color * particle.Alpha, ref viewInverse);
                }
                else if (ParticleType == ParticleType.ConstrainedBillboard)
                {
                    Vector3 forward = Vector3.Normalize(particle.Velocity);
                    forward *= 0.5f * particle.Size * Stretch * Texture.Width / Texture.Height;

                    primitive.AddConstrainedBillboard(Texture,
                                                particle.Position - forward,
                                                particle.Position + forward,
                                                particle.Size,
                                                null, null,
                                                particle.Color * particle.Alpha, eyePosition);
                }
                else if (ParticleType == ParticleType.ConstrainedBillboardUp)
                {
                    Vector3 forward = 0.5f * Vector3.Up * particle.Size * Stretch * Texture.Width / Texture.Height;

                    primitive.AddConstrainedBillboard(Texture,
                                                particle.Position - forward,
                                                particle.Position + forward,
                                                particle.Size,
                                                null, null,
                                                particle.Color * particle.Alpha, eyePosition);
                }
            }
        }
        #endregion

        #region Draw
        public void BeginDraw(DrawingContext context)
        {
            InsideViewFrustum = true;
        }

        public void Draw(DrawingContext context, Material material)
        {
            // Particle effect does not support other rendering modes.
            if (material != this.material)
                return;
            
            EnsureParticlesInitialized();

            try
            {
                if (isAsync)
                    canDraw.WaitOne();

                eyePosition = context.EyePosition;
                viewInverse = context.matrices.viewInverse;
                primitive.Draw(context, material);
            }
            finally
            {
                if (isAsync)
                {
                    // Once this particle effect is drawed, we start the update
                    // asychroniously to maximize parallelism.

                    // At least update once
                    if (numFramesBehind == 1)
                    {
                        UpdateAsync(this);
                        numFramesBehind = 0;
                    }
                    else
                    {
                        var updateCount = Math.Max(numFramesBehind - MaxFramesBehind, 0);
                        for (int i = 0; i < updateCount; i++)
                            UpdateAsync(this);
                        numFramesBehind -= updateCount;
                    }

                    canUpdate.Set();
                }
            }
        }

        void IDrawableObject.EndDraw(DrawingContext context) { }
        #endregion

        #region Threading
        
        // This number specifies how many frames we need to update before draw the object
        private int numFramesBehind;
        private AutoResetEvent canDraw = new AutoResetEvent(true);
        private AutoResetEvent canUpdate = new AutoResetEvent(true);

        /// <summary>
        /// Gets or sets max number of frames the update frame can run behind the draw frame.
        /// </summary>
        public static int MaxFramesBehind
        {
            get { return maxFramesBehind; }
            set { maxFramesBehind = value; }
        }
        private static int maxFramesBehind = 1;

        static AutoResetEvent ParticleQueueSyncEvent;
        static Thread ParticleThread;        
        static ConcurrentQueue<ParticleEffect> ActiveUpdates;
        
        static void UpdateAsync(ParticleEffect particleEffect)
        {
            if (ParticleThread == null)
            {
                ParticleQueueSyncEvent = new AutoResetEvent(false);
                ActiveUpdates = new ConcurrentQueue<ParticleEffect>();
                ParticleThread = new Thread((ThreadStart)ParticleUpdateWorker);
                ParticleThread.IsBackground = true;
                ParticleThread.Name = "ParticleEffect";
                ParticleThread.Start();
            }
            ActiveUpdates.Enqueue(particleEffect);
            ParticleQueueSyncEvent.Set();
        }

        static void ParticleUpdateWorker()
        {
            while (true)
            {
                ParticleQueueSyncEvent.WaitOne();
                ParticleEffect particleEffect;
                while (ActiveUpdates.TryDequeue(out particleEffect))
                    particleEffect.Update();
            }
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (primitive != null)
                {
                    primitive.Dispose();
                    primitive = null;
                }
            }
        }

        ~ParticleEffect()
        {
            Dispose(false);
        }
        #endregion
    }
}
