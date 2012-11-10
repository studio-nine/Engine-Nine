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
                var mainWindow = new MainWindow(editor, GetSplashWindow(args));
                app.Run(app.MainWindow = mainWindow);
                
                var settings = editor.FindSettings<Settings>();
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
            var culture = new CultureInfo(editor.FindSettings<Settings>().Language);

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            Nine.Studio.Strings.Culture = culture;
            Nine.Studio.Shell.Strings.Culture = culture;
        }

        public static FrameworkElement ToView(this object viewModel)
        {
            var viewLocator = string.Concat("/Nine.Studio.Shell;component/Views/" + viewModel.GetType().Name, "View.xaml");
            var frameworkElement = (FrameworkElement)Application.LoadComponent(new Uri(viewLocator, UriKind.Relative));
            frameworkElement.DataContext = viewModel;
            return frameworkElement;
        }
    }
}
