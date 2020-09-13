namespace BetterHostedServices.Test
{
    using System.Threading.Tasks;
    using FluentAssertions;
    using IntegrationUtils;
    using Microsoft.AspNetCore.TestHost;
    using Xunit;

    public class AddHostedServiceAsSingletonTests
    {
        private readonly CustomWebApplicationFactory<DummyStartup> _factory;

        public AddHostedServiceAsSingletonTests()
        {
            _factory = new CustomWebApplicationFactory<DummyStartup>();
        }

        [Fact]
        public async Task AddHostedServiceAsSingleton_WhenOnlyUsedWithConcreteClass_ShouldRunTheService_AndAllowForItToBeAccessibleInController()
        {
            var factory = this._factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddHostedServiceAsSingleton<SomeBackgroundService>();
                });
            });

            var client = factory
                .CreateClient();

            // The controller can get the service, and the ExecuteAsync has been called.
            var res = await client.GetAsync("concrete");
            res.EnsureSuccessStatusCode();
            var content =
                (await res.Content.ReadAsStringAsync());
            content.Should().Be("true");
        }

        [Fact]
        public async Task AddHostedServiceAsSingleton_WhenOnlyUsedWithInterface_ShouldRunTheService_AndAllowForItToBeAccessibleInController()
        {
            var factory = this._factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddHostedServiceAsSingleton<ISomeBackgroundService, SomeBackgroundService>();
                });
            });

            var client = factory
                .CreateClient();

            // The controller can get the service, and the ExecuteAsync has been called.
            var res = await client.GetAsync("interface");
            res.EnsureSuccessStatusCode();
            var content =
                (await res.Content.ReadAsStringAsync());
            content.Should().Be("true");
        }
    }

}
