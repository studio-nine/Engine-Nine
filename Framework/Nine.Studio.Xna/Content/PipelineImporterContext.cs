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
using Nine.Studio.Extensibility;
using Nine.Studio.Controls;
using Microsoft.Xna.Framework.Content.Pipeline;
#endregion

namespace Nine.Studio.Content
{
    class PipelineImporterContext : ContentImporterContext
    {
        public ICollection<string> Dependencies { get; private set; }

        public PipelineImporterContext()
        {
            Dependencies = new HashSet<string>();
        }

        public override void AddDependency(string filename)
        {
            Dependencies.Add(filename);
        }

        public override string IntermediateDirectory
        {
            get { return Path.Combine(Path.GetTempPath(), Strings.Title, Global.IntermediateDirectory); }
        }

        public override ContentBuildLogger Logger
        {
            get { return PipelineBuildLogger.Instance; }
        }

        public override string OutputDirectory
        {
            get { return Path.Combine(Path.GetTempPath(), Strings.Title, Global.OutputDirectory); }
        }
    }
}
