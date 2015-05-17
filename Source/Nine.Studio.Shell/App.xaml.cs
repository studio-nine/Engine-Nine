namespace Nine.Studio.Shell
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;

    public partial class App : Application
    {
        public static Editor Editor { get; private set; }
        public static IEditorShell Shell { get; private set; }

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

                mainWindow.SourceInitialized += (sender, args) =>
                {
                    if (settings.WindowPlacement != null)
                    {
                        // TODO: 
                    }
                    
                    mainWindow.WindowState = settings.WindowMaximized ? WindowState.Maximized : WindowState.Normal;

                    var primaryScreenBounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
                    mainWindow.Width = Math.Min(settings.WindowWidth, primaryScreenBounds.Width);
                    mainWindow.Height = Math.Min(settings.WindowHeight, primaryScreenBounds.Height);
                    mainWindow.Left = (primaryScreenBounds.Width - mainWindow.Width) / 2;
                    mainWindow.Top = (primaryScreenBounds.Height - mainWindow.Height) / 2;

                    if (splashWindow != IntPtr.Zero)
                        ; // NativeMethods.ShowWindow(splashWindow, SW.HIDE);
                };

                mainWindow.Closing += (sender, args) =>
                {
                    settings.WindowPlacement = GetWindowPlacement(mainWindow);
                };

                Shell = (IEditorShell)mainWindow;
                Shell.ShowDialogAsync(new FilesView { DataContext = App.Editor });

                mainWindow.Show();
            }
        }

        private static bool SetWindowPlacement(Window mainWindow, WINDOWPLACEMENT placement)
        {
            try
            {
                return NativeMethods.SetWindowPlacement(
                    new System.Windows.Interop.WindowInteropHelper(mainWindow).Handle, ref placement);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                return false;
            }
        }

        private static WINDOWPLACEMENT? GetWindowPlacement(Window mainWindow)
        {
            try
            {
                return NativeMethods.GetWindowPlacement(
                    new System.Windows.Interop.WindowInteropHelper(mainWindow).Handle);
            }
            catch (System.ComponentModel.Win32Exception) 
            {
                return null; 
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
        
        public static async Task<bool> EnsureSavedAsync()
        {
            var project = App.Editor.ActiveProject;
            if (project == null || !project.IsModified)
                return true;

            var description = string.Format(Strings.SaveChangesDescription, project.Name);
            var saveResule = await App.Shell.ShowDialogAsync(Strings.SaveChanges, description, null, Strings.Yes, Strings.No);
            if (saveResule == Strings.Yes)
            {
                await SaveAsync();
                return true;
            }
            return saveResule == Strings.No;
        }

        public static Task SaveAsync()
        {
            return App.Shell.RunAsync(() =>
            {
                App.Editor.ActiveProject.Save();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            });
        }
    }
}
