﻿namespace Nine.Studio.Shell.ViewModels
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using Nine.Studio.Extensibility;
    using Nine.Studio.Shell;

    public class ProjectView : INotifyPropertyChanged
    {
        public Project Project { get; private set; }
        public Editor Editor { get { return EditorView.Editor; } }
        public OldEditorView EditorView { get; private set; }
        public ObservableCollection<ProjectItemView> ProjectItems { get; private set; }

        public ProjectView(OldEditorView editorView, Project project)
        {
            this.Project = project;
            this.EditorView = editorView;
        }


        public void OpenProjectItem(string fileName)
        {
            ProjectItemView piView = new ProjectItemView(this, EditorView.Project.Import(fileName));
            ProjectItems.Add(piView);
            piView.Show();
            /*
            ProgressHelper.DoWork(
                (e) =>
                {
                    return EditorView.Project.OpenDocument(e.ToString());
                },
                (e) =>
                {
                    if (e != null)
                    {
                        DocumentView docView = new DocumentView(this, (Document)e);
                        Documents.Add(docView);
                        docView.Show();
                    }
                }, fileName, Strings.Loading, fileName);
             */
        }

        public void OpenProjectItem(string fileName, IImporter importer)
        {
            ProgressHelper.DoWork(
                (e) =>
                {
                    return EditorView.Project.Import(e.ToString(), importer);
                },
                (e) =>
                {
                    if (e != null)
                    {
                        /*
                        EditorView.Shell.Invoke((Action)delegate
                        {
                            ProjectItemView projectItemView = new ProjectItemView(this, (ProjectItem)e);
                            ProjectItems.Add(projectItemView);
                            //projectItemView.Show();
                        });
                         */
                    }
                }, fileName, Strings.Loading, fileName);
        }

        public void DeleteProjectItem(ProjectItemView projectItem)
        {
            if (ProjectItems.Contains(projectItem))
            {
                projectItem.Hide();
                projectItem.ProjectItem.Close();
                ProjectItems.Remove(projectItem);
                Project.IsModified = true;
            }
        }

        public void Save()
        {
            Project.Save();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
