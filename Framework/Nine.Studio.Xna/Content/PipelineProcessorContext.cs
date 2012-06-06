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
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Studio.Content
{
    class PipelineProcessorContext : ContentProcessorContext
    {
        ICollection<string> dependencies;
        ICollection<string> outputs;

        public PipelineProcessorContext()
        {
            dependencies = new HashSet<string>();
            outputs = new HashSet<string>();
        }

        public override void AddDependency(string filename)
        {
            dependencies.Add(filename);
        }

        public override void AddOutputFile(string filename)
        {
            outputs.Add(filename);
        }

        public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName)
        {
            return default(TOutput);
        }

        public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName, string assetName)
        {
            return new ExternalReference<TOutput>(sourceAsset.Filename, sourceAsset.Identity);
        }

        public override TOutput Convert<TInput, TOutput>(TInput input, string processorName, OpaqueDataDictionary processorParameters)
        {
            return default(TOutput);
        }

        public override string BuildConfiguration
        {
            get { return "Release"; }
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

        public override string OutputFilename
        {
            get { return "Output"; }
        }

        public override OpaqueDataDictionary Parameters
        {
            get { throw new NotImplementedException(); }
        }

        public override TargetPlatform TargetPlatform
        {
            get { return TargetPlatform.Windows; }
        }

        public override GraphicsProfile TargetProfile
        {
            get { return GraphicsProfile.Reach; }
        }
    }
}
