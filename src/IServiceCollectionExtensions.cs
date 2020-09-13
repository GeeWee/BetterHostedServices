namespace BetterHostedServices
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

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




        // Stolen from https://stackoverflow.com/a/52398431/8007580
        /**
     * This registers a HostedService, both as an IHostedService, but also as part of the normal
     * DI flow, where you can define the interface it is injected via as well.
     *
     * The service is registered as a singleton in the DI framework
     *
     * TService is the interface which the implementation should be injected via
     * and TImplementation is the actual class.
     */


        /// <summary>
        /// This registers an IHostedService, both as a HostedService, but also as a Singleton in the DI container
        /// so other services can get references to it.
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
        /// </summary>
        /// <typeparam name="TService">The concrete type of the IHostedService to instantiate.</typeparam>
        public static void AddHostedServiceAsSingleton<TService>(this IServiceCollection services)
            where TService : class, IHostedService
        {
            services.AddSingleton<TService>();
            services.AddHostedService<HostedServiceWrapper<TService>>();
        }


    }



}
