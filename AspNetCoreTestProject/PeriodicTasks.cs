namespace AspNetCoreTestProject
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BetterHostedServices;

    public class PrintingPeriodicTask : IPeriodicTask
    {
        public Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Hello world");
            return Task.CompletedTask;
        }
    }

    public class CountingPeriodicTask : IPeriodicTask
    {
        private SingletonStateHolder singletonStateHolder;
        private TransientStateHolder transientStateHolder;
        private ScopeStateHolder scopeStateHolder;

        public CountingPeriodicTask(TransientStateHolder transientStateHolder, SingletonStateHolder singletonStateHolder, ScopeStateHolder scopeStateHolder)
        {
            this.transientStateHolder = transientStateHolder;
            this.singletonStateHolder = singletonStateHolder;
            this.scopeStateHolder = scopeStateHolder;
        }

        public Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.transientStateHolder.Count += 1;
            this.singletonStateHolder.Count += 1;
            this.scopeStateHolder.Count += 1;

            var random = new Random();
            if (random.Next(0, 10) < 2)
            {
                throw new Exception("Oh no something went horribly wrong.");
            }

            Console.WriteLine($"Transient state: {this.transientStateHolder.Count}. Scope state: {this.scopeStateHolder.Count}. Singleton state: {this.singletonStateHolder.Count}");
            return Task.CompletedTask;
        }
    }

    public class SingletonStateHolder
    {
        public int Count { get; set; } = 0;
    }

    public class ScopeStateHolder
    {
        public int Count { get; set; } = 0;
    }

    public class TransientStateHolder
    {
        public int Count { get; set; } = 0;
    }

}
