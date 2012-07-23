namespace Nine.Studio.Shell
{
    using System;






    using System.Threading.Tasks;

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
