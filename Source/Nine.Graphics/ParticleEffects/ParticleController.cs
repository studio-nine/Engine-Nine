﻿namespace Nine.Graphics.ParticleEffects
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using Microsoft.Xna.Framework.Content;

    /// <summary>
    /// Defines an controller that controls the appearance and behavior of each
    /// individual particle.
    /// </summary>
    public interface IParticleController
    {
        /// <summary>
        /// Resets a newly emitted particle.
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
    [Nine.Serialization.BinarySerializable]
    public abstract class ParticleController : IParticleController
    {
        /// <summary>
        /// Gets the containing partcie effect.
        /// </summary>
        protected ParticleEffect ParticleEffect { get; private set; }

        /// <summary>
        /// Gets or sets whether this emitter is enabled.
        /// </summary>
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
            if (this.ParticleEffect != null && this.ParticleEffect != particleEffect)
            {
                throw new InvalidOperationException(
                    "The particle controller is already used by another particle effect.");
            }
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
                tags = new T[particleEffect.MaxParticleCount];
            
            base.Initialize(particleEffect);
        }
        
        protected ParticleController() { }

        protected override sealed void OnReset(ref Particle particle)
        {
            if (ParticleEffect.CurrentParticle >= tags.Length)
                Array.Resize(ref tags, ParticleEffect.MaxParticleCount);

            OnReset(ref particle, ref tags[ParticleEffect.CurrentParticle]);
        }

        protected override sealed void OnUpdate(float elapsedTime, ref Particle particle)
        {
            if (ParticleEffect.CurrentParticle >= tags.Length)
                Array.Resize(ref tags, ParticleEffect.MaxParticleCount);

            OnUpdate(elapsedTime, ref particle, ref tags[ParticleEffect.CurrentParticle]);
        }

        protected abstract void OnReset(ref Particle particle, ref T tag);
        protected abstract void OnUpdate(float elapsedTime, ref Particle particle, ref T tag);
    }

    /// <summary>
    /// Defines a collection of particle controllers.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ParticleControllerCollection : Collection<IParticleController> 
    {
        internal ParticleEffect ParticleEffect;

        internal ParticleControllerCollection() { }

        protected override void InsertItem(int index, IParticleController item)
        {
            OnAdded(index, item);
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, IParticleController item)
        {
            OnAdded(index, item);
            base.SetItem(index, item);
        }

        private void OnAdded(int index, IParticleController value)
        {
            if (value == null)
                throw new ArgumentNullException();
            ParticleController controller = value as ParticleController;
            if (controller != null && ParticleEffect != null)
                controller.Initialize(ParticleEffect);
        }
    }
}
