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
        public static IServiceCollection AddBetterHostedServices(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddTransient<IApplicationEnder, ApplicationLifetimeEnder>();
        }

        /// <summary>
        /// This registers an IHostedService, both as a HostedService, but also as a Singleton in the DI container
        /// so other services can get references to it.
        ///
        /// Heavily inspired by https://stackoverflow.com/a/52398431/8007580
        /// </summary>
        /// <typeparam name="TService">The interface type that the IHostedService should be available under in the DI</typeparam>
        /// <typeparam name="TImplementation">The concrete type of the IHostedService to instantiate</typeparam>
        public static void AddHostedServiceAsSingleton<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, IHostedService, TService
        {
            services.AddSingleton<TService, TImplementation>();
            services.AddHostedService<HostedServiceWrapper<TService>>();
        }


        /// <summary>
        /// This registers an IHostedService, both as a HostedService, but also as a Singleton in the DI container
        /// so other services can get references to it.
        ///
        /// Heavily inspired by https://stackoverflow.com/a/52398431/8007580
        /// </summary>
        /// <typeparam name="TService">The concrete type of the IHostedService to instantiate.</typeparam>
        public static void AddHostedServiceAsSingleton<TService>(this IServiceCollection services)
            where TService : class, IHostedService
        {
            services.AddSingleton<TService>();
            services.AddHostedService<HostedServiceWrapper<TService>>();
        }

        /// <summary>
        /// Add a periodic task
        /// TODO
        /// </summary>
        /// <param name="services"></param>
        /// <param name="failureMode"></param>
        /// <param name="timeBetweenTasks"></param>
        /// <typeparam name="TPeriodicTask"></typeparam>
        public static void AddPeriodicTask<TPeriodicTask>(this IServiceCollection services, PeriodicTaskFailureMode failureMode, TimeSpan timeBetweenTasks)
            where TPeriodicTask : class, IPeriodicTask
        {
            services.AddTransient<TPeriodicTask>();

            services.AddHostedService<PeriodicTaskRunnerBackgroundService<TPeriodicTask>>((services) =>
            {
                return new PeriodicTaskRunnerBackgroundService<TPeriodicTask>(
                    applicationEnder: services.GetRequiredService<IApplicationEnder>(),
                    logger: services.GetRequiredService<ILogger<PeriodicTaskRunnerBackgroundService<TPeriodicTask>>>(),
                    serviceProvider: services.GetRequiredService<IServiceProvider>(),
                    periodicTaskFailureMode: failureMode,
                    timeBetweenTasks: timeBetweenTasks
                );
            });
        }



    }



}
