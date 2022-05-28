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
        /// Require a <see cref="TPeriodicTask"/>
        /// </summary>
        /// <returns><see cref="TPeriodicTask"/> instance</returns>
        TPeriodicTask GetPeriodicTask();

        /// <summary>
        /// Try to get <see cref="TPeriodicTask"/> from container
        /// </summary>
        /// <returns>Value, indicates existing of <see cref="TPeriodicTask"/> in container</returns>
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
