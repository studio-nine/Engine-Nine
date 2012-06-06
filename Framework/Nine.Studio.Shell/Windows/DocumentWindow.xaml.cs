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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AvalonDock;
using Nine.Studio.Shell.ViewModels;
using Nine.Studio.Extensibility;
using System.Windows.Forms.Integration;
using Nine.Studio.Shell.Windows.Controls;
#endregion

namespace Nine.Studio.Shell.Windows
{
    /// <summary>
    /// Interaction logic for LibraryWindow.xaml
    /// </summary>
    public partial class DocumentWindow : DocumentContent
    {
        public Document Document { get; private set; }
        public DocumentView DocumentView { get; private set; }
        public IDocumentVisualizer DocumentVisualizer { get; private set; }

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
                DocumentView = (DocumentView)DataContext;
                Document = DocumentView.Document;

                IDocumentVisualizer visualizer = Editor.Extensions.FindVisualizer(Document.DocumentObject.GetType());
                if (visualizer != null)
                {
                    // We'll create a new visualizer for each document.
                    DocumentVisualizer = (IDocumentVisualizer)Activator.CreateInstance(visualizer.GetType());
                    AddChildContent(DocumentVisualizer.Visualize(Document.DocumentObject));
                }
                else
                {
                    Host.Children.Add(new NoDesignView());
                }
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            if (Document != null)
            {
                Host.Children.Clear();
                DocumentView = null;
                Document = null;
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
