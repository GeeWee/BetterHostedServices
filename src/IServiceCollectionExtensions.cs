namespace BetterHostedServices
{
    using Microsoft.Extensions.DependencyInjection;

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
    }
}
