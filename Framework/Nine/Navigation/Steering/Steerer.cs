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
    /// Represents a steerer that can steer a moving entity with steering behaviors.
    /// </summary>
    public class Steerer : ISteerable
    {
        public Vector2 Position { get; set; }
        public Vector2 Forward
        {
            get { return forward; }
            set { forward = value; Velocity = value * Speed; }
        }

        public float Speed { get; private set; }
        public float MaxSpeed { get; set; }
        public Vector2 Force { get { return force; } }
        public float MaxForce { get; private set; }
        public Vector2 Velocity { get; private set; }
        public float Acceleration { get; set; }  
        public float BoundingRadius { get; set; }
        public bool AllowPenetration { get; set; }
        public object Tag { get; set; }

        public SteeringBehaviorBlendMode BlendMode { get; set; }
        public SteeringBehaviorCollection Behaviors { get; private set; }

        private Vector2 force;
        private Vector2 forward;
                
        public Steerer()
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
            float elapsedSeconds = (float)(gameTime.ElapsedGameTime.TotalSeconds);

            if (elapsedSeconds <= 0)
                return;

            // The max acceleration can stops the moving entity from top speed.
            float MaxAcceleration = MaxSpeed / elapsedSeconds;

            if (Acceleration >= MaxAcceleration)
                Acceleration = MaxAcceleration;

            force = Vector2.Zero;
            MaxForce = Acceleration;

            // Accumulate force of each behavior
            for (int i = 0; i < Behaviors.Count; i++)
            {
                Vector2 steeringForce = Behaviors.GetWeightByIndex(i) * Behaviors[i].UpdateSteeringForce(elapsedSeconds, this);
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
                Velocity -= forward * MaxForce * elapsedSeconds;

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
                Velocity += force * elapsedSeconds;
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
            Vector2 newPosition = Position + Velocity * elapsedSeconds;

            // Perform collision detection when penetration is not allowed.
            if (!AllowPenetration)
            {
                // Find the max penetration depth.
                float? maxPenetration = null;
                foreach (ISteeringBehavior behavior in Behaviors)
                {
                    float? penetration = behavior.Collides(Position, newPosition, this);
                    if (penetration.HasValue && (!maxPenetration.HasValue || penetration.Value > maxPenetration.Value))
                    {
                        maxPenetration = penetration;
                    }
                }

                if (maxPenetration.HasValue)
                {
                    // Adjust target position based on penetration depth.
                    newPosition = Position + forward * maxPenetration.Value;
                }
            }

            Position = newPosition;
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

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SteeringBehaviorCollection : ICollection<ISteeringBehavior>
    {
        Collection<KeyValuePair<ISteeringBehavior, float>> array = new Collection<KeyValuePair<ISteeringBehavior, float>>();

        public void Add(ISteeringBehavior behavior, float weight)
        {
            array.Add(new KeyValuePair<ISteeringBehavior, float>(behavior, weight));
        }

        public float GetWeight(ISteeringBehavior behavior)
        {
            for (int i = 0; i < Count; i++)
            {
                if (array[i].Key == behavior)
                {
                    return array[i].Value;
                }
            }

            return 0;
        }

        public ISteeringBehavior this[int index]
        {
            get { return array[index].Key; }
        }

        public T FindFirst<T>()
        {
            for (int i = 0; i < Count; i++)
                if (array[i].Key is T)
                    return (T)array[i].Key;

            return default(T);
        }

        public IEnumerable<T> Find<T>()
        {
            for (int i = 0; i < Count; i++)
                if (array[i].Key is T)
                    yield return (T)array[i].Key;
        }

        internal float GetWeightByIndex(int index)
        {
            return array[index].Value;
        }

        #region ICollection<IFlockingBehavior> Members

        public void Add(ISteeringBehavior behavior)
        {
            array.Add(new KeyValuePair<ISteeringBehavior, float>(behavior, 1.0f));
        }

        public void Clear()
        {
            array.Clear();
        }

        public bool Contains(ISteeringBehavior item)
        {
            for (int i = 0; i < Count; i++)
            {
                if (array[i].Key == item)
                {
                    return true;
                }
            }

            return false;
        }

        public void CopyTo(ISteeringBehavior[] array, int arrayIndex)
        {
            for (int i = 0; i < Count; i++)
            {
                array[i + arrayIndex] = this.array[i].Key;
            }
        }

        public int Count
        {
            get { return array.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(ISteeringBehavior item)
        {
            for (int i = 0; i < Count; i++)
            {
                if (array[i].Key == item)
                {
                    array.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region IEnumerable<IFlockingBehavior> Members

        public IEnumerator<ISteeringBehavior> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return array[i].Key;
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }

}