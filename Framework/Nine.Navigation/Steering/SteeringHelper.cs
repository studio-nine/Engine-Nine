namespace Nine.Navigation.Steering
{
    using System;
    using Microsoft.Xna.Framework;

    internal static class SteeringHelper
    {
        public const int MaxAffectingEntities = 4;
        public const float Epsilon = 0.001f;
        public const float AvoidanceAngularEpsilon = 0.9f;

        /// <summary>
        /// Finds the miminum distance required to fully stop the moving entity from top speed.
        /// </summary>
        public static float GetDecelerateRange(Steerable movingEntity)
        {
            return movingEntity.Speed * movingEntity.Speed * 0.5f / movingEntity.Acceleration;
        }

        //static float HintRatio = 0.002f;
        static float HintRatio =  1 / (float)Math.Sin(MathHelper.ToRadians(45));

        /// <summary>
        /// Gets the forward vector that are influenced by the target.
        /// </summary>
        public static Vector2 GetTargetedForward(Steerable movingEntity)
        {
            Vector2 targetedForward = movingEntity.Forward;
            if (movingEntity.Target.HasValue)
            {
                if (movingEntity.Speed > 0)
                {
                    targetedForward += Vector2.Normalize(movingEntity.Target.Value - movingEntity.Position) * HintRatio;
                    targetedForward.Normalize();
                }
                else
                {
                    targetedForward = Vector2.Normalize(movingEntity.Target.Value - movingEntity.Position);
                }
            }
            return targetedForward;
        }

        /// <summary>
        /// Gets the forward vector that are influenced by the target.
        /// </summary>
        public static Vector2 GetTargetedForward(Steerable movingEntity, Vector2 relativeForward, float hintRatio, bool zeroSpeed)
        {
            Vector2 targetedForward = relativeForward;
            if (movingEntity.Target.HasValue)
            {
                if (zeroSpeed)
                {
                    targetedForward = Vector2.Normalize(movingEntity.Target.Value - movingEntity.Position);
                }
                else
                {
                    targetedForward += Vector2.Normalize(movingEntity.Target.Value - movingEntity.Position) * hintRatio;
                    targetedForward.Normalize();
                }
            }
            return targetedForward;
        }

        /// <summary>
        /// Calculates the steering force to seek to the target.
        /// </summary>
        public static Vector2 Seek(float elapsedTime, Steerable movingEntity)
        {
            if (!movingEntity.Target.HasValue)
                return Vector2.Zero;

            Vector2 toTarget = Vector2.Normalize(movingEntity.Target.Value - movingEntity.Position);
            float distance = toTarget.Length();
            if (distance > 0)
            {
                Vector2 desiredForce = toTarget * movingEntity.MaxSpeed - movingEntity.Velocity;
                if (Math.Abs(Vector2.Dot(Math2D.Rotate90DegreesCcw(toTarget), movingEntity.Velocity)) < movingEntity.MaxForce * elapsedTime)
                    return toTarget * movingEntity.MaxForce;
                return Vector2.Normalize(desiredForce) * movingEntity.MaxForce;
            }
            return Vector2.Zero;
        }

        /// <summary>
        /// Calculates the steering force to flee from the target.
        /// </summary>
        public static Vector2 Flee(float elapsedTime, Steerable movingEntity, Vector2 target)
        {
            Vector2 toTarget = Vector2.Normalize(movingEntity.Position - target);
            float distance = toTarget.Length();
            if (distance > 0)
            {
                Vector2 desiredForce = toTarget * movingEntity.MaxSpeed - movingEntity.Velocity;
                if (-Vector2.Dot(Math2D.Rotate90DegreesCcw(toTarget), movingEntity.Velocity) < movingEntity.MaxForce * elapsedTime)
                    return toTarget * movingEntity.MaxForce;
                return Vector2.Normalize(desiredForce) * movingEntity.MaxForce;
            }
            return Vector2.Zero;
        }

        /// <summary>
        /// Calculates the steering force to arrive at the target.
        /// </summary>
        public static Vector2 Arrive(float elapsedTime, Steerable movingEntity)
        {
            if (!movingEntity.Target.HasValue)
                return Vector2.Zero;

            Vector2 toTarget = movingEntity.Target.Value - movingEntity.Position;

            // Stop when the moving entity has passed target.
            if (toTarget.Length() <= movingEntity.BoundingRadius + movingEntity.Skin + movingEntity.DecelerationRange)
            {
                if (movingEntity.Velocity != Vector2.Zero && Vector2.Dot(toTarget, movingEntity.Velocity) <= 0)
                {
                    movingEntity.Target = null;
                    return Vector2.Zero;
                }
            }

            // Stop when the moving entity is close enough.
            float distance = toTarget.Length();
            if (distance <= movingEntity.DecelerationRange)
            {
                movingEntity.Target = null;
                return Vector2.Zero;
            }

            return SteeringHelper.Seek(elapsedTime, movingEntity);
        }

        /// <summary>
        /// Calculates the steering force to avoid a line segment.
        /// </summary>
        public static Vector2 AvoidWall(LineSegment line, float elapsedTime, Steerable movingEntity, Vector2 targetedForward)
        {
            System.Diagnostics.Debug.Assert(movingEntity.Target.HasValue);
            
            // Check if the entity has approached the target
            Vector2 toTarget = movingEntity.Target.Value - movingEntity.Position;
            if (toTarget.Length() <= movingEntity.BoundingRadius + movingEntity.Skin)
                return Vector2.Zero;

            // Allow the entity to move across from back to front.
            if (Vector2.Dot(targetedForward, line.Normal) > AvoidanceAngularEpsilon)
                return Vector2.Zero;
                        
            // Check if the entity has already moved through.
            if (Math2D.PointLineRelation(movingEntity.Position + line.Normal * movingEntity.BoundingRadius, line.Start, line.Normal) == Math2D.SpanType.Back)
                return Vector2.Zero;

            // Check if the entity wants to move through.
            if (Math2D.PointLineRelation(movingEntity.Target.Value - line.Normal * movingEntity.BoundingRadius, line.Start, line.Normal) != Math2D.SpanType.Back &&
                Math2D.PointLineRelation(movingEntity.Position - line.Normal * movingEntity.BoundingRadius, line.Start, line.Normal) != Math2D.SpanType.Back)
                return Vector2.Zero;

            // Check if the target position is in front of the line but the distance to line is less than bounding radius.
            Vector2 lineToEntity;
            if (Math2D.DistanceToLine(line.Start, line.End, movingEntity.Position) > movingEntity.BoundingRadius) 
            {
                float targetToLine = Math2D.DistanceToLineSegment(line.Start, line.End, movingEntity.Target.Value, out lineToEntity);
                if (targetToLine <= movingEntity.BoundingRadius)
                {
                    movingEntity.Target = movingEntity.Target.Value + lineToEntity * (movingEntity.BoundingRadius - targetToLine);
                    return Vector2.Zero;
                }
            }

            float distance = Math2D.DistanceToLineSegment(line.Start, line.End, movingEntity.Position, out lineToEntity);
            float decelerateRange = Vector2.Dot(movingEntity.Forward, -line.Normal) * movingEntity.DecelerationRange * 2;
            
            // If deceleration range is too small, like when the moving entity has a maximum acceleration, there won't be
            // enough space for it to turn or stop.
            if (decelerateRange < movingEntity.Skin)
                decelerateRange = movingEntity.Skin;

            if (decelerateRange + movingEntity.Skin + movingEntity.BoundingRadius >= distance)
            {
                Vector2 lineDirection = Math2D.Rotate90DegreesCcw(lineToEntity);

                // Determine which direction to move across the wall that might takes less time to reach the target.
                if (Vector2.Dot(lineDirection, targetedForward) < 0)
                    lineDirection = -lineDirection;

                // Moves the entity along the wall.
                float penetration = movingEntity.BoundingRadius + movingEntity.Skin - distance;
                if (Vector2.Dot(lineDirection, movingEntity.Forward) > AvoidanceAngularEpsilon && penetration < 0)
                    return lineDirection * movingEntity.MaxForce;
                
                // If somehow the entity has penetrate the wall, this force will pull the entity out.
                if (penetration > 0)
                    lineDirection += penetration / movingEntity.Skin * lineToEntity;

                Vector2 desiredForce = lineDirection * movingEntity.MaxSpeed - movingEntity.Velocity;
                return Vector2.Normalize(desiredForce) * movingEntity.MaxForce;
            }

            return Vector2.Zero;
        }

        /// <summary>
        /// Calculates the steering force to avoid a circular obstacle.
        /// </summary>
        public static Vector2 AvoidObstacle(BoundingCircle obstacle, float elapsedTime, Steerable movingEntity, Vector2 targetedForward)
        {
            return Vector2.Zero;
        }
    }
}