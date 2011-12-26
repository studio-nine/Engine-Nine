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

namespace Nine.Tools.PathGraphBuilder
{
    [Serializable()]
    public class BuildPathGraphTask
    {
        public Exception Error;
        public string Filename;
        public string ContentDirectory;
        public string OutputFilename = "PathGraph.xml";
        public float Step = 1;
        public float ActorHeight = 1;
        public float Slope = 60;
    }

    static class ParameterParser
    {
        public static BuildPathGraphTask ParseCommandLineParameters(string[] args)
        {
            BuildPathGraphTask task = new BuildPathGraphTask();

            for (int i = 0; i < args.Length; i++)
            {
                string token = args[i];

                switch (token)
                {
                    case "-out":
                        task.OutputFilename = Path.GetFullPath(args[++i]);
                        break;
                    case "-dir":
                        task.ContentDirectory = Path.GetFullPath(args[++i]);
                        break;
                    case "-step":
                        try
                        {
                            task.Step = Convert.ToSingle(args[++i]);
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine("Invalid step.");
                            return null;
                        }
                        break;
                    case "-actorHeight":
                        try
                        {
                            task.ActorHeight = Convert.ToSingle(args[++i]);
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine("Invalid actorHeight.");
                            return null;
                        }
                        break;
                    case "-slope":
                        try
                        {
                            task.Slope = Convert.ToSingle(args[++i]);
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine("Invalid slope.");
                            return null;
                        }
                        break;
                    case "-nologo":
                        break;
                    default:
                        if (token.StartsWith("-"))
                        {
                            Console.Error.WriteLine("Unknown switch: " + token);
                            return null;
                        }
                        else
                        {
                            task.Filename = Path.GetFullPath(token);
                            if (!File.Exists(task.Filename))
                            {
                                Console.Error.WriteLine("Cannot find world or scene: " + task.Filename);
                                return null;
                            }
                        }
                        break;
                }
            }

            if (string.IsNullOrEmpty(task.ContentDirectory))
                task.ContentDirectory = Directory.GetCurrentDirectory();
            return task;
        }
    }
}
