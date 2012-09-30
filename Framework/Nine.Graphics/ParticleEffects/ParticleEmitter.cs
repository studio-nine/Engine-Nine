namespace Nine.Graphics.ParticleEffects
{
    using System;
    using Microsoft.Xna.Framework;

    #region IParticleEmitter
    /// <summary>
    /// Defines an emitter that emit new particles for particle effect.
    /// </summary>
    public interface IParticleEmitter
    {
        /// <summary>
        /// Gets the world space bounding box that potentially contains
        /// all the particles emitted by this particle emitter.
        /// </summary>
        BoundingBox BoundingBox { get; }

        /// <summary>
        /// Updates the emitter, emits any new particles during the update.
        /// </summary>
        /// <returns>
        /// Returns true when this emitter has stopped emitting new particles.
        /// </returns>
        bool Update(ParticleEffect particleEffect, float elapsedSeconds);
    }
    #endregion

    #region ParticleEmitter
    /// <summary>
    /// Defines the base class for all particle emitters.
    /// </summary>
    [ContentSerializable]
    public abstract class ParticleEmitter : Transformable, IParticleEmitter
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ParticleEmitter"/> is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the number of particles emitted when triggered.
        /// When this value is greater then zero, Lifetime and Emission is ignored.
        /// </summary>
        public int EmitCount { get; set; }

        /// <summary>
        /// Gets or sets the number of particles emitted per second.
        /// </summary>
        public float Emission { get; set; }

        /// <summary>
        /// Gets or sets the time before the first particle is emitted.
        /// </summary>
        public TimeSpan Delay { get; set; }

        /// <summary>
        /// Gets or sets the total lifetime of this particle effect when triggered.
        /// The default value is forever.
        /// </summary>
        public TimeSpan Lifetime { get; set; }

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
        /// Gets the random number generator used by particle emitters.
        /// </summary>
        protected Random Random { get { return random; } }

        /// <summary>
        /// Gets the bounding box that potentially contains all the particles emitted by
        /// this particle emitter.
        /// </summary>
        public BoundingBox BoundingBox
        {
            get
            {
                BoundingBox box;
                GetBoundingBox(out box);
                float border = Speed.Max * Duration.Max + Size.Max * 0.5f;

                box.Min.X += transform.M41; box.Min.Y += transform.M42; box.Min.Z += transform.M43;
                box.Max.X += transform.M41; box.Max.Y += transform.M42; box.Max.Z += transform.M43;

                box.Max.X += border; box.Max.Y += border; box.Max.Z += border;
                box.Min.X -= border; box.Min.Y -= border; box.Min.Z -= border;
                return box;
            }
        }

        private float elapsedTime = 0;
        private float timeLeftOver = 0;
        private bool firstParticleEmitted;

        private static Random random = new Random();

        /// <summary>
        /// Initializes a new instance of the <see cref="ParticleEmitter"/> class.
        /// </summary>
        protected ParticleEmitter()
        {
            Enabled = true;
            Duration = 1;
            Emission = 1;
            Size = 1;
            Lifetime = TimeSpan.MaxValue;
            Color = Microsoft.Xna.Framework.Color.White;
        }

        /// <summary>
        /// Creates the random vector.
        /// </summary>
        protected void Randomize(out Vector3 result)
        {
            double r = random.NextDouble() * Math.PI * 2;
            double z = random.NextDouble() * 2 - 1;
            double p = Math.Sqrt(1 - (z * z));

            result = new Vector3();
            result.X = (float)(Math.Cos(r) * p);
            result.Y = (float)(Math.Sin(r) * p);
            result.Z = (float)z;
        }

        /// <summary>
        /// Creates the random vector.
        /// </summary>
        protected void Randomize(float spread, out Vector3 result)
        {
            if (spread >= MathHelper.Pi)
                Randomize(out result);
            else if (spread <= 0)
                result = Vector3.Up;
            else
            {
                double a = random.NextDouble() * Math.PI * 2;
                double b = random.NextDouble() * spread + MathHelper.PiOver2;
                double r = Math.Cos(b);

                result = new Vector3();
                result.X = (float)(r * Math.Cos(a));
                result.Y = (float)(r * Math.Sin(a));
                result.Z = (float)(Math.Sin(b));
            }
        }

        static Vector3 Up = Vector3.Up;
        
        /// <summary>
        /// Updates the emitter, emits any new particles during the update.
        /// </summary>
        public bool Update(ParticleEffect particleEffect, float elapsedSeconds)
        {
            if (!Enabled || elapsedSeconds < 0)
                return false;

            // Check for delay
            elapsedTime += elapsedSeconds;
            if (elapsedTime < Delay.TotalSeconds)
                return false;

            if (elapsedTime > Lifetime.TotalSeconds + Delay.TotalSeconds)
                return true;
            
            Particle particle = new Particle();

            if (EmitCount > 0)
            {
                for (int i = 0; i < EmitCount; ++i)
                    EmitNewParticle(particleEffect, 0, ref particle);
                return true;
            }

            // Emit when the particle emitter has just started.
            if (!firstParticleEmitted)
            {
                EmitNewParticle(particleEffect, 0, ref particle);
                firstParticleEmitted = true;
            }

            // Work out how much time has passed since the previous update.
            float timeBetweenParticles = 1.0f / Emission;

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

                // Emit a new particle
                EmitNewParticle(particleEffect, mu, ref particle);
            }

            // Store any time we didn't use, so it can be part of the next update.
            timeLeftOver = timeToSpend;

            return false;
        }

        private void EmitNewParticle(ParticleEffect particleEffect, float lerpAmount, ref Particle particle)
        {
            if (Emit(lerpAmount, ref particle.Position, ref particle.Velocity))
            {
                particle.Alpha = 1;
                particle.Rotation = Rotation.Min + (float)random.NextDouble() * (Rotation.Max - Rotation.Min);
                particle.Duration = Duration.Min + (float)random.NextDouble() * (Duration.Max - Duration.Min);
                particle.Velocity *= Speed.Min + (float)random.NextDouble() * (Speed.Max - Speed.Min);
                particle.Size = Size.Min + (float)random.NextDouble() * (Size.Max - Size.Min);
                particle.Color = Microsoft.Xna.Framework.Color.Lerp(Color.Min, Color.Max, (float)random.NextDouble());

                Vector3.Transform(ref particle.Position, ref transform, out particle.Position);
                Vector3.TransformNormal(ref particle.Velocity, ref transform, out particle.Velocity);

                particleEffect.Emit(ref particle);
            }
        }

        /// <summary>
        /// Gets the bounding box that defines the region of this particle emitter without been offset by Position.
        /// </summary>
        protected abstract void GetBoundingBox(out BoundingBox boundingBox);

        /// <summary>
        /// Emits a new particle.
        /// </summary>
        public abstract bool Emit(float lerpAmount, ref Vector3 position, ref Vector3 direction);
    }
    #endregion
}
