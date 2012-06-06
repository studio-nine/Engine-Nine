#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.Collections.Generic;
using System.ComponentModel;
using Nine.Graphics.ObjectModel;
using Nine.Graphics;
#endregion

namespace Nine.Navigation
{
    /// <summary>
    /// Extends <see cref="World"/> to be capable of moving objects.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class WorldExtensions
    {
        /// <summary>
        /// Creates the navigation info for this world.
        /// </summary>
        public static void CreateNavigation(this World world)
        {
            // Create ISpatialQuery<Navigator> service
            var navigatorManager = new QuadTreeSceneManager();            
            world.Services.AddService(typeof(ISpatialQuery<Navigator>), new SpatialQuery<ISpatialQueryable, Navigator>(navigatorManager));

            // Create IPathGraph service.
            foreach (var worldObject in world.WorldObjects)
            {
                foreach (var component in worldObject.Components)
                {
                    var pathGraph = component as PathGraphComponent;
                    if (pathGraph != null && pathGraph.PathGraph != null &&
                        world.Services.GetService<IPathGraph>() == null)
                    {
                        world.Services.AddService(typeof(IPathGraph), pathGraph.PathGraph);
                    }
                }
            }

            // Create ISurface service.
            if (world.Services.GetService<ISurface>() == null)
            {
                var scene = world.GetService<Scene>();
                if (scene != null)
                {
                    var surfaces = new List<ISurface>();
                    //scene.FindAll(surfaces); FIXME:
                    if (surfaces.Count > 0)
                    {
                        var surfaceContainer = new SurfaceCollection();
                        surfaceContainer.AddRange(surfaces);
                        world.Services.AddService(typeof(ISurface), surfaceContainer);
                    }
                }
            }

            // Create navigators.
            foreach (var worldObject in world.WorldObjects)
            {
                foreach (var component in worldObject.Components)
                {
                    var navigator = component as NavigatorComponent;
                    if (navigator != null)
                        navigator.CreateNavigator();
                }
            }
        }

        /// <summary>
        /// Destroies the navigation info of this world.
        /// </summary>
        public static void DestroyNavigation(this World world)
        {
            world.Services.RemoveService(typeof(IPathGraph));
            world.Services.RemoveService(typeof(ISpatialQuery<Navigator>));
        }
    }
}
