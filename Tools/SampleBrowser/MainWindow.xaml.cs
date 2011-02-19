using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Nine.Tools.SampleBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<SampleInfo> Samples { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = Samples = new ObservableCollection<SampleInfo>();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            Action<DispatcherObject> populate = PopulateData;
            populate.BeginInvoke(this, null, null);
        }

        private void PopulateData(DispatcherObject dispatcher)
        {
            foreach (var info in SampleScaner.Scan(@"..\Samples"))
            {
                dispatcher.Dispatcher.Invoke((Action)(() => Samples.Add(info)));
            }
        }
    }
}
