namespace Nine.Studio.Shell.Windows
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Threading;
    using System.Windows;
    using System.Windows.Input;

    static class ProgressHelper
    {
        class Item
        {
            public Func<object, object> Action;
            public Action<object> Completed;
            public object State;
            public string Title;
            public string Text;
        }

        static object queueLock = new object();
        static Mutex shownLock = new Mutex();
        static bool taskCompleted = false;
        static WaitWindow window = null;
        static Queue<Item> queue = new Queue<Item>();
        static int finishedTasks = 0;
        
        public static void DoWork(Func<object, object> action, Action<object> completed, object state, string title, string text)
        {
            App.Current.MainWindow.Cursor = Cursors.Wait;
            ((UIElement)(App.Current.MainWindow.Content)).IsEnabled = false;

            lock (queueLock)
            {
                queue.Enqueue(new Item() { Action = action, Completed = completed, State = state, Title = title, Text = text, });
                if (queue.Count > 1)
                    return;
                finishedTasks = 0;
            }

            shownLock.WaitOne();
            window = null;
            taskCompleted = false;
            shownLock.ReleaseMutex();

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, e) =>
            {
                bool empty = false;
                do
                {
                    Item item = null;
                    double? progress = null;
                    lock (queueLock)
                    {
                        if (queue.Count <= 0)
                            break;
                        item = queue.Peek();
                        if (!(queue.Count == 1 && finishedTasks == 0))
                            progress = 1.0 * (finishedTasks + 1) / (queue.Count + finishedTasks);
                    }

                    App.Current.Dispatcher.BeginInvoke((Action<string, string, double?>)
                                           UpdateUI, item.Title, item.Text, progress);
                    var result = item.Action(item.State);
                    App.Current.Dispatcher.BeginInvoke(item.Completed, result);

                    lock (queueLock)
                    {
                        queue.Dequeue();
                        empty = (queue.Count <= 0);
                        finishedTasks++;
                    }
                }
                while (!empty);
            };
            worker.RunWorkerCompleted += (s, e) =>
            {
                App.Current.Dispatcher.BeginInvoke((Action)HideWaitWindow);
                App.Current.MainWindow.Cursor = Cursors.Arrow;
                ((UIElement)(App.Current.MainWindow.Content)).IsEnabled = true;
            };
            worker.RunWorkerAsync();

            BackgroundWorker timer = new BackgroundWorker();
            timer.DoWork += (s, e) =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                shownLock.WaitOne();
                App.Current.Dispatcher.BeginInvoke((Action)ShowWaitWindow);
                shownLock.ReleaseMutex();
            };
            timer.RunWorkerAsync();
        }

        public static void UpdateState(string title, string text, double? progress)
        {
            App.Current.Dispatcher.BeginInvoke((Action<string, string, double?>)UpdateUI, title, text, progress);
        }

        private static void UpdateUI(string title, string text, double? progress)
        {
            if (window != null)
            {
                window.TextBox.Text = text;
                if (!string.IsNullOrEmpty(title))
                    window.Title = title;

                lock (queueLock)
                {
                    window.ProgressBar.IsIndeterminate = !progress.HasValue;
                    window.ProgressBar.Value = progress.HasValue ? 100 * progress.Value : 0;
                }
            }
        }

        private static void ShowWaitWindow()
        {
            shownLock.WaitOne();
            if (taskCompleted)
            {
                shownLock.ReleaseMutex();
                return;
            }

            window = new WaitWindow();
            window.Owner = App.Current.MainWindow;
            window.Loaded += (sender, e) =>
            {
                shownLock.ReleaseMutex();
            };

            Item item = null;
            lock (queueLock)
            {
                item = queue.Peek();
            }
            UpdateUI(item.Title, item.Text, null);
            window.ShowDialog();
        }

        private static void HideWaitWindow()
        {
            shownLock.WaitOne();
            taskCompleted = true;
            if (window != null)
            {
                window.Hide();
                window = null;
            }
            shownLock.ReleaseMutex();
        }
    }
}
