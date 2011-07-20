#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;
using Nine.Graphics.ParticleEffects;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics
{
    /// <summary>
    /// A view template defines the look and feel of an object.
    /// </summary>
    public class ViewTemplate : IUpdateable, IDrawableView
    {
        [ContentSerializer(Optional = true)]
        public bool Visible { get; set; }

        [ContentSerializer(Optional = true)]
        public List<object> Views { get; internal set; }

        public ViewTemplate()
        {
            Visible = true;
            Views = new List<object>();
        }

        public void Update(TimeSpan elapsedTime)
        {
            foreach (var view in Views)
            {
                IUpdateable updateable = view as IUpdateable;
                if (updateable != null)
                    updateable.Update(elapsedTime);
            }
        }

        public void Draw(GraphicsContext context)
        {
            if (!Visible)
                return;

            foreach (var view in Views)
            {
                IDrawableView drawableView = view as IDrawableView;
                if (drawableView != null)
                    drawableView.Draw(context);
            }
        }

        public void Draw(GraphicsContext context, Effect effect)
        {
            if (!Visible)
                return;

            foreach (var view in Views)
            {
                IDrawableView drawableView = view as IDrawableView;
                if (drawableView != null)
                    drawableView.Draw(context, effect);
            }
        }
    }
}