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
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Isles.Graphics.Models;
#endregion


namespace Isles.Pipeline
{
    public class SplatTextureContent
    {
        [ContentSerializer(Optional=true)]
        public string LayerR { get; set; }

        [ContentSerializer(Optional = true)]
        public string LayerG { get; set; }

        [ContentSerializer(Optional = true)]
        public string LayerB { get; set; }

        [ContentSerializer(Optional = true)]
        public string LayerA { get; set; }
    }
}
