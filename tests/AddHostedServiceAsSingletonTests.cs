namespace BetterHostedServices.Test
{
    using System.Threading.Tasks;
    using FluentAssertions;
    using IntegrationUtils;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Xunit;

    public class AddHostedServiceAsSingletonTests
    {

        [Fact]
        public async Task AddHostedServiceAsSingleton_WhenOnlyUsedWithConcreteClass_ShouldRunTheService()
        {
            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IApplicationEnder, ApplicationEnderMock>();
                    services.AddHostedServiceAsSingleton<SomeBackgroundService>();
                }).Build();

            await host.StartAsync();

            await Task.Delay(300);

            host.Services.GetRequiredService<SomeBackgroundService>().Activated.Should().BeTrue();

            await host.StopAsync();
        }

        [Fact]
        public async Task AddHostedServiceAsSingleton_WhenOnlyUsedWithInterface_ShouldRunTheService()
        {
            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IApplicationEnder, ApplicationEnderMock>();
                    services.AddHostedServiceAsSingleton<ISomeBackgroundService,SomeBackgroundService>();
                }).Build();

            await host.StartAsync();

            await Task.Delay(300);

            host.Services.GetRequiredService<ISomeBackgroundService>().Activated.Should().BeTrue();

            await host.StopAsync();
        }
    }

}
