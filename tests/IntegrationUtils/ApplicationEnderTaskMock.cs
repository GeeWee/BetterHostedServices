namespace BetterHostedServices.Test.IntegrationUtils
{
    using System.Threading.Tasks;

    public class ApplicationEnderTaskMock : IApplicationEnder
    {
        public Task ShutDownTask => this.completionSource.Task;

        private readonly TaskCompletionSource completionSource = new();

        public void ShutDownApplication() => this.completionSource.TrySetResult();
    }
}
