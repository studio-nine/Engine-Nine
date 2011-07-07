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
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;
using Nine.Graphics.ParticleEffects;
using Nine.Graphics.Passes;
using Nine.Graphics.ScreenEffects;
#endregion

namespace Nine.Graphics
{
    /// <summary>
    /// Defines a renderer that is used to render a list of object using the
    /// specified camera and light settings.
    /// </summary>
    public class Renderer : IDisposable
    {
        /// <summary>
        /// Gets the graphics settings
        /// </summary>
        public GraphicsSettings Settings { get; private set; }

        /// <summary>
        /// Gets or sets the active camera.
        /// </summary>
        public ICamera Camera
        {
            get { return camera ?? (camera = new ModelViewerCamera(GraphicsDevice)); }
            set { camera = value; }
        }

        /// <summary>
        /// Gets all the lights used by this renderer.
        /// </summary>
        public IList<ILight> Lights { get; private set; }

        /// <summary>
        /// Gets a collection of render passes used by this renderer.
        /// </summary>
        public IList<GraphicsPass> Passes { get; private set; }

        /// <summary>
        /// Gets or sets the post processing screen effect used by this renderer.
        /// </summary>
        public ScreenEffect ScreenEffect
        {
            get { return screenEffect ?? (screenEffect = new ScreenEffect(GraphicsDevice) { Enabled = false }); }
            set { screenEffect = value; }
        }

        /// <summary>
        /// Gets the underlying graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets or sets the graphics context.
        /// </summary>
        protected GraphicsContext GraphicsContext { get; set; }

        private ICamera camera;
        private ScreenEffect screenEffect;
        private SpatialQuery<object> cachedSpatialQuery;
        
        /// <summary>
        /// Creates a new instance of <c>Renderer</c>.
        /// </summary>
        public Renderer(GraphicsDevice graphics) : this(graphics, null)
        {

        }

        /// <summary>
        /// Creates a new instance of <c>Renderer</c>.
        /// </summary>
        public Renderer(GraphicsDevice graphics, GraphicsSettings settings)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            GraphicsDevice = graphics;
            Settings = settings ?? new GraphicsSettings();
            GraphicsContext = GraphicsContext ?? new GraphicsContext(graphics, settings);
            Passes = new List<GraphicsPass>();
            Passes.Add(new BasicDrawPass());
            Lights = new LightCollection();
        }

        /// <summary>
        /// Draws a list of objects.
        /// </summary>
        public void Draw(TimeSpan elapsedTime, IEnumerable<object> drawables)
        {
            if (cachedSpatialQuery == null)
                cachedSpatialQuery = new SpatialQuery<object>();
            cachedSpatialQuery.Objects = drawables;
            Draw(elapsedTime, cachedSpatialQuery);
        }

        /// <summary>
        /// Draws a list of objects managed by a scene manager.
        /// </summary>
        public void Draw(TimeSpan elapsedTime, ISpatialQuery<object> drawables)
        {
            GraphicsContext.View = Camera.View;
            GraphicsContext.Projection = Camera.Projection;
            GraphicsContext.ElapsedTime = elapsedTime;

            foreach (var pass in Passes)
            {
                pass.Draw(GraphicsContext, drawables);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) { }
    }
}