namespace Nine.Studio
{
    using System;
    using System.Diagnostics;
    using System.IO;

    static class FileOperation
    {
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
                Trace.WriteLine("Error deleting file " + fileName);
            }
        }

        public static IDisposable WatchFileContentChange(string fileName, Action<string> contentChanged)
        {
            Verify.IsNotNull(contentChanged, "contentChanged");
            Verify.IsValidPath(fileName, "fileName");

            string directory = Path.GetDirectoryName(fileName);
            string filter = Path.GetFileName(fileName);
            
            FileSystemWatcher watcher = new FileSystemWatcher(directory, filter);
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
