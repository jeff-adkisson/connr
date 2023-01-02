namespace Connr.App.Services;

public class NotificationService
{
    public event Action? OnAppStopping;
    
    public void NotifyStopping()
    {
        OnAppStopping?.Invoke();
    }
}