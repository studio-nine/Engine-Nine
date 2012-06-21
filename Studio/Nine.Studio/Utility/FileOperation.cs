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
        public static void BackupAndSave(string fileName, Action<Stream> save)
        {
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
            catch (Exception) { }
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
