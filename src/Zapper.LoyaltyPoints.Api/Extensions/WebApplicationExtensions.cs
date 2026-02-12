using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using Zapper.LoyaltyPoints.Api.HealthChecks;
using Zapper.LoyaltyPoints.Api.Middleware;
using Zapper.LoyaltyPoints.Infrastructure.Data;
using Zapper.LoyaltyPoints.Infrastructure.Persistence;

namespace Zapper.LoyaltyPoints.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseMiddlewarePipeline(this WebApplication app)
    {
        app.UseMiddleware<RequestHandlingMiddleware>();
        app.UseMiddleware<ErrorHandlingMiddleware>();
        app.UseSerilogRequestLogging();

        return app;
    }

    public static WebApplication MapEndpoints(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.MapControllers();
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResponseWriter = HealthCheckResponseWriter.WriteResponse
        });
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = HealthCheckResponseWriter.WriteResponse
        });
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteResponse
        });

        return app;
    }

    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LoyaltyDbContext>();

        if (db.Database.IsRelational())
        {
            await db.Database.MigrateAsync();
        }
        else
        {
            await db.Database.EnsureCreatedAsync();
        }

        if (app.Environment.IsDevelopment())
        {
            await DataSeeder.SeedAsync(db);
        }
    }
}
