#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.ComponentModel.Composition;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Nine.Studio.Extensibility;
using XnaImporter = Microsoft.Xna.Framework.Content.Pipeline.TextureImporter;
using XnaProcessor = Microsoft.Xna.Framework.Content.Pipeline.Processors.TextureProcessor;
#endregion

namespace Nine.Studio.Serializers
{
    [Export(typeof(IImporter))]
    [LocalizedDisplayName("Texture")]
    public class TextureImporter : PipelineDocumentImporter<TextureContent>
    {
        public TextureImporter()
        {
            FileExtensions.Add(".jpg");
            FileExtensions.Add(".bmp");
            FileExtensions.Add(".png");
            FileExtensions.Add(".tga");
            FileExtensions.Add(".dds");
        }

        public override IContentImporter ContentImporter
        {
            get { return new XnaImporter(); }
        }

        public override IContentProcessor ContentProcesser
        {
            get { return new XnaProcessor(); }
        }
    }
}
