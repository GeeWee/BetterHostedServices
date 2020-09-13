namespace BetterHostedServices.Test
{
    using System;
    using System.Threading.Tasks;
    using BetterHostedServices;
    using FluentAssertions;
    using HostedServices;
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
            // var lifeTimeMock = new IHostApplicationLifetimeMock();

            var factory = this._factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddHostedService<CriticalBackgroundServices.ImmediatelyCrashingCriticalBackgroundService>();
                    // services.AddSingleton<IHostApplicationLifetime>(s => lifeTimeMock);
                });
            });

            Action client = () => factory
                .CreateClient();

            client.Should().Throw<Exception>().WithMessage("Crash right away");
        }

        [Fact]
        public async Task WhenCriticalBackgroundService_YieldsBeforeThrowingError_ApplicationShouldWork()
        {
            var applicationEnder = new ApplicationEnderMock();

            var factory = this._factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddHostedService<CriticalBackgroundServices.YieldingAndThenCrashingCriticalBackgroundService>();
                    services.AddSingleton<IApplicationEnder>(s => applicationEnder);
                });
            });

            // Assert this also crashes, even though yielding happened
            // Action createClientAction = () => factory
                // .CreateClient();

            var client = factory.CreateClient();

            // due to https://github.com/dotnet/aspnetcore/issues/25857 we can't test if the process is closed directly
            applicationEnder.ShutDownRequested.Should().BeTrue();
        }
    }
}
