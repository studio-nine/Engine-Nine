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
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Studio.Serializers
{
    [Export(typeof(IImporter))]
    [LocalizedDisplayName("FbxModel")]
    [LocalizedCategory("Model")]
    public class FbxModelImporter : PipelineImporter<Model>
    {
        public FbxModelImporter()
        {
            FileExtensions.Add(".fbx");
        }

        public override IContentImporter ContentImporter
        {
            get { return new FbxImporter(); }
        }

        public override IContentProcessor ContentProcesser
        {
            get { return new ModelProcessor(); }
        }
    }
}
