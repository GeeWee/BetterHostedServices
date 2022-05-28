namespace BetterHostedServices.Test
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using IntegrationUtils;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public class PeriodicTasksFactoryTest
    {
        private readonly CustomWebApplicationFactory<DummyStartup> _factory;

        public PeriodicTasksFactoryTest()
        {
            _factory = new CustomWebApplicationFactory<DummyStartup>();
        }

        [Fact]
        public void PeriodicTaskFactory_CanResolvePeriodicTask()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddSingleton<TestPeriodicTaskFactory<TestPeriodicTask>>();

            ServiceProvider provider=services.BuildServiceProvider();

            var factory=provider.GetRequiredService<TestPeriodicTaskFactory<TestPeriodicTask>>();

            factory.CanResolvePeriodicTask().Should().BeFalse();



            services.AddTransient<TestPeriodicTask>();

            provider=services.BuildServiceProvider();

            factory = provider.GetRequiredService<TestPeriodicTaskFactory<TestPeriodicTask>>();

            factory.CanResolvePeriodicTask().Should().BeTrue();
        }

        [Fact]
        public void PeriodicTaskFactory_CreateInstance()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddSingleton<TestPeriodicTaskFactory<TestPeriodicTask>>();
            services.AddTransient<TestPeriodicTask>();

            var provider = services.BuildServiceProvider();

            var factory = provider.GetRequiredService<TestPeriodicTaskFactory<TestPeriodicTask>>();

            factory.GetPeriodicTask().Should().NotBeNull();
        }
    }
}
