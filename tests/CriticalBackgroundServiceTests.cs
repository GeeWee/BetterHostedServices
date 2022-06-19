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

        [Fact]
        public void WhenBackgroundService_ErrorsWithoutYielding_ApplicationCrashes()
        {
            var applicationEnder = new ApplicationEnderTaskMock();

            ImmediatelyCrashingCriticalBackgroundService backgroundService = new(applicationEnder);

            backgroundService
                .Invoking(backgroundService => backgroundService.StartAsync(CancellationToken.None))
                .Should().Throw<Exception>();
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
