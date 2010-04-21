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
namespace Isles.Navigation.Flocking
{
    #region IFlockingBehavior
    public interface IFlockingBehavior
    {
        Vector3 Update(GameTime gameTime, IMovable movingEntity);
    }
    #endregion

    #region FlockingBehaviorCollection
    public sealed class FlockingBehaviorCollection : ICollection<IFlockingBehavior>
    {
        Collection<KeyValuePair<IFlockingBehavior, float>> array = new Collection<KeyValuePair<IFlockingBehavior,float>>();
        
        public void Add(IFlockingBehavior behavior, float weight)
        {
            array.Add(new KeyValuePair<IFlockingBehavior, float>(behavior, weight));
        }

        public float GetWeight(IFlockingBehavior behavior)
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

        public IFlockingBehavior this[int index]
        {
            get { return array[index].Key; }
        }

        #region ICollection<IFlockingBehavior> Members

        public void Add(IFlockingBehavior behavior)
        {
            array.Add(new KeyValuePair<IFlockingBehavior, float>(behavior, 1.0f));
        }

        public void Clear()
        {
            array.Clear();
        }

        public bool Contains(IFlockingBehavior item)
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

        public void CopyTo(IFlockingBehavior[] array, int arrayIndex)
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

        public bool Remove(IFlockingBehavior item)
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

        public IEnumerator<IFlockingBehavior> GetEnumerator()
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

    #region FlockingMovement
    public class FlockingMovement : IMovable
    {
        public Vector3 Position { get; set; }

        public Vector3 Forward { get; private set; }

        public float Speed { get; private set; }

        public float MaxSpeed { get; set; }

        public FlockingBehaviorCollection Behaviors { get; private set; }

        public float Mass { get; set; }

        public float MaxForce { get; set; }

        public float StopSpeed { get; set; }


        Vector3 velocity;


        public FlockingMovement()
        {
            Mass = 1.0f;
            StopSpeed = 1.0f;
            MaxSpeed = 10.0f;
            MaxForce = 1.0f;
            Forward = Vector3.UnitX;
            
            Behaviors = new FlockingBehaviorCollection();
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
                                     Behaviors[i].Update(gameTime, this)))
                {
                    break;
                }
            }

            Vector3 acceleration = force / Mass;

            velocity += acceleration;

            Speed = velocity.Length();

            // Stop when speed is too small
            if (Speed < StopSpeed && acceleration.LengthSquared() <= 0)
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

            Position += velocity * elapsedTime;

            if (Speed > 0)
            {
                Forward = Vector3.Normalize(velocity);
            }
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