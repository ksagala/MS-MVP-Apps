﻿namespace MVP.App.Common
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines a model for executing actions with a timeout.
    /// </summary>
    public class TimeoutAction
    {
        /// <summary>
        /// Executes a task with an expected result asynchronously.
        /// </summary>
        /// <param name="taskToExecute">
        /// The task to execute.
        /// </param>
        /// <param name="cts">
        /// The cancellation token source.
        /// </param>
        /// <typeparam name="TValue">
        /// The expected result type.
        /// </typeparam>
        /// <returns>
        /// When this method completes, it returns a <see cref="TValue"/> object.
        /// </returns>
        public static async Task<TValue> ExecuteAsync<TValue>(Task<TValue> taskToExecute, CancellationTokenSource cts = null)
        {
            if (cts == null)
            {
                cts = new CancellationTokenSource();
            }

            var token = cts.Token;

            var result = default(TValue);

            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10), token);

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var task = await Task.WhenAny(taskToExecute, timeoutTask);
                    if (task == taskToExecute)
                    {
                        cts.Cancel();

                        if (!task.IsFaulted)
                        {
                            result = taskToExecute.Result;
                        }
                    }

                    if (task == timeoutTask)
                    {
                        // ToDo; Show dialog.
                        cts.Cancel();
                    }
                }
                catch (TaskCanceledException tce)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine(tce.ToString());
#endif
                }
            }

            return result;
        }
    }
}