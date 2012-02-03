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
using System.Threading;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;
using Nine.Graphics;
using Nine.Graphics.ObjectModel;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using System.Xml;
using System.Drawing;
#endregion

namespace Nine.Tools.PathGraphBuilder
{
    public static class PathGraphBuilder
    {
        static void Main(string[] args)
        {
            if (args.Length <= 0)
            {
                PrintHelp();
                return;
            }
            
            if (!args.Contains("-nologo"))
                PrintLogo();

            BuildPathGraphTask task = null;
            try
            {
                task = ParameterParser.ParseCommandLineParameters(args);
            }
            catch (Exception)
            {
                Console.WriteLine("Invalid command line argment.");
            }

            if (task == null)
                return;

            PrintTask(task);

            Console.WriteLine("Building Path Graph...");
            Console.Out.Flush();

            Build(task);
        }

        private static void PrintLogo()
        {
            Console.WriteLine("Copyright 2008 - 2011 (c) Engine Nine. All Rights Reserved.");
            Console.WriteLine();
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Generates a path graph for a world or scene.");
            Console.WriteLine("");
            Console.WriteLine("PathGraphBuilder [Filename] [-step [Step]] [-actorHeight [actorHeight]]");
            Console.WriteLine("                 [-slope [slope]] [-dir [dir]] [-out [OutputFilename]]");
            Console.WriteLine();
            Console.WriteLine("  Filename  Specifies the .xnb file that contains the world or scene.");
            Console.WriteLine();
            Console.WriteLine("  -step          Specifies the sampler step.");
            Console.WriteLine("  -actorHeight   Specifies the max height of actors");
            Console.WriteLine("  -slope         Specifies the max slope that can be passed.");
            Console.WriteLine("  -dir           Specifies the content directory.");
            Console.WriteLine("  -out           Specifies the output path graph file.");
            Console.WriteLine();
            Console.WriteLine("Example Usage:");
            Console.WriteLine("");
            Console.WriteLine("  PathGraphBuilder myWorld.xnb -step 1 -maxActorHeight 10 -out PathGraph.xml");
            Console.WriteLine("");
        }

        private static void PrintTask(BuildPathGraphTask task)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("  Filename:  ");
            Console.WriteLine(task.Filename);
            Console.Write("  Step:      ");
            Console.WriteLine(task.Step);
            Console.Write("  Slope:     ");
            Console.WriteLine(task.Slope);
            Console.Write("  ActorHeight:      ");
            Console.WriteLine(task.ActorHeight);
            Console.Write("  ContentDirectory: ");
            Console.WriteLine(task.ContentDirectory);
            Console.Write("  Output:    ");
            Console.WriteLine(task.OutputFilename);
            Console.WriteLine();
            Console.ResetColor();
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.Synchronized)]
        public static bool Build(BuildPathGraphTask task)
        {
            try
            {
                using (var graphicsDevice = GraphicsExtensions.CreateHiddenGraphicsDevice(GraphicsProfile.Reach))
                {
                    using (var contentManager = new PipelineContentManager(task.ContentDirectory, new GraphicsDeviceServiceProvider(graphicsDevice)))
                    {
                        try
                        {
                            // Sometimes the content manager will fail to load the content when it cannot find
                            // the assembly. Listen to the AssemblyResolve event will solve this.
                            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;

                            Scene scene = null;
                            var runtimeObject = contentManager.Load<object>(task.Filename);
                            if (runtimeObject is Scene)
                            {
                                scene = runtimeObject as Scene;
                            }
                            else if (runtimeObject is World)
                            {
                                var world = runtimeObject as World;
                                world.CreateContent(contentManager);
                                scene = world.CreateGraphics(graphicsDevice);
                            }

                            if (scene == null)
                            {
                                throw new ContentLoadException("Target xnb file is not a valid World or Scene");
                            }

                            var pathGrid = Builder.BuildPathGrid(scene, task.Step, task.Slope, task.ActorHeight);

                            var settings = new XmlWriterSettings();
                            settings.Indent = true;
                            settings.NewLineChars = Environment.NewLine;
                            settings.NewLineHandling = NewLineHandling.Replace;

                            using (var xmlWriter = XmlWriter.Create(task.OutputFilename, settings))
                            {
                                IntermediateSerializer.Serialize(xmlWriter, pathGrid, null);
                                Console.WriteLine("Path graph saved at " + task.OutputFilename);
                            }
                        }
                        finally
                        {
                            AppDomain.CurrentDomain.AssemblyResolve -= ResolveAssembly;
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                task.Error = e;
                Console.Error.WriteLine(task.Error.ToString());
                return false;
            }
        }

        private static Assembly ResolveAssembly(object sender, ResolveEventArgs e)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName == e.Name)
                    return assembly;
            }
            return null;
        }

        static void TestExtractRectangle()
        {  
            int i = 0;
            int numPathBricks = 0;
            //Bitmap bitmap = new Bitmap("C:\\Untitled.png");
            //Bitmap bitmap = new Bitmap("C:\\Collision.png");
            Bitmap bitmap = new Bitmap("C:\\Collision1.png");

            bool[] collisionMap = new bool[bitmap.Width * bitmap.Height];
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    collisionMap[i] = (bitmap.GetPixel(x, y).R > 128);
                    if (!collisionMap[i])
                        numPathBricks++;
                    i++;
                }
            }
            var rectangles = Builder.ExtractRectanglesFromCollisionMap(collisionMap, bitmap.Width, bitmap.Height);

            int rectangleArea = rectangles.Sum(rect => rect.Width * rect.Height);
            Debug.Assert(rectangleArea == numPathBricks);
            Console.WriteLine(rectangles.Count);

            Random random = new Random();
            Bitmap regions = new Bitmap(bitmap.Width, bitmap.Height);
            for (int y = 0; y < bitmap.Height; y++)
                for (int x = 0; x < bitmap.Width; x++)
                    regions.SetPixel(x, y, System.Drawing.Color.White);
            foreach (var rect in rectangles)
            {
                var color = System.Drawing.Color.FromArgb(random.Next(255), random.Next(255), random.Next(255));
                for (int y = 0; y < rect.Height; y++)
                    for (int x = 0; x < rect.Width; x++)
                        regions.SetPixel(rect.X + x, rect.Y + y, color);
            }
            regions.Save("C:\\Regions.bmp", System.Drawing.Imaging.ImageFormat.Bmp);
        }
    }
}
