namespace Nine.Studio.Shell
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public static class EditorView
    {
        public static void Exit()
        {
            Application.Current.MainWindow.Close();
        }

        public static void Maximize()
        {
            Application.Current.MainWindow.WindowState =
            Application.Current.MainWindow.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        public static void Minimize()
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }
    }
}