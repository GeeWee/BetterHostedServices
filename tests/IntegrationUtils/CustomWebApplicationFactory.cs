namespace BetterHostedServices.Test.IntegrationUtils
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.Hosting;

    // from https://stackoverflow.com/questions/61159115/asp-net-core-testing-no-method-public-static-ihostbuilder-createhostbuilders
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSolutionRelativeContentRoot("./tests/IntegrationUtils");
        }

        //from https://stackoverflow.com/questions/61159115/asp-net-core-testing-no-method-public-static-ihostbuilder-createhostbuilders
        protected override IHostBuilder CreateHostBuilder()
        {
            var builder = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(x =>
                {
                    x.UseStartup<DummyStartup>().UseTestServer();
                });
            return builder;
        }
    }
}
