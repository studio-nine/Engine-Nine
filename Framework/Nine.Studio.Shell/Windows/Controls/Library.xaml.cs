using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Nine.Studio.Shell.ViewModels;

namespace Nine.Studio.Shell.Windows.Controls
{
    /// <summary>
    /// Interaction logic for Library.xaml
    /// </summary>
    public partial class Library : UserControl
    {
        public LibraryView LibraryView { get; private set; }

        public Library()
        {
            InitializeComponent();
        }

        private void Library_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            LibraryView = DataContext as LibraryView;
            if (LibraryView == null) 
                return;

            ICollectionView view = CollectionViewSource.GetDefaultView(LibraryView.ProjectView.Documents);
            view.Filter = (o) =>
            {
                string searchPattern = SearchTextBox.Text;
                return ((DocumentView)o).Filename.IndexOf(searchPattern, StringComparison.InvariantCultureIgnoreCase) >= 0;
            };
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (LibraryView != null)
                CollectionViewSource.GetDefaultView(LibraryView.ProjectView.Documents).Refresh();
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ((FrameworkElement)(e.Source)).DataContext = LibraryView.EditorView;
        }

        private void CanDelete(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = LibraryView != null;
        }

        private void ExecuteDelete(object sender, ExecutedRoutedEventArgs e)
        {
            var doc = ((FrameworkElement)e.OriginalSource).DataContext as DocumentView;
            if (doc != null && LibraryView != null)
            {
                LibraryView.ProjectView.DeleteDocument(doc);
            }
        }
    }
}
