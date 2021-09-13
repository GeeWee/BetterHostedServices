namespace BetterHostedServices.Test.IntegrationUtils
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class CrashingPeriodicTask: IPeriodicTask
    {
        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(50, stoppingToken); // Just to yield control
            throw new Exception("oh no");
        }
    }

    public class IncrementingThenCrashingPeriodicTask: IPeriodicTask
    {
        private readonly SingletonStateHolder singletonStateHolder;

        public IncrementingThenCrashingPeriodicTask(SingletonStateHolder singletonStateHolder) => this.singletonStateHolder = singletonStateHolder;

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.singletonStateHolder.Count += 1;
            throw new Exception("oh no");
        }
    }

    public class SingletonStateHolder
    {
        public int Count { get; set; }
    }
}
