using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace ProcessSamples
{
    class Program
    {
        static void Main(string[] args)
        {
            string outputDirectory = Environment.CurrentDirectory;
            string targetDirectory = Environment.CurrentDirectory;

            if (args.Length > 0)
                targetDirectory = Path.GetFullPath(args[0]);
            if (args.Length > 1)
                outputDirectory = Path.GetFullPath(args[1]);

            Console.WriteLine("Processing Samples in " + targetDirectory);
            Console.WriteLine("                      " + outputDirectory);
            Console.WriteLine();

            try
            {
                Directory.SetCurrentDirectory(targetDirectory);

                var files = Directory.EnumerateFiles(targetDirectory, @"*.*", SearchOption.AllDirectories)
                               .Where(f => f.IndexOf("svn", StringComparison.OrdinalIgnoreCase) < 0 &&
                                           f.IndexOf(@"\obj\", StringComparison.OrdinalIgnoreCase) < 0 &&
                                           f.IndexOf(@".cache", StringComparison.OrdinalIgnoreCase) < 0 &&
                                           f.IndexOf(@".suo", StringComparison.OrdinalIgnoreCase) < 0 &&
                                           f.IndexOf(".pdb", StringComparison.OrdinalIgnoreCase) < 0 &&
                                           f.IndexOf("TutorialData", StringComparison.OrdinalIgnoreCase) < 0 &&
                                           !Regex.IsMatch(f,  @"Nine.*.xml"))
                               .ToArray();

                Directory.CreateDirectory(outputDirectory);

                Console.WriteLine();
                Console.WriteLine("Listing Files...");
                Console.ForegroundColor = ConsoleColor.Gray;
                //files.All(n => { Console.WriteLine(n); return true; });

                CopyExecutables(outputDirectory, files, "x86");
                CopyExecutables(outputDirectory, files, "Xbox 360");
                CopyExecutables(outputDirectory, files, "Windows Phone");

                Console.WriteLine("Archiving Samples...");
                files = files.Where(f => f.IndexOf(@"\bin", StringComparison.OrdinalIgnoreCase) < 0).ToArray();
                var archiveFile = Path.Combine(outputDirectory, "Samples", "Sources.zip");
                if (File.Exists(archiveFile))
                    File.Delete(archiveFile);
                Zip.ZipFile zip = new Zip.ZipFile(archiveFile);
                foreach (var f in files)
                    zip.AddFile(f.Substring(f.IndexOf(targetDirectory, StringComparison.OrdinalIgnoreCase) + targetDirectory.Length + 1), true);
                zip.Save();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void CopyExecutables(string outputDirectory, string[] files, string platform)
        {
            string binFolder = @"\bin\" + platform + @"\release";
            string executableFolder = @"Samples\" + platform;

            Directory.CreateDirectory(Path.Combine(outputDirectory, executableFolder));

            var executables = files.Where(f => f.IndexOf(binFolder, StringComparison.OrdinalIgnoreCase) >= 0)
                                      .ToArray();

            if (platform == "Windows Phone")
                executables = executables.Where(f => f.EndsWith(".xap", StringComparison.OrdinalIgnoreCase)).ToArray();
            else if (platform == "Xbox 360")
                executables = executables.Where(f => f.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)).ToArray();

            Console.ForegroundColor = ConsoleColor.Gray;
            //x86Executables.All(n => { Console.WriteLine(n); return true; });

            foreach (var g in executables.GroupBy(f => GetSampleName(f, binFolder)))
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Copying Sample: " + g.Key);
                Console.ForegroundColor = ConsoleColor.Gray;

                if (platform == "Windows")
                    Directory.CreateDirectory(Path.Combine(outputDirectory, executableFolder, g.Key));

                foreach (var file in g)
                {
                    string dest = null;
                    if (platform == "Windows Phone")
                    {
                        dest = Path.Combine(outputDirectory, executableFolder,
                                  file.Substring(file.IndexOf(binFolder, StringComparison.OrdinalIgnoreCase) + binFolder.Length + 1));
                    }
                    else if (platform == "Xbox 360")
                    {
                        dest = Path.Combine(outputDirectory, executableFolder, Path.GetFileName(file));
                        dest = Path.ChangeExtension(dest, ".ccgame");

                        var xnapack = Path.Combine(Environment.GetEnvironmentVariable("XNAGSShared"), "XnaPack", "XnaPack.exe");
                        Process p = new Process();
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.FileName = xnapack;
                        p.StartInfo.Arguments = "\"" + file + "\"" + " /o:\"" + dest + "\"";
                        p.Start();
                        p.WaitForExit();
                    }
                    else
                    {
                        dest = Path.Combine(outputDirectory, executableFolder, g.Key,
                                  file.Substring(file.IndexOf(binFolder, StringComparison.OrdinalIgnoreCase) + binFolder.Length + 1));
                    }

                    if (platform != "Xbox 360")
                    {
                        Console.WriteLine("Copying " + file + " to " + dest);
                        Directory.CreateDirectory(Path.GetDirectoryName(dest));
                        File.Copy(file, dest, true);
                    }
                }
            }

            Console.ForegroundColor = ConsoleColor.White;
        }

        private static string GetSampleName(string f, string binFolder)
        {
            int i = f.IndexOf(binFolder, StringComparison.OrdinalIgnoreCase) - 1;
            int j = f.LastIndexOfAny(new[] { '\\', '/' }, i);
            return f.Substring(j + 1, i - j);
        }
    }
}
