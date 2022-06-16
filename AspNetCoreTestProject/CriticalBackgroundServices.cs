namespace AspNetCoreTestProject
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BetterHostedServices;
    using Microsoft.Extensions.Logging.Abstractions;

    public class ImmediatelyCrashingCriticalBackgroundService : CriticalBackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
            throw new Exception("Crash right away");

        protected override void OnError(Exception exceptionFromExecuteAsync)
        {
            this._applicationEnder.ShutDownApplication();
        }

        public ImmediatelyCrashingCriticalBackgroundService(IApplicationEnder applicationEnder) : base(applicationEnder, NullLogger<ImmediatelyCrashingCriticalBackgroundService>.Instance)
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

        public YieldingAndThenCrashingCriticalBackgroundService(IApplicationEnder applicationEnder) : base(applicationEnder, NullLogger<YieldingAndThenCrashingCriticalBackgroundService>.Instance)
        {
        }
    }

    public class StubCriticalBackgroundService : CriticalBackgroundService
    {
        public bool Activated { get; private set; } = false;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield(); // Hand over control explicitly
            this.Activated = true;
        }

        protected override void OnError(Exception exceptionFromExecuteAsync)
        {
            this._applicationEnder.ShutDownApplication();
        }

        public StubCriticalBackgroundService(IApplicationEnder applicationEnder) : base(applicationEnder, NullLogger<StubCriticalBackgroundService>.Instance)
        {
        }
    }
}
