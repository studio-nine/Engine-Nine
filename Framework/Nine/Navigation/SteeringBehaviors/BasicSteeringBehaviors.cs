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
    #region SeekBehavior
    public sealed class SeekBehavior : ISteeringBehavior
    {
        public Vector3 Target { get; set; }

        public Vector3 CalculateForce(GameTime gameTime, IMovable movingEntity)
        {
            Vector3 toTarget = Target - movingEntity.Position;

            float dist = toTarget.Length();

            if (dist > 0)
                return Vector3.Normalize(toTarget) * movingEntity.MaxSpeed - movingEntity.Forward * movingEntity.Speed;

            return Vector3.Zero;
        }

        public bool WillPenetrate(Vector3 from, Vector3 to, IMovable movingEntity)
        {
            return false;
        }
    }
    #endregion

    #region FleeBehavior
    public sealed class FleeBehavior : ISteeringBehavior
    {
        public Vector3 Target { get; set; }

        public Vector3 CalculateForce(GameTime gameTime, IMovable movingEntity)
        {
            Vector3 toTarget = Target - movingEntity.Position;

            float dist = toTarget.Length();

            if (dist > 0)
                return Vector3.Normalize(movingEntity.Position - Target) * movingEntity.MaxSpeed - movingEntity.Forward * movingEntity.Speed;

            return Vector3.Zero;
        }

        public bool WillPenetrate(Vector3 from, Vector3 to, IMovable movingEntity)
        {
            return false;
        }
    }
    #endregion

    #region ArriveBehavior
    public sealed class ArriveBehavior : ISteeringBehavior
    {
        SeekBehavior seek = new SeekBehavior();

        public Vector3 Target { get; set; }
        public float Deceleration { get; set; }
        public float DecelerateRange { get; set; }
        public float StopRange { get; set; }

        public ArriveBehavior()
        {
            StopRange = 0.1f;
            Deceleration = 0.2f;
            DecelerateRange = 4;
        }

        public Vector3 CalculateForce(GameTime gameTime, IMovable movingEntity)
        {
            Vector3 toTarget = Target - movingEntity.Position;

            //calculate the distance to the target
            float dist = toTarget.Length();

            if (dist <= StopRange)
            {
                return Vector3.Zero;
            }

            if (dist < DecelerateRange)
            {
                //calculate the speed required to reach the target given the desired
                //deceleration
                float speed = dist / Deceleration;

                //make sure the velocity does not exceed the max
                speed = Math.Min(speed, movingEntity.MaxSpeed);

                //from here proceed just like Seek except we don't need to normalize 
                //the ToTarget vector because we have already gone to the trouble
                //of calculating its length: dist. 
                Vector3 desiredVelocity = toTarget * speed / dist;

                return desiredVelocity - movingEntity.Forward * movingEntity.Speed;
            }
            else
            {
                // Seek
                seek.Target = Target;
                return seek.CalculateForce(gameTime, movingEntity);
            }
        }

        public bool WillPenetrate(Vector3 from, Vector3 to, IMovable movingEntity)
        {
            return false;
        }
    }
    #endregion

    #region PursuitBehavior
    public sealed class PursuitBehavior : ISteeringBehavior
    {
        private SeekBehavior seek = new SeekBehavior();

        public Vector3 Offset { get; set; }
        public IMovable Evader { get; set; }

        public Vector3 CalculateForce(GameTime gameTime, IMovable movingEntity)
        {
            //if the evader is ahead and facing the agent then we can just seek
            //for the evader's current position.
            Vector3 toEvader = Evader.Position - movingEntity.Position;

            float relativeHeading = Vector3.Dot(movingEntity.Forward, Evader.Forward);

            if ((Vector3.Dot(toEvader, movingEntity.Forward) > 0) &&
                (relativeHeading < -0.95f))  //acos(0.95)=18 degs
            {
                seek.Target = Evader.Position + Offset;
                return seek.CalculateForce(gameTime, movingEntity);
            }

            //Not considered ahead so we predict where the evader will be.

            //the lookahead time is propotional to the distance between the evader
            //and the pursuer; and is inversely proportional to the sum of the
            //agent's velocities
            float lookAheadTime = toEvader.Length() / (movingEntity.MaxSpeed + Evader.Speed);

            //now seek to the predicted future position of the evader
            seek.Target = Evader.Position + Evader.Speed * lookAheadTime * Evader.Forward + Offset;
            return seek.CalculateForce(gameTime, movingEntity);
        }

        public bool WillPenetrate(Vector3 from, Vector3 to, IMovable movingEntity)
        {
            return false;
        }
    }
    #endregion

    #region EvadeBehavior
    public sealed class EvadeBehavior : ISteeringBehavior
    {
        private FleeBehavior flee = new FleeBehavior();

        public IMovable Pursuer { get; set; }
        public float ThreatRange { get; set; }

        public EvadeBehavior()
        {
            ThreatRange = 100.0f;
        }

        public Vector3 CalculateForce(GameTime gameTime, IMovable movingEntity)
        {
            // Not necessary to include the check for facing direction this time
            Vector3 toPursuer = Pursuer.Position - movingEntity.Position;

            //uncomment the following two lines to have Evade only consider pursuers 
            //within a 'threat range'
            if (toPursuer.LengthSquared() > ThreatRange * ThreatRange)
            {
                return Vector3.Zero;
            }

            //the lookahead time is propotional to the distance between the pursuer
            //and the pursuer; and is inversely proportional to the sum of the
            //agents' velocities
            float LookAheadTime = toPursuer.Length() / (movingEntity.MaxSpeed + Pursuer.Speed);

            //now flee away from predicted future position of the pursuer
            flee.Target = Pursuer.Position + Pursuer.Speed * LookAheadTime * Pursuer.Forward;
            return flee.CalculateForce(gameTime, movingEntity);
        }

        public bool WillPenetrate(Vector3 from, Vector3 to, IMovable movingEntity)
        {
            return false;
        }
    }
    #endregion

    #region WanderBehavior
    public sealed class WanderBehavior : ISteeringBehavior
    {
        public float Rate { get; set; }
        public Random Random { get; set; }

        static Random StaticRandom = new Random();

        public WanderBehavior()
        {
            Rate = 10.0f;
            Random = StaticRandom;
        }

        public Vector3 CalculateForce(GameTime gameTime, IMovable movingEntity)
        {
            Vector3 wander = new Vector3();

            //this behavior is dependent on the update rate, so this line must
            //be included when using time independent framerate.
            float jitterThisTimeSlice = (float)(Rate * gameTime.ElapsedGameTime.TotalSeconds);

            //first, add a small random vector to the target's position
            wander.X = (float)(Random.NextDouble() * 2 - 1) * jitterThisTimeSlice;
            wander.Y = (float)(Random.NextDouble() * 2 - 1) * jitterThisTimeSlice;
            //wander.Z = (float)(Random.NextDouble() * 2 - 1) * jitterThisTimeSlice;
            wander.Z = 0;
            
            //reproject this new vector back on to a unit circle
            return Vector3.Normalize(wander);
        }

        public bool WillPenetrate(Vector3 from, Vector3 to, IMovable movingEntity)
        {
            return false;
        }
    }
    #endregion

    #region IdleBehavior
    public sealed class IdleBehavior : ISteeringBehavior
    {
        private float timer;
        private float idleTime;
        private Vector3 location;

        public Random Random { get; set; }
        public float Activity { get; set; }
        public float Range { get; set; }
        public ArriveBehavior Arrive { get; private set; }

        static Random StaticRandom = new Random();

        public IdleBehavior()
        {
            timer = 0;
            idleTime = 0;
            location = Vector3.One * float.MinValue;

            Activity = 1.0f;
            Range = 100.0f;
            Random = StaticRandom;

            Arrive = new ArriveBehavior();
        }

        public Vector3 CalculateForce(GameTime gameTime, IMovable movingEntity)
        {
            if (Activity < 0f)
                Activity = 0f;

            // Check if we are outside our moving range
            if (Vector3.Subtract(movingEntity.Position, location).LengthSquared() > Range * Range)
            {
                idleTime = (float)(Random.NextDouble() * 10 / (Activity + 0.001f));
                location = movingEntity.Position;
            }

            // Select a random location when the timer expires
            timer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (timer > 0)
                return Arrive.CalculateForce(gameTime, movingEntity);

            if (timer < -idleTime)
            {
                Vector3 target = new Vector3();

                float a = (float)(Random.NextDouble() * Math.PI * 2);
                float b = (float)((Random.NextDouble() * 2 - 1) * Math.PI);
                float r = (float)(Math.Cos(b) * Range);

                target.X = (float)(Math.Cos(a) * r);
                target.Y = (float)(Math.Sin(a) * r);
                target.Z = (float)(Math.Sin(b) * Range);

                Arrive.Target = target + location;

                // Estimate arrive time
                float distance = Vector3.Subtract(Arrive.Target, movingEntity.Position).Length();

                float estimateTime = distance / movingEntity.MaxSpeed;

                idleTime = (float)(Random.NextDouble() * 10 / (Activity + 0.001f));
                timer = estimateTime;
            }

            return Vector3.Zero;
        }

        public bool WillPenetrate(Vector3 from, Vector3 to, IMovable movingEntity)
        {
            return false;
        }
    }
    #endregion
}