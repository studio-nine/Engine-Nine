#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Nine.Studio.Shell.ViewModels;
#endregion

namespace Nine.Studio.Shell.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LegacyMainWindow : Window
    {
        /*
        public ICommand OpenFile { get; private set; }
        public Editor Editor { get; private set; }
        public EditorView EditorView { get; private set; }
        
        public LegacyMainWindow()
        {
            InitializeComponent();
        }

        private void EditorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            EditorView = (EditorView)DataContext;
            Editor = EditorView.Editor;
            OpenFile = new DelegateCommand<string>(filename => {if (EnsureProjectSaved()) EditorView.OpenProject(filename);});
        }

        private void EditorWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !EnsureProjectSaved();
        }

        private void EditorWindow_Closed(object sender, EventArgs e)
        {
            EditorView.Exit();
        }

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
                MessageBoxResult result = MessageBox.Show(string.Format(Strings.SaveChanges, EditorView.Name), Editor.Title, 
                                          MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);

                if (result == MessageBoxResult.Yes)
                {
                    return SaveProject();
                }
                if (result == MessageBoxResult.Cancel)
                {
                    return false;
                }
            }
            return true;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        private void Options_Click(object sender, RoutedEventArgs e)
        {
            new OptionsWindow() { Owner = this, DataContext = EditorView }.ShowDialog();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(Editor.Title + "  " + string.Format(Strings.VersionFormat, Editor.Version),
                                                               Editor.Title, MessageBoxButton.OK,
                                                               MessageBoxImage.None, MessageBoxResult.OK);
        }
         */
    }
}
