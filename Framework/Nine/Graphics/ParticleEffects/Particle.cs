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


        internal void Reset(float duration)
        {
            Duration = duration;
            Color = Color.White;
            Rotation = ElapsedTime = Age = 0;
            Size = 1;
        }

        internal void Update(float elapsedTime)
        {
            ElapsedTime += elapsedTime;
            Age = ElapsedTime / Duration;
            Position += Velocity * elapsedTime;
        }

        public override string ToString()
        {
            return string.Format("Position: {0}, Size: {1}, Color {2}", Position, Size, Color);
        }
    }
}