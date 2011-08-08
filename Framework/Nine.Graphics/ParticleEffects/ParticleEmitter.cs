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
using System.ComponentModel;
#endregion

namespace Nine.Graphics.ParticleEffects
{
    /// <summary>
    /// Defines an emitter that emit new particles for particle effect.
    /// </summary>
    public interface IParticleEmitter
    {
        /// <summary>
        /// Gets the bounding box that defines the region of this particle emitter.
        /// </summary>
        BoundingBox BoundingBox { get; }

        /// <summary>
        /// Emits a new particle.
        /// </summary>
        void Emit(float lerpAmount, ref Vector3 position, ref Vector3 velocity);
    }

    /// <summary>
    /// Defines the base class for all particle emitters.
    /// </summary>
    public abstract class ParticleEmitter : IParticleEmitter
    {
        /// <summary>
        /// Gets the random number generator used by particle emitters.
        /// </summary>
        protected Random Random { get { return random; } }
        static Random random = new Random();

        protected void CreateRandomVelocity(ref Vector3 velocity)
        {
            double a = random.NextDouble() * Math.PI * 2;
            double b = random.NextDouble() * Math.PI - MathHelper.PiOver2;
            double r = Math.Cos(b);

            velocity.X = (float)(r * Math.Cos(a));
            velocity.Y = (float)(r * Math.Sin(a));
            velocity.Z = (float)(Math.Sin(b));
        }

        protected void CreateRandomVelocity(ref Vector3 velocity, ref Vector3 direction, float spread)
        {
            if (spread >= MathHelper.Pi)
                CreateRandomVelocity(ref velocity);
            else if (spread <= 0)
                velocity = direction;
            else
            {
                double a = random.NextDouble() * Math.PI * 2;
                double b = random.NextDouble() * spread + MathHelper.PiOver2;
                double r = Math.Cos(b);

                velocity.X = (float)(r * Math.Cos(a));
                velocity.Y = (float)(r * Math.Sin(a));
                velocity.Z = (float)(Math.Sin(b));

                if (direction != Vector3.UnitZ)
                {
                    Matrix rotation = MatrixHelper.CreateRotation(Vector3.UnitZ, direction);
                    Vector3.TransformNormal(ref velocity, ref rotation, out velocity);
                }
            }
        }

        /// <summary>
        /// Gets the bounding box that defines the region of this particle emitter.
        /// </summary>
        public abstract BoundingBox BoundingBox { get; }

        /// <summary>
        /// Emits a new particle.
        /// </summary>
        public abstract void Emit(float lerpAmount, ref Vector3 position, ref Vector3 velocity);
    }
}
