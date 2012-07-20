namespace Nine.Graphics.ParticleEffects
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;

    /// <summary>
    /// Defines a basic controller that changes the color of the particle
    /// effect based on time.
    /// </summary>
    public class ColorController : ParticleController<Range<Color>>
    {
        /// <summary>
        /// Range of values controlling the particle end color and alpha. 
        /// </summary>
        [ContentSerializer(Optional = true)]
        public Range<Color> EndColor { get; set; }

        public ColorController()
        {
            EndColor = Color.White;
        }

        protected override void OnReset(ref Particle particle, ref Range<Color> tag)
        {
            tag.Min = particle.Color;
            tag.Max = Color.Lerp(EndColor.Min, EndColor.Max, (float)Random.NextDouble());
        }

        protected override void OnUpdate(float elapsedTime, ref Particle particle, ref Range<Color> tag)
        {
            particle.Color = Color.Lerp(tag.Min, tag.Max, particle.Age);
        }
    }

    /// <summary>
    /// Defines a basic controller that fade the particle in and out.
    /// </summary>
    public class FadeController : ParticleController
    {
        protected override void OnReset(ref Particle particle) { }

        protected override void OnUpdate(float elapsedTime, ref Particle particle)
        {
            particle.Alpha = particle.Age * (1 - particle.Age) * (1 - particle.Age) * 6.7f;
        }
    }

    /// <summary>
    /// Defines a basic controller that changes the size of the particle
    /// effect based on time.
    /// </summary>
    public class SizeController : ParticleController<float>
    {
        /// <summary>
        /// Range of values controlling the particle end size.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public Range<float> EndSize { get; set; }

        public SizeController()
        {
            EndSize = 1;
        }

        protected override void OnReset(ref Particle particle, ref float tag)
        {
            tag = (MathHelper.Lerp(EndSize.Min, EndSize.Max, (float)Random.NextDouble()) - particle.Size) / particle.Duration;
        }

        protected override void OnUpdate(float elapsedTime, ref Particle particle, ref float tag)
        {
            particle.Size += tag * elapsedTime;
        }
    }

    /// <summary>
    /// Defines a basic controller that changes the rotation of the particle
    /// effect based on time.
    /// </summary>
    public class RotationController : ParticleController<float>
    {
        /// <summary>
        /// Range of values controlling the particle end rotation.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public Range<float> EndRotation { get; set; }

        protected override void OnReset(ref Particle particle, ref float tag)
        {
            tag = (MathHelper.Lerp(EndRotation.Min, EndRotation.Max, (float)Random.NextDouble()) - particle.Rotation) / particle.Duration;
        }

        protected override void OnUpdate(float elapsedTime, ref Particle particle, ref float tag)
        {
            particle.Rotation += tag * elapsedTime;
        }
    }

    /// <summary>
    /// Defines a basic controller that controls the acceleration of the particle effect.
    /// </summary>
    public class SpeedController : ParticleController<float>
    {
        /// <summary>
        /// Range of values representing the particle end speed in proportion to its start speed.
        /// A value of 1 means no change, a value of 0 means complete stop.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public Range<float> EndSpeed { get; set; }

        protected override void OnReset(ref Particle particle, ref float tag)
        {
            tag = MathHelper.Lerp(EndSpeed.Min, EndSpeed.Max, (float)Random.NextDouble());
        }

        protected override void OnUpdate(float elapsedTime, ref Particle particle, ref float tag)
        {
            float timeLeft = particle.Duration - particle.ElapsedTime;
            particle.Velocity -= particle.Velocity * (1 - tag) * (elapsedTime / timeLeft);
        }
    }
    
    /// <summary>
    /// Defines a basic controller that applies a constant linear force on the particle effect.
    /// </summary>
    public class ForceController : ParticleController
    {
        /// <summary>
        /// Gets or sets the force amount.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public Vector3 Force { get; set; }

        public ForceController()
        {
            Force = -Vector3.Up;
        }

        protected override void OnReset(ref Particle particle) { }

        protected override void OnUpdate(float elapsedTime, ref Particle particle)
        {
            particle.Velocity += Force * elapsedTime;
        }
    }

    /// <summary>
    /// Defines a basic controller that applies a constant tangent force on the particle effect.
    /// </summary>
    public class TangentForceController : ParticleController
    {
        /// <summary>
        /// Gets or sets the force amount.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public float Force { get; set; }

        /// <summary>
        /// Gets or sets the up axis.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public Vector3 Up { get; set; }

        /// <summary>
        /// Gets or sets the center position.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public Vector3 Center { get; set; }

        public TangentForceController()
        {
            Up = Vector3.Up;
        }

        protected override void OnReset(ref Particle particle) { }

        protected override void OnUpdate(float elapsedTime, ref Particle particle)
        {
            Vector3 normal = particle.Position - Center;
            Vector3 force = Vector3.Cross(Up, normal);
            Vector3.Normalize(ref force, out force);

            particle.Velocity += force * (Force * elapsedTime);
        }
    }

    /// <summary>
    /// Defines a basic controller that absorbs the particles to a given point.
    /// </summary>
    public class AbsorbController : ParticleController
    {
        /// <summary>
        /// Gets or sets the absorb position.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public Vector3 Position { get; set; }

        /// <summary>
        /// Gets or sets the absorb force.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public float Force { get; set; }

        protected override void OnReset(ref Particle particle) { }

        protected override void OnUpdate(float elapsedTime, ref Particle particle)
        {
            particle.Velocity += Vector3.Normalize(Position - particle.Position) * Force * elapsedTime;
        }
    }
}
