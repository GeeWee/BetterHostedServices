namespace BetterHostedServices.Test
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using IntegrationUtils;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Xunit;

    public class PeriodicTasksTest
    {
        [Fact]
        public async Task PeriodicTask_ShouldEndApplication_IfFailureModeIsSetToCrash()
        {
            var applicationEnder = new ApplicationEnderTaskMock();

            using IHost host=Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddTransient<IApplicationEnder>(s => applicationEnder);
                    services.AddPeriodicTask<CrashingPeriodicTask>(PeriodicTaskFailureMode.CrashApplication, TimeSpan.FromSeconds(1));
                })
                .Build();

            await host.StartAsync();

            Task.WaitAny(new Task[] { applicationEnder.ShutDownTask }, 5000).Should().Be(0);

            await host.StopAsync();
        }

        [Fact]
        public async Task PeriodicTask_ShouldContinueRunningTasks_IfFailureModeIsSetToRetry()
        {
            var applicationEnder = new ApplicationEnderMock();
            var stateHolder = new SingletonStateHolder();

            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddTransient<IApplicationEnder>(s => applicationEnder);
                    services.AddPeriodicTask<IncrementingThenCrashingPeriodicTask>(PeriodicTaskFailureMode.RetryLater, TimeSpan.FromMilliseconds(50));
                    services.AddSingleton<SingletonStateHolder>(s => stateHolder);
                })
                .Build();

            await host.StartAsync();

            Task.WaitAny(new Task[] { stateHolder.CalledFiveTimes }, 5000).Should().Be(0);

            await host.StopAsync();
        }
    }
}
