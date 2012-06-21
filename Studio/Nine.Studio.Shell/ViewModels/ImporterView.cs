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
        public string Category { get; private set; }
        public IImporter Importers { get; private set; }
        public Editor Editor { get; private set; }
        public EditorView EditorView { get; private set; }

        public ImporterView(EditorView editorView, IImporter importers)
        {
            this.EditorView = editorView;
            this.Editor = editorView.Editor;
            this.Importers = importers;
            //this.Category = importers.First().Category ?? importers.First().DisplayName;
            this.ImportCommand = new DelegateCommand(Import);
        }

        public void Import()
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Multiselect = true;
            open.Filter = BuildFilter();
            open.Title = string.Format(Strings.ImportAsset, Category);
            bool? result = open.ShowDialog();
            if (result.HasValue && result.Value)
            {
                foreach (var fileName in open.FileNames)
                {
                    EditorView.ActiveProject.OpenProjectItem(fileName);
                }
            }
        }

        private string BuildFilter()
        {
            throw new NotImplementedException();
            /*
            StringBuilder result = new StringBuilder();
            if (Importers.Count(d => d.FileExtensions != null && d.FileExtensions.Count() > 0) > 1)
            {
                result.AppendFormat(Strings.AllSupportedFormat, Category);
                result.Append("|");
                result.Append(string.Join("; ", Importers.SelectMany(i => i.FileExtensions ?? Enumerable.Empty<string>())
                                                                 .Select(ext => "*" + ext.GetNormalizedFileExtension())
                                                                 .Distinct()));
                result.Append("|");
            }
            foreach (var documentImporter in Importers)
            {
                if (documentImporter.FileExtensions != null && documentImporter.FileExtensions.Count() > 0)
                {
                    result.AppendFormat(Strings.AssetDialogFilter, documentImporter.DisplayName);
                    result.Append("|");
                    result.Append(string.Join("; ", documentImporter.FileExtensions.Select(ext => "*" + ext.GetNormalizedFileExtension())));
                    result.Append("|");
                }
            }
            result.AppendFormat("{0}|*.*", Strings.AllFiles);
            return result.ToString();
             */
        }
    }
}
