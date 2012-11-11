namespace Nine.Studio.Shell
{
    using System;
    using System.Linq;
    using System.Globalization;
    using System.Reflection;
    using System.Threading;
    using System.Windows;

    public static class BootStrapper
    {
        [STAThread]
        public static void Main(string[] args)
        {         
            using (var editor = new Editor())
            {
                editor.LoadExtensions();

                InitializeCulture(editor);

                var app = (Application)Application.LoadComponent(new Uri("/Nine.Studio.Shell;component/App.xaml", UriKind.Relative));
                var mainWindow = (Window)editor.ToView();
                var splashWindow = GetSplashWindow(args);
                var settings = editor.FindSettings<GeneralSettings>();

                // Walk-around glass flash problems
                mainWindow.Left = SystemParameters.VirtualScreenWidth;
                mainWindow.Top = SystemParameters.VirtualScreenHeight;

                mainWindow.ContentRendered += (sender, e) =>
                {
                    mainWindow.WindowState = settings.WindowMaximized ? WindowState.Maximized : WindowState.Normal;
                    mainWindow.Width = Math.Min(settings.WindowWidth, SystemParameters.VirtualScreenWidth);
                    mainWindow.Height = Math.Min(settings.WindowHeight, SystemParameters.VirtualScreenHeight);
                    mainWindow.Left = (SystemParameters.VirtualScreenWidth - mainWindow.Width) / 2;
                    mainWindow.Top = (SystemParameters.VirtualScreenHeight - mainWindow.Height) / 2;

                    if (splashWindow != IntPtr.Zero)
                        NativeMethods.ShowWindow(splashWindow, SW.HIDE);
                };

                app.Run(app.MainWindow = mainWindow);
                
                settings.WindowWidth = mainWindow.Width;
                settings.WindowHeight = mainWindow.Height;
                settings.WindowMaximized = mainWindow.WindowState == WindowState.Maximized;
            }
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
        
        private static void InitializeCulture(Editor editor)
        {
            var culture = new CultureInfo(editor.FindSettings<GeneralSettings>().Language);

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            Nine.Studio.Strings.Culture = culture;
            Nine.Studio.Shell.Strings.Culture = culture;
        }
    }
}
