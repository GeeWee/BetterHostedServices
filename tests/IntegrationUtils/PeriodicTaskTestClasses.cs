namespace BetterHostedServices.Test.IntegrationUtils
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

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
        private readonly ILogger<IncrementingThenCrashingPeriodicTask> logger;

        public IncrementingThenCrashingPeriodicTask(SingletonStateHolder singletonStateHolder,ILogger<IncrementingThenCrashingPeriodicTask> logger)
        {
            this.singletonStateHolder = singletonStateHolder;
            this.logger = logger;
        }

        public Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.singletonStateHolder.Call();
            this.logger.LogDebug("ExecuteAsync() called {Times} times", this.singletonStateHolder.Count);
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
        private readonly ILogger<SingletonStateHolder> logger;

        public Task CalledFiveTimes => this.taskCompletion.Task;
        public int Count { get; private set; } = 0;

        public SingletonStateHolder(ILogger<SingletonStateHolder> logger=null)
        {
            this.logger = logger;
        }

        public void Call()
        {
            this.logger?.LogDebug($"Call() called. Current calls count: {this.Count}");

            this.Count++;
            if (this.Count >= 5)
            {
                this.taskCompletion.TrySetResult();
            }
        }
    }
}
