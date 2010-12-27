#region Copyright 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2010 (c) Engine Nine. All Rights Reserved.
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
namespace Nine.Navigation.Steering
{
    #region ObstacleAvoidanceBehavior
    public class ObstacleAvoidanceBehavior : SteeringBehavior
    {
        public float DetectorLength { get; set; }
        public ISpatialQuery<ISteerable> Obstacles { get; set; }
        public BoundingSphere NearestObstacle { get; private set; }

        public ObstacleAvoidanceBehavior()
        {
            DetectorLength = 4.0f;
        }

        protected override Vector2 OnUpdateSteeringForce(float elapsedTime, ISteerable movingEntity)
        {
            NearestObstacle = new BoundingSphere();

            float nearestObstacle = float.MaxValue;
            Vector2 nearestSteering = Vector2.Zero;

            float detector = movingEntity.BoundingRadius + (movingEntity.Speed / movingEntity.MaxSpeed) * DetectorLength;

            foreach (ISteerable o in Obstacles.Find(new Vector3(movingEntity.Position, 0), detector))
            {
                BoundingSphere obstacle;

                if (o == movingEntity)
                    continue;

                obstacle.Center = new Vector3((o as ISteerable).Position, 0);
                obstacle.Radius = (o as ISteerable).BoundingRadius;

                float minDistanceToCenter = DetectorLength + obstacle.Radius;
                float totalRadius = obstacle.Radius + movingEntity.BoundingRadius;
                Vector2 localOffset = new Vector2();

                localOffset.X = obstacle.Center.X - movingEntity.Position.X;
                localOffset.Y = obstacle.Center.Y - movingEntity.Position.Y;

                Vector2 forward = new Vector2();

                forward.X = movingEntity.Forward.X;
                forward.Y = movingEntity.Forward.Y;
                forward.Normalize();

                float forwardComponent = Vector2.Dot(localOffset, forward);
                Vector2 forwardOffset = forwardComponent * forward;

                Vector2 offForwardOffset = localOffset - forwardOffset;

                bool inCylinder = offForwardOffset.Length() < totalRadius;
                bool nearby = forwardComponent < minDistanceToCenter;
                bool inFront = forwardComponent > 0;

                if (inCylinder && inFront && nearby)
                {
                    float length = offForwardOffset.Length();

                    if (length < nearestObstacle)
                    {
                        NearestObstacle = obstacle;
                        nearestObstacle = length;
                        nearestSteering = offForwardOffset * -1;
                    }
                }
            }

            return nearestSteering;
        }

        protected override bool OnCollides(Vector2 from, Vector2 to, ISteerable movingEntity)
        {
            if (Penetrates(from, movingEntity))
                return false;

            return from != to && Penetrates(to, movingEntity);
        }

        private bool Penetrates(Vector2 pt, ISteerable movingEntity)
        {
            float detector = movingEntity.BoundingRadius + DetectorLength + (movingEntity.Speed / movingEntity.MaxSpeed) * DetectorLength;

            foreach (ISteerable o in Obstacles.Find(new Vector3(movingEntity.Position, 0), detector))
            {
                BoundingSphere obstacle;
                
                if (o == movingEntity)
                    continue;
                
                obstacle.Center = new Vector3((o as ISteerable).Position, 0);
                obstacle.Radius = (o as ISteerable).BoundingRadius;
                
                Vector2 v = new Vector2();

                v.X = obstacle.Center.X - pt.X;
                v.Y = obstacle.Center.Y - pt.Y;

                if (v.Length() < movingEntity.BoundingRadius + obstacle.Radius)
                    return true;
            }

            return false;
        }
    }
    #endregion

    #region WallAvoidanceBehavior
    public class WallAvoidanceBehavior : SteeringBehavior
    {
        protected override Vector2 OnUpdateSteeringForce(float elapsedTime, ISteerable movingEntity)
        {
            return Vector2.Zero;
        }

        protected override bool OnCollides(Vector2 from, Vector2 to, ISteerable movingEntity)
        {
            return false;
        }
    }
    #endregion

    #region SurfaceAvoidBehavior
    public class SurfaceAvoidanceBehavior : SteeringBehavior
    {
        public ISurface Celling { get; set; }
        public ISurface Floor { get; set; }

        protected override Vector2 OnUpdateSteeringForce(float elapsedTime, ISteerable movingEntity)
        {
            return Vector2.Zero;
        }

        protected override bool OnCollides(Vector2 from, Vector2 to, ISteerable movingEntity)
        {
            return false;
        }
    }
    #endregion

    #region HideBehavior
    public class HideBehavior : SteeringBehavior
    {
        protected override Vector2 OnUpdateSteeringForce(float elapsedTime, ISteerable movingEntity)
        {
            return Vector2.Zero;
        }

        protected override bool OnCollides(Vector2 from, Vector2 to, ISteerable movingEntity)
        {
            return false;
        }
    }
    #endregion
    
    #region BoundAvoidanceBehavior
    public class BoundAvoidanceBehavior : SteeringBehavior
    {
        public float Skin { get; set; }
        public float Elasticity { get; set; }
        public BoundingRectangle Bounds { get; set; }

        public BoundAvoidanceBehavior() 
        {
            Skin = 2;
            Elasticity = MathHelper.E;
            Bounds = new BoundingRectangle(Vector2.One * float.MinValue, 
                                           Vector2.One * float.MaxValue); 
        }

        protected override Vector2 OnUpdateSteeringForce(float elapsedTime, ISteerable movingEntity)
        {
            Vector2 result = Vector2.Zero;

            // Min X
            if (movingEntity.Position.X < Bounds.Min.X + Skin)
            {
                result.X = movingEntity.MaxSpeed * Evaluate((Bounds.Min.X + Skin - movingEntity.Position.X) / Skin);
            }
            // Min Y
            if (movingEntity.Position.Y < Bounds.Min.Y + Skin)
            {
                result.Y = movingEntity.MaxSpeed * Evaluate((Bounds.Min.Y + Skin - movingEntity.Position.Y) / Skin);
            }
            // Max X
            if (movingEntity.Position.X > Bounds.Max.X - Skin)
            {
                result.X = -movingEntity.MaxSpeed * Evaluate((movingEntity.Position.X - Bounds.Max.X + Skin) / Skin);
            }
            // Max Y
            if (movingEntity.Position.Y > Bounds.Max.Y - Skin)
            {
                result.Y = -movingEntity.MaxSpeed * Evaluate((movingEntity.Position.Y - Bounds.Max.Y + Skin) / Skin);
            }

            return result;
        }

        private float Evaluate(float value)
        {            
            return (float)((Math.Pow(Elasticity, value) - 1) / (Elasticity - 1));
        }

        protected override bool OnCollides(Vector2 from, Vector2 to, ISteerable movingEntity)
        {
            return false;
        }
    }
    #endregion
}