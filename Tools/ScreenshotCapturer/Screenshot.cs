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
#endregion

namespace Nine.Tools.ScreenshotCapturer
{
    public static class Screenshot
    {
        static void Main(string[] args)
        {
            if (args.Length <= 0)
            {
                PrintHelp();
                return;
            }

            PrintLogo();

            ScreenCaptureTask task = null;
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

            Capture(task);
        }

        private static void PrintLogo()
        {
            Console.WriteLine("Copyright 2008 - 2011 (c) Engine Nine. All Rights Reserved.");
            Console.WriteLine();
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Captures a screenshot of an Xna game.");
            Console.WriteLine("");
            Console.WriteLine("ScreenshotCapturer [Filename] [-target [GameClassName]] [-frame [FrameNumber]]");
            Console.WriteLine("                   [-size [Size]] [-params [Params]] [-args [Arguments]] ");
            Console.WriteLine("                   [-out [OutputFilename]]");
            Console.WriteLine();
            Console.WriteLine("  Filename  Specifies the source of the game executable or library.");
            Console.WriteLine();
            Console.WriteLine("  -class    Specifies class name of the Game, or use the first Game class found");
            Console.WriteLine("            if this parameter is not specified.");
            Console.WriteLine("  -frame    Specifies the number of frames skipped before capture.");
            Console.WriteLine("  -size     Specifies the width and height of the output screenshot.");
            Console.WriteLine("  -params   Specifies the paramters that will be set to the game instance.");
            Console.WriteLine("  -dir      Specifies the working directory of the game executable.");
            Console.WriteLine("  -out      Specifies the output screenshot filename.");
            Console.WriteLine();
            Console.WriteLine("Example Usage:");
            Console.WriteLine("");
            Console.WriteLine("  ScreenshotCapturer MyGame.exe -class MyGameNamespace.MyGame -frame 10");
            Console.WriteLine("                     -params IsFixedTimeStep=true&IsMouseVisible=false");
            Console.WriteLine("                     -dir \"D:\\\" -size 800x600 -out Captured.png");
            Console.WriteLine("");
        }

        private static void PrintTask(ScreenCaptureTask task)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("  Filename:   ");
            Console.WriteLine(task.Filename);
            Console.Write("  ClassName:  ");
            Console.WriteLine(task.ClassName);
            Console.Write("  Frame:      ");
            Console.WriteLine(task.Frame);
            Console.Write("  Width:      ");
            Console.WriteLine(task.Width);
            Console.Write("  Height:     ");
            Console.WriteLine(task.Height);
            Console.Write("  Output:     ");
            Console.WriteLine(task.OutputFilename);
            Console.Write("  Directory:  ");
            Console.WriteLine(task.WorkingDirectory);
            Console.Write("  Parameters: ");
            Console.WriteLine();
            foreach (var pair in task.Parameters)
            {
                Console.Write("              ");
                Console.Write(pair.Key);
                Console.Write("\t");
                Console.Write(pair.Value);
                Console.WriteLine();
            }
            Console.WriteLine();
            Console.ResetColor();
        }

        static string GameExeDirectory = null;
        static Screenshot()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (o, e) =>
            {
                string name = e.Name.Contains(",") ? e.Name.Substring(0, e.Name.IndexOf(",")) : e.Name;
                return Assembly.LoadFile(Path.Combine(GameExeDirectory, name + ".dll"));
            };
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.Synchronized)]
        public static bool Capture(ScreenCaptureTask task)
        {
            // Use asyc call to avoid create the game window on the main thread.
            // This will avoid the exception when there is already a form shown in the main thread.
            Func<ScreenCaptureTask, bool> action = InternalCapture;
            IAsyncResult result = action.BeginInvoke(task, null, null);
            return action.EndInvoke(result);
        }
        
        private static bool InternalCapture(ScreenCaptureTask task)
        {
            string CurrentDirectory = Environment.CurrentDirectory;
            try
            {
                GameExeDirectory = task.Filename.Replace("/", "\\");
                GameExeDirectory = GameExeDirectory.Substring(0, GameExeDirectory.LastIndexOf('\\'));
                if (string.IsNullOrEmpty(task.WorkingDirectory))
                    task.WorkingDirectory = GameExeDirectory;

                Environment.CurrentDirectory = task.WorkingDirectory;

                return RunTask(task);
            }
            catch (Exception e)
            {
                task.Error = e;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error taken screenshot: " + e.Message);
                Console.ResetColor();
                return false;
            }
            finally
            {
                Environment.CurrentDirectory = CurrentDirectory;
            }
        }
        
        private static bool RunTask(ScreenCaptureTask task)
        {
            Assembly gameDll = null;
            Type gameType = null;
            Game game = null;

            gameDll = Assembly.LoadFile(task.Filename);
            foreach (var asm in gameDll.GetReferencedAssemblies())
                Assembly.Load(asm);

            if (!string.IsNullOrEmpty(task.ClassName))
            {
                gameType = Type.GetType(task.ClassName);
            }
            else
            {
                foreach (var type in gameDll.GetTypes())
                    if (typeof(Game).IsAssignableFrom(type))
                        gameType = type;
            }
            if (gameType == null)
                throw new InvalidOperationException("Cannot find main game class: " + task.ClassName);

            try
            {
                SetTitleLocation(Environment.CurrentDirectory);
                game = (Game)Activator.CreateInstance(gameType);
                GraphicsDeviceManagerWrapper wrapper = new GraphicsDeviceManagerWrapper(game, task);                
                game.Run();
            }
            catch (GameExitException)
            {
                return true;
            }
            finally
            {
                game.Dispose();
            }
            throw new InvalidOperationException("Game ends unexpectedly.");
        }

        private static void SetTitleLocation(string location)
        {
            Type type = typeof(TitleContainer).Assembly.GetType("Microsoft.Xna.Framework.TitleLocation");
            type.GetField("_titleLocation", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, "");
        }
    }
}
