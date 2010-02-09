#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion


namespace Isles.Graphics
{
    public sealed class PointLight
    {
        public Vector3 DiffuseColor { get; set; }

        public Vector3 Position { get; set; }

        public float Radius { get; set; }

        [ContentSerializer(Optional=true)]
        public bool Enabled { get; set; }

        public Vector3 SpecularColor { get; set; }


        public PointLight()
        {            
            Enabled = true;
            Radius = 1.0f;
            DiffuseColor = Vector3.One;
            SpecularColor = Vector3.One;
        }
    }
}
