using Mapster;
using NoResourcesRPG.Server.Mappers;
using NoResourcesRPG.Server.Services;

namespace NoResourcesRPG.Server;
public static class ProgramExtentions
{
    public static IServiceCollection AddServerServices(this IServiceCollection services)
    {
        return services
            // Add Scoped services
            // Add singleton services
            .AddSingleton<GameWorldService>()
            // Add hosted services
            .AddHostedService<GameLoopService>()
         ;
    }
}
