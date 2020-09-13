using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace BetterHostedServices
{
    /// <summary>
    /// Base class for implementing a long running <see cref="IHostedService"/>.
    /// It will shut down the application if the task returned from ExecuteAsync fails
    /// Override OnError if you want to handle it in a different way.
    ///
    /// Heavily inspired by the BackgroundService in ASP.NET Core, but with more error handling
    /// </summary>
    public abstract class CriticalBackgroundService : IHostedService, IDisposable
    {
        private Task _executingTask;
        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();
        /// <summary>
        /// Use this if you want to shutdown the application.
        /// </summary>
        protected IApplicationEnder _applicationEnder;

        /// <summary>
        /// </summary>
        protected CriticalBackgroundService(IApplicationEnder applicationEnder)
        {
            this._applicationEnder = applicationEnder;
        }

        /// <summary>
        /// This method is called when the <see cref="IHostedService"/> starts. The implementation should return a task that represents
        /// the lifetime of the long running operation(s) being performed.
        /// </summary>
        /// <param name="stoppingToken">Triggered when <see cref="IHostedService.StopAsync(CancellationToken)"/> is called.</param>
        /// <returns>A <see cref="Task"/> that represents the long running operations.</returns>
        protected abstract Task ExecuteAsync(CancellationToken stoppingToken);

        /// <summary>
        /// How to handle if an error is thrown from the ExecuteAsync function?
        /// It will default to logging to stderr, and shutting down the application
        /// but you can override it, if you want to handle it differently, or do some extra logging.
        /// </summary>
        /// <param name="exceptionFromExecuteAsync"></param>
        protected virtual void OnError(Exception exceptionFromExecuteAsync)
        {
            Console.Error.WriteLine($"Error happened while executing CriticalBackgroundTask {this.GetType().FullName}. Shutting down.");
            Console.Error.WriteLine(exceptionFromExecuteAsync.ToString());
            this._applicationEnder.ShutDownApplication();
        }

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            // Store the task we're executing
            this._executingTask = this.ExecuteAsync(this._stoppingCts.Token);

            // If the task is completed then return it, this will bubble cancellation and failure to the caller
            if (this._executingTask.IsCompleted)
            {
                return this._executingTask;
            }

            // Add an error handler here that shuts down the application
            // Do not save it to _executingTask - as that means it will hang on shutting down
            // until the grace period is over.
            this._executingTask.ContinueWith(t =>
            {
                if (t.Exception !=  null)
                {
                    this.OnError(t.Exception);
                }
            }, this._stoppingCts.Token);

            // Otherwise it's running
            return Task.CompletedTask;
        }

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            // Stop called without start
            if (this._executingTask == null)
            {
                return;
            }

            try
            {
                // Signal cancellation to the executing method
                this._stoppingCts.Cancel();
            }
            finally
            {
                // Wait until the task completes or the stop token triggers
                await Task.WhenAny(this._executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }

        /// <summary>
        /// Uses the stoppingToken to attempt to try to cancel the task
        /// </summary>
        public virtual void Dispose()
        {
            this._stoppingCts.Cancel();
        }
    }
}
