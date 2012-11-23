namespace Nine.Studio
{
    using System;

    using System.IO;
    using System.Reflection;



    static class Constants
    {
        public const int MaxHeaderBytes = 128;

        public static readonly string Title = "Engine Nine";
        public static readonly string TraceFilename = "Nine.log";

        public static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version;
        public static readonly string VersionString = string.Format("v{0}.{1}", Version.Major, Version.Minor);

        public static readonly string IntermediateDirectory = Path.Combine(Path.GetTempPath(), Strings.Title, VersionString, "Intermediate");
        public static readonly string OutputDirectory = Path.Combine(Path.GetTempPath(), Strings.Title, VersionString, "Bin");
    }
}
