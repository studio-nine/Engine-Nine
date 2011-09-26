#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Nine.Content.Pipeline.Processors;
#endregion

namespace Nine.Content.Pipeline.Graphics.ObjectModel
{
    partial class DrawableModelContent
    {
        [ContentSerializer(Optional = true)]
        public virtual Vector3 Position
        {
            get { return position; }
            set { position = value; UpdateTransform(); }
        }
        Vector3 position;

        /// <summary>
        /// Gets or sets the eular rotation in degrees.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public virtual Vector3 Rotation
        {
            get { return rotation; }
            set { rotation = value; }     
        }
        Vector3 rotation;

        [ContentSerializer(Optional = true)]
        public virtual Vector3 Scale
        {
            get { return scale; }
            set { scale = value; }
        }
        Vector3 scale = Vector3.One;
                
        private void UpdateTransform()
        {
            Transform = Matrix.CreateScale(scale) *
                        Matrix.CreateRotationX(MathHelper.ToRadians(rotation.X)) *
                        Matrix.CreateRotationY(MathHelper.ToRadians(rotation.Y)) *
                        Matrix.CreateRotationZ(MathHelper.ToRadians(rotation.Z)) *
                        Matrix.CreateTranslation(position);
        }
    }
}
