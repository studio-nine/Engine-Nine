namespace Nine.Content.Pipeline
{
    using System;
    using System.IO;
    using System.Reflection;
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Content.Pipeline.Graphics;


    static class PipelineConstants
    {
        public static string IntermediateDirectory { get; set; }
        public static string OutputDirectory { get; set; }
        public static string BuildConfiguration { get; set; }
        public static TargetPlatform TargetPlatform { get; set; }
        public static GraphicsProfile TargetProfile { get; set; }
        public static GraphicsDevice GraphicsDevice { get { return PipelineGraphics.GraphicsDevice; } }

        static PipelineConstants()
        {
            string guid = Guid.NewGuid().ToString("B");
            string baseDirectory = Path.Combine(Path.GetTempPath(), "Engine Nine", GetVersion(), "PipelineBuilder");

            IntermediateDirectory = Path.Combine(baseDirectory, "Intermediate");
            OutputDirectory = Path.Combine(baseDirectory, "Bin");
            BuildConfiguration = "Release";
            TargetProfile = GraphicsDevice.GraphicsProfile;
            TargetPlatform = TargetPlatform.Windows;
        }

        private static string GetVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return string.Format("v{0}.{1}", version.Major, version.Minor);
        }
    }
}
