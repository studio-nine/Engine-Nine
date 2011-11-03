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
using System.ComponentModel;
using System.Windows.Markup;
using Microsoft.Xna.Framework;
#endregion

namespace Nine
{
    /// <summary>
    /// Defines an object that has an abitrary transform.
    /// </summary>
    [Serializable]
    [ContentProperty("Components")]
    public class FreeObject : GameObjectContainer
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

        public event EventHandler<EventArgs> TransformChanged;
        protected virtual void OnTransformChanged(Matrix oldValue) { }
        #endregion
    }
}