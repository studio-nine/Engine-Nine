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
    public class ExamplePhysicsTeleportController : Component, IUpdateable
    {
        public Vector3 TeleportToPosition { get; set; }
        public Keys Key { get; set; }

        public void Update(TimeSpan elapsedTime)
        {
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Key))
            {
                var Body = Parent.Find<Nine.Physics.RigidBody>();
                Body.TeleportTo(TeleportToPosition);
            }
        }
    }
}