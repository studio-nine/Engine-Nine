#region Copyright 2009 - 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System.Windows.Markup;
using Nine.Content.Pipeline.Processors;
#endregion

namespace Nine.Content.Pipeline.Xaml
{
    [ContentProperty("Layers")]
    [DefaultContentProcessor(typeof(SplatterTextureProcessor))]
    public class Splatter
    {
        public IList<ExternalReference<Texture2DContent>> Layers
        {
            get { return layers; }
        }
        private List<ExternalReference<Texture2DContent>> layers = new List<ExternalReference<Texture2DContent>>();
    }
}
