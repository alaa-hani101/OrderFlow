using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OrderFlow.Application.Features.Orders.CreateOrder;
using OrderFlow.Infrastructure;
namespace OrderFlow.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            builder.Services.AddOpenTelemetry()
               .ConfigureResource(resource =>
                   resource.AddService(
                       serviceName: "OrderFlow.API",
                       serviceVersion: "1.0.0"))

               .WithTracing(tracing =>
               {
                   tracing.AddAspNetCoreInstrumentation();
                   tracing.AddEntityFrameworkCoreInstrumentation();
                   tracing.AddSource("OrderFlow");

                   tracing.AddOtlpExporter();
               })

               .WithMetrics(metrics =>
               {
                   metrics.AddAspNetCoreInstrumentation();
                   metrics.AddMeter("OrderFlow");

                   metrics.AddPrometheusExporter();
               });

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddInfrastructure(
                builder.Configuration);

            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(
                    typeof(CreateOrderCommand).Assembly);
            });

            // Replace AddOpenApi with Swashbuckle setup:
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                // Replace MapOpenApi with the Swashbuckle middleware:
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapPrometheusScrapingEndpoint();
            app.MapHealthChecks("/health");

            app.MapControllers();

            app.Run();
        }
    }
}
