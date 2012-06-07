#region Copyright 2009 - 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Win32;
using System.ComponentModel;
using Nine.Studio.Extensibility;
using System.Diagnostics;

#endregion

namespace Nine.Studio
{
    static class FileOperation
    {
        public static void BackupAndSave(string filename, Action<Stream> save)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filename)))
                Directory.CreateDirectory(Path.GetDirectoryName(filename));

            if (!File.Exists(filename))
            {
                SaveAndDeleteFileOnError(filename, save);
            }
            else
            {
                var tempFilename = filename + ".tmp";
                var backupFilename = filename + ".bak";

                try
                {
                    SaveAndDeleteFileOnError(tempFilename, save);
                    File.Replace(tempFilename, filename, backupFilename);
                    DeleteFile(backupFilename);
                }
                finally
                {
                    DeleteFile(backupFilename);
                }
            }
        }

        public static void SaveAndDeleteFileOnError(string filename, Action<Stream> save)
        {
            try
            {
                using (FileStream stream = new FileStream(filename, FileMode.Create))
                {
                    save(stream);
                }
            }
            catch (Exception e)
            {
                DeleteFile(filename);
                throw e;
            }
        }

        public static void DeleteFile(string filename)
        {
            try
            {
                if (File.Exists(filename))
                    File.Delete(filename);
            }
            catch (Exception) { }
        }

        public static IDisposable WatchFileContentChange(string filename, Action<string> contentChanged)
        {
            Verify.IsNotNull(contentChanged, "contentChanged");
            Verify.IsValidPath(filename, "filename");

            string directory = Path.GetDirectoryName(filename);
            string filter = Path.GetFileName(filename);
            
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
