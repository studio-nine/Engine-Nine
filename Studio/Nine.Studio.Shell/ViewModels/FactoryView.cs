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
    public class FactoryView
    {
        public ICommand NewCommand { get; private set; }
        public IMetadata Metadata { get; private set; }
        public IFactory Factory { get; private set; }
        public Editor Editor { get; private set; }
        public EditorView EditorView { get; private set; }

        public FactoryView(EditorView editorView, IFactory factory, IMetadata metadata)
        {
            this.EditorView = editorView;
            this.Editor = editorView.Editor;
            this.Factory = factory;
            this.NewCommand = new DelegateCommand(New);
            this.Metadata = metadata;
        }

        public void New()
        {
            EditorView.ActiveProject.CreateProjectItem(Factory);
        }

        public override string ToString()
        {
            return Metadata.DisplayName ?? base.ToString();
        }
    }
}
