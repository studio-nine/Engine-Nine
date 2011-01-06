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
        float DetectorLength = 4f;
        float FieldOfView = MathHelper.ToRadians(60);
        float HintRatio = 1f;

        public BoundingCircle? CurrentObstacle;

        public Vector2? TargetHint { get; set; }
        public ISpatialQuery<BoundingCircle> Obstacles { get; set; }

        protected override Vector2 OnUpdateSteeringForce(float elapsedTime, ISteerable movingEntity)
        {
            CurrentObstacle = null;

            float detectorLength = movingEntity.BoundingRadius + movingEntity.MaxSpeed * elapsedTime;

            foreach (BoundingCircle obstacle in Obstacles.Find(new Vector3(movingEntity.Position, 0), detectorLength + DetectorLength))
            {
                Vector2 hintedForward = movingEntity.Forward;
                if (TargetHint.HasValue)
                {
                    hintedForward += Vector2.Normalize(TargetHint.Value - movingEntity.Position) * HintRatio;
                    hintedForward.Normalize();
                }

                Vector2 toTarget = obstacle.Center - movingEntity.Position;
                float distance = toTarget.Length();
                if (distance > detectorLength + obstacle.Radius)
                    continue;

                toTarget.Normalize();

                float theta = (float)Math.Acos(Vector2.Dot(toTarget, hintedForward));
                if (theta > FieldOfView / 2)
                {
                    float distanceToCore = (float)Math.Sin(theta - FieldOfView / 2) * distance;
                    if (distanceToCore >= movingEntity.BoundingRadius + obstacle.Radius)
                        continue;
                }

                CurrentObstacle = obstacle;

                Vector2 force = new Vector2();
                float rotation = (float)Math.Atan2(hintedForward.Y, hintedForward.X);
                Vector2 LocalPos = Math2D.WorldToLocal(obstacle.Center, movingEntity.Position, rotation);
                int sign = LocalPos.Y >= 0 ? 1 : -1;
                force.X = sign * toTarget.Y * movingEntity.MaxSpeed;
                force.Y = -sign * toTarget.X * movingEntity.MaxSpeed;

                return Vector2.Normalize(force - movingEntity.Velocity) * movingEntity.MaxForce;
            }

            return Vector2.Zero;
        }

        protected override float? OnCollides(Vector2 from, Vector2 to, ISteerable movingEntity)
        {
            float detector = movingEntity.BoundingRadius + DetectorLength;

            float closestPenetration = -1;
            foreach (BoundingCircle obstacle in Obstacles.Find(new Vector3(movingEntity.Position, 0), detector))
            {
                if (obstacle.Contains(new BoundingCircle(to, movingEntity.BoundingRadius)) == ContainmentType.Disjoint)
                    continue;

                if (obstacle.Contains(new BoundingCircle(from, movingEntity.BoundingRadius)) != ContainmentType.Disjoint)
                    continue;

                float? closestIntersectionPoint = Math2D.RayCircleIntersectionTest(
                            to, Vector2.Normalize(from - to), obstacle.Center, obstacle.Radius + movingEntity.BoundingRadius);

                if (!closestIntersectionPoint.HasValue)
                    continue;

                float penetration = Vector2.Distance(from, to) - closestIntersectionPoint.Value;
                if (penetration < 0)
                    penetration = 0;                
                if (penetration < closestPenetration || closestPenetration < 0)
                    closestPenetration = penetration;
            }

            if (closestPenetration > 0)
            {
                return closestPenetration;
            }
            return null;
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
    }
    #endregion

    #region WallAvoidanceBehavior
    public class WallAvoidanceBehavior : SteeringBehavior
    {
        public float DetectorLength { get; set; }
        public Vector2? TargetHint { get; set; }
        public ISpatialQuery<LineSegment> Walls { get; set; }

        float HintRatio = 1 / (float)Math.Sin(MathHelper.ToRadians(45));
        
        protected override Vector2 OnUpdateSteeringForce(float elapsedTime, ISteerable movingEntity)
        {
            Vector2 hintedForward = movingEntity.Forward;
            if (TargetHint.HasValue)
            {
                hintedForward += Vector2.Normalize(TargetHint.Value - movingEntity.Position) * HintRatio;
                hintedForward.Normalize();
            }

            float detectorLength = movingEntity.BoundingRadius + movingEntity.MaxSpeed * elapsedTime * 8;

            foreach (LineSegment line in Walls.Find(new Vector3(movingEntity.Position, 0), detectorLength))
            {
                return SteeringHelper.AvoidLineSegment(line, elapsedTime, movingEntity, TargetHint, hintedForward);
            }

            return Vector2.Zero;
        }

        protected override float? OnCollides(Vector2 from, Vector2 to, ISteerable movingEntity)
        {
            float detectorLength = movingEntity.BoundingRadius;

            float closestPenetration = -1;
            foreach (LineSegment line in Walls.Find(new Vector3(movingEntity.Position, 0), detectorLength))
            {
                //if (Vector2.Dot(Vector2.Subtract(to, from), line.Normal) > 0)
                //    continue;

                LineSegment intersectionLine = line;
                intersectionLine.Offset(movingEntity.BoundingRadius);

                //if (Math2D.PointLineRelation(to - line.Normal * movingEntity.BoundingRadius, line.Start, line.Normal) == Math2D.SpanType.Front)
                //    continue;

                //if (Math2D.PointLineRelation(from + line.Normal * movingEntity.BoundingRadius, line.Start, line.Normal) == Math2D.SpanType.Back)
                //    continue;

                float? intersection = Math2D.LineSegmentIntersectionTest(from, to, intersectionLine.Start, intersectionLine.End);

                if (!intersection.HasValue)
                    continue;

                float penetration = intersection.Value;
                if (penetration < 0)
                    penetration = 0;
                if (penetration < closestPenetration || closestPenetration < 0)
                    closestPenetration = penetration;
            }

            if (closestPenetration > 0)
            {
                
                return closestPenetration;
            }
            return null;
        }
    }
    #endregion
}