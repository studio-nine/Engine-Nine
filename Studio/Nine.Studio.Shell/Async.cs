#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Threading;
using System.Windows.Threading;

#endregion

namespace Nine.Studio.Shell
{
    static class Async
    {
        private static object awaitLock = new object();
        private static TaskScheduler scheduler = null;

        private static void CheckSchedular()
        {
            lock (awaitLock)
            {
                if (scheduler == null)
                {
                    SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(App.Current.Dispatcher));
                    scheduler = TaskScheduler.FromCurrentSynchronizationContext();
                }
            }
        }

        public static Task Await<TInput>(this Task<TInput> task, Action<TInput> action)
        {
            CheckSchedular();
            return task.ContinueWith(t => action(t.Result), scheduler);
        }

        public static Task<TOutput> Await<TInput, TOutput>(this Task<TInput> task, Func<TInput, TOutput> func)
        {
            CheckSchedular();
            return task.ContinueWith<TOutput>(t => func(t.Result), scheduler);
        }

        public static Task<T> FromResult<T>(T result)
        {
            return Task<T>.Factory.StartNew(() => result);
        }

        public static Task Run(Action action)
        {
            return Task.Factory.StartNew(action, new CancellationToken(), TaskCreationOptions.None, TaskScheduler.Default);
        }

        public static Task Run(Action action, TaskScheduler scheduler)
        {
            return Task.Factory.StartNew(action, new CancellationToken(), TaskCreationOptions.None, scheduler);
        }

        public static Task<T> Run<T>(Func<T> action)
        {
            return Task<T>.Factory.StartNew(action, new CancellationToken(), TaskCreationOptions.None, TaskScheduler.Default);
        }

        public static Task<T> Run<T>(Func<T> action, TaskScheduler scheduler)
        {
            return Task<T>.Factory.StartNew(action, new CancellationToken(), TaskCreationOptions.None, scheduler);
        }
    }
}
