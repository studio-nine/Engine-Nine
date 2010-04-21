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
namespace Isles.Navigation.Flocking
{
    #region CohesionBehavior
    public sealed class CohesionBehavior : IFlockingBehavior
    {
        public float CohesionRadius { get; set; }
        public ISpacialObjectManager SpacialObjectManager { get; set; }

        public CohesionBehavior()
        {
            CohesionRadius = 100.0f;
        }

        public Vector3 Update(GameTime gameTime, IMovable movingEnity)
        {
            Vector3 totalForce = Vector3.Zero;

            foreach (object o in SpacialObjectManager.GetNearbyObjects(movingEnity.Position, CohesionRadius))
            {
                IMovable partner = o as IMovable;

                if (partner != null && partner != movingEnity)
                {
                    Vector3 toPartner = partner.Position - movingEnity.Position;

                    float desiredSpeed = movingEnity.MaxSpeed * toPartner.Length() / CohesionRadius;

                    totalForce += desiredSpeed * Vector3.Normalize(toPartner);
                }
            }

            return totalForce;
        }
    }
    #endregion

    #region SeperationBehavior
    public sealed class SeparationBehavior : IFlockingBehavior
    {
        public float SeparationRadius { get; set; }
        public ISpacialObjectManager SpacialObjectManager { get; set; }

        public SeparationBehavior()
        {
            SeparationRadius = 10.0f;
        }

        public Vector3 Update(GameTime gameTime, IMovable movingEnity)
        {
            Vector3 totalForce = Vector3.Zero;

            foreach (object o in SpacialObjectManager.GetNearbyObjects(movingEnity.Position, SeparationRadius))
            {
                IMovable partner = o as IMovable;

                if (partner != null && partner != movingEnity)
                {
                    Vector3 toPartner = partner.Position - movingEnity.Position;

                    float desiredSpeed = movingEnity.MaxSpeed * (1 - toPartner.Length() / SeparationRadius);

                    totalForce -= desiredSpeed * Vector3.Normalize(toPartner);
                }
            }

            return totalForce;
        }
    }
    #endregion

    #region AlignmentBehavior
    public class AlignmentBehavior : IFlockingBehavior
    {
        public float GroupRadius { get; set; }
        public ISpacialObjectManager SpacialObjectManager { get; set; }

        public AlignmentBehavior()
        {
            GroupRadius = 50.0f;
        }

        public Vector3 Update(GameTime gameTime, IMovable movingEnity)
        {
            int count = 0;
            Vector3 totalForce = Vector3.Zero;

            foreach (object o in SpacialObjectManager.GetNearbyObjects(movingEnity.Position, GroupRadius))
            {
                IMovable partner = o as IMovable;

                if (partner != null && partner != movingEnity)
                {
                    totalForce += partner.Forward;

                    count++;
                }
            }

            if (count > 0)
            {
                totalForce /= count;
                totalForce -= movingEnity.Forward;
            }

            return totalForce;
        }
    }
    #endregion
}