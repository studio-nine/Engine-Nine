namespace Nine.Studio.Shell
{
    using System;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;
    using Nine.Studio.Shell.ViewModels;

    public partial class MainWindow : Window, IEditorShell
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(Editor editor, IntPtr splashWindow)
        {
            this.ContentRendered += (sender, e) =>
            {
                var settings = editor.FindSettings<GeneralSettings>();
                this.WindowState = settings.WindowMaximized ? WindowState.Maximized : WindowState.Normal;
                this.Width = Math.Min(settings.WindowWidth, SystemParameters.VirtualScreenWidth);
                this.Height = Math.Min(settings.WindowHeight, SystemParameters.VirtualScreenHeight);
                this.Left = (SystemParameters.VirtualScreenWidth - Width) / 2;
                this.Top = (SystemParameters.VirtualScreenHeight - Height) / 2;

                if (splashWindow != IntPtr.Zero)
                    NativeMethods.ShowWindow(splashWindow, SW.HIDE);
            };

            InitializeComponent();

            this.DataContext = editor;
            this.Left = SystemParameters.VirtualScreenWidth;
            this.Top = SystemParameters.VirtualScreenHeight;
            this.Content = editor.ToView();
        }

        public Task<string> ShowDialogAsync(string title, string description, params string[] options)
        {
            return null;
        }

        public Task<string> ShowDialogAsync(string title, string description, object content, params string[] options)
        {
            return null;
        }

        public Task<string> ShowDialogAsync(string title, string description, object content, Dock dock, params string[] options)
        {
            return null;
        }

        public Task QueueWorkItem(string title, string description, Task task)
        {
            throw new NotImplementedException();
        }

        public object Invoke(Delegate method, params object[] args)
        {
            return Dispatcher.Invoke(method, args);
        }

        protected async override void OnClosing(CancelEventArgs e)
        {
            //e.Cancel = await EditorView.Closing();
            base.OnClosing(e);
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonMaximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = (WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized);
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
    }
}
