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
#endregion


namespace Isles.Graphics
{
    public sealed class DirectionalLight
    {
        public Vector3 DiffuseColor { get; set; }

        public Vector3 Direction { get; set; }

        public bool Enabled { get; set; }

        public Vector3 SpecularColor { get; set; }


        public DirectionalLight()
        {            
            Enabled = true;
            Direction = -Vector3.One;
            DiffuseColor = Vector3.One;
            SpecularColor = Vector3.One;
        }
    }
}
