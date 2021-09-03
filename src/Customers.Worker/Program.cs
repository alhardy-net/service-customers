using System;
using System.Threading.Tasks;
using Customers.Persistence;
using Customers.Worker.Components.Consumers;
using Customers.Worker.HealthChecks;
using Customers.Worker.Infrastructure;
using MassTransit;
using MassTransit.PrometheusIntegration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using Prometheus.SystemMetrics;
using Serilog;
using Serilog.Formatting.Json;

namespace Customers.Worker
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(new JsonFormatter())
                .CreateBootstrapLogger();

            Log.Information("Starting up");

            try
            {
                var metricsServer = new MetricServer(80);
                metricsServer.Start();
                
                await CreateHostBuilder(args).Build().RunAsync();

                Log.Information("Stopped");

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "An unhandled exception occured during bootstrapping");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(new JsonFormatter()))
                .ConfigureAppConfiguration((context, config) => { context.AddAwsSecretsManager(config); })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHealthChecks()
                        .AddCheck<TestHealthCheck>("test_health_check")
                        .ForwardToPrometheus();
                    
                    services.AddOpenTelemetryTracing(builder =>
                    {
                        builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(hostContext.Configuration["SERVICE_NAME"]));
                        builder.AddAWSInstrumentation();
                        builder.AddMassTransitInstrumentation();
                        builder.AddEntityFrameworkCoreInstrumentation();

                        if (hostContext.HostingEnvironment.IsDevelopment())
                        {
                            builder.AddConsoleExporter();
                        }
                        else
                        {
                            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
                            builder.AddOtlpExporter();
                        }
                    });
                    
                    services.AddSystemMetrics();
                    services.AddMemoryCache();
                    services.AddHostedService<PingCheckHostedService>();
                    services.AddHostedService<HealthCheckHostedService>();

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
                        x.AddConsumersFromNamespaceContaining<DeleteCustomerConsumer>();

                        x.UsingRabbitMq((context, cfg) =>
                        {
                            cfg.UsePrometheusMetrics(serviceName:hostContext.Configuration["SERVICE_NAME"]);
                            if (!hostContext.HostingEnvironment.IsDevelopment())
                            {
                                var rabbitUri = hostContext.Configuration.GetConnectionString("RabbitMQ");
                                var username =
                                    hostContext.Configuration.GetValue<string>("customers/shared:rabbitmq_username");
                                var password =
                                    hostContext.Configuration.GetValue<string>("customers/shared:rabbitmq_password");
                                cfg.Host(new Uri(rabbitUri), h =>
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
                            }

                            cfg.ConfigureEndpoints(context);
                        });
                    });

                    services.AddMassTransitHostedService(true);
                });
        }
    }
}