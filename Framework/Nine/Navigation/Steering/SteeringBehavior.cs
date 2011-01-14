#region Copyright 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Navigation.Steering
{
    /// <summary>
    /// Defines the behavior of the Steerer.
    /// </summary>
    public interface ISteeringBehavior
    {
        /// <summary>
        /// Updates and returns the steering force.
        /// </summary>
        Vector2 UpdateSteeringForce(float elapsedTime, Steerable movingEntity);

        /// <summary>
        /// Checks if the step collides with any obstacles. Return null if don't collide, 
        /// otherwise, returns the max penertration depth allowed.
        /// </summary>
        float? Collides(Vector2 from, Vector2 to, float elapsedTime, Steerable movingEntity);
    }

    /// <summary>
    /// Represents the base class for basic steering behaviors.
    /// </summary>
    public abstract class SteeringBehavior : ISteeringBehavior
    {
        /// <summary>
        /// Gets or sets whether this behavior is enabled.
        /// </summary>
        public bool Enabled { get; set; }
        
        /// <summary>
        /// Gets or sets the random number generator used by derived types.
        /// </summary>
        public static Random Random { get; set; }

        static SteeringBehavior() { Random = new Random(); }

        public SteeringBehavior() { Enabled = true; }

        public Vector2 UpdateSteeringForce(float elapsedTime, Steerable movingEntity)
        {
            if (!Enabled)
                return Vector2.Zero;

            return OnUpdateSteeringForce(elapsedTime, movingEntity);
        }

        public float? Collides(Vector2 from, Vector2 to, float elapsedTime, Steerable movingEntity)
        {
            if (!Enabled)
                return null;

            return OnCollides(from, to, elapsedTime, movingEntity);
        }

        protected abstract Vector2 OnUpdateSteeringForce(float elapsedTime, Steerable movingEntity);
        protected virtual float? OnCollides(Vector2 from, Vector2 to, float elapsedTime, Steerable movingEntity) { return null; }
    }
}