using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using System.Threading;
using System.Globalization;
using Nine.Studio.Shell.ViewModels;
using Nine.Studio.Shell.Windows;

namespace Nine.Studio.Shell
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            InitializeCultures();

            Editor editor = Editor.Launch();
            editor.Extensions.LoadDefault();

            EditorView editorView = new EditorView(editor);

            MainWindow mainWindow = new MainWindow();
            mainWindow.DataContext = editorView;
            mainWindow.Show();

            if (e.Args.Length > 0)
                OpenProject(editorView, e.Args[0]);
            else
                NewProject(editorView);
            
            base.OnStartup(e);
        }

        private void InitializeCultures()
        {
            var culture = Global.GetProperty("Culture");
            if (culture != null)
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            Trace.Flush();
        }

        private void OpenProject(EditorView editorView, string filename)
        {
            editorView.OpenProject(filename);
        }

        private void NewProject(EditorView editorView)
        {
            editorView.CreateProject();
        }
    }
}
