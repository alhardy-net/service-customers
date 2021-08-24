using System;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Customers.Persistence;
using Customers.Worker.Infrastructure;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
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
                .ConfigureAppConfiguration((context, config) => { context.AddAwsSecretsManager(config); })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddMemoryCache();
                    services.AddHostedService<PingCheckHostedService>();

                    var cache = new MemoryCache(new MemoryCacheOptions());
                    services.AddDbContext<CustomersContext>(options =>
                    {
                        options.UseNpgsql(hostContext.Configuration.GetConnectionString("CustomersContext"));

                        if (!hostContext.HostingEnvironment.IsDevelopment())
                            options.AddInterceptors(new RdsAuthenticationInterceptor(cache));
                    });

                    services.AddMassTransit(x =>
                    {
                        x.SetKebabCaseEndpointNameFormatter();

                        var entryAssembly = Assembly.GetEntryAssembly();

                        x.AddConsumers(entryAssembly);

                        x.UsingRabbitMq((context, cfg) =>
                        {
                            if (!hostContext.HostingEnvironment.IsDevelopment())
                            {
                                var rabbitUri = hostContext.Configuration.GetConnectionString("RabbitMQ");
                                var username =
                                    hostContext.Configuration.GetValue<string>("customers/shared:rabbitmq_username");
                                var password =
                                    hostContext.Configuration.GetValue<string>("customers/shared:rabbitmq_password");
                                cfg.Host(new Uri(rabbitUri), "/", h =>
                                {
                                    h.Username(username);
                                    h.Password(password);
                                });
                            }
                            else
                            {
                                cfg.Host("dev-alhardynet-rabbitmq", 5672, "/", h =>
                                {
                                    h.Username("guest");
                                    h.Password("guest");
                                });
                                cfg.ConfigureEndpoints(context);
                            }
                        });
                    });

                    services.AddMassTransitHostedService(true);
                });
        }
    }
}