namespace Nine.Navigation.Steering
{
    using System;
    using Microsoft.Xna.Framework;

    internal class StuckAvoidanceBehavior : SteeringBehavior
    {
        const int StuckedFramesBeforeRandomize = 5;
        const int StuckedFramesBeforeStop = 300;
        const int SpinCountBeforeStop = 50;
        
        int stuckedFramesForRandomize = 0;
        int stuckedFramesForStop = 0;
        int spinCount = 0;

        Vector2 trackedForward = Vector2.Zero;
        
        protected override Vector2 OnUpdateSteeringForce(float elapsedTime, Steerable movingEntity)
        {
            if (movingEntity.IsStucked)
            {
                stuckedFramesForRandomize++;
                stuckedFramesForStop++;

                if (stuckedFramesForStop >= StuckedFramesBeforeStop)
                {
                    movingEntity.Target = null;
                    return Vector2.Zero;
                }

                if (stuckedFramesForRandomize >= StuckedFramesBeforeRandomize)
                {
                    stuckedFramesForRandomize = 0;
                    float randomAngle = (float)(Random.NextDouble() * MathHelper.Pi * 2);
                    Vector2 randomDirection = new Vector2();
                    randomDirection.X = (float)(Math.Cos(randomAngle));
                    randomDirection.Y = (float)(Math.Sin(randomAngle));
                    movingEntity.Forward = randomDirection;
                    return randomDirection * movingEntity.MaxForce;
                }
            }
            else
            {
                stuckedFramesForRandomize = 0;
                stuckedFramesForStop = 0;
            }

            if (movingEntity.Target == null || movingEntity.IsStucked)
            {
                spinCount = 0;
            }
            else if (Vector2.Dot(movingEntity.Forward, trackedForward) <= 0)
            {
                if (++spinCount >= SpinCountBeforeStop)
                {
                    // Avoid sudden turn when stopped.
                    movingEntity.Forward = trackedForward;
                    movingEntity.Target = null;
                    spinCount = 0;
                }
                trackedForward = movingEntity.Forward;
            }

            return Vector2.Zero;
        }
    }
}