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
    public class FbxModelSerializer : PipelineDocumentSerializer<ModelContent>
    {
        XImporter xImporter = new XImporter();
        FbxImporter fbxImporter = new FbxImporter();
        ModelProcessor modelProcessor = new ModelProcessor();

        public FbxModelSerializer()
        {
            DisplayName = Strings.Model;
            FileExtensions.Add("*.fbx");
        }

        public override IContentImporter ContentImporter
        {
            get { return fbxImporter; }
        }

        public override IContentProcessor ContentProcesser
        {
            get { return modelProcessor; }
        }
    }
}
