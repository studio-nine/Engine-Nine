#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Nine.Graphics.ObjectModel;
#endregion

namespace Nine.Graphics
{
    /// <summary>
    /// Extends <see cref="World"/> to be capable of drawing graphics.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class WorldExtensions
    {
        /// <summary>
        /// Creates the graphics scene to render this world.
        /// </summary>
        public static Scene CreateGraphics(this World world, GraphicsDevice graphics)
        {
            return CreateGraphics(world, graphics, null, null);
        }

        /// <summary>
        /// Creates the graphics scene to render this world.
        /// </summary>
        public static Scene CreateGraphics(this World world, GraphicsDevice graphics, GraphicsSettings settings, ISceneManager<ISpatialQueryable> sceneManager)
        {
            var scene = new Scene(graphics, settings, sceneManager);
            UpdateScene(world, scene);
            return scene;
        }

        /// <summary>
        /// Destroies the graphics of this world.
        /// </summary>
        public static void DestroyGraphics(this World world)
        {
            UpdateScene(world, null);
        }

        private static void UpdateScene(this World world, Scene newScene)
        {
            var scene = world.Services.GetService<Scene>();

            if (scene != null)
            {
                foreach (var worldObject in world.WorldObjects)
                {
                    foreach (var component in worldObject.Components)
                    {
                        var graphicsComponent = component as GraphicsComponent;
                        if (graphicsComponent != null)
                            graphicsComponent.DestroyGraphicsObject();
                    }
                }

                scene.Dispose();
                world.Services.RemoveService(typeof(Scene));
                world.Services.RemoveService(typeof(GraphicsDevice));
#if !SILVERLIGHT
                world.Services.RemoveService(typeof(IGraphicsDeviceService));
#endif
                world.Drawing -= new EventHandler<TimeEventArgs>(Draw);
            }

            scene = newScene;

            if (scene != null)
            {
                world.Services.AddService(typeof(Scene), scene);
                world.Services.AddService(typeof(GraphicsDevice), newScene.GraphicsDevice);
#if !SILVERLIGHT
                world.Services.AddService(typeof(IGraphicsDeviceService), new GraphicsDeviceServiceProvider(newScene.GraphicsDevice));
#endif
                world.Drawing += new EventHandler<TimeEventArgs>(Draw);

                foreach (var worldObject in world.WorldObjects)
                {
                    foreach (var component in worldObject.Components)
                    {
                        var graphicsComponent = component as GraphicsComponent;
                        if (graphicsComponent != null)
                            graphicsComponent.CreateGraphicsObject();
                    }
                }
            }
        }

        private static void Draw(object sender, TimeEventArgs e)
        {
            var world = sender as World;
            if (world != null)
            {
                var scene = world.Services.GetService<Scene>();
                if (scene != null)
                    scene.Draw(e.ElapsedTime);
            }
        }
    }
}
