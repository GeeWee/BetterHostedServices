namespace BetterHostedServices
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Extensions on IServiceCollection
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Add the scaffolding needed to make the BetterHostedServices etc. work.
        /// Should be called once before making any IHostedServices run.
        /// </summary>
        public static IServiceCollection AddBetterHostedServices(this IServiceCollection serviceCollection) => serviceCollection
                .AddTransient<IApplicationEnder, ApplicationLifetimeEnder>();

        /// <summary>
        /// This registers an IHostedService, both as a HostedService, but also as a Singleton in the DI container
        /// so other services can get references to it.
        ///
        /// Heavily inspired by https://stackoverflow.com/a/52398431/8007580
        /// </summary>
        /// <typeparam name="TService">The interface type that the IHostedService should be available under in the DI</typeparam>
        /// <typeparam name="TImplementation">The concrete type of the IHostedService to instantiate</typeparam>
        public static IServiceCollection AddHostedServiceAsSingleton<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, IHostedService, TService
            => services.AddSingleton<TService, TImplementation>()
                       .AddHostedService<HostedServiceWrapper<TService>>();



        /// <summary>
        /// This registers an IHostedService, both as a HostedService, but also as a Singleton in the DI container
        /// so other services can get references to it.
        ///
        /// Heavily inspired by https://stackoverflow.com/a/52398431/8007580
        /// </summary>
        /// <typeparam name="TService">The concrete type of the IHostedService to instantiate.</typeparam>
        public static IServiceCollection AddHostedServiceAsSingleton<TService>(this IServiceCollection services)
            where TService : class, IHostedService
            => services.AddSingleton<TService>()
                       .AddHostedService<HostedServiceWrapper<TService>>();

        /// <summary>
        /// Add a periodic task.
        /// The task is recreated with a new scope for each invocation.
        /// </summary>
        /// <param name="services">The ServiceCollection</param>
        /// <param name="failureMode">Determines how the service behaves when a task fails.</param>
        /// <param name="timeBetweenTasks">How long after the completion of the previous task, should the next task run?</param>
        /// <typeparam name="TPeriodicTask">The periodic task to run</typeparam>
        public static IServiceCollection AddPeriodicTask<TPeriodicTask>(this IServiceCollection services, PeriodicTaskFailureMode failureMode, TimeSpan timeBetweenTasks)
            where TPeriodicTask : class, IPeriodicTask
            => services.AddTransient<TPeriodicTask>()

                    .AddHostedService((services) =>
                        new PeriodicTaskRunnerBackgroundService<TPeriodicTask>(
                            applicationEnder: services.GetRequiredService<IApplicationEnder>(),
                            logger: services.GetRequiredService<ILogger<PeriodicTaskRunnerBackgroundService<TPeriodicTask>>>(),
                            serviceProvider: services.GetRequiredService<IServiceProvider>(),
                            periodicTaskFailureMode: failureMode,
                            timeBetweenTasks: timeBetweenTasks
                    ));
    }
}
