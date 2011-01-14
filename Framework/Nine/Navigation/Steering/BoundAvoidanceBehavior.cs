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
    public class BoundAvoidanceBehavior : SteeringBehavior
    {
        public float Skin { get; set; }
        public float Elasticity { get; set; }
        public BoundingRectangle Bounds { get; set; }

        public BoundAvoidanceBehavior()
        {
            Skin = 2;
            Elasticity = MathHelper.E;
            Bounds = new BoundingRectangle(Vector2.One * float.MinValue,
                                           Vector2.One * float.MaxValue);
        }

        protected override Vector2 OnUpdateSteeringForce(float elapsedTime, Steerable movingEntity)
        {
            Vector2 result = Vector2.Zero;

            // Min X
            if (movingEntity.Position.X < Bounds.Min.X + Skin)
            {
                result.X = movingEntity.MaxSpeed * Evaluate((Bounds.Min.X + Skin - movingEntity.Position.X) / Skin);
            }
            // Min Y
            if (movingEntity.Position.Y < Bounds.Min.Y + Skin)
            {
                result.Y = movingEntity.MaxSpeed * Evaluate((Bounds.Min.Y + Skin - movingEntity.Position.Y) / Skin);
            }
            // Max X
            if (movingEntity.Position.X > Bounds.Max.X - Skin)
            {
                result.X = -movingEntity.MaxSpeed * Evaluate((movingEntity.Position.X - Bounds.Max.X + Skin) / Skin);
            }
            // Max Y
            if (movingEntity.Position.Y > Bounds.Max.Y - Skin)
            {
                result.Y = -movingEntity.MaxSpeed * Evaluate((movingEntity.Position.Y - Bounds.Max.Y + Skin) / Skin);
            }

            return result;
        }

        private float Evaluate(float value)
        {
            return (float)((Math.Pow(Elasticity, value) - 1) / (Elasticity - 1));
        }
    }
}