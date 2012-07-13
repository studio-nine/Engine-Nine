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
using System.Linq;
using System.Text;
using System.Windows.Input;
using Microsoft.Win32;
using Nine.Studio.Extensibility;

#endregion

namespace Nine.Studio.Shell.ViewModels
{
    public class ImporterView
    {
        public ICommand ImportCommand { get; private set; }
        public IMetadata Metadata { get; private set; }
        public IImporter Importer { get; private set; }
        public Editor Editor { get; private set; }
        public EditorView EditorView { get; private set; }

        public ImporterView(EditorView editorView, IImporter importer, IMetadata metadata)
        {
            this.EditorView = editorView;
            this.Editor = editorView.Editor;
            this.Importer = importer;
            this.Metadata = metadata;
            this.ImportCommand = new DelegateCommand(Import);
        }

        public void Import()
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Multiselect = true;
            open.Filter = BuildFilter();
            open.Title = string.Format(Strings.ImportAsset, Metadata.DisplayName);
            bool? result = open.ShowDialog();
            if (result.HasValue && result.Value)
            {
                foreach (var fileName in open.FileNames)
                {
                    EditorView.ActiveProject.OpenProjectItem(fileName, Importer);
                }
            }
        }

        private string BuildFilter()
        {
            StringBuilder result = new StringBuilder();
            if (Importer.FileExtensions != null && Importer.FileExtensions.Count() > 0)
            {
                result.AppendFormat(Strings.AssetDialogFilter, Metadata.DisplayName);
                result.Append("|");
                result.Append(string.Join("; ", Importer.FileExtensions.Select(ext => "*" + ext.GetNormalizedFileExtension())));
                result.Append("|");
            }
            result.AppendFormat("{0}|*.*", Strings.AllFiles);
            return result.ToString();
        }
    }
}
