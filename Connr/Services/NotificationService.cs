namespace Connr.Services;

public class NotificationService
{
    public event Action OnAppStopping;
    
    public void NotifyStopping()
    {
        OnAppStopping?.Invoke();
    }
}