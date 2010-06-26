#region CPL License
/*
Nuclex Framework
Copyright (C) 2002-2009 Nuclex Development Labs

This library is free software; you can redistribute it and/or
modify it under the terms of the IBM Common Public License as
published by the IBM Corporation; either version 1.0 of the
License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
IBM Common Public License for more details.

You should have received a copy of the IBM Common Public
License along with this library
*/
#endregion

using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nine
{
    #region GameComponent
    /// <summary>
    ///   Variant of the XNA GameComponent that doesn't reference the Game class
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     This is a lightweight version of GameComponent that can be used without
    ///     requiring a Game class to be present. Useful to get all the advantages
    ///     of the XNA GameServices architecture even when you have initialized and
    ///     manage the graphics device yourself.
    ///   </para>
    ///   <para>
    ///     The name of this class is the same as 'GameComponent' minus the 'Game' part
    ///     as the Game reference is what this class removes from its namesake.
    ///   </para>
    /// </remarks>
    public class GameComponent : IGameComponent, Microsoft.Xna.Framework.IUpdateable, IUpdateObject
    {
        /// <summary>Triggered when the value of the enabled property is changed.</summary>
        public event EventHandler<EventArgs> EnabledChanged;

        /// <summary>Triggered when the value of the update order property is changed.</summary>
        public event EventHandler<EventArgs> UpdateOrderChanged;

        /// <summary>Initializes a new drawable component.</summary>
        /// <param name="services">
        ///   Services container from which the graphics device service will be taken
        /// </param>
        public GameComponent(IServiceProvider services)
        {
            this.services = services;
            this.enabled = true;
        }

        /// <summary>Initializes a new drawable component.</summary>
        public GameComponent(Game game)
        {
            this.services = game.Services;
            this.enabled = true;
        }

        /// <summary>Gives the game component a chance to initialize itself</summary>
        public virtual void Initialize() { }

        /// <summary>Called when the component needs to update its state.</summary>
        /// <param name="gameTime">Provides a snapshot of the Game's timing values</param>
        public virtual void Update(GameTime gameTime) { }

        /// <summary>
        ///   Indicates when the updateable component should be updated in relation to
        ///   other updateables. Has no effect by itself.
        /// </summary>
        public int UpdateOrder
        {
            get { return this.updateOrder; }
            set
            {
                if (value != this.updateOrder)
                {
                    this.updateOrder = value;
                    OnUpdateOrderChanged();
                }
            }
        }

        /// <summary>
        ///   True when the updateable component is enabled and should be udpated.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return this.enabled;
            }
            set
            {
                if (value != this.enabled)
                {
                    this.enabled = value;
                    OnEnabledChanged();
                }
            }
        }

        /// <summary>Fires the UpdateOrderChanged event</summary>
        protected virtual void OnUpdateOrderChanged()
        {
            if (this.UpdateOrderChanged != null)
            {
                this.UpdateOrderChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>Fires the EnabledChanged event</summary>
        protected virtual void OnEnabledChanged()
        {
            if (this.EnabledChanged != null)
            {
                this.EnabledChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>Game service container the component was constructed for</summary>
        public IServiceProvider Services
        {
            get { return this.services; }
        }

        /// <summary>Game services container the component was constructed for</summary>
        private IServiceProvider services;
        /// <summary>
        ///   Used to determine the updating order of this object in relation to other
        ///   objects in the same list.
        /// </summary>
        private int updateOrder;
        /// <summary>Whether this object is enabled (and should thus be updated)</summary>
        private bool enabled;
    }
    #endregion

    #region DrawableGameComponent
    /// <summary>
    ///   Lightweight variant DrawableGameComponent that doesn't reference the Game class
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     This is a lightweight version of DrawableGameComponent that can be used
    ///     without requiring a Game class to be present. Useful to get all the
    ///     advantages of the XNA GameServices architecture even when you have
    ///     initialized and manage the graphics device yourself.
    ///   </para>
    ///   <para>
    ///     To work, this class requires to things: A GameServices collection and
    ///     an entry for the IGraphicsDeviceService. You can easily implement this
    ///     interface yourself for any custom graphics device manager.
    ///   </para>
    ///   <para>
    ///     The name of this class is the same as 'DrawableGameComponent' minus the
    ///     'Game' part as the Game reference is what this class removes from its namesake.
    ///   </para>
    /// </remarks>
    public class DrawableGameComponent : GameComponent, Microsoft.Xna.Framework.IDrawable, IDisplayObject, IDisposable
    {
        /// <summary>Triggered when the value of the draw order property is changed.</summary>
        public event EventHandler<EventArgs> DrawOrderChanged;

        /// <summary>Triggered when the value of the visible property is changed.</summary>
        public event EventHandler<EventArgs> VisibleChanged;

        /// <summary>Initializes a new drawable component.</summary>
        /// <param name="services">
        ///   Services container from which the graphics device service will be taken
        /// </param>
        public DrawableGameComponent(IServiceProvider services) : base(services)
        {
            this.visible = true;
        }

        /// <summary>Initializes a new drawable component.</summary>
        public DrawableGameComponent(Game game) : base(game)
        {
            this.visible = true;
        }

        /// <summary>Immediately releases all resources owned by this instance</summary>
        /// <remarks>
        ///   This method is not suitable for being called during a GC run, it is intended
        ///   for manual usage when you actually want to get rid of the Drawable object.
        /// </remarks>
        public virtual void Dispose()
        {
            // Unsubscribe from its events unset the graphics device service once.
            if (this.graphicsDeviceService != null)
            {
                unsubscribeFromGraphicsDeviceService();
                this.graphicsDeviceService = null;
            }
        }

        /// <summary>Gives the game component a chance to initialize itself</summary>
        public override void Initialize()
        {
            // Look for the graphics device service in the game's service container
            this.graphicsDeviceService = Services.GetService(
              typeof(IGraphicsDeviceService)
            ) as IGraphicsDeviceService;

            // Like our XNA pendant, we absolutely require the graphics device service
            if (graphicsDeviceService == null)
                throw new InvalidOperationException("Graphics device service not found");

            // Done, now we can register to the graphics device service's events
            subscribeToGraphicsDeviceService();
        }

        /// <summary>Called when the drawable component needs to draw itself.</summary>
        /// <param name="gameTime">Provides a snapshot of the game's timing values</param>
        public virtual void Draw(GameTime gameTime) { }

        /// <summary>GraphicsDevice this component is bound to. Can be null.</summary>
        public GraphicsDevice GraphicsDevice
        {
            get { return this.graphicsDeviceService.GraphicsDevice; }
        }

        /// <summary>
        ///   Indicates when the drawable component should be drawn in relation to other
        ///   drawables. Has no effect by itself.
        /// </summary>
        public int DrawOrder
        {
            get { return this.drawOrder; }
            set
            {
                if (value != this.drawOrder)
                {
                    this.drawOrder = value;
                    OnDrawOrderChanged();
                }
            }
        }

        /// <summary>True when the drawable component is visible and should be drawn.</summary>
        public bool Visible
        {
            get { return this.visible; }
            set
            {
                if (value != this.visible)
                {
                    this.visible = value;
                    OnVisibleChanged();
                }
            }
        }

        /// <summary>Fires the DrawOrderChanged event</summary>
        protected virtual void OnDrawOrderChanged()
        {
            if (this.DrawOrderChanged != null)
            {
                this.DrawOrderChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>Fires the VisibleChanged event</summary>
        protected virtual void OnVisibleChanged()
        {
            if (this.VisibleChanged != null)
            {
                this.VisibleChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        ///   Called when graphics resources need to be loaded. Override this method to load
        ///   any game-specific graphics resources.
        /// </summary>
        protected virtual void LoadContent() { }

        /// <summary>
        ///   Called when graphics resources need to be unloaded. Override this method to unload
        ///   any game-specific graphics resources.
        /// </summary>
        protected virtual void UnloadContent() { }

        /// <summary>
        ///   Subscribes this component to the events of the graphics device service.
        /// </summary>
        private void subscribeToGraphicsDeviceService()
        {
            // Register to the events of the graphics device service so we know when
            // the graphics device is set up, shut down or reset.
            this.graphicsDeviceService.DeviceCreated += new EventHandler<EventArgs>(deviceCreated);

            // If a graphics device has already been created, we need to simulate the
            // DeviceCreated event that we did miss because we weren't born yet :)
            if (this.graphicsDeviceService.GraphicsDevice != null)
            {
                LoadContent();
            }
        }

        /// <summary>
        ///   Unsubscribes this component from the events of the graphics device service.
        /// </summary>
        private void unsubscribeFromGraphicsDeviceService()
        {
            // Unsubscribe from the events again
            this.graphicsDeviceService.DeviceCreated -= new EventHandler<EventArgs>(deviceCreated);

            // If the graphics device is still active, we give the component a chance
            // to clean up its data
            if (this.graphicsDeviceService.GraphicsDevice != null)
            {
                UnloadContent();
            }
        }

        /// <summary>Called when the graphics device is created</summary>
        /// <param name="sender">Graphics device service that created a new device</param>
        /// <param name="arguments">Not used</param>
        private void deviceCreated(object sender, EventArgs arguments)
        {
            LoadContent();
        }

        /// <summary>Graphics device service this component is bound to.</summary>
        private IGraphicsDeviceService graphicsDeviceService;
        /// <summary>
        ///   Used to determine the drawing order of this object in relation to other
        ///   objects in the same list.
        /// </summary>
        private int drawOrder;
        /// <summary>Whether this object is visible (and should thus be drawn)</summary>
        private bool visible;
    }
    #endregion
}
