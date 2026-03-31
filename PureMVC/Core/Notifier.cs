public class Notifier : INotifier
{
    public void SendNotification(string notificationName, object body = null)
    {
        Facade.Instance.SendNotification(notificationName, body);
    }
}