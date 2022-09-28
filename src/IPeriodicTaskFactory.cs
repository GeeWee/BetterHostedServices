namespace BetterHostedServices
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Factory that require <see cref="IPeriodicTask"/> from container
    /// </summary>
    /// <typeparam name="TPeriodicTask">Type to require</typeparam>
    public interface IPeriodicTaskFactory<TPeriodicTask> where TPeriodicTask : IPeriodicTask
    {
        /// <summary>
        /// Require a task
        /// </summary>
        /// <returns>A task instance</returns>
        TPeriodicTask GetPeriodicTask();

        /// <summary>
        /// Try to get a task from container
        /// </summary>
        /// <returns>Value, indicates existing of a task in container</returns>
        bool CanResolvePeriodicTask();
    }

    internal class PeriodicTaskFactory<TPeriodicTask> : IPeriodicTaskFactory<TPeriodicTask> where TPeriodicTask : IPeriodicTask
    {
        private readonly IServiceProvider serviceProvider;

        public PeriodicTaskFactory(IServiceProvider serviceProvider)
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
