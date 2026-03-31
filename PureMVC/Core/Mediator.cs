using System.Collections.Generic;

public class Mediator : Notifier, IMediator
{
    public virtual string MediatorName { get; protected set; }
    public virtual object ViewComponent { get; set; }

    public Mediator(string mediatorName, object viewComponent = null)
    {
        MediatorName = mediatorName;
        ViewComponent = viewComponent;
    }

    public virtual List<string> ListNotificationInterests()
    {
        return new List<string>();
    }

    public virtual void HandleNotification(INotification notification)
    {
    }
}