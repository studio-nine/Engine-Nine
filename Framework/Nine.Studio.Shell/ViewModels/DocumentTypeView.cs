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
#endregion

namespace Nine.Studio.Shell.ViewModels
{
    public class DocumentTypeView
    {
        public ICommand NewCommand { get; private set; }
        public IDocumentType DocumentType { get; private set; }
        public Editor Editor { get; private set; }
        public EditorView EditorView { get; private set; }

        public DocumentTypeView(EditorView editorView, IDocumentType documentType)
        {
            this.EditorView = editorView;
            this.Editor = editorView.Editor;
            this.DocumentType = documentType;
            this.NewCommand = new DelegateCommand(New);
        }

        public void New()
        {
            App.Current.MainWindow.Cursor = Cursors.Wait;
            EditorView.ProjectView.CreateDocument(DocumentType);
            App.Current.MainWindow.Cursor = Cursors.Arrow;
        }
    }
}
