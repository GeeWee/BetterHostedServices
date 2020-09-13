namespace BetterHostedServices.Test.IntegrationUtils
{
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("/")]
    public class TestController : Controller
    {

        [HttpGet]
        public string HelloWorld() => "Hello world";
    }
}
