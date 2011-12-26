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
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Nine.Content.Pipeline.Processors;
using Nine.Content.Pipeline.Graphics.ObjectModel;
using Nine.Graphics;
#endregion

namespace Nine.Content.Pipeline.Components
{
    /// <summary>
    /// Content model for <see cref="GraphicsComponent"/>
    /// </summary>
    [ContentProperty("Content")]
    [RuntimeNameProperty("Name")]
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
        public List<object> Content { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsComponentContent"/> class.
        /// </summary>
        public GraphicsComponentContent()
        {
            Content = new List<object>();
        }

        [SelfProcess]
        static GraphicsComponent Process(GraphicsComponentContent input, ContentProcessorContext context)
        {
            var result = new GraphicsComponent { Name = input.Name, Tag = input.Tag };

            if (input.Content == null || input.Content.Count <= 0)
            {
                return result;
            }

            object build = input.Content[0];
            if (!(input.Content.Count == 1 && input.Content[0] is DisplayObjectContent))
            {
                var container = new DisplayObjectContent();
                container.Children.AddRange(input.Content);
                build = container;
            }

            var compiled = context.BuildAsset<object, object>(build, "DefaultContentProcessor", null, null);
            var startIndex = context.OutputDirectory.Length;
            result.Template = compiled.Filename.Substring(startIndex, compiled.Filename.Length - startIndex - 4);
            return result;
        }
    }
}
