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
        SeekBehavior seek = new SeekBehavior();

        public float Range { get; set; }
        public ISpatialQuery Neighbors { get; set; }

        public CohesionBehavior()
        {
            Range = 100.0f;
        }

        protected override Vector2 OnUpdateSteeringForce(float elapsedTime, ISteerable movingEntity)
        {
            int count = 0;
            Vector2 center = Vector2.Zero;

            foreach (ISteerable partner in Neighbors.Find<ISteerable>(new Vector3(movingEntity.Position, 0), Range))
            {
                if (partner != null && partner != movingEntity)
                {
                    center += partner.Position;

                    count++;
                }
            }

            if (count > 0)
            {
                center /= count;
                seek.Target = center;
                return seek.UpdateSteeringForce(elapsedTime, movingEntity);
            }

            return Vector2.Zero;
        }
    }
    #endregion

    #region SeperationBehavior
    public class SeparationBehavior : SteeringBehavior
    {
        public float Range { get; set; }
        public ISpatialQuery Neighbors { get; set; }

        public SeparationBehavior()
        {
            Range = 10.0f;
        }

        protected override Vector2 OnUpdateSteeringForce(float elapsedTime, ISteerable movingEntity)
        {
            Vector2 totalForce = Vector2.Zero;

            foreach (ISteerable partner in Neighbors.Find<ISteerable>(new Vector3(movingEntity.Position, 0), Range + movingEntity.BoundingRadius))
            {
                if (partner != null && partner != movingEntity)
                {
                    Vector2 toPartner = partner.Position - movingEntity.Position;

                    float distance = toPartner.Length();

                    if (distance > 0)
                    {
                        //totalForce -= Vector2.Normalize(toPartner) / distance;
                        totalForce -= Vector2.Normalize(toPartner) * movingEntity.MaxForce;
                    }
                }
            }

            return totalForce;
        }
    }
    #endregion

    #region AlignmentBehavior
    public class AlignmentBehavior : SteeringBehavior
    {
        public float Range { get; set; }
        public ISpatialQuery Neighbors { get; set; }

        public AlignmentBehavior()
        {
            Range = 50.0f;
        }

        protected override Vector2 OnUpdateSteeringForce(float elapsedTime, ISteerable movingEntity)
        {
            int count = 0;
            Vector2 totalForce = Vector2.Zero;

            foreach (ISteerable partner in Neighbors.Find<ISteerable>(new Vector3(movingEntity.Position, 0), Range))
            {
                if (partner != null && partner != movingEntity)
                {
                    totalForce += partner.Forward;

                    count++;
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