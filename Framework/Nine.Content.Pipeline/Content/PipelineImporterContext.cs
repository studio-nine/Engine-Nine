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
using Microsoft.Xna.Framework.Content.Pipeline;
#endregion

namespace Nine.Content.Pipeline
{
    class PipelineImporterContext : ContentImporterContext
    {
        public PipelineImporterContext()
        {

        }

        public override void AddDependency(string filename)
        {

        }

        public override string IntermediateDirectory
        {
            get { return PipelineConstants.IntermediateDirectory; }
        }

        public override ContentBuildLogger Logger
        {
            get { return PipelineBuildLogger.Instance; }
        }

        public override string OutputDirectory
        {
            get { return PipelineConstants.OutputDirectory; }
        }
    }
}
