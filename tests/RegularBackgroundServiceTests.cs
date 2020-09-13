namespace BetterHostedServices.Test
{
    using System;
    using System.Threading.Tasks;
    using BetterHostedServices;
    using FluentAssertions;
    using HostedServices;
    using IntegrationUtils;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    // These primarily exist to document the "regular" behaviour so I'm sure I've understood it correctly
    public class RegularBackgroundServiceTests
    {
        private readonly CustomWebApplicationFactory<DummyStartup> _factory;

        public RegularBackgroundServiceTests()
        {
            _factory = new CustomWebApplicationFactory<DummyStartup>();
        }

        [Fact]
        public void WhenBackgroundService_ErrorsWithoutYielding_ApplicationCrashes()
        {
            var factory = this._factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddHostedService<ImmediatelyCrashingBackgroundService>();
                });
            });

            Action client = () => factory
                .CreateClient();

            client.Should().Throw<Exception>().WithMessage("Crash right away");
        }

        [Fact]
        public async Task WhenBackgroundService_YieldsBeforeThrowingError_ApplicationShouldWork()
        {
            var applicationEnderMock = new ApplicationEnderMock();

            var factory = this._factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddHostedService<YieldingAndThenCrashingBackgroundService>();
                    services.AddTransient<IApplicationEnder>(provider => applicationEnderMock);
                });
            });

            // Create client
            var client = factory
                .CreateClient();

            // And assert it works even though backgroundservice crashed
            var res = await client.GetAsync("/");
            res.EnsureSuccessStatusCode();
        }
    }
}
