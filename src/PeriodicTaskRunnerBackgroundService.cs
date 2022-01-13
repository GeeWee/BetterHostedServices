namespace BetterHostedServices
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The service that runs a periodic task.
    /// </summary>
    /// <typeparam name="TPeriodicTask"></typeparam>
    public class PeriodicTaskRunnerBackgroundService<TPeriodicTask> : CriticalBackgroundService
        where TPeriodicTask : IPeriodicTask
    {
        private readonly ILogger<PeriodicTaskRunnerBackgroundService<TPeriodicTask>> logger;
        private readonly IServiceProvider serviceProvider;

        private readonly PeriodicTaskFailureMode periodicTaskFailureMode;
        private readonly TimeSpan timeBetweenTasks;

        /// <summary>
        ///
        /// </summary>
        /// <param name="applicationEnder"></param>
        /// <param name="logger"></param>
        /// <param name="criticalLogger"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="periodicTaskFailureMode"></param>
        /// <param name="timeBetweenTasks"></param>
        public PeriodicTaskRunnerBackgroundService(
            IApplicationEnder applicationEnder,
            ILogger<PeriodicTaskRunnerBackgroundService<TPeriodicTask>> logger,
            ILogger<CriticalBackgroundService> criticalLogger, 
            IServiceProvider serviceProvider,
            PeriodicTaskFailureMode periodicTaskFailureMode,
            TimeSpan timeBetweenTasks) : base(applicationEnder, criticalLogger)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
            this.periodicTaskFailureMode = periodicTaskFailureMode;
            this.timeBetweenTasks = timeBetweenTasks;
        }

        /// <summary>
        /// Executes the given TPeriodicTask in a new scope each time.
        /// </summary>
        /// <param name="stoppingToken"></param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Ensure that we can crate the service. Do this synchronously so that we'll fail-fast no matter the failure mode
            // if the task can't run at all.
            using (var scope = this.serviceProvider.CreateScope())
            {
                _ = scope.ServiceProvider.GetRequiredService<TPeriodicTask>();
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = this.serviceProvider.CreateScope();
                    var periodicTask = scope.ServiceProvider.GetRequiredService<TPeriodicTask>();
                    await periodicTask.ExecuteAsync(stoppingToken);
                }
                catch (Exception e)
                {
                    this.logger.LogError(e, $"Exception while processing message in {typeof(TPeriodicTask)}");
                    // If failure mode is set to end application, go through the normal OnError flow that crashes the application.
                    if (this.periodicTaskFailureMode == PeriodicTaskFailureMode.CrashApplication)
                    {
                        this.OnError(e);
                    }

                    if (this.periodicTaskFailureMode == PeriodicTaskFailureMode.RetryLater)
                    {
                        this.logger.LogWarning(e, $"Exception while processing message in {typeof(TPeriodicTask)}. Retrying in {this.timeBetweenTasks}");
                    }
                }

                await Task.Delay(this.timeBetweenTasks, stoppingToken);
            }
        }
    }
}
