namespace DefaultNamespace
{
    using BetterHostedServices;
    using Microsoft.Extensions.DependencyInjection;

    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddBetterHostedServices(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddTransient<IApplicationEnder, ApplicationEnder>();
        }
    }
}
