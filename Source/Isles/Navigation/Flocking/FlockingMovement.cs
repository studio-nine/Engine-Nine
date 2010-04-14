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
    public interface IFlockingBehavior
    {
        IMovable Movable { get; set; }
        
        Vector3 Update(GameTime gameTime);
    }


    public sealed class FlockingBehaviorCollection : Collection<IFlockingBehavior>
    {
        internal FlockingBehaviorCollection() { }

        internal IMovable Movable;

        internal void SetMovable(IMovable movable)
        {
            Movable = movable;

            foreach (IFlockingBehavior behavior in this)
                behavior.Movable = movable;
        }

        protected override void InsertItem(int index, IFlockingBehavior item)
        {
            if (item == null)
                throw new ArgumentNullException();

            item.Movable = Movable;

            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, IFlockingBehavior item)
        {
            if (item == null)
                throw new ArgumentNullException();

            item.Movable = Movable;

            base.SetItem(index, item);
        }
    }


    public class FlockingMovement : IMovable
    {
        private IMovable movable;

        public IMovable Movable
        {
            get { return movable; }
            set { movable = value; Behaviors.SetMovable(value); }
        }

        public Vector3 Position
        {
            get { return movable.Position; }
            set { movable.Position = value; }
        }

        public Vector3 Forward
        {
            get { return movable.Forward; }
        }

        public Matrix Transform
        {
            get { return movable.Transform; }
        }

        public float Speed
        {
            get { return movable.Speed; }
        }

        public float MaxSpeed
        {
            get { return movable.MaxSpeed; }
        }

        public FlockingBehaviorCollection Behaviors { get; private set; }

        public Vector3 Force { get; private set; }
        public float Mass { get; set; }


        public FlockingMovement()
        {
            Mass = 1.0f;
            Behaviors = new FlockingBehaviorCollection();
        }

        public FlockingMovement(IMovable innerMovable)
        {
            Behaviors = new FlockingBehaviorCollection();
            Movable = innerMovable;
        }

        public void ApplyForce(Vector3 steeringForce)
        {
            Movable.ApplyForce(steeringForce);
        }

        public void Update(GameTime gameTime)
        {
            float multiplier = Mass;

            if (gameTime.ElapsedGameTime.TotalSeconds > 0)
            {
                multiplier = (float)(Mass / (gameTime.ElapsedGameTime.TotalSeconds *
                                             gameTime.ElapsedGameTime.TotalSeconds));
            }

            foreach (IFlockingBehavior behavior in Behaviors)
            {
                Force += multiplier *behavior.Update(gameTime);
            }

            movable.ApplyForce(Force);
            movable.Update(gameTime);

            Force = Vector3.Zero;
        }
    }
}