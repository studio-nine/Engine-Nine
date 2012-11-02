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

        public static string NextName(IEnumerable<string> existingNames, string name)
        {
            if (existingNames.All(n => !n.Equals(name, StringComparison.OrdinalIgnoreCase)))
                return name;

            int index = 1;
            string next;
            do
            {
                next = string.Format("{0}{1}", name, index++);
            }
            while (existingNames.Any(n => n.Equals(next, StringComparison.OrdinalIgnoreCase)));
            return next;
        }

        public static string NextName(string name, string ext)
        {
            if (string.IsNullOrEmpty(name))
                name = Strings.Untitled;

            if (name == null)
                name = "";  
          
            if (ext == null)
                ext = "";
            else if (!string.IsNullOrEmpty(ext))
                ext = ext.GetNormalizedFileExtension();

            var fileName = "";
            var index = 1;
            do
            {
                fileName = string.Format("{0}{1}{2}", name, index++, ext);
            }
            while (File.Exists(fileName));

            return fileName;
        }
        
        public static bool MatchFileExtension(this IImporter serializer, string match)
        {
            foreach (var ext in serializer.FileExtensions)
            {
                if (FileHelper.GetNormalizedFileExtension(match) == FileHelper.GetNormalizedFileExtension(ext))
                    return true;
            }
            return false;
        }

        public static bool MatchFileExtension(this IExporter serializer, string match)
        {
            foreach (var ext in serializer.FileExtensions)
            {
                if (FileHelper.GetNormalizedFileExtension(match) == FileHelper.GetNormalizedFileExtension(ext))
                    return true;
            }
            return false;
        }
    }
}
