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
    using System;
    using Microsoft.Xna.Framework;

    public class BoundAvoidanceBehavior : SteeringBehavior
    {
        public float Skin { get; set; }
        public float Elasticity { get; set; }
        public BoundingRectangle Bounds { get; set; }

        public BoundAvoidanceBehavior()
        {
            Skin = 2;
            Elasticity = MathHelper.E;
            Bounds = new BoundingRectangle(float.MinValue, float.MinValue, float.MaxValue, float.MaxValue);
        }

        protected override Vector2 OnUpdateSteeringForce(float elapsedTime, Steerable movingEntity)
        {
            Vector2 result = Vector2.Zero;

            // Min X
            if (movingEntity.Position.X < Bounds.X + Skin)
            {
                result.X = movingEntity.MaxSpeed * Evaluate((Bounds.X + Skin - movingEntity.Position.X) / Skin);
            }
            // Min Y
            if (movingEntity.Position.Y < Bounds.Y + Skin)
            {
                result.Y = movingEntity.MaxSpeed * Evaluate((Bounds.Y + Skin - movingEntity.Position.Y) / Skin);
            }
            // Max X
            if (movingEntity.Position.X > Bounds.X + Bounds.Width - Skin)
            {
                result.X = -movingEntity.MaxSpeed * Evaluate((movingEntity.Position.X - Bounds.X - Bounds.Width + Skin) / Skin);
            }
            // Max Y
            if (movingEntity.Position.Y > Bounds.Y + Bounds.Height - Skin)
            {
                result.Y = -movingEntity.MaxSpeed * Evaluate((movingEntity.Position.Y - Bounds.Y - Bounds.Height + Skin) / Skin);
            }

            return result;
        }

        private float Evaluate(float value)
        {
            return (float)((Math.Pow(Elasticity, value) - 1) / (Elasticity - 1));
        }
    }
}