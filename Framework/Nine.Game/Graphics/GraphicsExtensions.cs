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
        /// <summary>
        /// Attach graphics information to all the world objects.
        /// </summary>
        public static Renderer CreateGraphics(this World world, GraphicsDevice graphics)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException("graphics");
            }
            if (!world.WorldObjects.IsAttached(typeof(IDrawableView)))
            {
                Renderer renderer = new Renderer(graphics);
                world.Renderer = renderer;
                world.WorldObjects.Attach(typeof(IDrawableView), worldObject =>
                {
                    var drawable = worldObject as IWorldObject;
                    if (drawable != null)
                        return world.CreateFromTemplate(drawable.Template);
                    return null;
                }, renderer);
                world.Drawing += new EventHandler<TimeEventArgs>(Draw);
                return renderer;
            }
            throw new InvalidOperationException("Graphics has already been created.");
        }

        /// <summary>
        /// Gets the graphics render attached to this world objects.
        /// </summary>
        public static Renderer GetGraphics(this World world)
        {
            return world.Renderer;
        }
        
        static void Draw(object sender, TimeEventArgs e)
        {
            Renderer renderer = ((World)sender).Renderer;
            if (renderer != null)
            {
                renderer.Draw(e.ElapsedTime, ((World)sender).WorldObjects.GetExtensions(typeof(IDrawableView)).OfType<object>());
            }
        }
    }
}
