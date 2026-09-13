using OrderFlow.Application.Features.Orders.CreateOrder;
using OrderFlow.Infrastructure;
namespace OrderFlow.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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

            app.MapControllers();

            app.Run();
        }
    }
}
