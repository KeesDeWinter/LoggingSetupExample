
using Serilog;
using Serilog.Events;

namespace LoggingExampleBase
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // based on https://github.com/serilog/serilog-aspnetcore

            // initial logger, 
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            Log.Information("Starting up");

            try
            {
                var builder = WebApplication.CreateBuilder(args);

                // This replaces the initial logger.
                builder.Host.UseSerilog((context, services, configuration) => configuration
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.Seq("http://localhost:5341"));

                // Add services to the container.

                builder.Services.AddControllers();
                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                var app = builder.Build();

                // Serilog includes middleware for smarter HTTP request logging. The default request logging implemented
                // by ASP.NET Core is noisy, with multiple events emitted per request. The included middleware condenses
                // these into a single event that carries method, path, status code, and timing information.
                // Add the following line, and ".MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)"
                app.UseSerilogRequestLogging(); 

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();

                app.UseAuthorization();

                app.MapControllers();

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}