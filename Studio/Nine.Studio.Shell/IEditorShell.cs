namespace Nine.Studio.Shell
{
    using System;
    using System.Threading.Tasks;

    public interface IEditorShell
    {
        Task ShowDialogAsync(object content);
                /*
        Task<string> ShowDialogAsync(string title, string description, object content, params string[] options);
                
        Task QueueWorkItem(string title, string description, Task task);

        object Invoke(Delegate method, params object[] args);
                 */
    }
}
