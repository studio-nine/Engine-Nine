#region Copyright 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2010 (c) Nightin Games. All Rights Reserved.
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
    #region CohesionBehavior
    public sealed class CohesionBehavior : ISteeringBehavior
    {
        SeekBehavior seek = new SeekBehavior();

        public float GroupRadius { get; set; }
        public ISpacialObjectManager SpacialObjectManager { get; set; }

        public CohesionBehavior()
        {
            GroupRadius = 100.0f;
        }

        public Vector3 CalculateForce(GameTime gameTime, IMovable movingEntity)
        {
            int count = 0;
            Vector3 center = Vector3.Zero;

            foreach (object o in SpacialObjectManager.GetNearbyObjects(movingEntity.Position, GroupRadius))
            {
                IMovable partner = o as IMovable;

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
                return seek.CalculateForce(gameTime, movingEntity);
            }

            return Vector3.Zero;
        }

        public bool WillPenetrate(Vector3 from, Vector3 to, IMovable movingEntity)
        {
            return false;
        }
    }
    #endregion

    #region SeperationBehavior
    public sealed class SeparationBehavior : ISteeringBehavior
    {
        public Random Random { get; set; }
        public float SeparationRadius { get; set; }
        public ISpacialObjectManager SpacialObjectManager { get; set; }

        static Random StaticRandom = new Random();

        public SeparationBehavior()
        {
            SeparationRadius = 10.0f;
            Random = StaticRandom;
        }

        public Vector3 CalculateForce(GameTime gameTime, IMovable movingEntity)
        {
            Vector3 totalForce = Vector3.Zero;

            foreach (object o in SpacialObjectManager.GetNearbyObjects(movingEntity.Position, SeparationRadius))
            {
                IMovable partner = o as IMovable;

                if (partner != null && partner != movingEntity)
                {
                    Vector3 toPartner = partner.Position - movingEntity.Position;

                    float dist = toPartner.Length();

                    if (dist > 0)
                    {
                        totalForce -= Vector3.Normalize(toPartner) / dist;
                    }
                    else
                    {
                        Vector3 v;

                        v.X = (float)Random.NextDouble();
                        v.Y = (float)Random.NextDouble();
                        //v.Z = (float)Random.NextDouble();
                        v.Z = 0;

                        totalForce += Vector3.Normalize(v);
                    }
                }
            }

            return totalForce * movingEntity.MaxSpeed;
        }

        public bool WillPenetrate(Vector3 from, Vector3 to, IMovable movingEntity)
        {
            return false;
        }
    }
    #endregion

    #region AlignmentBehavior
    public class AlignmentBehavior : ISteeringBehavior
    {
        public float GroupRadius { get; set; }
        public ISpacialObjectManager SpacialObjectManager { get; set; }

        public AlignmentBehavior()
        {
            GroupRadius = 50.0f;
        }

        public Vector3 CalculateForce(GameTime gameTime, IMovable movingEntity)
        {
            int count = 0;
            Vector3 totalForce = Vector3.Zero;

            foreach (object o in SpacialObjectManager.GetNearbyObjects(movingEntity.Position, GroupRadius))
            {
                IMovable partner = o as IMovable;

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

        public bool WillPenetrate(Vector3 from, Vector3 to, IMovable movingEntity)
        {
            return false;
        }
    }
    #endregion
}