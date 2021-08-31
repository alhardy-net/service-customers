using System;
using Customers.Api.HealthChecks;
using Customers.Persistence;
using MassTransit;
using MassTransit.Definition;
using MassTransit.PrometheusIntegration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Prometheus;

namespace Customers.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }

        public IHostEnvironment Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Customers.Api", Version = "v1" });
            });

            services.AddMemoryCache();

            var cache = new MemoryCache(new MemoryCacheOptions());
            services.AddDbContext<CustomersContext>(options =>
            {
                options.UseNpgsql(Configuration.GetConnectionString("CustomersContext"));

                if (!Environment.IsDevelopment()) options.AddInterceptors(new RdsAuthenticationInterceptor(cache));
            });

            services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);
            services.AddMassTransit(mt =>
            {
                mt.UsingRabbitMq((context, cfg) =>
                {
                    cfg.UsePrometheusMetrics(serviceName:"customers-api");
                    if (!Environment.IsDevelopment())
                    {
                        var rabbitUri = Configuration.GetConnectionString("RabbitMQ");
                        var username = Configuration.GetValue<string>("customers/shared:rabbitmq_username");
                        var password = Configuration.GetValue<string>("customers/shared:rabbitmq_password");
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
                });
            });
            services.AddMassTransitHostedService();
            services.AddHealthChecks()
                .AddCheck<TestHealthCheck>("test_health_check")
                .ForwardToPrometheus();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Customers.Api v1"));
            app.UseMetricServer();
            app.UseRouting();
            app.UseHttpMetrics();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/healthz", new HealthCheckOptions
                {
                    ResponseWriter = HealthCheckFormat.WriteResponse
                });
                endpoints.MapControllers();
                endpoints.MapMetrics();
            });
        }
    }
}