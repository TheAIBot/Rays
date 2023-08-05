using Microsoft.Extensions.DependencyInjection;

namespace WorkProgress;

public static class WorkProgressExtensions
{
    public static IServiceCollection AddWorkReporting(this IServiceCollection services)
    {
        services.AddSingleton<IWorkReporting, WorkReporting>();

        return services;
    }
}