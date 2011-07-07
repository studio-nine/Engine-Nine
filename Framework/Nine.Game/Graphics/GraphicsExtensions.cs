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
#endregion

namespace Nine.Graphics
{
    /// <summary>
    /// Contains extension methods for graphics.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class GraphicsExtensions
    {
        public static Renderer CreateGraphics(this World world, GraphicsDevice graphics)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            if (!world.WorldObjects.IsAttached(typeof(IDrawable)))
            {
                Renderer renderer = new Renderer(graphics);
                world.Renderer = renderer;
                world.WorldObjects.Attach(typeof(IDrawableView), CreateDrawable, renderer);
                world.Drawing += new EventHandler<TimeEventArgs>(Draw);
                return renderer;
            }
            return world.WorldObjects.GetAttachedState<IDrawableView, Renderer>();
        }

        public static Renderer GetGraphics(this World world)
        {
            return world.Renderer;
        }

        static object CreateDrawable(object worldObject)
        {
            var drawable = worldObject as IDrawableWorldObject;
            if (drawable != null)
                return CreateTemplate(drawable.Template) as IDrawableView;
            return null;
        }

        private static object CreateTemplate(string templateName)
        {
            throw new NotImplementedException();
        }

        static void Draw(object sender, TimeEventArgs e)
        {
            if (((World)sender).Renderer != null)
            {
                foreach (IDrawable drawable in ((World)sender).WorldObjects.GetExtensions(typeof(IDrawable)))
                {
                    drawable.Draw(e.ElapsedTime);
                }
            }
        }
    }
}
