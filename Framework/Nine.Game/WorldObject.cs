#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Markup;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine
{
    /// <summary>
    /// Defines an object that has a position, rotation and scale.
    /// </summary>
    [Serializable]
    [ContentProperty("Components")]
    public class WorldObject : GameObjectContainer
    {
        #region Position
        public Vector3 Position
        {
            get { return position; }
            set 
            {
                if (position != value)
                {
                    transformNeedsUpdate = true;
                    Vector3 oldValue = position;
                    position = value;
                    if (PositionChanged != null)
                        PositionChanged(this, EventArgs.Empty);
                    OnPositionChanged(oldValue);
                    if (TransformChanged != null)
                        TransformChanged(this, EventArgs.Empty);
                    OnTransformChanged(transform);
                }
            }
        }
        private Vector3 position;

        public event EventHandler<EventArgs> PositionChanged;
        protected virtual void OnPositionChanged(Vector3 oldValue) { }
        #endregion

        #region Rotation
        public float Rotation
        {
            get { return rotation; }
            set
            {
                if (rotation != value)
                {
                    transformNeedsUpdate = true;
                    float oldValue = rotation;
                    rotation = value;
                    if (RotationChanged != null)
                        RotationChanged(this, EventArgs.Empty);
                    OnRotationChanged(oldValue);
                    if (TransformChanged != null)
                        TransformChanged(this, EventArgs.Empty);
                    OnTransformChanged(transform);
                }
            }
        }
        private float rotation;

        public event EventHandler<EventArgs> RotationChanged;
        protected virtual void OnRotationChanged(float oldValue) { }
        #endregion

        #region Scale
        public float Scale
        {
            get { return scale; }
            set
            {
                if (scale != value)
                {
                    transformNeedsUpdate = true;
                    float oldValue = scale;
                    scale = value;
                    if (ScaleChanged != null)
                        ScaleChanged(this, EventArgs.Empty);
                    OnScaleChanged(oldValue);
                    if (TransformChanged != null)
                        TransformChanged(this, EventArgs.Empty);
                    OnTransformChanged(transform);
                }
            }
        }
        private float scale = 1;

        public event EventHandler<EventArgs> ScaleChanged;
        protected virtual void OnScaleChanged(float oldValue) { }
        #endregion

        #region Transform
        public Matrix Transform
        {
            get
            {
                if (transformNeedsUpdate)
                {
                    Matrix temp;
                    Matrix.CreateScale(scale, out transform);
                    Matrix.CreateRotationZ(rotation, out temp);
                    Matrix.Multiply(ref transform, ref temp, out transform);
                    transform.M41 = position.X;
                    transform.M42 = position.Y;
                    transform.M43 = position.Z;
                    transformNeedsUpdate = false;
                }
                return transform;
            }
        }
        private Matrix transform = Matrix.Identity;
        private bool transformNeedsUpdate = false;

        public event EventHandler<EventArgs> TransformChanged;
        protected virtual void OnTransformChanged(Matrix oldValue) { }
        #endregion
    }
}