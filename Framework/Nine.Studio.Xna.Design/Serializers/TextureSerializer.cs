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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using Nine.Studio.Extensibility;
using Nine.Studio.Controls;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
#endregion

namespace Nine.Studio.Serializers
{
    [Export(typeof(IDocumentSerializer))]
    public class TextureSerializer : PipelineDocumentSerializer<TextureContent>
    {
        TextureImporter textureImporter = new TextureImporter();
        TextureProcessor textureProcessor = new TextureProcessor();

        public TextureSerializer()
        {
            DisplayName = Strings.Texture;

            FileExtensions.Add("*.jpg");
            FileExtensions.Add("*.bmp");
            FileExtensions.Add("*.png");
            FileExtensions.Add("*.tga");
            FileExtensions.Add("*.dds");
        }

        public override IContentImporter ContentImporter
        {
            get { return textureImporter; }
        }

        public override IContentProcessor ContentProcesser
        {
            get { return textureProcessor; }
        }
    }
}
