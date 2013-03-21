namespace Nine.Studio.Shell
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;

    public partial class DialogView : UserControl
    {
        public string Title { get; internal set; }
        public string Text { get; internal set; }
        public string[] Buttons { get; internal set; }
        public object DialogContent { get; internal set; }

        public DialogView(string title, string text, object content, string[] buttons)
        {
            this.Title = title;
            this.Text = text;
            this.Buttons = buttons;
            this.DialogContent = content;
            this.DataContext = this;

            InitializeComponent();
        }

        private void DialogButton_Click(object sender, RoutedEventArgs e)
        {
            App.Shell.EndDialog(this, ((FrameworkElement)sender).DataContext.ToString());
        }
    }
}
