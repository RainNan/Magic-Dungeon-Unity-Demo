public class Notification : INotification
{
    public string Name { get; private set; }
    public object Body { get; private set; }

    public Notification(string name, object body = null)
    {
        Name = name;
        Body = body;
    }
}