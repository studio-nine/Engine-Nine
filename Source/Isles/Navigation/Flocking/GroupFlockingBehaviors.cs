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
    public class CohesionBehavior : IFlockingBehavior
    {
        public IMovable Movable { get; set; }

        public Vector3 Update(GameTime gameTime)
        {
            return Vector3.Zero;
        }
    }


    public class SeparationBehavior : IFlockingBehavior
    {
        public IMovable Movable { get; set; }

        public Vector3 Update(GameTime gameTime)
        {
            return Vector3.Zero;
        }
    }


    public class AlignmentBehavior : IFlockingBehavior
    {
        public IMovable Movable { get; set; }

        public Vector3 Update(GameTime gameTime)
        {
            return Vector3.Zero;
        }
    }
}