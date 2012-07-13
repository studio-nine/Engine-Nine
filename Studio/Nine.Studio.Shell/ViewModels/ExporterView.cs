#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.Windows.Input;
using Nine.Studio.Extensibility;

#endregion

namespace Nine.Studio.Shell.ViewModels
{
    public class ExporterView
    {
        public ICommand ExportCommand { get; private set; }
        public IMetadata Metadata { get; private set; }
        public IExporter Exporter { get; private set; }
        public Editor Editor { get; private set; }
        public EditorView EditorView { get; private set; }

        public ExporterView(EditorView editorView, IExporter documentExporter, IMetadata metadata)
        {
            this.EditorView = editorView;
            this.Editor = editorView.Editor;
            this.Metadata = metadata;
            this.Exporter = documentExporter;
            this.ExportCommand = new DelegateCommand(Export);
        }

        public void Export()
        {
        }
    }
}
