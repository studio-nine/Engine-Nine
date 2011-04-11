#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics.ParticleEffects
{
    /// <summary>
    /// Defines an controller that controls the appearence and behavior of each
    /// individual particle.
    /// </summary>
    public interface IParticleController
    {
        /// <summary>
        /// Gets the approximate max change of particle positions by this particle controller.
        /// </summary>
        Vector3 Border { get; }

        /// <summary>
        /// Resets a new emitted particle.
        /// </summary>
        void Reset(ref Particle particle);

        /// <summary>
        /// Updates an existing particle.
        /// </summary>
        void Update(float elapsedTime, ref Particle particle);
    }

    /// <summary>
    /// Defines the base class for all particle controllers.
    /// </summary>
    public abstract class ParticleController : IParticleController
    {
        /// <summary>
        /// Gets the containing partcie effect.
        /// </summary>
        protected ParticleEffect ParticleEffect { get; private set; }

        /// <summary>
        /// Gets or sets whether this emitter is enabled.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets the random number generator used by particle emitters.
        /// </summary>
        public Random Random { get { return random; } }
        static Random random = new Random();

        protected ParticleController()
        {
            Enabled = true;
        }

        /// <summary>
        /// Gets the approximate max change of particle positions by this particle controller.
        /// </summary>
        public abstract Vector3 Border { get; }

        /// <summary>
        /// Updates an existing particle.
        /// </summary>
        public void Update(float elapsedTime, ref Particle particle)
        {
            if (Enabled)
                OnUpdate(elapsedTime, ref particle);
        }

        /// <summary>
        /// Resets a new emitted particle.
        /// </summary>
        public void Reset(ref Particle particle)
        {
            if (Enabled)
                OnReset(ref particle);
        }

        internal virtual void Initialize(ParticleEffect particleEffect)
        {
            this.ParticleEffect = particleEffect;
        }

        protected abstract void OnReset(ref Particle particle);
        protected abstract void OnUpdate(float elapsedTime, ref Particle particle);
    }

    /// <summary>
    /// Defines the base class for all particle controllers.
    /// </summary>
    public abstract class ParticleController<T> : ParticleController where T : struct
    {
        T[] tags = null;

        internal sealed override void Initialize(ParticleEffect particleEffect)
        {
            if (tags == null)
                tags = new T[particleEffect.maxParticles];
            
            base.Initialize(particleEffect);
        }
        
        protected ParticleController() { }

        protected override sealed void OnReset(ref Particle particle)
        {
            OnReset(ref particle, ref tags[ParticleEffect.currentParticle]);
        }

        protected override sealed void OnUpdate(float elapsedTime, ref Particle particle)
        {
            OnUpdate(elapsedTime, ref particle, ref tags[ParticleEffect.currentParticle]);
        }

        protected abstract void OnReset(ref Particle particle, ref T tag);
        protected abstract void OnUpdate(float elapsedTime, ref Particle particle, ref T tag);
    }

    /// <summary>
    /// Defines a collection of particle controllers.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ParticleControllerCollection : EnumerableCollection<IParticleController> 
    {
        internal ParticleEffect ParticleEffect;

        internal ParticleControllerCollection() { }

        protected override void OnAdded(int index, IParticleController value)
        {
            if (value == null)
                throw new ArgumentNullException();

            ParticleController controller = value as ParticleController;
            if (controller != null && ParticleEffect != null)
                controller.Initialize(ParticleEffect);
        }

        protected override void OnChanged(int index, IParticleController value, IParticleController previousValue)
        {
            if (value == null)
                throw new ArgumentNullException();

            ParticleController controller = value as ParticleController;
            if (controller != null && ParticleEffect != null)
                controller.Initialize(ParticleEffect);
        }
    }
}
