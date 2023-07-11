using Bit.Core.Tools.Queries;
using Bit.Core.Tools.Queries.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Bit.Core.Tools;

public static class ToolsServiceCollectionExtensions
{
    public static void AddToolsServices(this IServiceCollection services)
    {
        // TODO Add other Tools services/commands/queries
        services.AddReportsQueries();
    }
    
    private static void AddReportsQueries(this IServiceCollection services)
    {
        services.AddScoped<IGetInactiveTwoFactorQuery, GetInactiveTwoFactorQuery>();
    }
}
