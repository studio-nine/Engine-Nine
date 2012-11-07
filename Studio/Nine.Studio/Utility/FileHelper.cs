namespace Nine.Studio
{
    using System;
    using System.Diagnostics;
    using System.IO;

    static class FileHelper
    {
        public static string FindNextValidFileName(string filename, string extension, bool startWithDigits = true)
        {
            var i = 1;
            var result = "";
            
            if (!startWithDigits && !File.Exists(result = filename + extension))
                return result;

            while (File.Exists(result = filename + i + extension)) i++;
            return result;
        }

        public static string NormalizeExtension(string fileExtension)
        {
            if (string.IsNullOrEmpty(fileExtension))
                return "";

            fileExtension = fileExtension.Trim(' ', '.', '|', '*');
            fileExtension = fileExtension.Insert(0, ".");
            return fileExtension.ToLowerInvariant();
        }

        public static string NormalizeFileName(string fileName)
        {
            if (fileName == null)
                return null;
            return Path.GetFullPath(new Uri(Path.GetFullPath(fileName)).LocalPath)
                       .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        public static string NormalizePath(string fileName)
        {
            if (fileName == null)
                return null;
            fileName = Path.GetFullPath(new Uri(Path.GetFullPath(fileName)).LocalPath)
                           .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            if (!fileName.EndsWith(Path.DirectorySeparatorChar.ToString()))
                fileName = fileName + Path.DirectorySeparatorChar;
            return fileName;
        }

        public static bool FileNameEquals(string filename1, string filename2)
        {
            return string.Compare(NormalizeFileName(filename1),
                                  NormalizeFileName(filename2),
                                  StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public static string GetRelativeFileName(string fileName, string path)
        {
            Uri uri = new Uri(NormalizeFileName(fileName));
            Uri dir = new Uri(NormalizePath(path));
            return dir.MakeRelativeUri(uri).OriginalString;
        }

        public static void BackupAndSave(string fileName, Action<Stream> save)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName");

            if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));

            if (!File.Exists(fileName))
            {
                SaveAndDeleteFileOnError(fileName, save);
            }
            else
            {
                var tempFilename = fileName + ".tmp";
                var backupFilename = fileName + ".bak";

                try
                {
                    SaveAndDeleteFileOnError(tempFilename, save);
                    File.Replace(tempFilename, fileName, backupFilename);
                    DeleteFile(backupFilename);
                }
                finally
                {
                    DeleteFile(backupFilename);
                }
            }
        }

        public static void SaveAndDeleteFileOnError(string fileName, Action<Stream> save)
        {
            try
            {
                using (FileStream stream = new FileStream(fileName, FileMode.Create))
                {
                    save(stream);
                }
            }
            catch (Exception e)
            {
                DeleteFile(fileName);
                throw e;
            }
        }

        public static void DeleteFile(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }
            catch (Exception) 
            {
                Trace.TraceError("Error deleting file " + fileName);
            }
        }

        public static IDisposable WatchFileChanged(string fileName, Action<string> contentChanged)
        {
            Verify.IsNotNull(contentChanged, "contentChanged");
            Verify.IsValidPath(fileName, "fileName");

            string directory = Path.GetDirectoryName(fileName);
            string filter = Path.GetFileName(fileName);
            
            var watcher = new FileSystemWatcher(directory, filter);
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Changed += (sender, e) => 
            {
                if (e.ChangeType == WatcherChangeTypes.Changed)
                    contentChanged(Path.Combine(e.FullPath, e.Name));
            };
            watcher.EnableRaisingEvents = true;
            return watcher;
        }
    }
}
