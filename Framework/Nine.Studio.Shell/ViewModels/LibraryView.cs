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
using System.Windows.Input;
#endregion

namespace Nine.Studio.Shell.ViewModels
{
    public class LibraryView : INotifyPropertyChanged
    {
        public Project Project { get { return ProjectView.Project; } }
        public ProjectView ProjectView { get; private set; }
        public Editor Editor { get { return EditorView.Editor; } }
        public EditorView EditorView { get { return ProjectView.EditorView; } }
        public ObservableCollection<ProjectView> ReferencedProjects { get; private set; }

        public ProjectView SelectedProject
        {
            get { return _SelectedProject; }
            set
            {
                if (value != _SelectedProject)
                {
                    _SelectedProject = value;
                    NotifyPropertyChanged("SelectedProject");
                }
            }
        }
        private ProjectView _SelectedProject;

        public LibraryView(ProjectView projectView)
        {
            this.ProjectView = projectView;
            this.ReferencedProjects = new ObservableCollection<ProjectView>(
                 Project.References.Select(p => new ProjectView(EditorView, p)));
            this.ReferencedProjects.Insert(0, ProjectView);
            this.SelectedProject = ProjectView;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
