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
namespace Nine.Navigation.SteeringBehaviors
{
    #region ObstacleAvoidanceBehavior
    public sealed class ObstacleAvoidanceBehavior : ISteeringBehavior
    {
        public float DetectorLength { get; set; }
        public ISpatialQuery Obstacles { get; set; }
        public BoundingSphere NearestObstacle { get; private set; }

        public ObstacleAvoidanceBehavior()
        {
            DetectorLength = 4.0f;
        }

        public Vector3 CalculateForce(GameTime gameTime, IMovable movingEntity)
        {
            NearestObstacle = new BoundingSphere();

            float nearestObstacle = float.MaxValue;
            Vector2 nearestSteering = Vector2.Zero;

            float detector = movingEntity.BoundingRadius + (movingEntity.Speed / movingEntity.MaxSpeed) * DetectorLength;
            
            foreach (object o in Obstacles.Find(movingEntity.Position, detector))
            {
                BoundingSphere obstacle;

                if (o is BoundingSphere)
                {
                    obstacle = (BoundingSphere)o;
                }
                else if (o is IMovable && o != movingEntity)
                {
                    obstacle.Center = (o as IMovable).Position;
                    obstacle.Radius = (o as IMovable).BoundingRadius;
                }
                else
                {
                    continue;
                }

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

            return new Vector3(nearestSteering, 0);
        }

        public bool WillPenetrate(Vector3 from, Vector3 to, IMovable movingEntity)
        {
            if (Penetrates(from, movingEntity))
                return false;

            return from != to && Penetrates(to, movingEntity);
        }

        private bool Penetrates(Vector3 pt, IMovable movingEntity)
        {
            float detector = movingEntity.BoundingRadius + DetectorLength + (movingEntity.Speed / movingEntity.MaxSpeed) * DetectorLength;

            foreach (object o in Obstacles.Find(movingEntity.Position, detector))
            {
                BoundingSphere obstacle;

                if (o is BoundingSphere)
                {
                    obstacle = (BoundingSphere)o;
                }
                else if (o is IMovable && o != movingEntity)
                {
                    obstacle.Center = (o as IMovable).Position;
                    obstacle.Radius = (o as IMovable).BoundingRadius;
                }
                else
                {
                    continue;
                }

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
    public sealed class WallAvoidanceBehavior : ISteeringBehavior
    {
        public Vector3 CalculateForce(GameTime gameTime, IMovable movingEntity)
        {
            return Vector3.Zero;
        }

        public bool WillPenetrate(Vector3 from, Vector3 to, IMovable movingEntity)
        {
            return false;
        }
    }
    #endregion

    #region SurfaceAvoidBehavior
    public sealed class SurfaceAvoidanceBehavior : ISteeringBehavior
    {
        public ISurface Celling { get; set; }
        public ISurface Floor { get; set; }

        public Vector3 CalculateForce(GameTime gameTime, IMovable movingEntity)
        {
            return Vector3.Zero;
        }

        public bool WillPenetrate(Vector3 from, Vector3 to, IMovable movingEntity)
        {
            return false;
        }
    }
    #endregion

    #region HideBehavior
    public sealed class HideBehavior : ISteeringBehavior
    {
        public Vector3 CalculateForce(GameTime gameTime, IMovable movingEntity)
        {
            return Vector3.Zero;
        }

        public bool WillPenetrate(Vector3 from, Vector3 to, IMovable movingEntity)
        {
            return false;
        }
    }
    #endregion
    
    #region BoundAvoidanceBehavior
    public sealed class BoundAvoidanceBehavior : ISteeringBehavior
    {
        public float Skin { get; set; }
        public float Elasticity { get; set; }
        public BoundingBox Bounds { get; set; }

        public BoundAvoidanceBehavior() 
        {
            Skin = 2;
            Elasticity = MathHelper.E;

            Bounds = new BoundingBox(Vector3.One * float.MinValue, 
                                     Vector3.One * float.MaxValue); 
        }

        public Vector3 CalculateForce(GameTime gameTime, IMovable movingEntity)
        {
            Vector3 result = Vector3.Zero;

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
            // Min Z
            if (movingEntity.Position.Z < Bounds.Min.Z + Skin)
            {
                result.Z = movingEntity.MaxSpeed * Evaluate((Bounds.Min.Z + Skin - movingEntity.Position.Z) / Skin);
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
            // Max Z
            if (movingEntity.Position.Z > Bounds.Max.Z - Skin)
            {
                result.Z = -movingEntity.MaxSpeed * Evaluate((movingEntity.Position.Z - Bounds.Max.Z + Skin) / Skin);
            }

            return result;
        }

        private float Evaluate(float value)
        {            
            return (float)((Math.Pow(Elasticity, value) - 1) / (Elasticity - 1));
        }

        public bool WillPenetrate(Vector3 from, Vector3 to, IMovable movingEntity)
        {
            return false;
        }
    }
    #endregion
}