using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Customers.Worker.Infrastructure
{
    public class HealthCheckHostedService : IHostedService
    {
        private readonly HealthCheckService _healthCheckService;
        private readonly int _port;
        private readonly string _url;
        private readonly bool _useHttps;
        private readonly ILogger<HealthCheckHostedService> _logger;
        private HealthCheckServer _healthCheckServer;

        public HealthCheckHostedService(
            int port,
            HealthCheckService healthCheckService, ILogger<HealthCheckHostedService> logger, string url = "/healthz",
            bool useHttps = false)
        {
            _port = port;
            _url = url;
            _useHttps = useHttps;
            _healthCheckService = healthCheckService;
            _logger = logger;
        }

        public HealthCheckHostedService(ILogger<HealthCheckHostedService> logger, HealthCheckService healthCheckService)
        {
            _logger = logger;
            _port = 80;
            _url = "/healthz";
            _useHttps = false;
            _healthCheckService = healthCheckService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting health-check server...");
            _healthCheckServer = new HealthCheckServer(_port, _healthCheckService, _url, _useHttps);
            _healthCheckServer.Start();
            _logger.LogInformation("Health-check server started");

            await Task.FromResult(true);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping health server...");
            await _healthCheckServer.StopAsync();
            _logger.LogInformation("Health server stopped");
        }
    }
}