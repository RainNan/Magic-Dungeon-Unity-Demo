using System.Collections.Generic;
using UnityEngine.Playables;

public interface IMediator : INotifier
{
    string MediatorName { get; }
    object ViewComponent { get; set; }

    List<string> ListNotificationInterests();
    void HandleNotification(INotification notification);
}