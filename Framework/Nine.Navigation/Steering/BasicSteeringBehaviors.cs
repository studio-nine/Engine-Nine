#region Copyright 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using Microsoft.Xna.Framework;

#endregion

//=============================================================================
//
// Special Thanks to 
//      Mat Buckland (fup@ai-junkie.com)
// And his
//      Programming Game AI By Example
//
//=============================================================================
namespace Nine.Navigation.Steering
{
    #region SeekBehavior
    public class SeekBehavior : SteeringBehavior
    {
        protected override Vector2 OnUpdateSteeringForce(float elapsedTime, Steerable movingEntity)
        {
            return SteeringHelper.Seek(elapsedTime, movingEntity);
        }
    }
    #endregion

    #region FleeBehavior
    public class FleeBehavior : SteeringBehavior
    {
        public Vector2 Target { get; set; }

        protected override Vector2 OnUpdateSteeringForce(float elapsedTime, Steerable movingEntity)
        {
            return SteeringHelper.Flee(elapsedTime, movingEntity, Target);
        }
    }
    #endregion
    
    #region ArriveBehavior
    public class ArriveBehavior : SteeringBehavior
    {
        protected override Vector2 OnUpdateSteeringForce(float elapsedTime, Steerable movingEntity)
        {
            return SteeringHelper.Arrive(elapsedTime, movingEntity); 
        }
    }
    #endregion

    #region PursuitBehavior
    public class PursuitBehavior : SteeringBehavior
    {
        public Vector2 Offset { get; set; }
        public Steerable Evader { get; set; }

        protected override Vector2 OnUpdateSteeringForce(float elapsedTime, Steerable movingEntity)
        {
            //if the evader is ahead and facing the agent then we can just seek
            //for the evader's current position.
            Vector2 toEvader = Evader.Position - movingEntity.Position;

            float relativeHeading = Vector2.Dot(movingEntity.Forward, Evader.Forward);

            if ((Vector2.Dot(toEvader, movingEntity.Forward) > 0) &&
                (relativeHeading < -0.95f))  //acos(0.95)=18 degs
            {
                movingEntity.Target = Evader.Position + Offset;
                return SteeringHelper.Seek(elapsedTime, movingEntity);
            }

            //Not considered ahead so we predict where the evader will be.

            //the lookahead time is propotional to the distance between the evader
            //and the pursuer; and is inversely proportional to the sum of the
            //agent's velocities
            float lookAheadTime = toEvader.Length() / (movingEntity.MaxSpeed + Evader.Speed);

            //now seek to the predicted future position of the evader
            movingEntity.Target = Evader.Position + Evader.Velocity * lookAheadTime + Offset;
            return SteeringHelper.Seek(elapsedTime, movingEntity);
        }
    }
    #endregion

    #region EvadeBehavior
    public class EvadeBehavior : SteeringBehavior
    {
        public Steerable Pursuer { get; set; }
        public float ThreatRange { get; set; }

        public EvadeBehavior()
        {
            ThreatRange = 100.0f;
        }

        protected override Vector2 OnUpdateSteeringForce(float elapsedTime, Steerable movingEntity)
        {
            // Not necessary to include the check for facing direction this time
            Vector2 toPursuer = Pursuer.Position - movingEntity.Position;

            //uncomment the following two lines to have Evade only consider pursuers 
            //within a 'threat range'
            if (toPursuer.LengthSquared() > ThreatRange * ThreatRange)
            {
                return Vector2.Zero;
            }

            //the lookahead time is propotional to the distance between the pursuer
            //and the pursuer; and is inversely proportional to the sum of the
            //agents' velocities
            float LookAheadTime = toPursuer.Length() / (movingEntity.MaxSpeed + Pursuer.Speed);

            //now flee away from predicted future position of the pursuer
            return SteeringHelper.Flee(elapsedTime, movingEntity, Pursuer.Position + Pursuer.Velocity * LookAheadTime);
        }
    }
    #endregion

    #region WanderBehavior
    public class WanderBehavior : SteeringBehavior
    {
        public float Jitter { get; set; }
        public float Distance { get; set; }
        public float Radius { get; set; }

        private Vector2 wanderTarget;

        public WanderBehavior()
        {
            Jitter = 80;
            Distance = 8;
            Radius = 5f;
        }

        protected override Vector2 OnUpdateSteeringForce(float elapsedTime, Steerable movingEntity)
        {
            //this behavior is dependent on the update rate, so this line must
            //be included when using time independent framerate.
            float JitterThisTimeSlice = Jitter * elapsedTime;

            //first, add a small random vector to the target's position
            wanderTarget += new Vector2((float)(Random.NextDouble() - Random.NextDouble()) * JitterThisTimeSlice,
                                        (float)(Random.NextDouble() - Random.NextDouble()) * JitterThisTimeSlice);

            //reproject this new vector back on to a unit circle
            wanderTarget.Normalize();

            //increase the length of the vector to the same as the radius
            //of the wander circle
            wanderTarget *= Radius;

            //move the target into a position WanderDist in front of the agent
            Vector2 target = wanderTarget + new Vector2(Distance, 0);

            //project the target into world space
            Vector2 Target = Math2D.LocalToWorld(target, movingEntity.Position, (float)Math.Atan2(
                movingEntity.Forward.Y,
                movingEntity.Forward.X));

            //and steer towards it
            return Vector2.Normalize(Target - movingEntity.Position) * movingEntity.MaxForce; 
        }
    }
    #endregion

    #region PatrolBehavior
    public class PatrolBehavior : SteeringBehavior
    {
        private float timer;
        private Vector2 location;

        public float MovesPerMinute { get; set; }
        public float PatrolRange { get; set; }        

        public PatrolBehavior()
        {
            timer = 0;
            location = Vector2.One * float.MinValue;

            MovesPerMinute = 6.0f;
            PatrolRange = 50.0f;
        }

        protected override Vector2 OnUpdateSteeringForce(float elapsedTime, Steerable movingEntity)
        {
            if (MovesPerMinute < 0f)
                MovesPerMinute = 0f;

            // Check if we are outside our moving range
            if (Vector2.Subtract(movingEntity.Position, location).LengthSquared() > PatrolRange * PatrolRange * 1.2F)
            {
                timer = (float)(Random.NextDouble() * 60 / (MovesPerMinute + 0.001f));
                location = movingEntity.Position;
            }

            // Select a random location when the timer expires
            timer -= (float)elapsedTime;

            if (timer < 0)
            {
                Vector2 target = new Vector2();

                float a = (float)(Random.NextDouble() * Math.PI * 2);
                float r = (float)(Random.NextDouble());

                target.X = (float)(Math.Cos(a) * PatrolRange * r);
                target.Y = (float)(Math.Sin(a) * PatrolRange * r);

                movingEntity.Target = target + location;

                timer = (float)(Random.NextDouble() * 60 / (MovesPerMinute + 0.001f));
            }

            return SteeringHelper.Arrive(elapsedTime, movingEntity);
        }
    }
    #endregion
}