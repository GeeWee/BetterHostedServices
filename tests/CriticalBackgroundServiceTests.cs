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
            var applicationEnder = new ApplicationEnderMock();

            var factory = this._factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddTransient<IApplicationEnder>(s => applicationEnder);
                    services.AddHostedService<YieldingAndThenCrashingCriticalBackgroundService>();
                });
            });

            var client = factory.CreateClient();
            var res = await client.GetAsync("/");

            // Task is hella flaky because it depends on the internals of the IHostedService - try yielding a bunch of times
            // to hope that it's done requesting application shutdown at this point
            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(50);
                await Task.Yield();
            }

            // due to https://github.com/dotnet/aspnetcore/issues/25857 we can't test if the process is closed directly
            applicationEnder.ShutDownRequested.Should().BeTrue();


        }
    }
}
