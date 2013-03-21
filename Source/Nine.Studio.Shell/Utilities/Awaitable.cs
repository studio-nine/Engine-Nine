namespace Nine.Studio.Shell
{
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// A minimum class that supports the .NET async/await syntax.
    /// This class is not thread safe.
    /// </summary>
    public class Awaitable<T> : INotifyCompletion
    {
        private Action continuation;

        public T Result { get; private set; }
        public bool IsCompleted { get; private set; }

        public event Action Completed;

        public Awaitable<T> GetAwaiter() 
        {
            return this; 
        }

        public T GetResult()
        {
            return Result;
        }

        public void OnCompleted(Action continuation)
        {
            this.continuation = continuation;
        }

        public void NotifyCompletion(T result)
        {
            if (!IsCompleted)
            {
                Result = result;
                IsCompleted = true;

                var completed = Completed;
                if (completed != null)
                {
                    completed();
                    Completed = null;
                }

                if (continuation != null)
                {
                    continuation();
                    continuation = null;
                }
            }
        }
    }
}
