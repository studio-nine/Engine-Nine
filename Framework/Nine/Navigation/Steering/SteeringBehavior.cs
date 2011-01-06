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
    /// Interface for an object that can be steered by the Steerer.
    /// </summary>
    /// <remarks>
    /// This interface is to be used by ISteeringBehavior and is not meant
    /// to be implemented.
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface ISteerable
    {
        /// <summary>
        /// Gets the position of the moving entity.
        /// </summary>
        Vector2 Position { get; }

        /// <summary>
        /// Gets the forward moving direction of the moving entity.
        /// </summary>
        Vector2 Forward { get; }

        /// <summary>
        /// Gets the velocity of the moving entity.
        /// </summary>
        Vector2 Velocity { get; }

        /// <summary>
        /// Gets the moving speed of the moving entity.
        /// </summary>
        float Speed { get; }

        /// <summary>
        /// Gets the maximum moving speed of the moving entity.
        /// </summary>
        float MaxSpeed { get; }

        /// <summary>
        /// Gets the maximum force that can be applied to the moving entity.
        /// </summary>
        float MaxForce { get; }

        /// <summary>
        /// Gets the acceleration of the moving entity.
        /// </summary>
        float Acceleration { get; }

        /// <summary>
        /// Gets the bounding radius of the moving entity.
        /// </summary>
        float BoundingRadius { get; }
    }

    /// <summary>
    /// Defines the behavior of the Steerer.
    /// </summary>
    public interface ISteeringBehavior
    {
        /// <summary>
        /// Updates and returns the steering force.
        /// </summary>
        Vector2 UpdateSteeringForce(float elapsedTime, ISteerable movingEntity);

        /// <summary>
        /// Checks if the step collides with any obstacles. Return null if don't collide, 
        /// otherwise, returns the max penertration depth allowed.
        /// </summary>
        float? Collides(Vector2 from, Vector2 to, ISteerable movingEntity);
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

        public Vector2 UpdateSteeringForce(float elapsedTime, ISteerable movingEntity)
        {
            if (!Enabled)
                return Vector2.Zero;

            return OnUpdateSteeringForce(elapsedTime, movingEntity);
        }

        public float? Collides(Vector2 from, Vector2 to, ISteerable movingEntity)
        {
            if (!Enabled)
                return null;

            return OnCollides(from, to, movingEntity);
        }

        protected abstract Vector2 OnUpdateSteeringForce(float elapsedTime, ISteerable movingEntity);
        protected virtual float? OnCollides(Vector2 from, Vector2 to, ISteerable movingEntity) { return null; }
    }
}