using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace app.logging.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSerilog(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog((servicesProvider, lc) => lc
            .ReadFrom.Configuration(configuration)
            .Enrich.WithProperty("AssemblyVersion",
                Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown AssemblyVersion"));

        return services;
    }
}