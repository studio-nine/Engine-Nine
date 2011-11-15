#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.IO;
using System.Xaml;
using System.Windows.Markup;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Nine.Components;
using Nine.Content.Pipeline.Processors;
#endregion

namespace Nine.Content.Pipeline.Components
{
    /// <summary>
    /// Content model for <see cref="GraphicsComponent"/>
    /// </summary>
    [ContentProperty("Content")]
    [RuntimeNameProperty("Name")]
    [DefaultContentProcessor(typeof(GraphicsComponentContentProcessor))]
    public class GraphicsComponentContent
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the tag.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Gets or sets the graphics object content.
        /// </summary>
        public object Content { get; set; }
    }
}
