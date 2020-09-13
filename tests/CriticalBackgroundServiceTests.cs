namespace BetterHostedServices.Test
{
    using BetterHostedServices;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.AspNetCore.TestHost;
    using Xunit;

    public class CriticalBackgroundServiceTests : IClassFixture<CustomWebApplicationFactory<DummyStartup>>
    {
        private readonly CustomWebApplicationFactory<DummyStartup> _factory;

        public CriticalBackgroundServiceTests(CustomWebApplicationFactory<DummyStartup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public void WhenBackgroundService_ExitsWithoutErrors_ApplicationShouldWork()
        {
            var client = this._factory
                // .WithWebHostBuilder(conf => conf.UseSolutionRelativeContentRoot("./tests/IntegrationUtils"))
                .CreateClient();


        }
    }
}
