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
    public class SeekBehavior : IFlockingBehavior
    {
        public IMovable Movable { get; set; }
        public Vector3 Target { get; set; }

        public Vector3 Update(GameTime gameTime)
        {
            return Vector3.Normalize(Target - Movable.Position) * Movable.MaxSpeed - Movable.Forward * Movable.Speed;
        }
    }


    public class FleeBehavior : IFlockingBehavior
    {
        public IMovable Movable { get; set; }
        public Vector3 Target { get; set; }

        public Vector3 Update(GameTime gameTime)
        {
            return Vector3.Normalize(Movable.Position - Target) * Movable.MaxSpeed - Movable.Forward * Movable.Speed;
        }
    }


    public class ArriveBehavior : IFlockingBehavior
    {
        public IMovable Movable { get; set; }
        public Vector3 Target { get; set; }
        public float Deceleration { get; set; }
        public float StopRange { get; set; }

        public ArriveBehavior()
        {
            StopRange = 0.1f;
            Deceleration = 4.0f;
        }

        public Vector3 Update(GameTime gameTime)
        {
            Vector3 toTarget = Target - Movable.Position;

            //calculate the distance to the target
            float dist = toTarget.Length();

            if (dist <= StopRange)
            {
                return Vector3.Zero;
            }

            //calculate the speed required to reach the target given the desired
            //deceleration
            float speed = dist / Deceleration;

            //make sure the velocity does not exceed the max
            speed = Math.Min(speed, Movable.MaxSpeed);

            //from here proceed just like Seek except we don't need to normalize 
            //the ToTarget vector because we have already gone to the trouble
            //of calculating its length: dist. 
            Vector3 desiredVelocity = toTarget * speed / dist;

            return desiredVelocity - Movable.Forward * Movable.Speed;
        }
    }


    public class PursuitBehavior : IFlockingBehavior
    {
        private SeekBehavior seek = new SeekBehavior();

        public IMovable Movable { get; set; }
        public IMovable Evader { get; set; }
        public Vector3 Force { get; private set; }
        public float FieldOfView { get; set; }

        public PursuitBehavior()
        {
            FieldOfView = 0.95f;
        }

        public Vector3 Update(GameTime gameTime)
        {
            seek.Movable = Movable;

            //if the evader is ahead and facing the agent then we can just seek
            //for the evader's current position.
            Vector3 toEvader = Evader.Position - Movable.Position;

            float relativeHeading = Vector3.Dot(Movable.Forward, Evader.Forward);

            if ((Vector3.Dot(toEvader, Movable.Forward) > 0) &&
                (relativeHeading < -FieldOfView))  //acos(0.95)=18 degs
            {
                seek.Target = Evader.Position;
                return seek.Update(gameTime);
            }

            //Not considered ahead so we predict where the evader will be.

            //the lookahead time is propotional to the distance between the evader
            //and the pursuer; and is inversely proportional to the sum of the
            //agent's velocities
            float lookAheadTime = toEvader.Length() / (Movable.MaxSpeed + Evader.Speed);

            //now seek to the predicted future position of the evader
            seek.Target = Evader.Position + Evader.Speed * lookAheadTime * Evader.Forward;
            return seek.Update(gameTime);
        }
    }


    public class EvadeBehavior : IFlockingBehavior
    {
        private FleeBehavior flee = new FleeBehavior();

        public IMovable Movable { get; set; }
        public IMovable Pursuer { get; set; }
        public Vector3 Force { get; private set; }
        public float ThreatRange { get; set; }

        public EvadeBehavior()
        {
            ThreatRange = 100.0f;
        }

        public Vector3 Update(GameTime gameTime)
        {
            // Not necessary to include the check for facing direction this time
            Vector3 toPursuer = Pursuer.Position - Movable.Position;

            //uncomment the following two lines to have Evade only consider pursuers 
            //within a 'threat range'
            if (toPursuer.LengthSquared() > ThreatRange * ThreatRange)
            {
                return Vector3.Zero;
            }

            //the lookahead time is propotional to the distance between the pursuer
            //and the pursuer; and is inversely proportional to the sum of the
            //agents' velocities
            float LookAheadTime = toPursuer.Length() / (Movable.MaxSpeed + Pursuer.Speed);

            //now flee away from predicted future position of the pursuer
            flee.Movable = Movable;
            flee.Target = Pursuer.Position + Pursuer.Speed * LookAheadTime * Pursuer.Forward;
            return flee.Update(gameTime);
        }
    }


    public class WanderBehavior : IFlockingBehavior
    {
        private Vector3 wanderDirection;
        private float wanderRadius;
        private float wanderDistance;

        public IMovable Movable { get; set; }
        public Vector3 Force { get; private set; }
        public Vector3 Up { get; set; }
        public float Rate { get; set; }
        public Random Random { get; set; }

        public WanderBehavior()
        {
            Rate = 10.0f;
            wanderRadius = (float)Math.Sqrt(2);
            wanderDistance = 1.0f;
            wanderDirection = Vector3.UnitX;
            Up = Vector3.UnitZ;

            Random = new Random();
        }

        public Vector3 Update(GameTime gameTime)
        {
            //this behavior is dependent on the update rate, so this line must
            //be included when using time independent framerate.
            float jitterThisTimeSlice = (float)(Rate * gameTime.ElapsedGameTime.TotalSeconds);

            //first, add a small random vector to the target's position
            wanderDirection.X += (float)(Random.NextDouble() * 2 - 1) * jitterThisTimeSlice;
            wanderDirection.Y += (float)(Random.NextDouble() * 2 - 1) * jitterThisTimeSlice;
            wanderDirection.Z += (float)(Random.NextDouble() * 2 - 1) * jitterThisTimeSlice;
            
            //reproject this new vector back on to a unit circle
            wanderDirection.Normalize();

            //move the target into a position WanderDist in front of the agent
            Vector3 target = wanderDirection + Vector3.Forward * wanderDistance;

            //transform to world space
            Matrix transform = Matrix.Identity;

            transform.Forward = Vector3.Normalize(Movable.Forward);
            transform.Right = Vector3.Cross(transform.Forward, Up);
            transform.Up = Vector3.Cross(transform.Right, transform.Forward);

            //and steer towards it            
            return Vector3.Transform(target, transform);
        }
    }


    public class IdleBehavior : IFlockingBehavior
    {
        private float timer;
        private float idleTime;
        private Vector3 location;
        private ArriveBehavior arrive = new ArriveBehavior();

        public Random Random { get; set; }
        public IMovable Movable { get; set; }
        public float Activity { get; set; }
        public float Range { get; set; }


        public IdleBehavior()
        {
            timer = 0;
            idleTime = 0;
            location = Vector3.One * float.MinValue;

            Activity = 1.0f;
            Range = 100.0f;
            Random = new Random();
        }

        public Vector3 Update(GameTime gameTime)
        {
            arrive.Movable = Movable;

            if (Activity < 0f)
                Activity = 0f;

            // Check if we are outside our moving range
            if (Vector3.Subtract(Movable.Position, location).LengthSquared() > Range * Range)
            {
                idleTime = (float)(Random.NextDouble() * 10 / (Activity + 0.001f));
                location = Movable.Position;
            }

            // Select a random location when the timer expires
            timer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (timer > 0)
                return arrive.Update(gameTime);

            if (timer < -idleTime)
            {
                Vector3 target;

                float a = (float)(Random.NextDouble() * Math.PI * 2);
                float b = (float)((Random.NextDouble() * 2 - 1) * Math.PI);
                float r = (float)(Math.Cos(b) * Range);

                target.X = (float)(Math.Cos(a) * r);
                target.Y = (float)(Math.Sin(a) * r);
                target.Z = (float)(Math.Sin(b) * Range);

                arrive.Target = target + location;

                // Estimate arrive time
                float distance = Vector3.Subtract(arrive.Target, Movable.Position).Length();

                float estimateTime = 4 * distance / Movable.MaxSpeed;

                idleTime = (float)(Random.NextDouble() * 10 / (Activity + 0.001f));
                timer = estimateTime;
            }

            return Vector3.Zero;
        }
    }
}