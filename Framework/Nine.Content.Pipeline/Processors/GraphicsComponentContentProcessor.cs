#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Linq;
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
using Nine.Content.Pipeline.Graphics.Effects;
using Nine.Content.Pipeline.Graphics.Effects.EffectParts;
using Nine.Content.Pipeline.Components;
using Nine.Components;
#endregion

namespace Nine.Content.Pipeline.Processors
{
    [ContentProcessor(DisplayName = "Graphics Component - Engine Nine")]
    class GraphicsComponentContentProcessor : ContentProcessor<GraphicsComponentContent, GraphicsComponent>
    {
        public override GraphicsComponent Process(GraphicsComponentContent input, ContentProcessorContext context)
        {
            var compiled = context.BuildAsset<object, object>(input.Content, "DefaultContentProcessor", null, null);
            var startIndex = context.OutputDirectory.Length;

            return new GraphicsComponent()
            {
                Name = input.Name,
                Tag = input.Tag,
                Template = compiled.Filename.Substring(startIndex, compiled.Filename.Length - startIndex - 4)
            };
        }
    }
}
