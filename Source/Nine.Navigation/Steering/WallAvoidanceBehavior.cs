namespace Nine.Navigation.Steering
{
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;

    public class WallAvoidanceBehavior : SteeringBehavior
    {
        static List<LineSegment> Lines = new List<LineSegment>();

        public float DetectorLength { get; set; }
        public ISpatialQuery<LineSegment> Walls { get; set; }
        
        protected override Vector2 OnUpdateSteeringForce(float elapsedTime, Steerable movingEntity)
        {
            if (!movingEntity.Target.HasValue)
                return Vector2.Zero;

            // Check if the entity has approached the target
            Vector2 toTarget = movingEntity.Target.Value - movingEntity.Position;
            if (toTarget.Length() <= movingEntity.BoundingRadius + movingEntity.Skin)
                return Vector2.Zero;

            LineSegment? nearestLineSegment = null;
            float minDistanceToLineSq = float.MaxValue;
            float detectorLength = movingEntity.BoundingRadius + movingEntity.DecelerationRange + movingEntity.Skin;

            Vector2 targetedForward = movingEntity.TargetedForward;

            var boundingSphere = new BoundingSphere(new Vector3(movingEntity.Position, 0), detectorLength);
            Walls.FindAll(ref boundingSphere, Lines);

            foreach (var line in Lines)
            {
                // Allow the entity to move across from back to front.
                if (Vector2.Dot(targetedForward, line.Normal) > SteeringHelper.AvoidanceAngularEpsilon)
                    continue;

                // Check if the entity has already moved through.
                if (Math2D.PointLineRelation(movingEntity.Position + line.Normal * movingEntity.BoundingRadius, line.Start, line.Normal) == Math2D.SpanType.Back)
                    continue;

                float distanceSq = Math2D.DistanceToLineSegmentSquared(line.Start, line.End, movingEntity.Position + targetedForward * movingEntity.MaxSpeed * elapsedTime);
                if (distanceSq < minDistanceToLineSq)
                {
                    minDistanceToLineSq = distanceSq;
                    nearestLineSegment = line;
                }
            }
            Lines.Clear();

            if (nearestLineSegment.HasValue)
            {
                LineSegment line = nearestLineSegment.Value;

                // Check if the entity wants to move through.
                // Ignore when both target position and entity position are in front of the wall.
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
                    if (Vector2.Dot(lineDirection, movingEntity.Forward) > SteeringHelper.AvoidanceAngularEpsilon && penetration < 0)
                        return lineDirection * movingEntity.MaxForce;

                    // If somehow the entity has penetrate the wall, this force will pull the entity out.
                    if (penetration > 0)
                        lineDirection += penetration / movingEntity.Skin * lineToEntity;

                    Vector2 desiredForce = lineDirection * movingEntity.MaxSpeed - movingEntity.Velocity;
                    return Vector2.Normalize(desiredForce) * movingEntity.MaxForce;
                }
            }
            return Vector2.Zero;
        }

        protected override float? OnCollides(Vector2 from, Vector2 to, float elapsedTime, Steerable movingEntity)
        {
            float detectorLength = movingEntity.BoundingRadius;

            var boundingSphere = new BoundingSphere(new Vector3(movingEntity.Position, 0), detectorLength);
            Walls.FindAll(ref boundingSphere, Lines);

            foreach (var line in Lines)
            {
                if (Vector2.Dot(Vector2.Subtract(to, from), line.Normal) > 0)
                    continue;

                if (Math2D.PointLineRelation(to - line.Normal * movingEntity.BoundingRadius, line.Start, line.Normal) == Math2D.SpanType.Front)
                    continue;

                if (Math2D.PointLineRelation(from + line.Normal * movingEntity.BoundingRadius, line.Start, line.Normal) == Math2D.SpanType.Back)
                    continue;

                if (Math2D.DistanceToLineSegment(line.Start, line.End, to) < movingEntity.BoundingRadius)
                {
                    Lines.Clear();
                    return 0;
                }
            }
            Lines.Clear();
            return null;
        }
    }
}