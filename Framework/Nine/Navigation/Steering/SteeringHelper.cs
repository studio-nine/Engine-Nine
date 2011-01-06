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
    internal static class SteeringHelper
    {
        const float Epsilon = 0.001f;
        const float AngularEpsilon = 0.99f;

        /// <summary>
        /// Finds the miminum distance required to fully stop the moving entity from top speed.
        /// </summary>
        public static float GetDecelerateRange(ISteerable movingEntity)
        {
            return movingEntity.Speed * movingEntity.Speed * 0.5f / movingEntity.Acceleration;
        }

        /// <summary>
        /// Finds the miminum distance required to fully stop the moving entity from top speed.
        /// </summary>
        public static float GetMaxDecelerateRange(ISteerable movingEntity)
        {
            return movingEntity.MaxSpeed * movingEntity.MaxSpeed * 0.5f / movingEntity.Acceleration;
        }

        /// <summary>
        /// Calculates the steering force to seek to the target.
        /// </summary>
        public static Vector2 SeekToTarget(ISteerable movingEntity, Vector2 target)
        {
            Vector2 toTarget = Vector2.Normalize(target - movingEntity.Position);
            float distance = toTarget.Length();
            if (distance > 0)
            {
                Vector2 desiredForce = toTarget * movingEntity.MaxSpeed - movingEntity.Velocity;
                if (Vector2.Dot(toTarget, movingEntity.Forward) > AngularEpsilon)
                    return toTarget * movingEntity.MaxForce;
                return Vector2.Normalize(desiredForce) * movingEntity.MaxForce;
            }
            return Vector2.Zero;
        }

        /// <summary>
        /// Calculates the steering force to flee from the target.
        /// </summary>
        public static Vector2 FleeFromTarget(ISteerable movingEntity, Vector2 target)
        {
            Vector2 toTarget = Vector2.Normalize(movingEntity.Position - target);
            float distance = toTarget.Length();
            if (distance > 0)
            {
                Vector2 desiredForce = toTarget * movingEntity.MaxSpeed - movingEntity.Velocity;
                if (Vector2.Dot(toTarget, movingEntity.Forward) > AngularEpsilon)
                    return toTarget * movingEntity.MaxForce;
                return Vector2.Normalize(desiredForce) * movingEntity.MaxForce;
            }
            return Vector2.Zero;
        }

        /// <summary>
        /// Calculates the steering force to avoid a line segment.
        /// </summary>
        public static Vector2 AvoidLineSegment(LineSegment line, float elapsedTime, ISteerable movingEntity, Vector2? hint, Vector2 hintedForward)
        {
            // Allow the entity to move across from back to front.
            if (Vector2.Dot(hintedForward, line.Normal) > AngularEpsilon)
                return Vector2.Zero;
                        
            // Check if the entity has already moved through.
            if (Math2D.PointLineRelation(movingEntity.Position, line.Start, line.Normal) == Math2D.SpanType.Back)
                return Vector2.Zero;

            // Check if the entity wants to move through.
            if (hint.HasValue && Math2D.PointLineRelation(hint.Value - line.Normal * movingEntity.BoundingRadius, line.Start, line.Normal) == Math2D.SpanType.Front)
                return Vector2.Zero;

            float distance = Math2D.DistanceToLineSegment(line.Start, line.End, movingEntity.Position);
            float decelerateRange = Vector2.Dot(movingEntity.Forward, -line.Normal) * SteeringHelper.GetDecelerateRange(movingEntity) * 2;
            //float decelerateRange = SteeringHelper.GetMaxDecelerateRange(movingEntity) * 2;

            if (distance < decelerateRange + movingEntity.BoundingRadius + movingEntity.MaxSpeed * elapsedTime)
            {
                Vector2 lineDirection = Vector2.Normalize(line.Start - line.End);
                if (Vector2.Dot(lineDirection, hintedForward) < 0)
                    lineDirection = -lineDirection;

                if (Vector2.Dot(lineDirection, movingEntity.Forward) > AngularEpsilon)
                    return lineDirection * movingEntity.MaxForce;

                Vector2 desiredForce = lineDirection * movingEntity.MaxSpeed - movingEntity.Velocity;
                return Vector2.Normalize(desiredForce) * movingEntity.MaxForce;
            }
            return Vector2.Zero;
        }
    }
}