#region Copyright 2009 - 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.ComponentModel.Composition;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Nine.Studio.Extensibility;
using XnaImporter = Microsoft.Xna.Framework.Content.Pipeline.TextureImporter;
using XnaProcessor = Microsoft.Xna.Framework.Content.Pipeline.Processors.TextureProcessor;
using System;
#endregion

namespace Nine.Studio.Serializers
{
    [Export(typeof(IImporter))]
    [LocalizedDisplayName("Texture")]
    public class TextureImporter : PipelineImporter<Texture2D>
    {
        public TextureImporter()
        {
            FileExtensions.Add(".gif");
            FileExtensions.Add(".jpg");
            FileExtensions.Add(".bmp");
            FileExtensions.Add(".png");
            FileExtensions.Add(".tga");
            FileExtensions.Add(".dds");
        }

        protected override Texture2D Import(Stream input)
        {
            FileStream fileStream = input as FileStream;
            if (fileStream == null)
                throw new NotSupportedException();

            if (fileStream.Name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                fileStream.Name.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                fileStream.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                return Texture2D.FromStream(GraphicsDevice, input);
            }
            return base.Import(input);
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
