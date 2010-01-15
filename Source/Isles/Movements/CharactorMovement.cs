#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
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


namespace Isles.Movements
{
    public class CharactorMovement : Movement
    {
        protected override Vector3 TargetChanged(Vector3 oldTarget)
        {
            Vector3 heading;

            heading.X = Target.X - Position.X;
            heading.Y = Target.Y - Position.Y;
            heading.Z = 0;
            heading.Normalize();

            Heading = heading;

            heading.X = Target.X;
            heading.Y = Target.Y;
            heading.Z = 0;

            return heading;
        }

        protected override void UpdatePosition(GameTime time)
        {
            float height = 0;
            Vector3 normal = Up;
            Vector3 newPosition;
            

            newPosition = Position + Heading * (float)(Speed * time.ElapsedGameTime.TotalSeconds);

            if (Surface != null)
                Surface.TryGetHeightAndNormal(newPosition, out newPosition.Z, out normal);

            Position = newPosition;
        }
    }
}