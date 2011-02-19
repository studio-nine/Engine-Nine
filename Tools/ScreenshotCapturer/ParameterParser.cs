#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Tools.ScreenshotCapturer
{
    [Serializable()]
    public class ScreenCaptureTask
    {
        public Exception Error;
        public string Filename;
        public string OutputFilename = "Captured.png";
        public string ClassName;
        public string WorkingDirectory;
        public int Frame = 0;
        public int Width = 512;
        public int Height = 512;
        public Dictionary<string, string> Parameters = new Dictionary<string, string>();
    }

    static class ParameterParser
    {
        public static ScreenCaptureTask ParseCommandLineParameters(string[] args)
        {
            ScreenCaptureTask task = new ScreenCaptureTask();

            for (int i = 0; i < args.Length; i++)
            {
                string token = args[i];

                switch (token)
                {
                    case "-out":
                        task.OutputFilename = Path.GetFullPath(args[++i]);
                        break;
                    case "-class":
                        task.ClassName = args[++i];
                        break;
                    case "-dir":
                        task.WorkingDirectory = args[++i];
                        break;
                    case "-frame":
                        try
                        {
                            task.Frame = Convert.ToInt32(args[++i]);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Invalid frame.");
                            return null;
                        }
                        break;
                    case "-size":
                        try
                        {
                            string[] size = args[++i].Split('x');
                            task.Width = Convert.ToInt32(size[0]);
                            task.Height = Convert.ToInt32(size[1]);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Invalid size format. -size must be of format 800x600");
                            return null;
                        }
                        break;
                    case "-params":
                        try
                        {
                            string[] parameters = args[++i].Split('&');
                            foreach (var pair in parameters)
                            {
                                string[] keyValue = pair.Split('=');
                                task.Parameters.Add(keyValue[0], keyValue[1]);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Invalid paramter format.");
                            return null;
                        }
                        break;
                    default:
                        if (token.StartsWith("-"))
                        {
                            Console.WriteLine("Unknown switch: " + token);
                            return null;
                        }
                        else
                        {
                            task.Filename = Path.GetFullPath(token);
                            if (!File.Exists(task.Filename))
                            {
                                Console.WriteLine("Cannot find game: " + task.Filename);
                                return null;
                            }
                        }
                        break;
                }
            }

            task.Frame = Math.Max(0, task.Frame);
            if (task.Width <= 0)
                task.Width = 258;
            if (task.Height <= 0)
                task.Height = 258;

            return task;
        }
    }
}
