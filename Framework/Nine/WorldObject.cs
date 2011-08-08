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
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine
{
    /// <summary>
    /// Defines an object that has a position, rotation and scale.
    /// </summary>
    [Serializable]
    public class WorldObject : IWorldObject
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
                }
            }
        }
        private Vector3 position;
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
                }
            }
        }
        private float rotation;
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
                }
            }
        }
        private float scale = 1;
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
        #endregion

        #region Template
        public Template Template
        {
            get { return template; }
            set
            {
                if (template != value)
                {
                    Template oldValue = template;
                    template = value;
                    if (TemplateChanged != null)
                        TemplateChanged(this, EventArgs.Empty);
                    OnTemplateChanged(oldValue);
                }
            }
        }
        private Template template;
        #endregion

        public event EventHandler<EventArgs> PositionChanged;
        public event EventHandler<EventArgs> RotationChanged;
        public event EventHandler<EventArgs> ScaleChanged;
        public event EventHandler<EventArgs> TemplateChanged;        

        protected virtual void OnPositionChanged(Vector3 oldValue) { }
        protected virtual void OnRotationChanged(float oldValue) { }
        protected virtual void OnScaleChanged(float oldValue) { }
        protected virtual void OnTemplateChanged(Template oldValue) { }
    }
}