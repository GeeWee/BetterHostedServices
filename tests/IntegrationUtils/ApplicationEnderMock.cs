namespace BetterHostedServices.Test.IntegrationUtils
{
    public class ApplicationEnderMock : IApplicationEnder
    {
        public bool ShutDownRequested { get; private set; } = false;
        public void ShutDownApplication() => this.ShutDownRequested = true;
    }
}
