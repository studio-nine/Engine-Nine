#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using AvalonDock;
using Nine.Studio.Shell.Windows;
using Microsoft.Win32;
using System.Windows.Shell;
using Nine.Studio.Extensibility;
using System.Windows.Input;
#endregion

namespace Nine.Studio.Shell
{
    public class DelegateCommand<T> : ICommand
    {
        readonly Action<T> _execute;
        readonly Predicate<T> _canExecute;

        public DelegateCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        public DelegateCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }
    }

    public class DelegateCommand : DelegateCommand<object>
    {
        public DelegateCommand(Action execute)
            : base(o => execute(), null)
        {
        }
    }
}
