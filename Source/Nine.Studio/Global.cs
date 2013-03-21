namespace Nine.Studio
{

    using System;
    using System.Collections.Generic;

    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Win32;
    using Nine.Studio.Extensibility;

    static class Global
    {
        public static readonly string Title = "Engine Nine";        
        public static readonly string ExtensionDirectory = ".";
        
        public static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version;
        public static readonly string VersionString = string.Format("v{0}.{1}", Version.Major, Version.Minor);

        public static readonly string IntermediateDirectory = Path.Combine(Path.GetTempPath(), Strings.Title, VersionString, "Intermediate");
        public static readonly string OutputDirectory = Path.Combine(Path.GetTempPath(), Strings.Title, VersionString, "Bin");

        static Global()
        {
            if (!Directory.Exists(IntermediateDirectory))
                Directory.CreateDirectory(IntermediateDirectory);
            if (!Directory.Exists(OutputDirectory))
                Directory.CreateDirectory(OutputDirectory);
        }
    }
}
