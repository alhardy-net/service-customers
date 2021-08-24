using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Customers.Worker.Infrastructure
{
    public class PingCheckHostedService : IHostedService
    {
        private PingCheckServer _pingCheckServer;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Console.Out.WriteLineAsync("Starting ping-check server...");
            _pingCheckServer = new PingCheckServer();
            _pingCheckServer.Start();
            await Console.Out.WriteLineAsync("Health-check ping started");

            await Task.FromResult(true);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Console.Out.WriteLineAsync("Stopping ping server...");
            await _pingCheckServer.StopAsync();
            await Console.Out.WriteLineAsync("Metrics ping stopped");
        }
    }
}