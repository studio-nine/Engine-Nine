#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Xml;
using Nine.Studio.Extensibility;
using System.Xml.Linq;
using System.Reflection;
using Microsoft.Win32;
#endregion

namespace Nine.Studio
{
    static class Global
    {
        public static readonly string Title = "Engine Nine";
        public static readonly string TraceFilename = "Nine.log";
        public static readonly string ExtensionDirectory = "Extensions";
        public static readonly string IntermediateDirectory = "Intermediate";
        public static readonly string OutputDirectory = "Bin";
        public static readonly string FileExtension = ".ix";

        public static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version;
        public static readonly string VersionString = string.Format("v{0}.{1}", Version.Major, Version.Minor);

        public static string NextName(string name, string ext)
        {
            name = string.Join("", name.Split(' '));
            if (!nextFilenameIndex.ContainsKey(name))
                nextFilenameIndex.Add(name, 1);
            if (string.IsNullOrEmpty(ext))
                return string.Format("{0}{1}", name, nextFilenameIndex[name]++);
            return string.Format("{0}{1}{2}", name, nextFilenameIndex[name]++, ext);
        }
        private static Dictionary<string, int> nextFilenameIndex = new Dictionary<string, int>();

        public static void SetProperty(string name, string value)
        {
            Registry.SetValue(GetRegistry(null), name, value);
        }

        public static string GetProperty(string name)
        {
            return (string)Registry.GetValue(GetRegistry(null), name, null);
        }

        public static string GetRegistry(string key)
        {
            string keyPath = string.Format(@"HKEY_LOCAL_MACHINE\Software\{0}\{1}\", Title, VersionString);
            if (!string.IsNullOrEmpty(key))
                keyPath = Path.Combine(keyPath, key);
            return keyPath;
        }

        public static string GetUserRegistry(string key)
        {
            string keyPath = string.Format(@"HKEY_CURRENT_USER\Software\{0}\{1}\", Title, VersionString);
            if (!string.IsNullOrEmpty(key))
                keyPath = Path.Combine(keyPath, key);
            return keyPath;
        }

        public static RegistryKey GetRegistryKey(string key)
        {
            string keyPath = string.Format(@"Software\{0}\{1}\", Title, VersionString);
            if (!string.IsNullOrEmpty(key))
                keyPath = Path.Combine(keyPath, key);
            RegistryKey result = Registry.LocalMachine.OpenSubKey(keyPath);
            if (result == null)
                result = Registry.LocalMachine.CreateSubKey(keyPath);
            return result;
        }

        public static RegistryKey GetUserRegistryKey(string key)
        {
            string keyPath = string.Format(@"Software\{0}\{1}\", Title, VersionString);
            if (!string.IsNullOrEmpty(key))
                keyPath = Path.Combine(keyPath, key);
            RegistryKey result = Registry.CurrentUser.OpenSubKey(keyPath);
            if (result == null)
                result = Registry.CurrentUser.CreateSubKey(keyPath);
            return result;
        }

        public static void SafeSave(string filename, Action<Stream> save)
        {
            MemoryStream tempStream = new MemoryStream();
            save(tempStream);
            byte[] bytes = new byte[tempStream.Length];
            tempStream.Seek(0, SeekOrigin.Begin);
            tempStream.Read(bytes, 0, (int)tempStream.Length);

            using (FileStream stream = new FileStream(filename, FileMode.Create))
            {
                stream.Write(bytes, 0, (int)tempStream.Length);
                stream.Flush();
            }
        }
    }
}
