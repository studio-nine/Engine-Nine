namespace Nine.Studio.Shell
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;

    public interface IEditorShell
    {
        DialogResult ShowDialogAsync(object content);

        DialogResult ShowDialogAsync(string title, string text, object content, params string[] options);

        void EndDialog(FrameworkElement dialog, string dialogResult);

        Task RunAsync(Action action);

                /*
        Task QueueWorkItem(string title, string description, Task task);

        object Invoke(Delegate method, params object[] args);
                 */
    }
    
    public class DialogResult : Awaitable<string>
    {
        public FrameworkElement Dialog { get; set; }
    }
}
