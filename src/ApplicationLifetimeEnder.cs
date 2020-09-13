namespace BetterHostedServices
{
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// Interface for different ways to shut down the application.
    /// </summary>
    public interface IApplicationEnder
    {
        /// <summary>
        /// Requests that the application is shut down gracefully
        /// </summary>
        void ShutDownApplication();
    }

    /// <summary>
    /// IApplicationEnder that requests that the application stops through calling
    /// IHostApplicationLifetime
    /// </summary>
    public class ApplicationLifetimeEnder : IApplicationEnder
    {
        private IHostApplicationLifetime _lifetime;

        /// <summary>
        /// Constructor - not meant to be used directly
        /// </summary>
        public ApplicationLifetimeEnder(IHostApplicationLifetime lifetime)
        {
            this._lifetime = lifetime;
        }

        /// <inheritdoc />
        public void ShutDownApplication() => this._lifetime.StopApplication();
    }
}
