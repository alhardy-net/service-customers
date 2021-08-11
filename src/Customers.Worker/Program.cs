using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Customers.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Customers.Worker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddMemoryCache();

                    var cache = new MemoryCache(new MemoryCacheOptions());
                    services.AddDbContext<CustomersContext>(options =>
                    {
                        options.UseNpgsql(hostContext.Configuration.GetConnectionString("CustomersContext"));

                        if (!hostContext.HostingEnvironment.IsDevelopment())
                        {
                            options.AddInterceptors(new RdsAuthenticationInterceptor(cache));
                        }
                    });
                    
                    services.AddMassTransit(x =>
                    {
                        x.SetKebabCaseEndpointNameFormatter();

                        // By default, sagas are in-memory, but should be changed to a durable
                        // saga repository.
                        x.SetInMemorySagaRepositoryProvider();

                        var entryAssembly = Assembly.GetEntryAssembly();

                        x.AddConsumers(entryAssembly);
                        x.AddSagaStateMachines(entryAssembly);
                        x.AddSagas(entryAssembly);
                        x.AddActivities(entryAssembly);

                        x.UsingRabbitMq((context,cfg) =>
                        {
                            cfg.Host("dev-alhardynet-rabbitmq", 5672, "/", h =>
                            {
                                h.Username("guest");
                                h.Password("guest");
                            });
                            cfg.ConfigureEndpoints(context);
                        });
                    });

                    services.AddMassTransitHostedService(true);
                });
        }
    }
}