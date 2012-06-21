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
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Nine.Studio.Extensibility;
#endregion

namespace Nine.Studio.Serializers
{
    [Export(typeof(IImporter))]
    [LocalizedDisplayName("XModel")]
    [LocalizedCategory("Model")]
    public class XModelImporter : PipelineDocumentImporter<ModelContent>
    {
        public XModelImporter()
        {
            FileExtensions.Add(".x");
        }

        public override IContentImporter ContentImporter
        {
            get { return new XImporter(); }
        }

        public override IContentProcessor ContentProcesser
        {
            get { return new ModelProcessor(); }
        }
    }
}
