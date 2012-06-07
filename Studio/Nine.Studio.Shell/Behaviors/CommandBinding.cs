#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
#endregion

namespace Nine.Studio.Shell.Behaviors
{
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
