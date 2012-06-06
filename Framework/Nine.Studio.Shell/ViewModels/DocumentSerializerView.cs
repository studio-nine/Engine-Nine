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
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Input;
using Nine.Studio.Extensibility;
using Microsoft.Win32;
using System.Threading;
using System.Text;
#endregion

namespace Nine.Studio.Shell.ViewModels
{
    public class DocumentSerializerView
    {
        public ICommand ExportCommand { get; private set; }
        public ICommand ImportCommand { get; private set; }
        public IDocumentSerializer DocumentSerializer { get; private set; }
        public Editor Editor { get; private set; }
        public EditorView EditorView { get; private set; }

        public DocumentSerializerView(EditorView editorView, IDocumentSerializer documentSerializer)
        {
            this.EditorView = editorView;
            this.Editor = editorView.Editor;
            this.DocumentSerializer = documentSerializer;
            this.ExportCommand = new DelegateCommand(Export);
            this.ImportCommand = new DelegateCommand(Import);
        }

        public void Export()
        {
        }

        public void Import()
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Multiselect = true;
            open.Filter = BuildFilter();
            open.Title = string.Format(Strings.ImportAsset, DocumentSerializer.DisplayName);
            bool? result = open.ShowDialog();
            if (result.HasValue && result.Value)
            {
                App.Current.MainWindow.Cursor = Cursors.Wait;
                foreach (var filename in open.FileNames)
                {
                    EditorView.ProjectView.OpenDocument(filename, DocumentSerializer);
                }
                App.Current.MainWindow.Cursor = Cursors.Arrow;
            }
        }

        private string BuildFilter()
        {
            StringBuilder result = new StringBuilder();
            if (DocumentSerializer.FileExtensions != null && DocumentSerializer.FileExtensions.Count() > 0)
            {
                result.AppendFormat(Strings.AssetDialogFilter, DocumentSerializer.DisplayName);
                result.Append("|");
                result.Append(string.Join("; ", DocumentSerializer.FileExtensions.Select(ext => "*" + ext.GetNormalizedFileExtension())));
                result.Append("|");
            }
            result.Append("All Files|*.*");
            return result.ToString();
        }
    }
}
