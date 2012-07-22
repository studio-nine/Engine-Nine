namespace Nine.Studio.Shell.Windows
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
        public Editor Editor { get; private set; }
        public EditorView EditorView { get; private set; }

        private Dock dialogDock = Dock.Left;

        public MainWindow()
        {
            Editor = Editor.Launch();
            Editor.Extensions.LoadDefault();
            EditorView = new EditorView(Editor, this);
            DataContext = EditorView;
            
            InitializeComponent();
        }

        public Task<string> ShowDialogAsync(string title, string description, params string[] options)
        {
            return ShowDialogAsync(title, description, null, dialogDock, options);
        }

        public Task<string> ShowDialogAsync(string title, string description, object content, params string[] options)
        {
            return ShowDialogAsync(title, description, content, dialogDock, options);
        }

        public Task<string> ShowDialogAsync(string title, string description, object content, Dock dock, params string[] options)
        {
            Dialog dialog = new Dialog();
            DialogContainer.Children.Add(dialog);
            return dialog.Show(title, description, content, dock, options);
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
            e.Cancel = await EditorView.Closing();
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