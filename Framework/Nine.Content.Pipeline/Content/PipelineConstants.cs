namespace Nine.Content.Pipeline
{
    using System;
    using System.IO;
    using System.Reflection;
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Content.Pipeline.Graphics;


    class PipelineConstants
    {
        public string IntermediateDirectory;
        public string OutputDirectory;
        public string BuildConfiguration;
        public GraphicsDevice GraphicsDevice;
        public TargetPlatform TargetPlatform;
        public GraphicsProfile TargetProfile { get { return GraphicsDevice.GraphicsProfile; } }

        public PipelineConstants(string intermediateDirectory, string outputDirectory, TargetPlatform targetPlatform, GraphicsDevice graphics)
        {
            if (intermediateDirectory == null || outputDirectory == null)
            {
                var guid = Guid.NewGuid().ToString("B");
                var baseDirectory = Path.Combine(Path.GetTempPath(), "Engine Nine", GetVersion(), "PipelineBuilder");
                IntermediateDirectory = Path.Combine(baseDirectory, "Intermediate");
                OutputDirectory = Path.Combine(baseDirectory, "Bin");
            }

            IntermediateDirectory = intermediateDirectory ?? IntermediateDirectory;
            OutputDirectory = outputDirectory ?? OutputDirectory;
            GraphicsDevice = graphics ?? PipelineGraphics.GraphicsDevice;            
            BuildConfiguration = "Release";
        }

        private static string GetVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return string.Format("v{0}.{1}", version.Major, version.Minor);
        }
    }
}
