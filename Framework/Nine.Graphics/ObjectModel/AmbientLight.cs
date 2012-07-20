#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.Drawing;
#endregion

namespace Nine.Graphics.ObjectModel
{
    [ContentSerializable]
    public partial class AmbientLight : Object, ISceneObject
    {
        public GraphicsDevice GraphicsDevice { get; private set; }

        private DrawingContext context;
        
        public AmbientLight(GraphicsDevice graphics)
        {
            GraphicsDevice = graphics;
            AmbientLightColor = Vector3.One * 0.2f;
        }

        public bool Enabled
        {
            get { return enabled; }
            set 
            {
                if (enabled != value)
                {
                    if (context != null)
                    {
                        if (value)
                            context.AmbientLight.Value += ambientLightColor;
                        else
                            context.AmbientLight.Value -= ambientLightColor;
                    }
                    enabled = value; 
                }
            }
        }
        private bool enabled = true;

        public Vector3 AmbientLightColor
        {
            get { return ambientLightColor; }
            set
            {
                if (context != null)
                {
                    // Updates ambient light color in the owner context
                    context.AmbientLight.Value += (value - ambientLightColor);
                }
                ambientLightColor = value; 
            }
        }
        private Vector3 ambientLightColor;

        void ISceneObject.OnAdded(DrawingContext context)
        {
            this.context = context;
            if (enabled)
                context.AmbientLight.Value = context.AmbientLight.Value + ambientLightColor;
        }

        void ISceneObject.OnRemoved(DrawingContext context)
        {
            if (enabled)
                context.AmbientLight.Value = context.AmbientLight.Value - ambientLightColor;
            this.context = null;
        }
    }
}