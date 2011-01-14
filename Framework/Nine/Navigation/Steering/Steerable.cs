#region Copyright 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Diagnostics;
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
    /// Defines how the final force of steering behaviors are blended.
    /// </summary>
    public enum SteeringBehaviorBlendMode
    {
        /// <summary>
        /// Sum up all each steering force with weight until the max force limit is reached.
        /// </summary>
        WeightedSum,

        /// <summary>
        /// Only use the first non-zero steering force.
        /// </summary>
        Solo,
    }

    /// <summary>
    /// Represents a steerable moving entity that can with steering behaviors.
    /// </summary>
    public class Steerable
    {
        /// <summary>
        /// Gets or sets the target position of the moving entity.
        /// </summary>
        public Vector2? Target { get; set; }

        /// <summary>
        /// Gets the position of the moving entity.
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Gets or sets the forward moving direction of the moving entity.
        /// </summary>
        public Vector2 Forward
        {
            get { return forward; }
            set { forward = value; Velocity = value * Speed; }
        }

        /// <summary>
        /// Gets or sets the acceleration of the moving entity.
        /// </summary>
        public float Acceleration
        {
            get { return currentAcceleration; }
            set { acceleration = value; currentAcceleration = value; }
        }

        /// <summary>
        /// Gets the moving speed of the moving entity.
        /// </summary>
        public float Speed { get; private set; }

        /// <summary>
        /// Gets or sets the maximum moving speed of the moving entity.
        /// </summary>
        public float MaxSpeed { get; set; }

        /// <summary>
        /// Gets the current force that is applied to the moving entity.
        /// </summary>
        public Vector2 Force { get { return force; } }

        /// <summary>
        /// Gets the maximum force that can be applied to the moving entity.
        /// </summary>
        public float MaxForce { get; private set; }

        /// <summary>
        /// Gets the velocity of the moving entity.
        /// </summary>
        public Vector2 Velocity { get; private set; }

        /// <summary>
        /// Gets the bounding radius of the moving entity.
        /// </summary>
        public float BoundingRadius { get; set; }

        /// <summary>
        /// Gets or sets whether this moving entity can penetrate through obstacles.
        /// </summary>
        public bool AllowPenetration { get; set; }

        /// <summary>
        /// Gets whether this moving entity is stucked this frame.
        /// </summary>
        public bool IsStucked { get; private set; }

        /// <summary>
        /// Gets or sets user data.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Gets or sets the technique used to blend between behaviors
        /// </summary>
        public SteeringBehaviorBlendMode BlendMode { get; set; }

        /// <summary>
        /// Gets the collection of behaviors used by this moving entity.
        /// </summary>
        public SteeringBehaviorCollection Behaviors { get; private set; }
        
        internal float DecelerationRange;
        internal float Skin;
        internal Vector2 TargetedForward;

        private float acceleration = float.MaxValue;
        private float currentAcceleration = float.MaxValue;
        private Vector2 force;
        private Vector2 forward;
                
        public Steerable()
        {
            MaxSpeed = 10.0f;
            Acceleration = 20.0f;
            BoundingRadius = 1.0f;
            Forward = Vector2.UnitX;
            AllowPenetration = false;
            
            Behaviors = new SteeringBehaviorCollection();
        }

        public void Update(GameTime gameTime)
        {
            float elapsedTime = (float)(gameTime.ElapsedGameTime.TotalSeconds);
            if (elapsedTime <= 0)
                return;

            // The max acceleration can stops the moving entity from top speed.
            float MaxAcceleration = MaxSpeed / elapsedTime;
            if (acceleration >= MaxAcceleration)
                currentAcceleration = MaxAcceleration;

            force = Vector2.Zero;
            MaxForce = Acceleration;

            DecelerationRange = SteeringHelper.GetDecelerateRange(this);
            TargetedForward = SteeringHelper.GetTargetedForward(this);
            Skin = MaxSpeed * elapsedTime * 2;

            // Accumulate force of each behavior
            for (int i = 0; i < Behaviors.Count; i++)
            {
                Vector2 steeringForce = Behaviors.GetWeightByIndex(i) * Behaviors[i].UpdateSteeringForce(elapsedTime, this);
                if (BlendMode == SteeringBehaviorBlendMode.WeightedSum)
                {
                    if (!AccumulateForceWeightedSum(ref force, MaxForce, steeringForce))
                        break;
                }
                else if (BlendMode == SteeringBehaviorBlendMode.Solo)
                {
                    if (!AccumulateForceSolo(ref force, MaxForce, steeringForce))
                        break;
                }
            }

            if (Speed >= 0 && force == Vector2.Zero)
            {
                // The moving entity has fully stopped.
                if (Speed == 0)
                    return;

                // Apply friction when there is no force but the entity is still moving.
                Vector2 previousVelocity = Velocity;
                Velocity -= forward * MaxForce * elapsedTime;

                // When the velocity has changed its direction, that indicates the moving
                // entity has fully stopped.
                if (Vector2.Dot(previousVelocity, Velocity) <= 0)
                {
                    Velocity = Vector2.Zero;
                    Speed = 0;
                    return;
                }

                Speed = Velocity.Length();
            }
            else
            {
                // We don't multiply elapsedTime here because our force
                // is calculated based on the subtracting desired speed and current speed.
                Velocity += force * elapsedTime;
                Speed = Velocity.Length();

                if (Speed <= 0)
                    return;

                // Turncate speed
                if (Speed > MaxSpeed)
                {
                    Velocity *= (float)(MaxSpeed / Speed);
                    Speed = MaxSpeed;
                }
            }
            
            Debug.Assert(Speed > 0);
            forward = Vector2.Normalize(Velocity);

            // Update position
            Vector2 newPosition;
            IsStucked = DetectCollision(Position, Position + Velocity * elapsedTime, elapsedTime, out newPosition);
            Position = newPosition;
        }

        private bool DetectCollision(Vector2 from, Vector2 to, float elapsedTime, out Vector2 position)
        {
            // Perform collision detection when penetration is not allowed.
            if (AllowPenetration)
            {
                position = to;
                return false;
            }

            // Find the min penetration depth.
            float? minPenetration = null;
            foreach (ISteeringBehavior behavior in Behaviors)
            {
                float? penetration = behavior.Collides(from, to, elapsedTime, this);
                if (penetration.HasValue && (!minPenetration.HasValue || penetration.Value < minPenetration.Value))
                {
                    minPenetration = penetration;
                    if (minPenetration <= 0.0001f)
                    {
                        // No penetration is allowed.
                        position = from;
                        return true;
                    }
                }
            }

            if (minPenetration.HasValue)
            {
                // Adjust target position based on penetration depth.
                position = from + forward * minPenetration.Value;
                return false;
            }

            position = to;
            return false;
        }

        private bool AccumulateForceWeightedSum(ref Vector2 force, float maxForce, Vector2 steeringForce)
        {
            //calculate how much steering force the vehicle has used so far
            float MagnitudeSoFar = force.Length();

            //calculate how much steering force remains to be used by this vehicle
            float MagnitudeRemaining = maxForce - MagnitudeSoFar;

            //return false if there is no more force left to use
            if (MagnitudeRemaining <= float.Epsilon) return false;

            //calculate the magnitude of the force we want to add
            float MagnitudeToAdd = steeringForce.Length();

            //if the magnitude of the sum of ForceToAdd and the running total
            //does not exceed the maximum force available to this vehicle, just
            //add together. Otherwise add as much of the ForceToAdd vector is
            //possible without going over the max.
            if (MagnitudeToAdd < MagnitudeRemaining)
            {
                force += steeringForce;
            }
            else
            {
                //add it to the steering force
                force += (Vector2.Normalize(steeringForce) * MagnitudeRemaining);
            }

            return true;
        }

        private bool AccumulateForceSolo(ref Vector2 force, float maxForce, Vector2 steeringForce)
        {
            //calculate the magnitude of the force we want to add
            float MagnitudeToAdd = steeringForce.Length();
            float MagnitudeRemaining = maxForce;

            if (MagnitudeToAdd > 0)
            {
                if (MagnitudeToAdd < MagnitudeRemaining)
                {
                    force += steeringForce;
                }
                else
                {
                    //add it to the steering force
                    force += (Vector2.Normalize(steeringForce) * MagnitudeRemaining);
                }
                return false;
            }
            return true;
        }
    }
}