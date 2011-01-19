#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Nine.Graphics;
#endregion

namespace Nine.Content.Pipeline.Graphics
{
    /// <summary>
    /// Content for ModelMeshPart.
    /// </summary>
    [ContentSerializerRuntimeType("Nine.Graphics.ModelMeshPartTag, Nine")]
    internal class ModelMeshPartTagContent
    {
        /// <summary>
        /// Gets the additional textures (E.g. normalmap) attached to the ModelMeshPart.
        /// </summary>
        [ContentSerializer()]
        public Dictionary<string, ExternalReference<TextureContent>> Textures { get; internal set; }
    }
}
