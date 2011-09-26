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
    partial class SpotLightContent
    {
        [ContentSerializer(Optional = true)]
        public virtual Vector3 Position
        {
            get { return position; }
            set { position = value; UpdateTransform(); }
        }
        Vector3 position;

        [ContentSerializer(Optional = true)]
        public virtual Vector3 Direction
        {
            get { return direction; }
            set { direction = value; }
        }
        Vector3 direction = Vector3.Forward;

        private void UpdateTransform()
        {
            if (direction.X == 0 && direction.Y == 0)
                Transform = Matrix.CreateLookAt(position, position + direction, Vector3.Up);
            else
                Transform = Matrix.CreateLookAt(position, position + direction, Vector3.UnitZ);
        }
    }
}
