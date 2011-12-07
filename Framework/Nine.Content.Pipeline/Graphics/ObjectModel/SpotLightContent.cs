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
            Vector3.Normalize(ref direction, out direction);

            if (direction.X == 0 && direction.Y == 0)
                Transform = Matrix.CreateLookAt(position, direction, Vector3.Up);
            else
                Transform = Matrix.CreateWorld(position, direction, Vector3.UnitZ);
        }

        partial void OnCreate()
        {
            OuterAngle = MathHelper.ToDegrees(OuterAngle);
            InnerAngle = MathHelper.ToDegrees(InnerAngle);
        }
    }

    partial class SpotLightContentWriter
    {
        partial void BeginWrite(ContentWriter output, SpotLightContent value)
        {
            var outer = value.OuterAngle;
            var inner = value.InnerAngle;

            value.OuterAngle = MathHelper.ToRadians(Math.Max(outer, inner));
            value.InnerAngle = MathHelper.ToRadians(Math.Min(outer, inner));
        }
    }
}
