using Microsoft.Extensions.DependencyInjection;

namespace Connr.Process;

public static class ConfigureProcessService
{
    public static void AddProcessService(this IServiceCollection services)
    {
        services.AddSingleton<ProcessService>();
    }
}