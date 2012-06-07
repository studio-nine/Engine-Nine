#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using Nine.Studio.Shell.ViewModels;
using Nine.Studio.Shell.Windows.Controls;
#endregion

namespace Nine.Studio.Shell.Windows
{
    public partial class DocumentWindow : UserControl
    {
        public ProjectItem Document { get; private set; }
        public ProjectItemView DocumentView { get; private set; }

        public Editor Editor { get { return Document.Editor; } }
        public EditorView EditorView { get { return DocumentView.EditorView; } }

        public DocumentWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Document == null)
            {
                DocumentView = (ProjectItemView)DataContext;
                Document = DocumentView.Document;
                DocumentView.PropertyChanged += DocumentView_PropertyChanged;
                UpdateVisualizer();
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            if (Document != null)
            {
                DocumentView.PropertyChanged -= DocumentView_PropertyChanged;
                DocumentView = null;
                Document = null;
            }
        }

        private void DocumentView_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ActiveVisualizer")
            {
                UpdateVisualizer();
            }
        }

        private void UpdateVisualizer()
        {
            Host.Children.Clear();
            if (DocumentView != null && DocumentView.ActiveVisualizer != null)
            {
                VisualizerView visualizer = DocumentView.ActiveVisualizer;
                if (visualizer != null)
                {
                    // We'll create a new visualizer for each document.
                    AddChildContent(DocumentView.ActiveVisualizer.Visualize());
                }
                else
                {
                    Host.Children.Add(new NoDesignView());
                }
            }
        }

        private void AddChildContent(object content)
        {
            if (content == null)
                return;

            if (content is System.Windows.Forms.Control)
            {
                Host.Children.Add(new WindowsFormsHost() { Child = (System.Windows.Forms.Control)content });
                return;
            }

            if (content is UIElement)
            {
                Host.Children.Add((UIElement)content);
                return;
            }
            Host.Children.Add(new NoDesignView());
        }
    }
}
