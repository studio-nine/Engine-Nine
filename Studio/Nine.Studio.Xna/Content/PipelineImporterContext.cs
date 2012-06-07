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

namespace Nine.Studio.Content
{
    class PipelineImporterContext : ContentImporterContext
    {
        public Action<string> NewDependency;

        public override void AddDependency(string filename)
        {
            if (NewDependency != null)
                NewDependency(Path.GetFullPath(filename));
        }

        public override string IntermediateDirectory
        {
            get { return Global.IntermediateDirectory; }
        }

        public override ContentBuildLogger Logger
        {
            get { return PipelineBuildLogger.Instance; }
        }

        public override string OutputDirectory
        {
            get { return Global.OutputDirectory; }
        }
    }
}
