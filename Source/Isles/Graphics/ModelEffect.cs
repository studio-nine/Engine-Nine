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
    public abstract class ModelEffect : GraphicsEffect
    {
        [ContentSerializerIgnore]
        public Matrix[] Bones { get; set; }

        [ContentSerializerIgnore]
        public bool VertexSkinningEnabled { get; set; }
    }
}
