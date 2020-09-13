namespace BetterHostedServices.Test
{
    public class ApplicationEnderMock : IApplicationEnder
    {
        public bool ShutDownRequested { get; private set; } = false;
        public void ShutDownApplication() => ShutDownRequested = true;
    }
}
