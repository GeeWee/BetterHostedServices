namespace BetterHostedServices
{
    using Microsoft.Extensions.Hosting;

    public interface IApplicationEnder
    {
        void ShutDownApplication();
    }

    public class ApplicationEnder : IApplicationEnder
    {
        private IHostApplicationLifetime _lifetime;

        public ApplicationEnder(IHostApplicationLifetime lifetime)
        {
            this._lifetime = lifetime;
        }

        public void ShutDownApplication() => this._lifetime.StopApplication();
    }
}
