namespace BetterHostedServices.Test.IntegrationUtils
{
    using System;
    using Microsoft.Extensions.DependencyInjection;

    public class TestPeriodicTaskFactory<TPeriodicTask> : IPeriodicTaskFactory<TPeriodicTask> where TPeriodicTask : IPeriodicTask
    {
        private readonly IServiceProvider serviceProvider;

        public TestPeriodicTaskFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public bool CanResolvePeriodicTask()
        {
            using var scope = this.serviceProvider.CreateScope();

            return scope.ServiceProvider.GetService<TPeriodicTask>() != null;

        }
        public TPeriodicTask GetPeriodicTask()
        {
            using var scope = this.serviceProvider.CreateScope();

            return scope.ServiceProvider.GetRequiredService<TPeriodicTask>();

        }
    }
}
