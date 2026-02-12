using Serilog;
using Zapper.LoyaltyPoints.Api.Extensions;
using Zapper.LoyaltyPoints.Application;
using Zapper.LoyaltyPoints.Infrastructure;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}")
    .Enrich.FromLogContext()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}"));

var messagingProvider = builder.Configuration["AppSettings:Messaging:Provider"] ?? "";
Log.Information("Messaging provider: {Provider}", messagingProvider);
Log.Information("Environment: {Env}", builder.Environment.EnvironmentName);
    
builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddApplicationServices(messagingProvider)
    .AddApiServices(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseMiddlewarePipeline();
app.MapEndpoints();

app.Run();

namespace Zapper.LoyaltyPoints.Api.Queries
{
    public partial class Program;
}
