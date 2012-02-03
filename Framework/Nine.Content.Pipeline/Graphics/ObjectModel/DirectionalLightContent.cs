#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;

#endregion

namespace Nine.Content.Pipeline.Graphics.ObjectModel
{
    partial class DirectionalLightContent
    {
        [ContentSerializer(Optional = true)]
        public virtual Vector3 Direction
        {
            get { return direction; }
            set { direction = value; Transform = MatrixHelper.CreateWorld(Vector3.Zero, Direction); }     
        }
        Vector3 direction = Vector3.Forward;
    }
}
