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

namespace Nine.Navigation.Steering
{
    public class SteerableAvoidanceBehavior : SteeringBehavior
    {
        public ISpatialQuery<Steerable> Neighbors { get; set; }

        protected override Vector2 OnUpdateSteeringForce(float elapsedTime, Steerable movingEntity)
        {
            if (!movingEntity.Target.HasValue)
                return Vector2.Zero;

            AdjustTargetPositionWhenOverlapped(elapsedTime, movingEntity);

            Steerable nearestSteerable = null;
            float minDistanceToSteerable = float.MaxValue;
            float detectorLength = movingEntity.BoundingRadius + movingEntity.Skin;

            // FindAll nearest steerable
            foreach (Steerable partner in Neighbors.FindAll(new Vector3(movingEntity.Position, 0), detectorLength))
            {
                if (partner == null || partner == movingEntity)
                    continue;

                Vector2 toTarget = partner.Position - movingEntity.Position;

                // Ignore entities behind us.
                if (Vector2.Dot(movingEntity.TargetedForward, toTarget) < 0)
                    continue;

                float distance = toTarget.Length();
                
                // Ignore entities too far away
                if (distance > movingEntity.BoundingRadius + partner.BoundingRadius + movingEntity.Skin)
                    continue;

                if (distance < minDistanceToSteerable)
                {
                    minDistanceToSteerable = distance;
                    nearestSteerable = partner;
                }
            }

            if (nearestSteerable != null)
            {
                Steerable partner = nearestSteerable;

                // FindAll the adjacent steerable
                Steerable secondNearestSteerable = null;
                float minDistanceToSecondSteerable = float.MaxValue;
                float secondDetectorLength = movingEntity.BoundingRadius * 2 + movingEntity.Skin * 2 + partner.BoundingRadius;

                foreach (Steerable secondPartner in Neighbors.FindAll(new Vector3(movingEntity.Position, 0), secondDetectorLength))
                {
                    if (secondPartner == null || secondPartner == partner || secondPartner == movingEntity)
                        continue;

                    float minAcceptableDistance = secondDetectorLength + secondPartner.BoundingRadius;
                    if (Vector2.Subtract(secondPartner.Position, partner.Position).LengthSquared() > minAcceptableDistance * minAcceptableDistance)
                        continue;

                    float distance = Math.Abs(Vector2.Dot(secondPartner.Position - movingEntity.Position, movingEntity.Forward));
                    if (distance < 0)
                        continue;

                    if (distance <= minDistanceToSecondSteerable)
                    {
                        minDistanceToSecondSteerable = distance;
                        secondNearestSteerable = secondPartner;
                    }
                }

                if (secondNearestSteerable != null)
                {
                    // Avoid the tangent line segment from two partners
                    Vector2 start, end;
                    Vector2 lineNormal = Math2D.Rotate90DegreesCcw(nearestSteerable.Position - secondNearestSteerable.Position);
                    if (Vector2.Dot(lineNormal, movingEntity.Position - nearestSteerable.Position) < 0)
                    {
                        lineNormal = -lineNormal;
                        start = nearestSteerable.Position + lineNormal * nearestSteerable.BoundingRadius;
                        end = secondNearestSteerable.Position + lineNormal * secondNearestSteerable.BoundingRadius;
                    }
                    else
                    {
                        start = secondNearestSteerable.Position + lineNormal * nearestSteerable.BoundingRadius;
                        end = nearestSteerable.Position + lineNormal * secondNearestSteerable.BoundingRadius;
                    }
                    Vector2 lineDirection = Vector2.Normalize(end - start);
                    lineNormal = Math2D.Rotate90DegreesCcw(lineDirection);

                    // Determine which direction to move across the wall that might takes less time to reach the target.
                    if (Vector2.Dot(lineDirection, movingEntity.TargetedForward) < 0)
                        lineDirection = -lineDirection;

                    // Moves the entity along the wall.
                    float penetration = -Vector2.Dot(movingEntity.Forward, lineNormal);
                    if (Vector2.Dot(lineDirection, movingEntity.Forward) > SteeringHelper.AvoidanceAngularEpsilon && penetration < 0)
                        return lineDirection * movingEntity.MaxForce;

                    // If somehow the entity has penetrate the wall, this force will pull the entity out.
                    if (penetration > 0)
                        lineDirection += penetration * lineNormal;

                    Vector2 desiredForce = lineDirection * movingEntity.MaxSpeed - movingEntity.Velocity;
                    return Vector2.Normalize(desiredForce) * movingEntity.MaxForce;
                }
                else
                {
                    // Avoid steerable
                    Vector2 toTarget = partner.Position - movingEntity.Position;
                    toTarget.Normalize();

                    Vector2 lineDirection = Math2D.Rotate90DegreesCcw(toTarget);
                    if (Vector2.Dot(lineDirection, movingEntity.TargetedForward) < 0)
                        lineDirection = -lineDirection;

                    float penetration = movingEntity.BoundingRadius + partner.BoundingRadius + movingEntity.Skin - minDistanceToSteerable;
                    if (Vector2.Dot(lineDirection, movingEntity.Forward) > SteeringHelper.AvoidanceAngularEpsilon && penetration < 0)
                        return lineDirection * movingEntity.MaxForce;

                    // If somehow the entity has penetrate the wall, this force will pull the entity out.
                    if (penetration > 0)
                        lineDirection += penetration / -movingEntity.Skin * toTarget;

                    Vector2 desiredForce = lineDirection * movingEntity.MaxSpeed - movingEntity.Velocity;
                    return Vector2.Normalize(desiredForce) * movingEntity.MaxForce;
                }
            }

            return Vector2.Zero;
        }

        private void AdjustTargetPositionWhenOverlapped(float elapsedTime, Steerable movingEntity)
        {
            foreach (Steerable partner in Neighbors.FindAll(new Vector3(movingEntity.Target.Value, 0), movingEntity.BoundingRadius))
            {
                if (partner == null || partner == movingEntity)
                    continue;

                Vector2 toTarget = Vector2.Normalize(movingEntity.Target.Value - movingEntity.Position);

                if (float.IsNaN(toTarget.X))
                    continue;

                if (Vector2.Dot(movingEntity.Forward, toTarget) < 0.8f &&
                    new BoundingCircle(partner.Position, partner.BoundingRadius).Contains(movingEntity.Target.Value) != ContainmentType.Disjoint)
                {
                    Vector2 normal = Math2D.Rotate90DegreesCcw(toTarget);
                    if (Vector2.Dot(normal, movingEntity.Forward) < 0)
                        normal = -normal;
                    normal = (normal - toTarget) * 0.5f * 1.414f;
                    movingEntity.Target = partner.Position + normal * (movingEntity.BoundingRadius + partner.BoundingRadius);
                    break;
                }
            }
        }

        protected override float? OnCollides(Vector2 from, Vector2 to, float elapsedTime, Steerable movingEntity)
        {
            float detectorLength = movingEntity.BoundingRadius + movingEntity.Skin;

            foreach (Steerable partner in Neighbors.FindAll(new Vector3(movingEntity.Position, 0), detectorLength))
            {
                if (partner == null || partner == movingEntity)
                    continue;

                BoundingCircle obstacle = new BoundingCircle(partner.Position, partner.BoundingRadius);

                if (obstacle.Contains(new BoundingCircle(from, movingEntity.BoundingRadius)) != ContainmentType.Disjoint)
                    continue;

                if (obstacle.Contains(new BoundingCircle(to, movingEntity.BoundingRadius)) == ContainmentType.Disjoint)
                    continue;
                
                return 0;
            }
            return null;
        }
    }
}