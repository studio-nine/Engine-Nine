#region Copyright 2009 - 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Shell;
using Nine.Studio.Shell.Windows.Controls;
using System.Windows.Input;
using System.Threading.Tasks;
using Nine.Studio.Shell.ViewModels.Data;
#endregion

namespace Nine.Studio.Shell.ViewModels
{
    /// <summary>
    /// View model for editor.
    /// </summary>
    public class EditorView : INotifyPropertyChanged
    {
        #region Properties
        public IEditorShell Shell { get; private set; }
        public Editor Editor { get; private set; }
        public Project Project { get { return ActiveProject != null ? ActiveProject.Project : null; } }

        public string Name { get { return Path.GetFileName(Filename); } }
        public string Filename { get { return ActiveProject != null ? ActiveProject.Filename : ""; } }
        public string Title { get { return ActiveProject == null ? Editor.Title : string.Format("{0} - {1}", Name, Editor.Title); } }
        
        public ObservableCollection<FactoryView> Factories { get; private set; }
        public ObservableCollection<SettingsView> Settings { get; private set; }
        public ObservableCollection<ImporterView> Importers { get; private set; }
        public ObservableCollection<ExporterView> Exporters { get; private set; }

        public ReadOnlyObservableCollection<string> RecentProjects { get { return Editor.RecentProjects; } }

        public bool IsViewVisible
        {
            get
            {
                return ActiveProjectItem != null &&
                      (ActiveProjectItem.Visualizers.Count > 1 ||
                      (ActiveProjectItem.ActiveVisualizer != null && ActiveProjectItem.ActiveVisualizer.Properties.Count > 0));
            }
        }

        public ProjectView ActiveProject
        {
            get { return activeProject; }
            set
            {
                if (value != activeProject)
                {
                    if (value != null)
                    {
                        Verify.IsTrue(value.Editor == Editor, "The project is not owned by this editor");
                        Verify.IsTrue(Editor.Projects.Any(proj => proj == value.Project), "The project is not opened by this editor");
                    }

                    activeProject = value;
                    
                    Importers.Clear();
                    Exporters.Clear();
                    Factories.Clear();

                    if (activeProject != null)
                    {
                        Importers.AddRange(Editor.Extensions.Importers.Select(s => new ImporterView(this, s.Value)));
                        Exporters.AddRange(Editor.Extensions.Exporters.Select(s => new ExporterView(this, s.Value)));
                        Factories.AddRange(Editor.Extensions.Factories.Select(f => new FactoryView(this, f.Value, f.Metadata)));
                    }

                    NotifyPropertyChanged("Filename");
                    NotifyPropertyChanged("Title");
                    NotifyPropertyChanged("Name");
                    NotifyPropertyChanged("Project");
                    NotifyPropertyChanged("ActiveProject");
                }
            }
        }
        private ProjectView activeProject;

        /// <summary>
        /// Gets or sets the active document.
        /// </summary>
        public ProjectItemView ActiveProjectItem
        {
            get { return activeProjectItem; }
            set
            {
                if (value != activeProjectItem)
                {
                    if (value != null)
                    {
                        Verify.IsTrue(value.Project == Project, "The project item is not owned by this project");
                        Verify.IsTrue(Project.ProjectItems.Any(item => item.Project == value.Project), "The project item is not opened by this project");
                    }

                    activeProjectItem = value;
                    NotifyPropertyChanged("IsViewVisible");
                    NotifyPropertyChanged("ActiveProjectItem");
                }
            }
        }
        private ProjectItemView activeProjectItem;

        public object SelectedObject
        {
            get { return _SelectedObject; }
            set
            {
                if (value != _SelectedObject)
                {
                    _SelectedObject = value;
                    NotifyPropertyChanged("SelectedObject");
                }
            }
        }
        private object _SelectedObject;
        #endregion

