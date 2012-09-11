namespace TutorialData
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;
    using Nine;
    using Nine.Physics;

    public class ExamplePhysicsController : Component, IUpdateable
    {
        public float Speed { get; set; }
        public Vector3 ResetPosition { get; set; }
        public Keys ResetKey { get; set; }

        public void Update(TimeSpan elapsedTime)
        {
            var Body = Parent.Find<RigidBody>();
            var keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Left))
                Body.ApplyImpulse(Speed * Vector3.Left);

            if (keyboardState.IsKeyDown(Keys.Right))
                Body.ApplyImpulse(Speed * Vector3.Right);

            if (keyboardState.IsKeyDown(Keys.Down))
                Body.ApplyImpulse(Speed * Vector3.Backward);

            if (keyboardState.IsKeyDown(Keys.Up))
                Body.ApplyImpulse(Speed * Vector3.Forward);

            if (keyboardState.IsKeyDown(ResetKey))
            {
                var body = Parent.Find<RigidBody>();
                body.Position = ResetPosition;
                body.Velocity = Vector3.Zero;
                body.AngularVelocity = Vector3.Zero;
            }
        }
    }
}