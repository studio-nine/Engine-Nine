#region File Description
//-----------------------------------------------------------------------------
// GameComponent.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// A base class for a component that can be updated.
    /// </summary>
    public abstract class GameComponent : IGameComponent, IUpdateable, IDisposable
    {
        private bool enabled = true;
        private int updateOrder = 0;

        /// <summary>
        /// Gets or sets whether the component should be updated.
        /// </summary>
        public bool Enabled
        {
            get
            {
                VerifyNotDisposed(); 
                return enabled;
            }
            set
            {
                VerifyNotDisposed();
                if (enabled != value)
                {
                    enabled = value;
                    if (EnabledChanged != null)
                        EnabledChanged(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the order in which the component should be updated.
        /// Components with smaller values are updated first.
        /// </summary>
        public int UpdateOrder
        {
            get
            {
                VerifyNotDisposed();
                return updateOrder; 
            }
            set
            {
                VerifyNotDisposed();
                if (updateOrder != value)
                {
                    updateOrder = value;
                    if (UpdateOrderChanged != null)
                        UpdateOrderChanged(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets whether or not the component has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Invoked when the Enabled property changes.
        /// </summary>
        public event EventHandler<EventArgs> EnabledChanged;

        /// <summary>
        /// Invoked when the UpdateOrder property changes.
        /// </summary>
        public event EventHandler<EventArgs> UpdateOrderChanged;

        /// <summary>
        /// Invoked when the component is disposed.
        /// </summary>
        public event EventHandler<EventArgs> Disposed;

        /// <summary>
        /// Initializes a new GameComponent.
        /// </summary>
        protected GameComponent()
        {
            // Hook our own events to surface them as protected virtual methods
            EnabledChanged += OnEnabledChanged;
            UpdateOrderChanged += OnUpdateOrderChanged;
        }

        ~GameComponent()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes the component. This does not remove the component from any collections; that must
        /// be down manually.
        /// </summary>
        public void Dispose()
        {
            // Don't dispose more than once
            if (IsDisposed)
                return;

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Allows the component to dispose any resources.
        /// </summary>
        /// <param name="disposing">True if the Dispose method was called, false if the finalizer is running.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (this)
                {
                    if (Disposed != null)
                        Disposed(this, EventArgs.Empty);
                }
            }

            IsDisposed = true;
        }

        /// <summary>
        /// Initializes the component.
        /// </summary>
        public virtual void Initialize() { }

        /// <summary>
        /// Updates the component.
        /// </summary>
        public virtual void Update(GameTime gameTime) { }

        /// <summary>
        /// Called when the component's Enabled property has changed.
        /// </summary>
        protected virtual void OnEnabledChanged(object sender, EventArgs args) { }

        /// <summary>
        /// Called when the component's UpdateOrder property has changed.
        /// </summary>
        protected virtual void OnUpdateOrderChanged(object sender, EventArgs args) { }

        /// <summary>
        /// Helper that throws an exception if the component is disposed.
        /// </summary>
        protected void VerifyNotDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException("GameComponent");
        }
    }
}
