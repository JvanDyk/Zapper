using Microsoft.EntityFrameworkCore;
using Serilog;
using Zapper.LoyaltyPoints.Application;
using Zapper.LoyaltyPoints.Infrastructure;
using Zapper.LoyaltyPoints.Infrastructure.Persistence;
using Zapper.LoyaltyPoints.Worker;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .Enrich.FromLogContext()
    .CreateBootstrapLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Logging.ClearProviders();
    builder.Services.AddSerilog((services, configuration) => configuration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"));
    
    var messagingProvider = builder.Configuration["AppSettings:Messaging:Provider"] ?? "";
    Log.Information("Messaging provider: {Provider}", messagingProvider);
    Log.Information("Environment: {Env}", builder.Environment.EnvironmentName);

    builder.Services
        .AddApplicationServices(messagingProvider)
        .AddInfrastructure(builder.Configuration);
        
    builder.Services.AddHostedService<SemaphoreSlimQueueWorker>();

    var host = builder.Build();

    // Run migrations before starting the worker to ensure database schema exists
    await MigrateDatabaseAsync(host.Services);
    
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Worker terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

static async Task MigrateDatabaseAsync(IServiceProvider services)
{
    const int maxRetries = 10;
    const int delayMs = 3000;

    for (var attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<LoyaltyDbContext>();
            await db.Database.MigrateAsync();
            Log.Information("Database migration completed successfully");
            return;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Database migration attempt {Attempt}/{MaxRetries} failed. Retrying in {DelayMs}ms...", attempt, maxRetries, delayMs);
            if (attempt == maxRetries)
            {
                Log.Error("Database migration failed after {MaxRetries} attempts. Worker will start and retry operations individually.", maxRetries);
                return;
            }
            await Task.Delay(delayMs);
        }
    }
}
