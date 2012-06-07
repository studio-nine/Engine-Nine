#region Copyright 2009 - 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using Nine.Studio.Shell.ViewModels;

#endregion

namespace Nine.Studio.Shell.Windows
{
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

        #region Methods
        public Task<string> ShowDialogTaskAsync(string title, string description, params string[] options)
        {
            return ShowDialogTaskAsync(title, description, null, dialogDock, options);
        }

        public Task<string> ShowDialogTaskAsync(string title, string description, object content, params string[] options)
        {
            return ShowDialogTaskAsync(title, description, content, dialogDock, options);
        }

        public Task<string> ShowDialogTaskAsync(string title, string description, object content, Dock dock, params string[] options)
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

        /*
        private void AlwayExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void HasProject(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (EditorView.Project != null);
        }

        private void ExecuteNew(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (EnsureProjectSaved())
                {
                    EditorView.PerformNewProject();
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(Strings.ErrorCreateProject);
                Trace.WriteLine(ex.ToString());
                MessageBox.Show(Strings.ErrorCreateProject, Editor.Title, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ExecuteOpen(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (EnsureProjectSaved())
                {
                    OpenFileDialog open = new OpenFileDialog();
                    open.Title = Editor.Title;                    
                    open.Filter = string.Format(@"{0}|*.ix|{1}|*.*", Strings.NineProject, Strings.AllFiles);
                    bool? result = open.ShowDialog();
                    if (result.HasValue && result.Value)
                    {
                        EditorView.OpenProject(open.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(Strings.ErrorOpenProject);
                Trace.WriteLine(ex.ToString());
                MessageBox.Show(Strings.ErrorOpenProject, Editor.Title, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ExecuteClose(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (EnsureProjectSaved())
                    EditorView.CloseProject();
            }
            catch (Exception ex)
            {
                Trace.TraceError(Strings.ErrorCloseProject);
                Trace.WriteLine(ex.ToString());
                MessageBox.Show(Strings.ErrorCloseProject, Editor.Title, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ExecuteSave(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                SaveProject();
            }
            catch (Exception ex)
            {
                Trace.TraceError(Strings.ErrorSaveProject);
                Trace.WriteLine(ex.ToString());
                MessageBox.Show(Strings.ErrorSaveProject, Editor.Title, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ExecuteSaveAs(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                SaveProjectAs();
            }
            catch (Exception ex)
            {
                Trace.TraceError(Strings.ErrorSaveProject);
                Trace.WriteLine(ex.ToString());
                MessageBox.Show(Strings.ErrorSaveProject, Editor.Title, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool SaveProject()
        {
            if (EditorView.Project == null)
                return true;

            if (!string.IsNullOrEmpty(EditorView.Project.Filename))
            {
                EditorView.SaveProject();
                return true;
            }
            return SaveProjectAs();
        }

        private bool SaveProjectAs()
        {
            if (EditorView.Project == null)
                return true;

            SaveFileDialog save = new SaveFileDialog();
            save.Title = Editor.Title;
            save.FileName = EditorView.Name;
            save.Filter = string.Format(@"{0}|*.ix", Strings.NineProject);
            bool? openResult = save.ShowDialog();
            if (openResult.HasValue && openResult.Value)
            {
                EditorView.SaveProject();
                return true;
            }
            return false;
        }

        private bool EnsureProjectSaved()
        {
            if (EditorView.Project != null && EditorView.Project.IsModified)
            {
                string description = string.Format(Strings.SaveChangesDescription, EditorView.Name);
                ShowDialogTaskAsync(Strings.SaveChanges, description, Dock.Right, Strings.Yes, Strings.No).
                ContinueWith(dialogResult =>
                {
                    Global.EnsureThreadCulture();

                    if (dialogResult.Result == Strings.Yes)
                    {
                        Dispatcher.Invoke((Action)delegate { SaveProject(); });
                    }
                });
                return false;
            }
            return true;
        }

        private bool EnsureProjectSavedThenExit()
        {
            if (EditorView.Project != null && EditorView.Project.IsModified)
            {
                string description = string.Format(Strings.SaveChangesDescription, EditorView.Name);
                ShowDialogTaskAsync(Strings.SaveChanges, description, Dock.Right, Strings.Yes, Strings.No, Strings.Cancel).
                ContinueWith(dialogResult =>
                {
                    Global.EnsureThreadCulture();

                    if (dialogResult.Result == Strings.Yes)
                    {
                        Dispatcher.Invoke((Action)delegate
                        {
                            if (SaveProject())
                                App.Current.Shutdown();
                        });
                    }
                    else if (dialogResult.Result == Strings.No)
                    {
                        Dispatcher.Invoke((Action)delegate { App.Current.Shutdown(); });
                    }
                });
                return false;
            }
            return true;
        }
        #endregion

        #region Windows Events
        protected override void OnContentRendered(EventArgs e)
        {   
            // Equvilent to shown event. Triggered only once.
            base.OnContentRendered(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = !EnsureProjectSavedThenExit();
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            EditorView.Exit();
            base.OnClosed(e);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            dialogDock = e.GetPosition(this).X > ActualWidth * 0.5 ? Dock.Right : Dock.Left;
            base.OnPreviewMouseDown(e);
        }
        */

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
        #endregion
    }
}