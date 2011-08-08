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
    #region CohesionBehavior
    public class CohesionBehavior : SteeringBehavior
    {
        public float Range { get; set; }
        public ISpatialQuery<Steerable> Neighbors { get; set; }

        public CohesionBehavior()
        {
            Range = 100.0f;
        }

        protected override Vector2 OnUpdateSteeringForce(float elapsedTime, Steerable movingEntity)
        {
            int count = 0;
            Vector2 center = Vector2.Zero;

            foreach (Steerable partner in Neighbors.FindAll(new Vector3(movingEntity.Position, 0), Range))
            {
                if (partner != null && partner != movingEntity)
                {
                    center += partner.Position;
                    if (++count >= SteeringHelper.MaxAffectingEntities)
                        break;
                }
            }

            if (count > 0)
            {
                center /= count;
                movingEntity.Target = center;
                return SteeringHelper.Seek(elapsedTime, movingEntity);
            }
            return Vector2.Zero;
        }
    }
    #endregion

    #region SeperationBehavior
    public class SeparationBehavior : SteeringBehavior
    {
        public float Range { get; set; }
        public ISpatialQuery<Steerable> Neighbors { get; set; }

        public SeparationBehavior()
        {
            Range = 10.0f;
        }

        protected override Vector2 OnUpdateSteeringForce(float elapsedTime, Steerable movingEntity)
        {
            int count = 0;
            Steerable nearestPartner = null;
            Vector2 nearestToPartner = new Vector2();
            float minDistanceToPartner = float.MaxValue;
            
            // Make sure seperation gets called earlier then steerable avoidance.
            float detectorLength = movingEntity.BoundingRadius + movingEntity.Skin * 2; 

            foreach (Steerable partner in Neighbors.FindAll(new Vector3(movingEntity.Position, 0), detectorLength))
            {
                if (partner == null || partner == movingEntity)
                    continue;

                Vector2 toPartner = partner.Position - movingEntity.Position;

                // Ignore entities moving away
                //if (Vector2.Dot(toPartner, partner.Velocity) >= 0)
                //    continue;

                float distance = toPartner.Length() - detectorLength - partner.BoundingRadius;

                if (distance <= 0 && distance < minDistanceToPartner)
                {
                    nearestToPartner = toPartner;
                    nearestPartner = partner;
                    minDistanceToPartner = distance;
                }

                if (++count >= SteeringHelper.MaxAffectingEntities)
                    break;
            }

            if (nearestPartner != null)
            {
                return -Vector2.Normalize(nearestToPartner) * movingEntity.MaxForce;
            }
            return Vector2.Zero;
        }
    }
    #endregion

    #region AlignmentBehavior
    public class AlignmentBehavior : SteeringBehavior
    {
        public float Range { get; set; }
        public ISpatialQuery<Steerable> Neighbors { get; set; }

        public AlignmentBehavior()
        {
            Range = 50.0f;
        }

        protected override Vector2 OnUpdateSteeringForce(float elapsedTime, Steerable movingEntity)
        {
            int count = 0;
            Vector2 totalForce = Vector2.Zero;

            foreach (Steerable partner in Neighbors.FindAll(new Vector3(movingEntity.Position, 0), Range))
            {
                if (partner != null && partner != movingEntity)
                {
                    totalForce += partner.Forward;
                    if (++count >= SteeringHelper.MaxAffectingEntities)
                        break;
                }
            }

            if (count > 0)
            {
                totalForce /= count;
                totalForce -= movingEntity.Forward;
            }

            return totalForce;
        }
    }
    #endregion
}