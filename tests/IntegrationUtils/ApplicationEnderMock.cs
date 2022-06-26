namespace BetterHostedServices.Test.IntegrationUtils
{
    using Microsoft.Extensions.Logging;

    public class ApplicationEnderMock : IApplicationEnder
    {
        private readonly ILogger<ApplicationEnderMock> logger;

        public ApplicationEnderMock(ILogger<ApplicationEnderMock> logger = null)
        {
            this.logger = logger;
        }

        public bool ShutDownRequested { get; private set; } = false;
        public void ShutDownApplication()
        {
            logger?.LogDebug($"{nameof(ShutDownApplication)} called");
            this.ShutDownRequested = true;
        }
    }
}
