namespace Connr.Services;

public static class ConfigureNotificationService
{
    public static void AddNotificationService(this IServiceCollection services)
    {
        services.AddSingleton<NotificationService>();
    }
}