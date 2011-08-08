#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
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
        /// Gets or sets the lifetime or duration of this particle effect.
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
        /// Gets a collection of particle effects that is running simultaneously with this effect.
        /// </summary>
        public ParticleEffectCollection SiblingEffects { get; private set; }

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
        /// Gets the current active particle count
        /// </summary>
        public int ParticleCount
        {
            get { return (lastParticle + maxParticles) % maxParticles - firstParticle; }
        }        

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
        public IList<ParticleAnimation> Triggers { get; private set; }

        // An array of particles, treated as a circular queue.
        internal Particle[] particles;
        internal int firstParticle = 0;
        internal int lastParticle = 0;
        internal int currentParticle = 0;
        internal int maxParticles;

        private Random random = new Random();
        private float timeLeftOver = 0;

        /// <summary>
        /// Creates a new particle effect.
        /// </summary>
        public ParticleEffect() : this(true, 1024)
        {

        }

        /// <summary>
        /// Creates a new particle effect.
        /// </summary>
        public ParticleEffect(int maxParticles) : this(true, maxParticles)
        {
            // TODO: Add array dynamic expanding to eliminate maxParticles.
        }

        /// <summary>
        /// Creates a new particle effect.
        /// </summary>
        public ParticleEffect(bool triggerOnStartup, int maxParticles)
        {
            Up = Vector3.UnitZ;
            Lifetime = TimeSpan.MaxValue;
            Enabled = true;
            Stretch = 1;
            Duration = 2;
            BlendState = BlendState.Additive;
            Emitter = new PointEmitter();
            Triggers = new List<ParticleAnimation>();

            Controllers = new ParticleControllerCollection();
            Controllers.ParticleEffect = this;

            ChildEffects = new ParticleEffectCollection() { EnsureHasTrigger = true };
            SiblingEffects = new ParticleEffectCollection() { EnsureHasTrigger = true };
            EndingEffects = new ParticleEffectCollection() { ClearTriggerList = true };

            particles = new Particle[this.maxParticles = maxParticles];
            if (triggerOnStartup)
                Trigger();
        }

        /// <summary>
        /// Creates an ever lasting the particle effect.
        /// </summary>
        public ParticleAnimation Trigger()
        {
            return Trigger(Vector3.Zero, Lifetime);
        }

        /// <summary>
        /// Creates the particle effect at the specified position that last forever.
        /// </summary>
        public ParticleAnimation Trigger(Vector3 position)
        {
            return Trigger(position, Lifetime);
        }

        /// <summary>
        /// Creates the particle effect at the specified position that
        /// lasts for the given amount of time.
        /// </summary>
        public ParticleAnimation Trigger(Vector3 position, TimeSpan lifetime)
        {
            ParticleAnimation result = new ParticleAnimation() { Position = position, Duration = lifetime };
            Triggers.Add(result);
            result.Effect = this;
            result.Play();
            return result;
        }

        /// <summary>
        /// Creates an impulse of particles at the specified position.
        /// </summary>
        public void Trigger(Vector3 position, int particleCount)
        {
            for (int i = 0; i < particleCount; i++)
            {
                if (!EmitNewParticle(position, 0))
                    break;
            }
        }

        /// <summary>
        /// Traverses all active particles.
        /// </summary>
        public void ForEach(Action<Particle> action)
        {
            for (int currentParticle = firstParticle; currentParticle != lastParticle;  currentParticle = (currentParticle + 1) % maxParticles)
            {
                if (particles[currentParticle].Age <= 1)
                {
                    action(particles[currentParticle]);
                }
            }
        }

        /// <summary>
        /// Updates the particle system.
        /// </summary>
        public void Update(TimeSpan time)
        {
            Update(time, false);
        }

        public void Update(TimeSpan elapsedTime, bool ignoreTrigger)
        {
            float elapsedSeconds = (float)elapsedTime.TotalSeconds;

            if (!ignoreTrigger)
                UpdateTriggers(elapsedTime);
            UpdateControllers(elapsedSeconds);
            UpdateParticles(elapsedTime);

            foreach (var siblingEffect in SiblingEffects)
            {
                siblingEffect.Update(elapsedTime);
            }

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
                ParticleAnimation anim = Triggers[i];
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

        private bool EmitNewParticle(Vector3 position, float mu)
        {
            if (Emitter == null)
                return false;

            // Don't add new particles when the queue is full.
            currentParticle = lastParticle;
            int nextParticle = (currentParticle + 1) % maxParticles;
            if (nextParticle == firstParticle - 1)
                return false;
            lastParticle = nextParticle;

            Emitter.Emit(mu, ref particles[currentParticle].Position, ref particles[currentParticle].Velocity);

            particles[currentParticle].Age = 0;
            particles[currentParticle].Alpha = 1;
            particles[currentParticle].ElapsedTime = 0;
            particles[currentParticle].Position += position;
            particles[currentParticle].Rotation = Rotation.Min + (float)random.NextDouble() * (Rotation.Max - Rotation.Min); ;
            particles[currentParticle].Duration = Duration.Min + (float)random.NextDouble() * (Duration.Max - Duration.Min);
            particles[currentParticle].Velocity *= Speed.Min + (float)random.NextDouble() * (Speed.Max - Speed.Min);
            particles[currentParticle].Size = Size.Min + (float)random.NextDouble() * (Size.Max - Size.Min);
            particles[currentParticle].Color = Microsoft.Xna.Framework.Color.Lerp(Color.Min, Color.Max, (float)random.NextDouble());

            for (int currentController = 0; currentController < Controllers.Count; currentController++)
            {
                Controllers[currentController].Reset(ref particles[currentParticle]);
            }
            return true;
        }

        private void UpdateControllers(float elapsedTime)
        {   
            if (firstParticle == lastParticle)
                return;

            for (int currentController = 0; currentController < Controllers.Count; currentController++)
            {
                for (currentParticle = firstParticle; currentParticle != lastParticle;
                                                      currentParticle = (currentParticle + 1) % maxParticles)
                {
                    Controllers[currentController].Update(elapsedTime, ref particles[currentParticle]);
                }
            }
        }

        private void UpdateParticles(TimeSpan elapsedTime)
        {
            float elapsedSeconds = (float)elapsedTime.TotalSeconds;

            bool hasEndingEffects = EndingEffects.Count > 0;
            bool hasChildEffects = ChildEffects.Count > 0;

            for (currentParticle = firstParticle; currentParticle != lastParticle;
                                                  currentParticle = (currentParticle + 1) % maxParticles)
            {
                particles[currentParticle].Update(elapsedSeconds);

                if (hasChildEffects)
                {
                    foreach (var childEffect in ChildEffects)
                    {
                        foreach (var trigger in childEffect.Triggers)
                            trigger.Position = particles[currentParticle].Position;
                        childEffect.UpdateTriggers(elapsedTime);
                    }
                }

                if (particles[currentParticle].Age >= 1)
                {
                    if (currentParticle == firstParticle)
                    {
                        firstParticle = (firstParticle + 1) % maxParticles;
                    }

                    if (hasEndingEffects && particles[currentParticle].Age < float.MaxValue)
                    {
                        foreach (var endingEffect in EndingEffects)
                            endingEffect.Trigger(particles[currentParticle].Position);

                        particles[currentParticle].Age = float.MaxValue;
                    }
                }
            }
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
