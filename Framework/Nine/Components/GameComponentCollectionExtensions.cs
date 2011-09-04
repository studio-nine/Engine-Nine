#region Copyright 2009 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Components
{
    /// <summary>
    /// Contains extension methods related to GameComponentCollection.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class GameComponentCollectionExtensions
    {
        public static void Add(this GameComponentCollection components, object component)
        {
            if (component == null)
                throw new ArgumentNullException("component");

            components.Add(new GameComponentAdapter() { InnerComponent = component });
        }

        public static void Insert(this GameComponentCollection components, int index, object component)
        {
            if (component == null)
                throw new ArgumentNullException("component");

            components.Insert(index, new GameComponentAdapter() { InnerComponent = component });
        }

        public static void Remove(this GameComponentCollection components, object component)
        {
            if (component == null)
                throw new ArgumentNullException("component");

            var item = components.FirstOrDefault(c =>
                (c is GameComponentAdapter) && ((GameComponentAdapter)c).InnerComponent == component);
            if (item != null)
                components.Remove(item);
        }
    }

    class GameComponentAdapter : IGameComponent, Microsoft.Xna.Framework.IDrawable,
                                                 Microsoft.Xna.Framework.IUpdateable
    {
        public object InnerComponent;

        public void Initialize()
        {
            if (InnerComponent is IGameComponent)
                ((IGameComponent)InnerComponent).Initialize();
        }

        public void Update(GameTime gameTime)
        {
            if (Enabled && InnerComponent is IUpdateable)
                ((IUpdateable)InnerComponent).Update(gameTime.ElapsedGameTime);
        }

        public void Draw(GameTime gameTime)
        {
            if (Visible && InnerComponent is IDrawable)
                ((IDrawable)InnerComponent).Draw(gameTime.ElapsedGameTime);
        }



        public int DrawOrder
        {
            get { return _DrawOrder; }
            set
            {
                if (value != _DrawOrder)
                {
                    _DrawOrder = value;
                    if (DrawOrderChanged != null)
                        DrawOrderChanged(this, EventArgs.Empty);
                }
            }
        }
        private int _DrawOrder;
        public event EventHandler<EventArgs> DrawOrderChanged;

        public bool Visible
        {
            get { return _Visible; }
            set
            {
                if (value != _Visible)
                {
                    _Visible = value;
                    if (VisibleChanged != null)
                        VisibleChanged(this, EventArgs.Empty);
                }
            }
        }
        private bool _Visible = true;
        public event EventHandler<EventArgs> VisibleChanged;

        public bool Enabled
        {
            get { return _Enabled; }
            set
            {
                if (value != _Enabled)
                {
                    _Enabled = value;
                    if (EnabledChanged != null)
                        EnabledChanged(this, EventArgs.Empty);
                }
            }
        }
        private bool _Enabled = true;
        public event EventHandler<EventArgs> EnabledChanged;

        public int UpdateOrder
        {
            get { return _UpdateOrder; }
            set
            {
                if (value != _UpdateOrder)
                {
                    _UpdateOrder = value;
                    if (UpdateOrderChanged != null)
                        UpdateOrderChanged(this, EventArgs.Empty);
                }
            }
        }
        private int _UpdateOrder;        
        public event EventHandler<EventArgs> UpdateOrderChanged;
    }
}