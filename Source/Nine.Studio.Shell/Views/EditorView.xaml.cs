namespace Nine.Studio.Shell
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;

    public partial class EditorView : Window, IEditorShell
    {
        public EditorView()
        {
            InitializeComponent();

            Library.Click += (s, e) =>
            {
                ShowDialogAsync("TITLE", "HELLO WORLD", new NoDesignView(), "OPTION");
            };
        }

        #region Windows Buttons
        private void Exit(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        private void Maximize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }
        
        private void Minimize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        #endregion
        
        #region Dialog
        public static readonly DependencyProperty DialogAwaitableProperty = DependencyProperty.RegisterAttached(
            "DialogAwaitable",
            typeof(DialogResult),
            typeof(DialogView));

        public static DialogResult GetDialogAwaitable(DependencyObject obj)
        {
            return (DialogResult)obj.GetValue(DialogAwaitableProperty);
        }

        public static void SetDialogAwaitable(DependencyObject obj, DialogResult value)
        {
            obj.SetValue(DialogAwaitableProperty, value);
        }

        public FrameworkElement CurrentDialog
        {
            get { return DialogStack.Count > 0 ? (FrameworkElement)DialogStack.Peek() : null; }
        }

        public DialogResult ShowDialogAsync(object content)
        {
            return BeginDialog((FrameworkElement)content.ToView());
        }

        public DialogResult ShowDialogAsync(string title, string text, object content, params string[] options)
        {
            return BeginDialog(new DialogView(title, text, content.ToView(), options));
        }

        private DialogResult BeginDialog(FrameworkElement dialog)
        {
            var result = new DialogResult();
            result.Dialog = dialog;
            result.Completed += PopDialog;
            SetDialogAwaitable(dialog, result);
            DialogStack.Push(dialog);

            UpdateDialogView();
            return result;
        }

        public void EndDialog(FrameworkElement dialog, string dialogResult)
        {
            if (dialog != DialogStack.Peek())
                throw new InvalidOperationException();

            GetDialogAwaitable(dialog).NotifyCompletion(dialogResult);
        }

        private void PopDialog()
        {
            if (DialogStack.Count > 0)
                DialogStack.Pop();

            UpdateDialogView();
        }

        private void PopDialog(object sender, RoutedEventArgs e)
        {
            if (DialogStack.Count > 0)
                EndDialog(CurrentDialog, null);
        }

        private void UpdateDialogView()
        {
            var showBackButton = (App.Editor.ActiveProject != null && DialogStack.Count > 0) ||
                                 (App.Editor.ActiveProject == null && DialogStack.Count > 1);

            DialogContent.Content = CurrentDialog;
            DialogBack.Visibility = showBackButton ? Visibility.Visible : Visibility.Collapsed;
        }
        private Stack<object> DialogStack = new Stack<object>();
        #endregion

        #region StatusBar
        public Task RunAsync(Action action)
        {
            ProgressBar.Visibility = Visibility.Visible;
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke((Action<Exception>)ShowErrorMessage, ex);
                }
                finally
                {
                    Dispatcher.Invoke((Action)(() => { ProgressBar.Visibility = Visibility.Collapsed; }));
                }
            });
        }

        private async void ShowErrorMessage(Exception ex)
        {
            await ShowDialogAsync(null, null, new ExceptionView() { DataContext = ex }, Strings.Ok);

            if (App.Editor.ActiveProject == null && DialogStack.Count <= 0)
            {
                await ShowDialogAsync(new FilesView { DataContext = App.Editor });
            }
        }
        #endregion

        #region Save
        protected async override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = !await App.EnsureSavedAsync();
            base.OnClosing(e);
        }
        /*
        public async Task<bool> Closing()
        {
            if (await App.EnsureSavedAsync())
            {
                shouldClose = true;
                Application.Current.Shutdown();
            }
            return !shouldClose;
        }
        private bool shouldClose = false;
         */
        #endregion
    }
}