        #region Initialization
        public EditorView(Editor editor, IEditorShell shell)
        {
            Verify.IsNotNull(editor, "editor");
            Verify.IsNotNull(shell, "shell");

            this.Shell = shell;
            this.Editor = editor;
            this.Editor.ProgressChanged += new ProgressChangedEventHandler(Editor_ProgressChanged);
            this.Factories = new ObservableCollection<FactoryView>();
            this.Settings = new ObservableCollection<SettingsView>();
            this.Importers = new ObservableCollection<ImporterView>();
            this.Exporters = new ObservableCollection<ExporterView>();

            NewCommand = new DelegateCommand(PerformNew);
            OpenCommand = new DelegateCommand(PerformOpen);
            SaveCommand = new DelegateCommand(PerformSave, HasProject);
            CloseCommand = new DelegateCommand(PerformClose, HasProject);
            ExitCommand = new DelegateCommand(PerformExit);
            HelpCommand = new DelegateCommand(PerformHelp);

            UpdateSettings();
        }
        #endregion

        #region Commands
        public ICommand NewCommand { get; private set; }
        public ICommand OpenCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }
        public ICommand HelpCommand { get; private set; }

        private bool HasProject()
        {
            return Project != null;
        }

        private void PerformNew()
        {
            var creationParams = new ProjectCreationParameters();
            ShowDialogTaskAsync(Strings.Create, null, creationParams, Strings.Create, Strings.Cancel).
            ContinueWith(dialogResult =>
            {
                if (dialogResult.IsCompleted && dialogResult.Result == Strings.Create)
                {
                    Invoke((Action)delegate()
                    {
                        ActiveProject = new ProjectView(this, Editor.CreateProject(creationParams.ProjectFilename));
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    });
                }
            });
        }

        private void PerformOpen()
        {
            /*
            try
            {
                ProgressHelper.DoWork(
                    (e) =>
                    {
                        return Editor.OpenProject(filename);
                    },
                    (e) =>
                    {
                        if (e != null)
                        {
                            ProjectView = new ProjectView(this, (Project)e);
                        }
                    }, filename, Strings.Loading, filename);
            }
            catch (Exception ex)
            {
                Trace.TraceError(Strings.ErrorOpenProject);
                Trace.WriteLine(ex.ToString());
                MessageBox.Show(Strings.ErrorOpenProject, Editor.Title, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
             */
        }

        private void PerformSave()
        {
            ActiveProject.Save();
            GC.Collect();
        }

        private void PerformClose()
        {
            if (Project != null)
            {
                Project.Close();
                ActiveProject = null;
                GC.Collect();
            }
        }

        private void PerformExit()
        {

        }

        private void PerformHelp()
        {
            OpenUrl("http://nine.codeplex.com");
        }
        #endregion

        #region Methods
        public Task<string> ShowDialogTaskAsync(string title, string description, object content, params string[] options)
        {
            return Shell.ShowDialogTaskAsync(title, description, content, options);
        }

        public Task QueueWorkItem(string title, string description, Task task)
        {
            return Shell.QueueWorkItem(title, description, task);
        }

        public object Invoke(Delegate method, params object[] args)
        {
            return Shell.Invoke(method, args);
        }

        /// <summary>
        /// http://support.microsoft.com/kb/305703
        /// </summary>
        public void OpenUrl(string url)
        {
            try { Process.Start(url); } catch { }
        }

        private Task<bool> EnsureProjectSavedTaskAsync()
        {
            if (Project == null || !Project.IsModified)
                return Task<bool>.Factory.StartNew(() => false);

            string description = string.Format(Strings.SaveChangesDescription, Name);
            return ShowDialogTaskAsync(Strings.SaveChanges, description, Strings.Yes, Strings.No)
            .ContinueWith(dialogResult =>
            {
                if (dialogResult.Result == Strings.Yes)
                {
                    PerformSave();
                    return true;
                }
                return false;
            });
        }

        private void Editor_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressHelper.UpdateState(Strings.Loading, e.UserState.ToString(), e.ProgressPercentage / 100.0);
        }

        private void UpdateSettings()
        {
            /*
            var settings = Editor.Extensions.FindSettings(typeof(Editor));
            if (ActiveDocument != null)
                settings = settings.Concat(Editor.Extensions.FindSettings(ActiveDocument.Document.DocumentObject.GetType()))
                                   .Distinct();

            var groupedSettings = settings.Select(s => new SettingsView(this, s))
                                          .GroupBy(v => v.Category)
                                          .Select(g => new SettingsView(this, g.First().DocumentSettings, g));

            Settings.AddRange(groupedSettings);
             */
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
