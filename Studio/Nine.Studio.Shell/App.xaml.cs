namespace Nine.Studio.Shell
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Windows;

    public partial class App : Application
    {
        public Editor Editor { get; private set; }
        public IEditorShell EditorShell { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            using (Editor = new Editor())
            {
                Editor.LoadExtensions();

                InitializeCulture(Editor);

                var splashWindow = GetSplashWindow(e.Args);
                var settings = Editor.FindSettings<GeneralSettings>();

                var mainWindow = (Window)Editor.ToView();

                // Walk-around glass flash problems
                mainWindow.Left = SystemParameters.VirtualScreenWidth;
                mainWindow.Top = SystemParameters.VirtualScreenHeight;

                mainWindow.ContentRendered += (sender, args) =>
                {
                    mainWindow.WindowState = settings.WindowMaximized ? WindowState.Maximized : WindowState.Normal;
                    mainWindow.Width = Math.Min(settings.WindowWidth, SystemParameters.VirtualScreenWidth);
                    mainWindow.Height = Math.Min(settings.WindowHeight, SystemParameters.VirtualScreenHeight);
                    mainWindow.Left = (SystemParameters.VirtualScreenWidth - mainWindow.Width) / 2;
                    mainWindow.Top = (SystemParameters.VirtualScreenHeight - mainWindow.Height) / 2;

                    if (splashWindow != IntPtr.Zero)
                        NativeMethods.ShowWindow(splashWindow, SW.HIDE);
                };

                mainWindow.Closed += (sender, args) =>
                {
                    if (!(settings.WindowMaximized = mainWindow.WindowState == WindowState.Maximized))
                    {
                        settings.WindowWidth = mainWindow.Width;
                        settings.WindowHeight = mainWindow.Height;
                    }
                };

                EditorShell = (IEditorShell)mainWindow;
                EditorShell.ShowDialogAsync(Editor.ToView("WelcomeView"));

                mainWindow.Show();
            }
        }
        
        private static void InitializeCulture(Editor editor)
        {
            var culture = new CultureInfo(editor.FindSettings<GeneralSettings>().Language);

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        private static IntPtr GetSplashWindow(string[] args)
        {
            int result;
            var windowHandle = args.LastOrDefault();
            return windowHandle != null && int.TryParse(windowHandle, out result) ? new IntPtr(result) : IntPtr.Zero;
        }

        public static int Run(string commandLine)
        {
            return AppDomain.CreateDomain("Nine").ExecuteAssemblyByName(
                   Assembly.GetExecutingAssembly().GetName(), ParseArguments(commandLine));
        }

        private static string[] ParseArguments(string commandLine)
        {
            char[] parmChars = commandLine.ToCharArray();
            bool inQuote = false;
            for (int index = 0; index < parmChars.Length; index++)
            {
                if (parmChars[index] == '"')
                    inQuote = !inQuote;
                if (!inQuote && parmChars[index] == ' ')
                    parmChars[index] = '\n';
            }
            return (new string(parmChars)).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
