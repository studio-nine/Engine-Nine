namespace Nine.Studio.Shell
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;

    public partial class Dialog : Grid
    {
        public Dialog()
        {
            InitializeComponent();
        }

        #region Properties
        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(Dialog), new UIPropertyMetadata(""));
        
        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(Dialog), new UIPropertyMetadata(""));
        
        public Dock Dock
        {
            get { return (Dock)GetValue(DockProperty); }
            set { SetValue(DockProperty, value); }
        }

        public static readonly DependencyProperty DockProperty =
            DependencyProperty.Register("Dock", typeof(Dock), typeof(Dialog), new UIPropertyMetadata(Dock.Left));
        #endregion

        #region ShowMessage
        public Task<string> Show(string title, string description, object content, Dock dock, params string[] options)
        {
            /*
            Style buttonStyle = (Style)FindResource("Button-Dialog");
            buttons.Children.Clear();
            foreach (var option in options)
            {
                Button optionButton = new Button{ Content = option, Style = buttonStyle, MinWidth = 80 };
                optionButton.Click += new RoutedEventHandler(OptionButton_Click);
                buttons.Children.Add(optionButton);
            }

            Dock = dock;
            Header = title;
            Description = description;
            Visibility = Visibility.Visible;
            PropertyGrid.SelectedObject = content;
            PropertyGrid.Visibility = content != null ? Visibility.Visible : Visibility.Collapsed;

            return Task<string>.Factory.StartNew(RunDialog, new CancellationToken(), TaskCreationOptions.LongRunning, TaskScheduler.Default);
             */
            return null;
        }

        private void OptionButton_Click(object sender, RoutedEventArgs e)
        {
            dialogResult = ((Button)sender).Content;
            dialogResultWaitHandle.Set();
        }

        private string RunDialog()
        {
            dialogResultWaitHandle.WaitOne();
            Dispatcher.Invoke((Action)delegate { ((Panel)Parent).Children.Remove(this); });
            return dialogResult.ToString();
        }

        private AutoResetEvent dialogResultWaitHandle = new AutoResetEvent(false);
        private object dialogResult = null;
        #endregion
    }
}
