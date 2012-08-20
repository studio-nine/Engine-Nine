#region Copyright 2009 - 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nine;
#endregion

namespace TutorialData
{
    public class ExamplePhysicsMoveController : Component, IUpdateable
    {
        public float Speed { get; set; }

        public void Update(TimeSpan elapsedTime)
        {
            var Body = Parent.Find<Nine.Physics.RigidBody>();

            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Left))
            {
                Vector3 vector = Speed * Vector3.Left;
                Body.Entity.ApplyLinearImpulse(ref vector);
            }
            if (keyboardState.IsKeyDown(Keys.Right))
            {
                Vector3 vector = Speed * Vector3.Right;
                Body.Entity.ApplyLinearImpulse(ref vector);
            }
            if (keyboardState.IsKeyDown(Keys.Down))
            {
                Vector3 vector = Speed * Vector3.Backward;
                Body.Entity.ApplyLinearImpulse(ref vector);
            }
            if (keyboardState.IsKeyDown(Keys.Up))
            {
                Vector3 vector = Speed * Vector3.Forward;
                Body.Entity.ApplyLinearImpulse(ref vector);
            }
        }
    }
}