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

namespace Nine.Graphics.ObjectModel
{
    /// <summary>
    /// Contains extension methods for world.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class GraphicsExtensions
    {
        /// <summary>
        /// Create all the graphics objects in the world.
        /// </summary>
        public static Renderer CreateGraphics(this World world, GraphicsDevice graphics)
        {
            return CreateGraphics(world, graphics, null, null);
        }

        /// <summary>
        /// Create all the graphics objects in the world.
        /// </summary>
        public static Renderer CreateGraphics(this World world, GraphicsDevice graphics, GraphicsSettings settings, ISceneManager<ISpatialQueryable> sceneManager)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            if (world.Renderer != null)
                throw new InvalidOperationException("Graphics has already been created.");

            Renderer renderer = new Renderer(graphics, settings, sceneManager);
            world.Renderer = renderer; 
            world.TemplateFactories.Add(new ContentTemplateFactory(new GraphicsDeviceServiceProvider(graphics)));
            world.WorldObjects.ForEach(o => renderer.Add(CreateGraphicsObject(world, o)));
            world.WorldObjects.Added += WorldObjects_Added;
            world.WorldObjects.Removed += WorldObjects_Removed;
            world.Drawing += new EventHandler<TimeEventArgs>(Draw);
            return renderer;
        }

        /// <summary>
        /// Gets the graphics render attached to this world objects.
        /// </summary>
        public static Renderer GetGraphics(this World world)
        {
            return (Renderer)world.Renderer;
        }

        static object CreateGraphicsObject(World world, object obj)
        {
            IWorldObject worldObject = obj as IWorldObject;
            if (worldObject == null)
                return obj;
            if (worldObject.Template == null)
                return obj;
            return world.CreateFromTemplate(worldObject.Template);
        }

        static object GetGraphicsObject(object obj)
        {
            IWorldObject worldObject = obj as IWorldObject;
            if (worldObject == null)
                return null;
            return worldObject.Template.Value;
        }

        static void WorldObjects_Added(object sender, NotifyCollectionChangedEventArgs<object> e)
        {
            Renderer renderer = (Renderer)((World)sender).Renderer;
            if (renderer != null)
            {
                renderer.Add(CreateGraphicsObject((World)sender, e.Value));
            }
        }

        static void WorldObjects_Removed(object sender, NotifyCollectionChangedEventArgs<object> e)
        {
            Renderer renderer = (Renderer)((World)sender).Renderer;
            if (renderer != null)
            {
                renderer.Remove(GetGraphicsObject(e.Value));
            }
        }
        
        static void Draw(object sender, TimeEventArgs e)
        {
            Renderer renderer = (Renderer)((World)sender).Renderer;
            if (renderer != null)
            {
                renderer.Draw(e.ElapsedTime);
            }
        }
    }
}
