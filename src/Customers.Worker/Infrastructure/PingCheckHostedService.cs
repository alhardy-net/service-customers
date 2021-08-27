using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Customers.Worker.Infrastructure
{
    public class PingCheckHostedService : IHostedService
    {
        private readonly ILogger<PingCheckHostedService> _logger;
        private PingCheckServer _pingCheckServer;

        public PingCheckHostedService(ILogger<PingCheckHostedService> logger)
        {
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting ping-check server...");
            
            _pingCheckServer = new PingCheckServer();
            _pingCheckServer.Start();
            
            _logger.LogInformation("Health-check ping started");


            await Task.FromResult(true);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping ping server...");

            await _pingCheckServer.StopAsync();
            
            _logger.LogInformation("Ping server stopped");
        }
    }
}