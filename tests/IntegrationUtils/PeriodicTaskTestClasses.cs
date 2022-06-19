namespace BetterHostedServices.Test.IntegrationUtils
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class CrashingPeriodicTask : IPeriodicTask
    {
        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(50, stoppingToken); // Just to yield control
            throw new Exception("oh no");
        }
    }

    public class IncrementingThenCrashingPeriodicTask : IPeriodicTask
    {
        private readonly SingletonStateHolder singletonStateHolder;

        public IncrementingThenCrashingPeriodicTask(SingletonStateHolder singletonStateHolder) => this.singletonStateHolder = singletonStateHolder;

        public Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.singletonStateHolder.Call();
            throw new Exception("oh no");
        }
    }

    public class TestPeriodicTask : IPeriodicTask
    {
        public Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;
    }

    public class SingletonStateHolder
    {
        private readonly TaskCompletionSource taskCompletion = new();

        public Task CalledFiveTimes => this.taskCompletion.Task;
        private int count = 0;

        public void Call()
        {
            this.count++;
            if (this.count >= 5)
            {
                this.taskCompletion.TrySetResult();
            }
        }
    }
}
