#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Input;
using Nine.Studio.Extensibility;
using Nine.Studio.Shell.Windows;
#endregion

namespace Nine.Studio.Shell.ViewModels
{
    public class DocumentView : INotifyPropertyChanged
    {
        public ICommand ShowCommand { get; private set; }
        public string Name { get { return Path.GetFileName(Filename); } }
        public string Filename { get; private set; }
        public string Title { get { return Name; } }

        public Document Document { get; private set; }
        public IDocumentType DocumentType { get { return Document.DocumentType; } }
        public Project Project { get { return ProjectView.Project; } }
        public ProjectView ProjectView { get; private set; }
        public Editor Editor { get { return EditorView.Editor; } }
        public EditorView EditorView { get { return ProjectView.EditorView; } }

        private DocumentWindow documentWindow;

        public DocumentView(ProjectView projectView, Document document)
        {
            this.Document = document;
            this.ProjectView = projectView;
            this.Filename = document.Filename ?? Path.GetFullPath(Global.NextName(DocumentType.DisplayName, null));
            this.ShowCommand = new DelegateCommand(Show);
        }

        public void Show()
        {
            if (documentWindow == null)
                documentWindow = new DocumentWindow() { DataContext = this };
            if (!EditorView.DocumentContents.Contains(documentWindow))
                EditorView.DocumentContents.Add(documentWindow);
            documentWindow.Activate();
        }

        public void Hide()
        {
            if (documentWindow != null && EditorView.DocumentContents.Contains(documentWindow))
            {
                documentWindow.Hide();
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
