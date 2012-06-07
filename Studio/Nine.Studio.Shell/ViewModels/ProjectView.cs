#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Nine.Studio.Extensibility;
using Nine.Studio.Shell.Windows.Controls;
#endregion

namespace Nine.Studio.Shell.ViewModels
{
    public class ProjectView : INotifyPropertyChanged
    {
        public string Name { get { return Path.GetFileName(Filename); } }
        public string Filename { get; private set; }
        public Project Project { get; private set; }
        public Editor Editor { get { return EditorView.Editor; } }
        public EditorView EditorView { get; private set; }
        public ObservableCollection<ProjectItemView> Documents { get; private set; }
        public LibraryView LibraryView { get; private set; }

        public ProjectView(EditorView editorView, Project project)
        {
            this.Project = project;
            this.EditorView = editorView;
            this.Filename = Project.Filename ?? Path.GetFullPath(Global.NextName(Strings.Untitled, Global.ProjectExtension));
            //this.Documents = new ObservableCollection<ProjectItemView>(Project.Documents.Select(doc => new ProjectItemView(this,doc)));
            this.LibraryView = new LibraryView(this);
        }

        public void CreateDocument(object documentObject)
        {
            ProjectItem doc = EditorView.Project.CreateProjectItem(documentObject);
            ProjectItemView docView = new ProjectItemView(this, doc);
            Documents.Add(docView);
            docView.Show();
        }

        public void CreateDocument(IFactory documentFactory)
        {
            ProjectItem doc = EditorView.Project.CreateProjectItem(documentFactory);
            ProjectItemView docView = new ProjectItemView(this, doc);
            Documents.Add(docView);
            docView.Show();
        }

        public void OpenDocument(string filename)
        {
            ProjectItemView docView = new ProjectItemView(this, EditorView.Project.Import(filename));
            Documents.Add(docView);
            docView.Show();
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
                }, filename, Strings.Loading, filename);
             */
        }

        public void OpenDocument(string filename, IImporter documentSerializer)
        {
            ProgressHelper.DoWork(
                (e) =>
                {
                    return EditorView.Project.Import(e.ToString(), documentSerializer);
                },
                (e) =>
                {
                    if (e != null)
                    {
                        ProjectItemView docView = new ProjectItemView(this, (ProjectItem)e);
                        Documents.Add(docView);
                        docView.Show();
                    }
                }, filename, Strings.Loading, filename);
        }

        public void DeleteDocument(ProjectItemView doc)
        {
            if (Documents.Contains(doc))
            {
                doc.Hide();
                doc.Document.Close();
                Documents.Remove(doc);
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
