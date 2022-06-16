namespace BetterHostedServices.Test
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BetterHostedServices;
    using FluentAssertions;
    using HostedServices;
    using IntegrationUtils;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Hosting.Internal;
    using Xunit;

    public class CriticalBackgroundServiceTests
    {
        private readonly CustomWebApplicationFactory<DummyStartup> _factory;

        public CriticalBackgroundServiceTests()
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
                    services.AddHostedService<ImmediatelyCrashingCriticalBackgroundService>();
                });
            });

            Action client = () => factory
                .CreateClient();

            client.Should().Throw<Exception>().WithMessage("Crash right away");
        }

        [Fact]
        public async Task WhenCriticalBackgroundService_YieldsBeforeThrowingError_ApplicationShouldCrash()
        {
            var applicationEnder = new ApplicationEnderTaskMock();

            YieldingAndThenCrashingCriticalBackgroundService backgroundService = new(applicationEnder);

            await backgroundService.StartAsync(CancellationToken.None);

            Task.WaitAny(new Task[] { applicationEnder.ShutDownTask }, 3000).Should().Be(0);
        }
    }
}
