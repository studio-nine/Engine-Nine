#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

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
            Transform = MatrixHelper.CreateWorld(Position, Direction);
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
