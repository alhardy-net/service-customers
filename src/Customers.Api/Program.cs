using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Customers.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            await CreateDbIfNotExists(host);
            
            await host.RunAsync();
        }
        
        private static async Task CreateDbIfNotExists(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            
            // try
            // {
            //     var context = services.GetRequiredService<CustomerContext>();
            //     await DbInitializer.InitAsync(context);
            // }
            // catch (Exception ex)
            // {
            //     var logger = services.GetRequiredService<ILogger<Program>>();
            //     logger.LogError(ex, "An error occurred creating the DB");
            // }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}