namespace BetterHostedServices
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A periodic tasks to be executed by a PeriodicTaskRunnerBackgroundService.
    /// This task is re-created with a new scope for each invocation to ExecuteAsync.
    /// </summary>
    public interface IPeriodicTask
    {
        /// <summary>
        /// This ExecuteAsync is called by the PeriodicTaskRunnerBackgroundService at a given interval.
        /// Note that you will get a newly created IPeriodicTask for each invocation.
        /// </summary>
        /// <param name="stoppingToken">CancellationToken to listen to if you should abandon your work.</param>
        /// <returns>Should return a task that will be awaited by the PeriodicTaskRunnerBackgroundService</returns>
        public Task ExecuteAsync(CancellationToken stoppingToken);
    }
}
