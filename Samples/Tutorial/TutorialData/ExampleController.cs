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
    /// <summary>
    /// The follow class defines a basic component that updates the position of the parent object
    /// based on keyboard input.
    /// 
    /// To create a custom game component, inherit from Nine.Component so you can gain access to
    /// the containing object. But this is not necessary if your component do not interact with other
    /// components or the containing game object.
    /// 
    /// To be able to author your custom component using Xaml, the component must be compatible with
    /// both Xaml as well as Xna content compiler. Because the custom component will be first parsed
    /// using Xaml, and then compiled with the Xna content compiler into the binary .xnb format, and
    /// finally be read through the content manager.
    /// </summary>
    public class ExampleController : Component, IUpdateable
    {
        public float Speed { get; set; }

        public void Update(TimeSpan elapsedTime)
        {
            var transform = Parent.Transform;

            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Left))
                transform.Translation += Speed * (float)elapsedTime.TotalSeconds * -Vector3.UnitX;
            if (keyboardState.IsKeyDown(Keys.Right))
                transform.Translation += Speed * (float)elapsedTime.TotalSeconds * Vector3.UnitX;
            if (keyboardState.IsKeyDown(Keys.Down))
                transform.Translation += Speed * (float)elapsedTime.TotalSeconds * -Vector3.UnitY;
            if (keyboardState.IsKeyDown(Keys.Up))
                transform.Translation += Speed * (float)elapsedTime.TotalSeconds * Vector3.UnitY;

            Parent.Transform = transform;
        }
    }
}