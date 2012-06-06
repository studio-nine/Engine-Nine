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
        public ObservableCollection<DocumentView> Documents { get; private set; }
        public LibraryView LibraryView { get; private set; }

        public ProjectView(EditorView editorView, Project project)
        {
            this.Project = project;
            this.EditorView = editorView;
            this.Filename = Project.Filename ?? Path.GetFullPath(Global.NextName(Strings.Untitled, Global.FileExtension));
            this.Documents = new ObservableCollection<DocumentView>(Project.Documents.Select(doc => new DocumentView(this,doc)));
            this.LibraryView = new LibraryView(this);
        }

        public void CreateDocument(object documentObject)
        {
            Document doc = Editor.Project.CreateDocument(documentObject);
            DocumentView docView = new DocumentView(this, doc);
            Documents.Add(docView);
            docView.Show();
        }

        public void CreateDocument(IDocumentType documentType)
        {
            Document doc = Editor.Project.CreateDocument(documentType);
            DocumentView docView = new DocumentView(this, doc);
            Documents.Add(docView);
            docView.Show();
        }

        public void OpenDocument(string filename, IDocumentSerializer documentSerializer)
        {
            Document doc = Editor.Project.OpenDocument(filename, documentSerializer);
            DocumentView docView = new DocumentView(this, doc);
            Documents.Add(docView);
            docView.Show();
        }

        public void DeleteDocument(DocumentView doc)
        {
            if (Documents.Contains(doc))
            {
                doc.Hide();
                Documents.Remove(doc);
                Project.Documents.Remove(doc.Document);
                Project.IsModified = true;
            }
        }

        public void Save(string filename)
        {
            Project.Save(filename);
            Filename = Project.Filename;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
