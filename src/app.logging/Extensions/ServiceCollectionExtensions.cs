using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;

namespace app.logging.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSerilog(this IServiceCollection services, IConfiguration configuration)
    {
        var levelSwitch = new LoggingLevelSwitch();
        
        services.AddSerilog((_, lc) => lc
            .ReadFrom.Configuration(configuration)
            .MinimumLevel.ControlledBy(levelSwitch)
            .Enrich.WithProperty("AssemblyVersion",
                Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown AssemblyVersion"));

        return services;
    }
}