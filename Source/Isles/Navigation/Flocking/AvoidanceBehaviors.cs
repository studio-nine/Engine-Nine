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
    #region ObstacleAvoidanceBehavior
    public sealed class ObstacleAvoidanceBehavior : IFlockingBehavior
    {
        public Vector3 Update(GameTime gameTime, IMovable movingEnity)
        {
            return Vector3.Zero;
        }
    }
    #endregion

    #region WallAvoidanceBehavior
    public sealed class WallAvoidanceBehavior : IFlockingBehavior
    {
        public Vector3 Update(GameTime gameTime, IMovable movingEnity)
        {
            return Vector3.Zero;
        }
    }
    #endregion

    #region SurfaceAvoidBehavior
    public sealed class SurfaceAvoidanceBehavior : IFlockingBehavior
    {
        public ISurface Celling { get; set; }
        public ISurface Floor { get; set; }

        public Vector3 Update(GameTime gameTime, IMovable movingEnity)
        {
            return Vector3.Zero;
        }
    }
    #endregion

    #region HideBehavior
    public sealed class HideBehavior : IFlockingBehavior
    {
        public Vector3 Update(GameTime gameTime, IMovable movingEnity)
        {
            return Vector3.Zero;
        }
    }
    #endregion
    
    #region BoundAvoidanceBehavior
    public sealed class BoundAvoidanceBehavior : IFlockingBehavior
    {
        public float Skin { get; set; }
        public float Elasticity { get; set; }
        public BoundingBox Bounds { get; set; }

        public BoundAvoidanceBehavior() 
        {
            Skin = 2;
            Elasticity = MathHelper.E;

            Bounds = new BoundingBox(Vector3.One * float.MinValue, 
                                     Vector3.One * float.MaxValue); 
        }

        public Vector3 Update(GameTime gameTime, IMovable movingEnity)
        {
            Vector3 result = Vector3.Zero;

            // Min X
            if (movingEnity.Position.X < Bounds.Min.X + Skin)
            {
                result.X = movingEnity.MaxSpeed * Evaluate((Bounds.Min.X + Skin - movingEnity.Position.X) / Skin);
            }
            // Min Y
            if (movingEnity.Position.Y < Bounds.Min.Y + Skin)
            {
                result.Y = movingEnity.MaxSpeed * Evaluate((Bounds.Min.Y + Skin - movingEnity.Position.Y) / Skin);
            }
            // Min Z
            if (movingEnity.Position.Z < Bounds.Min.Z + Skin)
            {
                result.Z = movingEnity.MaxSpeed * Evaluate((Bounds.Min.Z + Skin - movingEnity.Position.Z) / Skin);
            }
            // Max X
            if (movingEnity.Position.X > Bounds.Max.X - Skin)
            {
                result.X = -movingEnity.MaxSpeed * Evaluate((movingEnity.Position.X - Bounds.Max.X + Skin) / Skin);
            }
            // Max Y
            if (movingEnity.Position.Y > Bounds.Max.Y - Skin)
            {
                result.Y = -movingEnity.MaxSpeed * Evaluate((movingEnity.Position.Y - Bounds.Max.Y + Skin) / Skin);
            }
            // Max Z
            if (movingEnity.Position.Z > Bounds.Max.Z - Skin)
            {
                result.Z = -movingEnity.MaxSpeed * Evaluate((movingEnity.Position.Z - Bounds.Max.Z + Skin) / Skin);
            }

            return result;
        }

        private float Evaluate(float value)
        {            
            return (float)((Math.Pow(Elasticity, value) - 1) / (Elasticity - 1));
        }
    }
    #endregion
}