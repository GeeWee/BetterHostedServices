namespace BetterHostedServices.Test
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using IntegrationUtils;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public class PeriodicTasksTest
    {
        private readonly CustomWebApplicationFactory<DummyStartup> _factory;

        public PeriodicTasksTest()
        {
            _factory = new CustomWebApplicationFactory<DummyStartup>();
        }

        [Fact]
        public async Task PeriodicTask_ShouldEndApplication_IfFailureModeIsSetToCrash()
        {
            // Arrange
            var applicationEnder = new ApplicationEnderMock();

            var factory = this._factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddTransient<IApplicationEnder>(s => applicationEnder);
                    services.AddPeriodicTask<CrashingPeriodicTask>(PeriodicTaskFailureMode.CrashApplication, TimeSpan.FromSeconds(1));
                });
            });

            var client = factory.CreateClient();
            // Act & assert - we should crash here at some point

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

        [Fact]
        public async Task PeriodicTask_ShouldContinueRunningTasks_IfFailureModeIsSetToRetry()
        {
            // Arrange
            var applicationEnder = new ApplicationEnderMock();
            var stateHolder = new SingletonStateHolder();

            var factory = this._factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddTransient<IApplicationEnder>(s => applicationEnder);
                    services.AddPeriodicTask<IncrementingThenCrashingPeriodicTask>(PeriodicTaskFailureMode.RetryLater, TimeSpan.FromMilliseconds(50));
                    services.AddSingleton<SingletonStateHolder>(s => stateHolder);
                });
            });

            var client = factory.CreateClient();
            // Act & assert - we crash here after each invocation, but we truck on. The stateHolder should keep being incremented

            // Task is hella flaky because it depends on the internals of the IHostedService - try yielding a bunch of times
            // to hope that it's done requesting application shutdown at this point
            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(200); // 1s ms all in all
                await Task.Yield();
            }

            stateHolder.Count.Should().BeGreaterThan(1);

            // due to https://github.com/dotnet/aspnetcore/issues/25857 we can't test if the process is closed directly
            applicationEnder.ShutDownRequested.Should().BeFalse();
        }
    }
}
