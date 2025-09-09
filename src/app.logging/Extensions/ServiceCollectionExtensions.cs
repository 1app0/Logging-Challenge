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
            .ReadFrom.Services(servicesProvider));

        return services;
    }
}