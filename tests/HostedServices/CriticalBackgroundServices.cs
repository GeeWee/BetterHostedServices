namespace BetterHostedServices.Test.HostedServices
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;


    public class ImmediatelyCrashingCriticalBackgroundService : CriticalBackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken) => throw new Exception("Crash right away");
        protected override void OnError(Exception exceptionFromExecuteAsync)
        {
            this._applicationEnder.ShutDownApplication();
        }

        public ImmediatelyCrashingCriticalBackgroundService(IApplicationEnder lifeTime) : base(lifeTime)
        {
        }
    }

    public class YieldingAndThenCrashingCriticalBackgroundService : CriticalBackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield(); // Hand over control explicitly, to ensure this behaviour also works
            throw new Exception("Crash after yielding");
        }

        protected override void OnError(Exception exceptionFromExecuteAsync)
        {
            this._applicationEnder.ShutDownApplication();
        }

        public YieldingAndThenCrashingCriticalBackgroundService(IApplicationEnder lifeTime) : base(lifeTime)
        {
        }

    }
}
