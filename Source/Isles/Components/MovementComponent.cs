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
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Isles.Graphics;
using Isles.Graphics.Models;
#endregion


namespace Isles.Components
{
    public class MovementComponent : IUpdateObject, IComponent
    {
        private IDisplayObject displayObject;

        public IAnimation Animation { get; set; }
        public IMovable Movement { get; set; }

        #region IComponent Members

        IComponentContainer parent;

        public IComponentContainer Parent
        {
            get { return parent; }
            set
            {
                parent = value;
                displayObject = parent as IDisplayObject;
            }
        }

        #endregion


        public void Update(GameTime gameTime)
        {
            if (Movement != null)
            {
                Movement.Update(gameTime);

                if (displayObject != null)
                    displayObject.Transform = Movement.Transform;
            }

            if (Animation != null)
                Animation.Update(gameTime);
        }
    }
}