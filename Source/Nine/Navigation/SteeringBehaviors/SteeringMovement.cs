#region Copyright 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion


//=============================================================================
//
// Special Thanks to 
//      Mat Buckland (fup@ai-junkie.com)
// And his
//      Programming Game AI By Example
//
//=============================================================================
namespace Nine.Navigation.SteeringBehaviors
{
    #region ISteeringBehavior
    public interface ISteeringBehavior
    {
        Vector3 CalculateForce(GameTime gameTime, IMovable movingEntity);

        bool WillPenetrate(Vector3 from, Vector3 to, IMovable movingEntity);
    }
    #endregion

    #region SteeringBehaviorCollection
    public sealed class SteeringBehaviorCollection : ICollection<ISteeringBehavior>
    {
        Collection<KeyValuePair<ISteeringBehavior, float>> array = new Collection<KeyValuePair<ISteeringBehavior,float>>();
        
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

        public T Get<T>()
        {
            for (int i = 0; i < Count; i++)
                if (array[i].Key is T)
                    return (T)array[i].Key;

            return default(T);
        }

        internal float GetWeightByIndex(int index)
        {
            return array[index].Value;
        }

        public ISteeringBehavior this[int index]
        {
            get { return array[index].Key; }
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
    #endregion

    #region SteeringMovement
    public class SteeringMovement : IMovable
    {
        public Vector3 Position { get; set; }

        public Vector3 Forward { get; private set; }

        public float Speed { get; private set; }

        public float Resistance { get; set; }

        public float MaxSpeed { get; set; }

        public SteeringBehaviorCollection Behaviors { get; private set; }

        public float MaxForce { get; set; }

        public float StopSpeed { get; set; }

        public float BoundingRadius { get; set; }

        public bool EnsureNoPenetration { get; set; }


        Vector3 velocity;


        public SteeringMovement()
        {
            Resistance = 1.0f;
            StopSpeed = 1.0f;
            MaxSpeed = 10.0f;
            MaxForce = 0.5f;
            BoundingRadius = 1.0f;
            Forward = Vector3.UnitX;
            EnsureNoPenetration = true;
            
            Behaviors = new SteeringBehaviorCollection();
        }

        public void Update(GameTime gameTime)
        {
            float elapsedTime = (float)(gameTime.ElapsedGameTime.TotalSeconds);

            if (elapsedTime <= 0)
                return;

            Vector3 force = Vector3.Zero;

            // Calculate force
            for (int i = 0; i < Behaviors.Count; i++)
            {
                if (!AccumulateForce(ref force,
                                     Behaviors.GetWeightByIndex(i) *
                                     Behaviors[i].CalculateForce(gameTime, this)))
                {
                    break;
                }
            }

            /*
            if (force.Length() > 0)
            {
                force.Normalize();
                force *= MaxForce;
            }*/

            if (force.Length() <= 0)
            {
                force = -velocity * elapsedTime * Resistance;
            }

            // We don't multiply elapsedTime here because our force
            // is calculated based on the subtracting desired speed and current speed.
            velocity += force;

            Speed = velocity.Length();

            // Stop when speed is too small
            if (Speed < StopSpeed && force.LengthSquared() <= 0)
            {
                velocity = Vector3.Zero;
                Speed = 0;
            }

            // Turncate speed
            if (Speed > MaxSpeed)
            {
                float inv = (float)(MaxSpeed / Speed);

                velocity.X *= inv;
                velocity.Y *= inv;
                velocity.Z *= inv;

                Speed = MaxSpeed;
            }

            if (Speed > 0)
            {
                Forward = Vector3.Normalize(velocity);
            }

            // Update position
            Vector3 newPosition = Position + velocity * elapsedTime;

            if (EnsureNoPenetration)
            {
                foreach (ISteeringBehavior behavior in Behaviors)
                {
                    if (behavior.WillPenetrate(Position, newPosition, this))
                    {
                        // Maybe we should allow certain amount of penetration
                        // and let this be a configurable property.
                        Position = (Position + newPosition) / 2;
                        return;
                    }
                }
            }

            Position = newPosition;
        }

        private bool AccumulateForce(ref Vector3 force, Vector3 steeringForce)
        {
            force += steeringForce;

            float lengthSq = force.LengthSquared();

            if (lengthSq > MaxForce * MaxForce)
            {
                float inv = (float)(MaxForce / Math.Sqrt(lengthSq));

                force.X *= inv;
                force.Y *= inv;
                force.Z *= inv;

                return false;
            }

            return true;
        }
    }
    #endregion
}