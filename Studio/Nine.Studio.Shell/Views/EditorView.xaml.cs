namespace Nine.Studio.Shell
{
    using System;
    using System.Collections.Generic;
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

        #region SideBar
        private Stack<object> sideBarStack = new Stack<object>();

        public Task ShowDialogAsync(object content)
        {
            sideBarStack.Push(content.ToView());
            DialogContent.Content = sideBarStack.Peek();
            return null;
        }
        #endregion
    }
}
