#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using Nine.Tools.ScreenshotCapturer;
using Microsoft.Xna.Framework;
#endregion

namespace Nine.Tools.SampleBrowser
{
    static class SampleScaner
    {
        public static IEnumerable<SampleInfo> Scan(string path)
        {
            return from file in Directory.GetFiles(path, "*.exe", SearchOption.AllDirectories)
                   where !(file.Contains(@"\obj\") || file.Contains(@"\Xbox") || file.Contains(@"\Debug\"))
                   orderby file select new SampleInfo()
            {
                Filename = file,
                Name = GetGameName(file),
                Description = GetGameDescription(file),
                Thumbnail = GetGameThumbnail(file),
                Category = GetGameGategory(file),
                TargetPlatform = TargetPlatform.Windows,
            };
        }

        private static Type GetMainGameType(Assembly dll)
        {
            foreach (var type in dll.GetTypes())
                if (typeof(Game).IsAssignableFrom(type))
                    return type;
            return null;
        }

        private static string GetGameName(string assemblyFilename)
        {
            try
            {
                Assembly dll = Assembly.LoadFrom(assemblyFilename);
                object[] attributes = GetMainGameType(dll).GetCustomAttributes(typeof(DisplayNameAttribute), false);
                if (attributes.Length > 0)
                    return ((DisplayNameAttribute)attributes[0]).DisplayName;
            }
            catch (Exception) { }
            return Path.GetFileNameWithoutExtension(assemblyFilename);
        }

        private static string GetGameDescription(string assemblyFilename)
        {
            try
            {
                Assembly dll = Assembly.LoadFrom(assemblyFilename);
                object[] attributes = GetMainGameType(dll).GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attributes.Length > 0)
                    return ((DescriptionAttribute)attributes[0]).Description;
            }
            catch (Exception) { }
            return Path.GetFileNameWithoutExtension(assemblyFilename);
        }

        private static string[] GetGameGategory(string assemblyFilename)
        {
            try
            {
                Assembly dll = Assembly.LoadFrom(assemblyFilename);
                object[] attributes = GetMainGameType(dll).GetCustomAttributes(typeof(CategoryAttribute), false);
                if (attributes.Length > 0)
                    return ((CategoryAttribute)attributes[0]).Category.Split(' ', ',');
            }
            catch (Exception) { }
            return new string[0];
        }

        static object thumbnailLock = new object();
        private static string GetGameThumbnail(string assemblyFilename)
        {
            lock (thumbnailLock)
            {
                string thumbnailPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Engine Nine\v1.0\Samples\Thumbnails\");
                string thumbnailFile = Path.Combine(thumbnailPath, Path.GetFileNameWithoutExtension(assemblyFilename) + ".png");

                if (File.Exists(thumbnailFile))
                {
                    return thumbnailFile;
                }

                if (!Directory.Exists(thumbnailPath))
                {
                    Directory.CreateDirectory(thumbnailPath);
                }

                ScreenshotCapturer.ScreenshotCapturer.Capture(new Task()
                {
                    Filename = assemblyFilename,
                    OutputFilename = thumbnailFile,
                    Frame = 0,
                    Width = 64,
                    Height = 64
                });

                if (File.Exists(thumbnailFile))
                    return thumbnailFile;
                
                return "EngineNineIcon.64.png";
            }
        }
    }
}