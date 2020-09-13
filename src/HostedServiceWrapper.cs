namespace BetterHostedServices
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// This is a wrapper for a HostedService, which can start/stop it.
    /// Works by being an IHostedService, and then getting the TService injected into it
    /// and then just calling start/stop on that.
    /// </summary>
    /// <typeparam name="TService"></typeparam>
    internal class HostedServiceWrapper<TService> : IHostedService where TService : notnull
    {
        private readonly IHostedService _hostedService;

        public HostedServiceWrapper(TService hostedService)
        {
            _hostedService = (IHostedService) hostedService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _hostedService.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _hostedService.StopAsync(cancellationToken);
        }
    }
}
