#region Copyright 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
#endregion

namespace Nine.Studio.Shell
{
    /// <summary>
    /// Defines the interactive editor shell.
    /// </summary>
    public interface IEditorShell
    {
        /// <summary>
        /// Shows a dialog.
        /// </summary>
        Task<string> ShowDialogAsync(string title, string description, object content, params string[] options);
                
        /// <summary>
        /// Queues a work item.
        /// </summary>
        Task QueueWorkItem(string title, string description, Task task);

        /// <summary>
        /// Executes the specified delegate with the specified arguments synchronously.
        /// </summary>
        object Invoke(Delegate method, params object[] args);
    }
}
