namespace BetterHostedServices.Test.IntegrationUtils
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;


    public interface ISomeBackgroundService
    {
        public bool Activated { get; }
    }

    public class SomeBackgroundService : CriticalBackgroundService, ISomeBackgroundService
    {
        public SomeBackgroundService(IApplicationEnder applicationEnder) : base(applicationEnder)
        {
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.Activated = true;
            return Task.CompletedTask;
        }

        protected override void OnError(Exception exceptionFromExecuteAsync) { }

        public bool Activated { get; internal set; } = false;
    }

    [ApiController]
    [Route("interface")]
    public class SomeControllerThatDependsOnInterface : Controller
    {
        [HttpGet]
        public bool Get() => this.someBackgroundServiceFromInterface.Activated;

        private ISomeBackgroundService someBackgroundServiceFromInterface;

        public SomeControllerThatDependsOnInterface(ISomeBackgroundService someBackgroundServiceFromInterface)
        {
            this.someBackgroundServiceFromInterface = someBackgroundServiceFromInterface;
        }
    }

    [ApiController]
    [Route("concrete")]
    public class SomeControllerThatDependsOnConcreteClass : Controller
    {
        [HttpGet]
        public bool Get() => this.someBackgroundService.Activated;

        private SomeBackgroundService someBackgroundService;

        public SomeControllerThatDependsOnConcreteClass(SomeBackgroundService someBackgroundService)
        {
            this.someBackgroundService = someBackgroundService;
        }
    }
}
