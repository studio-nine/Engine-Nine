namespace Nine.Graphics.ParticleEffects
{
    using System;
    using Microsoft.Xna.Framework;

    /// <summary>
    /// Represents each individual particle in a particle system effect.
    /// </summary>
#if WINDOWS
    [Serializable]
#endif
    public struct Particle
    {
        /// <summary>
        /// Gets or sets the position of this particle.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Gets or sets the moving speed of this particle.
        /// </summary>
        public Vector3 Velocity;

        /// <summary>
        /// Gets or sets the rotation of this particle.
        /// </summary>
        public float Rotation;

        /// <summary>
        /// Gets or sets the transparency of this particle.
        /// </summary>
        public float Alpha;

        /// <summary>
        /// Gets or sets the color of this particle.
        /// </summary>
        public Color Color;
        
        /// <summary>
        /// Gets or sets the size of this particle.
        /// </summary>        
        public float Size;

        /// <summary>
        /// Gets or sets the elapsed time (in seconds) since this particle was created.
        /// </summary>
        public float ElapsedTime;

        /// <summary>
        /// Gets or sets the duration (in seconds) of this particle.
        /// </summary>
        public float Duration;

        /// <summary>
        /// Gets or sets age of this particle in the range 0 to 1.
        /// </summary>
        public float Age;

        /// <summary>
        /// Gets or sets the animation frame of this particle.
        /// </summary>
        public int Frame;

        internal void Update(float elapsedTime)
        {
            ElapsedTime += elapsedTime;
            Age = ElapsedTime / Duration;

            Position.X += Velocity.X * elapsedTime;
            Position.Y += Velocity.Y * elapsedTime;
            Position.Z += Velocity.Z * elapsedTime;
        }

        public override string ToString()
        {
            return string.Format("Position: {0}, Size: {1}, Color {2}", Position, Size, Color * Alpha);
        }
    }
}