namespace Nine.Studio.Shell.Behaviors
{



    using System.Windows.Input;

    public class CommandBinding : System.Windows.Input.CommandBinding
    {
        public ICommand TargetCommand { get; set; }

        public CommandBinding()
        {
            Executed += new ExecutedRoutedEventHandler(CommandBinding_Executed);
            CanExecute += new CanExecuteRoutedEventHandler(CommandBinding_CanExecute);
        }

        void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = TargetCommand != null && TargetCommand.CanExecute(e.Parameter);
        }

        void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (TargetCommand != null)
                TargetCommand.Execute(e.Parameter);
        }
    }
}
