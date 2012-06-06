#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using AvalonDock;
using Nine.Studio.Shell.Windows;
using Microsoft.Win32;
using System.Windows.Shell;
using Nine.Studio.Extensibility;
using System.Windows;
#endregion

namespace Nine.Studio.Shell.ViewModels
{
    public class EditorView : INotifyPropertyChanged
    {
        const int MaxRecentFilesCount = 10;
        
        public string Name { get { return Path.GetFileName(Filename); } }
        public string Filename { get { return ProjectView != null ? ProjectView.Filename : ""; } }
        public string Title { get { return ProjectView == null ? Editor.Title : string.Format("{0} - {1}", Name, Editor.Title); } }

        public ObservableCollection<DocumentContent> DocumentContents { get; private set; }
        public ObservableCollection<DocumentTypeView> DocumentTypes { get; private set; }
        public ObservableCollection<DocumentSerializerView> Importers { get; private set; }
        public ObservableCollection<DocumentSerializerView> Exporters { get; private set; }
        public ObservableCollection<string> RecentFiles { get; private set; }
        
        public Project Project { get { return ProjectView != null ? ProjectView.Project : null; } }
        public Editor Editor { get; private set; }

        public ProjectView ProjectView
        {
            get { return _ProjectView; }
            set
            {
                if (value != _ProjectView)
                {
                    _ProjectView = value;
                    
                    Importers.Clear();
                    Exporters.Clear();
                    DocumentTypes.Clear();
                    DocumentContents.Clear();

                    if (_ProjectView != null)
                    {
                        Importers.AddRange(Editor.Extensions.FindImporters().Select(s => new DocumentSerializerView(this, s)));
                        Exporters.AddRange(Editor.Extensions.FindExporters().Select(s => new DocumentSerializerView(this, s)));
                        DocumentTypes.AddRange(Editor.Extensions.DocumentTypes.Select(doc => new DocumentTypeView(this, doc)));
                    }

                    NotifyPropertyChanged("Filename");
                    NotifyPropertyChanged("Title");
                    NotifyPropertyChanged("Name");
                    NotifyPropertyChanged("Project");
                    NotifyPropertyChanged("ProjectView");
                }
            }
        }
        private ProjectView _ProjectView;
        

        public EditorView(Editor editor)
        {
            this.Editor = editor;
            this.DocumentContents = new ObservableCollection<DocumentContent>();
            this.DocumentTypes = new ObservableCollection<DocumentTypeView>();
            this.Importers = new ObservableCollection<DocumentSerializerView>();                       
            this.Exporters = new ObservableCollection<DocumentSerializerView>();
            this.RecentFiles = new ObservableCollection<string>();

            LoadCurrentDirectoryFromRegistry();
            LoadRecentFiles();
        }

        public void Exit()
        {
            SaveRecentFiles();
            SaveCurrentDirectoryToRegistry();
        }

        public void CreateProject()
        {
            try
            {
                Editor.CreateProject();
                ProjectView = new ProjectView(this, Editor.Project);
                GC.Collect();
            }
            catch (Exception ex)
            {
                Trace.TraceError(Strings.ErrorOpenProject);
                Trace.WriteLine(ex.ToString());
                MessageBox.Show(Strings.ErrorCreateProject, Editor.Title, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void OpenProject(string filename)
        {
            try
            {
                Editor.OpenProject(filename);
                ProjectView = new ProjectView(this, Editor.Project);
                NewRecentFile(filename);
            }
            catch (Exception ex)
            {
                Trace.TraceError(Strings.ErrorOpenProject);
                Trace.WriteLine(ex.ToString());
                MessageBox.Show(Strings.ErrorOpenProject, Editor.Title, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public void SaveProject(string filename)
        {
            ProjectView.Save(filename);
            GC.Collect();

            NewRecentFile(Filename);
            
            NotifyPropertyChanged("Filename");
            NotifyPropertyChanged("Title");
            NotifyPropertyChanged("Name");
        }

        private void NewRecentFile(string filename)
        {
            if (!string.IsNullOrEmpty(filename))
            {
                RecentFiles.Remove(filename);
                RecentFiles.Insert(0, filename);
                while (RecentFiles.Count > MaxRecentFilesCount)
                    RecentFiles.RemoveAt(RecentFiles.Count - 1);

                JumpList.AddToRecentCategory(filename);
            }
        }

        private void LoadRecentFiles()
        {
            object value = null;
            string keyName = Global.GetUserRegistry("Recent Files");
            RecentFiles.Clear();
            for (int i = 0; i < MaxRecentFilesCount; i++)
            {
                if ((value = Registry.GetValue(keyName, i.ToString(), null)) == null)
                    break;

                if (File.Exists(value.ToString()))
                    RecentFiles.Add(value.ToString());
            }
        }

        private void SaveRecentFiles()
        {
            string keyName = Global.GetUserRegistry("Recent Files");
            for (int i = 0; i < Math.Min(MaxRecentFilesCount, RecentFiles.Count); i++)
            {
                Registry.SetValue(keyName, i.ToString(), RecentFiles[i]);
            }
        }
        
        private void LoadCurrentDirectoryFromRegistry()
        {
            Directory.SetCurrentDirectory(Global.GetProperty("StartupDirectory") ?? Directory.GetCurrentDirectory());
        }

        private void SaveCurrentDirectoryToRegistry()
        {
            Global.SetProperty("StartupDirectory", Directory.GetCurrentDirectory());
        }

        public void CloseProject()
        {
            if (Editor.Project != null)
            {
                ProjectView = null;
                Editor.Project.Close();
                GC.Collect();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
