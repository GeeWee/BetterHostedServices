namespace AspNetCoreTestProject.Controllers
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("[controller]")]
    public class HostedServiceController : ControllerBase
    {
        private readonly StubCriticalBackgroundService backgroundService;

        public HostedServiceController(StubCriticalBackgroundService backgroundService)
        {
            this.backgroundService = backgroundService;
        }

        [HttpGet]
        public bool Get()
        {
            return this.backgroundService.Activated;
        }
    }
}
