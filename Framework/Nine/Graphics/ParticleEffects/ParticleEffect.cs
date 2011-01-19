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
#endregion

namespace Nine.Graphics.ParticleEffects
{
    /// <summary>
    /// Defines a special visual effect made up of particles.
    /// </summary>
    public class ParticleEffect : IDisposable
    {
        /// <summary>
        /// Gets or sets whether this particle effect is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the number of particles emitted per second.
        /// </summary>
        public float Emission { get; set; }

        /// <summary>
        /// Gets or sets the emitter of this particle effect.
        /// </summary>
        public IParticleEmitter Emitter { get; set; }

        /// <summary>
        /// Gets a collection of controllers that defines the visual of this particle effect.
        /// </summary>
        public ParticleControllerCollection Controllers { get; private set; }

        /// <summary>
        /// Gets or sets the position of this particle effect.
        /// </summary>
        public Vector3 Position { get; set; }

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
        /// Gets the graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

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
        /// Gets or sets a scale factor along the forward axis when drawing this
        /// particle effect using constrained billboard.
        /// </summary>
        public float Stretch { get; set; }

        /// <summary>
        /// Gets or sets user data.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Gets the approximate bounds of this particle effect.
        /// </summary>
        public BoundingBox BoundingBox
        {
            get
            {
                if (Emitter == null)
                    return new Microsoft.Xna.Framework.BoundingBox();

                // TODO: Should cache this value
                BoundingBox box = Emitter.BoundingBox;

                Vector3 maxBorder = Vector3.Zero;
                for (int currentController = 0; currentController < Controllers.Count; currentController++)
                {
                    Vector3 border = Controllers[currentController].Border;

                    if (border.X > maxBorder.X)
                        maxBorder.X = border.X;
                    if (border.Y > maxBorder.Y)
                        maxBorder.Y = border.Y;
                    if (border.Z > maxBorder.Z)
                        maxBorder.Z = border.Z;
                }

                box.Max += maxBorder;
                box.Min -= maxBorder;
                return box;
            }
        }

        /// <summary>
        /// Gets the current active particle count
        /// </summary>
        public int ParticleCount
        {
            get { return (lastParticle + maxParticles) % maxParticles - firstParticle; }
        }


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
        public ParticleEffect(int maxParticles)
        {
            Stretch = 1;
            Duration = 2;
            BlendState = BlendState.Additive;
            Controllers = new ParticleControllerCollection(this);
            Emitter = new PointEmitter();

            particles = new Particle[this.maxParticles = maxParticles];
        }
        
        /// <summary>
        /// Updates the particle system
        /// </summary>
        public void Update(GameTime time)
        {
            if (Emitter == null)
                return;

            float elapsedTime = (float)time.ElapsedGameTime.TotalSeconds;

            UpdateEmitter(elapsedTime);
            UpdateControllers(elapsedTime);
            UpdateParticles(elapsedTime);
            
            if (ParticleCount > 0)
                System.Diagnostics.Debug.WriteLine(particles[firstParticle]);
        }

        private void UpdateEmitter(float elapsedTime)
        {
            // Work out how much time has passed since the previous update.
            float timeBetweenParticles = 1.0f / Emission;

            if (elapsedTime > 0)
            {
                // If we had any time left over that we didn't use during the
                // previous update, add that to the current elapsed time.
                float timeToSpend = timeLeftOver + elapsedTime;

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
                    float mu = currentTime / elapsedTime;

                    // Don't add new particles when the queue is full.
                    currentParticle = lastParticle;
                    int nextParticle = (currentParticle + 1) % maxParticles;
                    if (nextParticle == firstParticle - 1)
                        break;
                    lastParticle = nextParticle;

                    Emitter.Emit(mu, ref particles[currentParticle].Position, ref particles[currentParticle].Velocity);

                    particles[currentParticle].Age = 0;
                    particles[currentParticle].ElapsedTime = 0;
                    particles[currentParticle].Position += Position;
                    particles[currentParticle].Rotation = Rotation.Min + (float)random.NextDouble() * (Rotation.Max - Rotation.Min); ;
                    particles[currentParticle].Duration = Duration.Min + (float)random.NextDouble() * (Duration.Max - Duration.Min);
                    particles[currentParticle].Velocity *= Speed.Min + (float)random.NextDouble() * (Speed.Max - Speed.Min);
                    particles[currentParticle].Size = Size.Min + (float)random.NextDouble() * (Size.Max - Size.Min);
                    particles[currentParticle].Color = Microsoft.Xna.Framework.Color.Lerp(Color.Min, Color.Max, (float)random.NextDouble());
                    
                    for (int currentController = 0; currentController < Controllers.Count; currentController++)
                    {
                        Controllers[currentController].Reset(ref particles[currentParticle]);
                    }
                }

                // Store any time we didn't use, so it can be part of the next update.
                timeLeftOver = timeToSpend;
            }
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

        private void UpdateParticles(float elapsedTime)
        {
            for (currentParticle = firstParticle; currentParticle != lastParticle;
                                                  currentParticle = (currentParticle + 1) % maxParticles)
            {
                particles[currentParticle].Update(elapsedTime);
                if (currentParticle == firstParticle && particles[currentParticle].Age >= 1)
                {
                    firstParticle = (firstParticle + 1) % maxParticles;
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
