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
using Nine.Graphics;
using Nine.Graphics.Views;
using System.Xml.Serialization;
#endregion

namespace Nine
{
    /// <summary>
    /// Defines an object that has an abitrary transform.
    /// </summary>
    [Serializable]
    public class FreeObject : IDrawableWorldObject
    {
        #region Transform
        public Matrix Transform
        {
            get { return transform; }
            set
            {
                Matrix oldValue = transform;
                transform = value;
                if (TransformChanged != null)
                    TransformChanged(this, EventArgs.Empty);
                OnTransformChanged(oldValue);
            }
        }
        private Matrix transform;
        #endregion

        #region Template
        public string Template
        {
            get { return template; }
            set
            {
                if (template != value)
                {
                    string oldValue = template;
                    template = value;
                    if (TemplateChanged != null)
                        TemplateChanged(this, EventArgs.Empty);
                    OnTemplateChanged(oldValue);
                }
            }
        }
        private string template;
        #endregion

        public event EventHandler<EventArgs> TransformChanged;
        public event EventHandler<EventArgs> TemplateChanged;

        protected virtual void OnTransformChanged(Matrix oldValue) { }
        protected virtual void OnTemplateChanged(string oldValue) { }
    }
}