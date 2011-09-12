#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Text;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
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

        /// <summary>
        /// The particle will be rendered as 3D ribbon trail.
        /// </summary>
        RibbonTrail,
    }

    /// <summary>
    /// Event args for ParticleEffect.ParticleEnds.
    /// </summary>
    public class ParticleEndsEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the particle that is going to die.
        /// </summary>
        public Particle Particle { get; internal set; }
    }

    /// <summary>
    /// Action for particles.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public delegate void ParticleAction(ref Particle particle);

    /// <summary>
    /// Defines a special visual effect made up of particles.
    /// </summary>
    public class ParticleEffect : IDisposable
    {
        /// <summary>
        /// Gets or sets the type of each particle.
        /// </summary>
        public ParticleType ParticleType { get; set; }

        /// <summary>
        /// Gets or sets whether this particle effect is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the number of particles emitted when triggered.
        /// When this value is greater then zero, the Lifetime and Emission attribute is ignored.
        /// </summary>
        public int TriggerCount { get; set; }

        /// <summary>
        /// Gets or sets the total lifetime of this particle effect when triggered.
        /// The default value is forever.
        /// </summary>
        public TimeSpan Lifetime { get; set; }

        /// <summary>
        /// Gets or sets the number of particles emitted per second.
        /// </summary>
        public float Emission { get; set; }

        /// <summary>
        /// Gets or sets the duration of each particle.
        /// </summary>
        public Range<float> Duration { get; set; }

        /// <summary>
        /// Gets or sets the range of values controlling the particle start color and alpha. 
        /// </summary>
        public Range<Color> Color { get; set; }

        /// <summary>
        /// Gets or sets the range of values controlling the particle start size.
        /// </summary>
        public Range<float> Size { get; set; }

        /// <summary>
        /// Gets or sets the range of values controlling the particle start rotation.
        /// </summary>
        public Range<float> Rotation { get; set; }

        /// <summary>
        /// Gets or sets the range of values controlling the particle start speed.
        /// </summary>
        public Range<float> Speed { get; set; }

        /// <summary>
        /// Gets or sets a scale factor along the forward axis when drawing this
        /// particle effect using constrained billboard.
        /// </summary>
        public float Stretch { get; set; }

        /// <summary>
        /// Gets or sets the up axis of each particle.
        /// </summary>
        public Vector3 Up { get; set; }

        /// <summary>
        /// Gets or sets the texture used by this particle effect.
        /// </summary>
        public Texture2D Texture { get; set; }

        /// <summary>
        /// Gets or sets the source rectangle in the texture.
        /// </summary>
        public Rectangle? SourceRectangle { get; set; }

        /// <summary>
        /// Gets or sets the blend state used by this particle effect.
        /// </summary>
        public BlendState BlendState { get; set; }

        /// <summary>
        /// Gets or sets the emitter of this particle effect.
        /// </summary>
        public IParticleEmitter Emitter { get; set; }

        /// <summary>
        /// Gets a collection of controllers that defines the visual of this particle effect.
        /// </summary>
        public ParticleControllerCollection Controllers { get; internal set; }

        /// <summary>
        /// Gets a collection of particle effects that is used as the appareance of each
        /// particle spawned by this particle effect.
        /// </summary>
        public ParticleEffectCollection ChildEffects { get; private set; }

        /// <summary>
        /// Gets a collection of particle effects that is fired when each particle spawned
        /// by this particle effect is about to die.
        /// </summary>
        public ParticleEffectCollection EndingEffects { get; private set; }

        /// <summary>
        /// Gets or sets user data.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Gets the max particle count.
        /// </summary>
        public int MaxParticleCount { get; private set; }

        /// <summary>
        /// Gets the approximate particle count.
        /// </summary>
        public int ParticleCount { get; private set; }

        /// <summary>
        /// Gets the approximate bounds of all triggered effects.
        /// Use <c>ParticleAnimation.BoundingBox</c> to get the bounds of each triggered effects.
        /// </summary>
        public BoundingBox BoundingBox
        {
            get 
            {
                // TODO: Use IBoundable
                if (Triggers.Count <= 0)
                    return new BoundingBox();

                BoundingBox box = Triggers[0].BoundingBox;
                for (int i = 1; i < Triggers.Count; i++)
                    box = BoundingBox.CreateMerged(box, Triggers[i].BoundingBox);
                return box;
            }
        }
        
        /// <summary>
        /// Gets a list of triggers owned by this <c>ParticleEffect</c>.
        /// </summary>
        public IList<ParticleTrigger> Triggers { get; private set; }

        /// <summary>
        /// Occurs when a particle is about to die.
        /// </summary>
        public event EventHandler<ParticleEndsEventArgs> ParticleEnds;

        // An array of particles, treated as a circular queue.
        private Particle[] particles;
        private int firstParticle = 0;
        private int lastParticle = 0;
        internal int CurrentParticle = 0;

        private Random random = new Random();
        private float timeLeftOver = 0;
        private bool triggerOnStartup = false;
        private ParticleEndsEventArgs endsEventArgs = new ParticleEndsEventArgs();

        /// <summary>
        /// Creates a new particle effect.
        /// </summary>
        public ParticleEffect() : this(false, 1024)
        {

        }

        /// <summary>
        /// Creates a new particle effect.
        /// </summary>
        public ParticleEffect(int maxParticles) : this(false, maxParticles)
        {
            // TODO: Add array dynamic expanding to eliminate maxParticles.
        }

        /// <summary>
        /// Creates a new particle effect.
        /// </summary>
        public ParticleEffect(bool triggerOnStartup, int maxParticles)
        {
            this.Up = Vector3.UnitZ;
            this.Lifetime = TimeSpan.MaxValue;
            this.Enabled = true;
            this.Stretch = 1;
            this.Duration = 2;
            this.triggerOnStartup = triggerOnStartup;
            this.BlendState = BlendState.Additive;
            this.Emitter = new PointEmitter();
            this.Triggers = new List<ParticleTrigger>();

            this.Controllers = new ParticleControllerCollection();
            this.Controllers.ParticleEffect = this;

            this.ChildEffects = new ParticleEffectCollection();
            this.EndingEffects = new ParticleEffectCollection();

            this.particles = new Particle[this.MaxParticleCount = maxParticles];
        }

        /// <summary>
        /// Creates a merged effect from serveral input particle effects.
        /// See http://nine.codeplex.com/discussions/272121
        /// </summary>
        public static ParticleEffect CreateMerged(IEnumerable<ParticleEffect> effects)
        {
            if (effects == null)
                throw new ArgumentNullException("effects");

            var root = new ParticleEffect(16);
            root.TriggerCount = 1;
            root.Emission = 1;
            root.Duration = float.MaxValue;
            foreach (var effect in effects)
            {
                if (effect != null)
                    root.ChildEffects.Add(effect);
            }
            return root;
        }

        /// <summary>
        /// Creates an ever lasting the particle effect.
        /// </summary>
        public ParticleTrigger Trigger()
        {
            return Trigger(Vector3.Zero, Lifetime, TimeSpan.Zero);
        }

        /// <summary>
        /// Creates the particle effect at the specified position that last forever.
        /// </summary>
        public ParticleTrigger Trigger(Vector3 position)
        {
            return Trigger(position, Lifetime, TimeSpan.Zero);
        }

        /// <summary>
        /// Creates the particle effect at the specified position that
        /// lasts for the given amount of time.
        /// </summary>
        public ParticleTrigger Trigger(Vector3 position, TimeSpan lifetime, TimeSpan delay)
        {
            return Trigger(position, lifetime, TriggerCount, delay);
        }

        /// <summary>
        /// Creates an impulse of particles at the specified position.
        /// </summary>
        public ParticleTrigger Trigger(Vector3 position, int triggerCount)
        {
            return Trigger(position, triggerCount, TimeSpan.Zero);
        }

        /// <summary>
        /// Creates an impulse of particles at the specified position.
        /// </summary>
        public ParticleTrigger Trigger(Vector3 position, int triggerCount, TimeSpan delay)
        {
            return Trigger(position, Lifetime, triggerCount, delay);
        }

        private ParticleTrigger Trigger(Vector3 position, TimeSpan lifetime, int triggerCount, TimeSpan delay)
        {
            EnsureChildEffectHasTrigger();

            ParticleTrigger result = new ParticleTrigger();
            Triggers.Add(result);
            result.Position = position;
            result.Delay = delay;
            result.TriggerCount = triggerCount;
            result.Duration = lifetime + delay;
            result.Effect = this;
            result.Play();
            return result;
        }

        private void EnsureChildEffectHasTrigger()
        {
            foreach (var child in ChildEffects)
            {
                if (child.Triggers.Count <= 0)
                    child.Trigger();
            }
        }

        /// <summary>
        /// Traverses all active particles.
        /// </summary>
        public void ForEach(ParticleAction action)
        {
            ForEachInternal((ref Particle particle) =>
            {
                if (particle.Age <= 1)
                    action(ref particles[CurrentParticle]);
            });
        }

        private void ForEachInternal(ParticleAction action)
        {
            if (ParticleCount > 0)
            {
                if (firstParticle < lastParticle)
                {
                    // ParticleConstroller<T>.Update requires the CurrentParticle to be the correct index.
                    for (CurrentParticle = firstParticle; CurrentParticle < lastParticle; CurrentParticle++)
                        action(ref particles[CurrentParticle]);
                }
                else
                {
                    // UpdateParticles requires the enumeration to always start from firstParticle.
                    for (CurrentParticle = firstParticle; CurrentParticle < MaxParticleCount; CurrentParticle++)
                        action(ref particles[CurrentParticle]);
                    for (CurrentParticle = 0; CurrentParticle < lastParticle; CurrentParticle++)
                        action(ref particles[CurrentParticle]);
                }
            }
        }

        /// <summary>
        /// Updates the particle system.
        /// </summary>
        public void Update(TimeSpan time)
        {
            if (triggerOnStartup && Triggers.Count <= 0)
            {
                Trigger();
                triggerOnStartup = false;
            }
            Update(time, false);
        }

        internal void Update(TimeSpan elapsedTime, bool ignoreTrigger)
        {
            float elapsedSeconds = (float)elapsedTime.TotalSeconds;

            if (!ignoreTrigger)
                UpdateTriggers(elapsedTime);
            UpdateControllers(elapsedSeconds);
            UpdateParticles(elapsedTime);

            foreach (var childEffect in ChildEffects)
            {
                childEffect.Update(elapsedTime, true);
            }

            foreach (var endEffect in EndingEffects)
            {
                endEffect.Update(elapsedTime);
            }
        }

        private void UpdateTriggers(TimeSpan elapsedTime)
        {
            if (!Enabled)
                return;

            for (int i = 0; i < Triggers.Count; i++)
            {
                ParticleTrigger anim = Triggers[i];
                if (anim.State == Animations.AnimationState.Stopped)
                {
                    Triggers.RemoveAt(i);
                    continue;
                }

                anim.Update(elapsedTime);
            }
        }

        internal void UpdateEmitter(Vector3 position, TimeSpan elapsedTime)
        {
            // Work out how much time has passed since the previous update.
            float timeBetweenParticles = 1.0f / Emission;
            float elapsedSeconds = (float)elapsedTime.TotalSeconds;

            if (elapsedSeconds > 0)
            {
                // If we had any time left over that we didn't use during the
                // previous update, add that to the current elapsed time.
                float timeToSpend = timeLeftOver + elapsedSeconds;

                // Counter for looping over the time interval.
                float currentTime = -timeLeftOver;

                // Create particles as long as we have a big enough time interval.
                while (timeToSpend > timeBetweenParticles)
                {
                    currentTime += timeBetweenParticles;
                    timeToSpend -= timeBetweenParticles;

                    // Work out the optimal position for this particle. This will produce
                    // evenly spaced particles regardless of the object speed, particle
                    // creation frequency, or game update rate.
                    float mu = currentTime / elapsedSeconds;

                    if (!EmitNewParticle(position, mu))
                        break;
                }

                // Store any time we didn't use, so it can be part of the next update.
                timeLeftOver = timeToSpend;
            }
        }

        internal bool EmitNewParticle(Vector3 position, float mu)
        {
            if (Emitter == null)
                return false;

            // Don't add new particles when the queue is full.
            if (ParticleCount >= MaxParticleCount)
                return false;
            CurrentParticle = lastParticle;

            Emitter.Emit(mu, ref particles[CurrentParticle].Position, ref particles[CurrentParticle].Velocity);

            ResetParticle(ref particles[CurrentParticle], ref position);

            ParticleCount++;
            lastParticle = (lastParticle + 1) % MaxParticleCount;
            return true;
        }

        private void ResetParticle(ref Particle particle, ref Vector3 offset)
        {
            particle.Age = 0;
            particle.Alpha = 1;
            particle.ElapsedTime = 0;
            particle.Rotation = Rotation.Min + (float)random.NextDouble() * (Rotation.Max - Rotation.Min); ;
            particle.Duration = Duration.Min + (float)random.NextDouble() * (Duration.Max - Duration.Min);
            particle.Velocity *= Speed.Min + (float)random.NextDouble() * (Speed.Max - Speed.Min);
            particle.Size = Size.Min + (float)random.NextDouble() * (Size.Max - Size.Min);
            particle.Color = Microsoft.Xna.Framework.Color.Lerp(Color.Min, Color.Max, (float)random.NextDouble());
            
            Vector3.Add(ref particle.Position, ref offset, out particle.Position);

            for (int currentController = 0; currentController < Controllers.Count; currentController++)
            {
                Controllers[currentController].Reset(ref particle);
            }
        }

        private void UpdateControllers(float elapsedTime)
        {   
            if (ParticleCount <= 0)
                return;

            for (int currentController = 0; currentController < Controllers.Count; currentController++)
            {
                ForEachInternal((ref Particle particle) =>
                {
                    Controllers[currentController].Update(elapsedTime, ref particle);
                });
            }
        }

        private void UpdateParticles(TimeSpan elapsedTime)
        {
            float elapsedSeconds = (float)elapsedTime.TotalSeconds;

            bool hasParticleEndsEvent = ParticleEnds != null;
            bool hasEndingEffects = EndingEffects.Count > 0;
            bool hasChildEffects = ChildEffects.Count > 0;
            bool hasAliveParticle = false;

            ForEachInternal((ref Particle particle) =>
            {
                particle.Update(elapsedSeconds);

                if (hasChildEffects)
                {
                    foreach (var childEffect in ChildEffects)
                    {
                        foreach (var trigger in childEffect.Triggers)
                            trigger.Position = particle.Position;
                        childEffect.UpdateTriggers(elapsedTime);
                    }
                }

                if (particle.Age < 1)
                {
                    hasAliveParticle = true;
                }
                else
                {
                    if (ParticleCount > 0 && !hasAliveParticle)
                    {
                        firstParticle = (firstParticle + 1) % MaxParticleCount;
                        ParticleCount--;
                    }

                    if ((hasEndingEffects || hasParticleEndsEvent) && particle.Age < float.MaxValue)
                    {
                        if (hasEndingEffects)
                        {
                            foreach (var endingEffect in EndingEffects)
                                endingEffect.Trigger(particle.Position);
                        }
                        if (hasParticleEndsEvent)
                        {
                            endsEventArgs.Particle = particle;
                            ParticleEnds(this, endsEventArgs);
                        }
                        particle.Age = float.MaxValue;
                    }
                }
            });
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        ~ParticleEffect()
        {
            Dispose(false);
        }
    }
}
