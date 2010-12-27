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
    /// Represents a steerer that can steer a moving entity with steering behaviors.
    /// </summary>
    public class Steerer : ISteerable
    {
        public Vector2 Position { get; set; }
        public Vector2 Forward { get; set; }
        public float Speed { get; private set; }
        public float MaxSpeed { get; set; }
        public float MaxForce { get; private set; }
        public Vector2 Velocity { get; private set; }
        public float Acceleration { get; set; }  
        public float BoundingRadius { get; set; }
        public bool CollisionDetectionEnabled { get; set; }
        public object Tag { get; set; }

        public SteeringBehaviorCollection Behaviors { get; private set; }
                
        public Steerer()
        {
            MaxSpeed = 10.0f;
            Acceleration = 20.0f;
            BoundingRadius = 1.0f;
            Forward = Vector2.UnitX;
            CollisionDetectionEnabled = true;
            
            Behaviors = new SteeringBehaviorCollection();
        }

        public void Update(GameTime gameTime)
        {
            float elapsedTime = (float)(gameTime.ElapsedGameTime.TotalSeconds);

            if (elapsedTime <= 0)
                return;

            float MaxAcceleration = MaxSpeed * 10;

            if (Acceleration >= MaxAcceleration)
                Acceleration = MaxAcceleration;

            Vector2 force = Vector2.Zero;
            MaxForce = Acceleration * elapsedTime;

            // Calculate force
            for (int i = 0; i < Behaviors.Count; i++)
            {
                if (!AccumulateForce(ref force, MaxForce,
                                     Behaviors.GetWeightByIndex(i) *
                                     Behaviors[i].UpdateSteeringForce(elapsedTime, this)))
                {
                    break;
                }
            }
            
            // We don't multiply elapsedTime here because our force
            // is calculated based on the subtracting desired speed and current speed.
            Velocity += force;
            Speed = Velocity.Length();

            // Stop when speed is too small
            if (Speed <= Acceleration * elapsedTime)
            {
                if (force.LengthSquared() <= Speed * Speed)
                {
                    Velocity = Vector2.Zero;
                    Speed = 0;
                }
            }
            else if (force.LengthSquared() <= 0)
            {
                Velocity -= Forward * MaxForce;
                Speed = Velocity.Length();
            }

            // Turncate speed
            if (Speed > MaxSpeed)
            {
                Velocity *= (float)(MaxSpeed / Speed);
                Speed = MaxSpeed;
            }

            if (Speed > 0)
            {
                Forward = Vector2.Normalize(Velocity);
            }

            // Update position
            Vector2 newPosition = Position + Velocity * elapsedTime;

            if (CollisionDetectionEnabled)
            {
                foreach (ISteeringBehavior behavior in Behaviors)
                {
                    if (behavior.Collides(Position, newPosition, this))
                    {
                        // Maybe we should allow certain amount of penetration
                        // and let this be a configurable property.
                        //Position = (Position + newPosition) / 2;
                        return;
                    }
                }
            }

            Position = newPosition;
        }

        private bool AccumulateForce(ref Vector2 force, float maxForce, Vector2 steeringForce)
        {
            force += steeringForce;

            float lengthSq = force.LengthSquared();
            
            if (lengthSq > maxForce * maxForce)
            {
                float inv = (float)(maxForce / Math.Sqrt(lengthSq));

                force.X *= inv;
                force.Y *= inv;

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