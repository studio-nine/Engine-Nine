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

namespace Nine.Graphics.Views
{
    public class ViewTemplate : IUpdateable, IDrawableView, IEnumerable<IDrawableView>
    {
        [ContentSerializer(Optional = true)]
        public bool Visible { get; set; }

        [ContentSerializer(Optional = true)]
        public List<IDrawableView> Views { get; internal set; }

        public ViewTemplate()
        {
            Views = new List<IDrawableView>();
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
            if (Visible)
            {
                foreach (var view in Views)
                    view.Draw(context);
            }
        }

        public void Draw(GraphicsContext context, Effect effect)
        {
            if (Visible)
            {
                foreach (var view in Views)
                    view.Draw(context, effect);
            }
        }

        public IEnumerator<IDrawableView> GetEnumerator()
        {
            return Views.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}