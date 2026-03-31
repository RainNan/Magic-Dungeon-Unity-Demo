using System;
using System.Collections.Generic;

public class Facade : Notifier
{
    private static Facade _instance;
    public static Facade Instance => _instance ??= new Facade();

    private readonly Dictionary<string, Func<ICommand>> _commandMap = new();
    private readonly Dictionary<string, IMediator> _mediatorMap = new();
    private readonly Dictionary<string, IProxy> _proxyMap = new();

    public void RegisterCommand(string notificationName, Func<ICommand> commandFactory)
    {
        if (!_commandMap.ContainsKey(notificationName))
        {
            _commandMap.Add(notificationName, commandFactory);
        }
    }

    public void RegisterMediator(IMediator mediator)
    {
        if (!_mediatorMap.ContainsKey(mediator.MediatorName))
        {
            _mediatorMap.Add(mediator.MediatorName, mediator);
        }
    }

    public void RegisterProxy(IProxy proxy)
    {
        if (!_proxyMap.ContainsKey(proxy.ProxyName))
        {
            _proxyMap.Add(proxy.ProxyName, proxy);
        }
    }

    public T RetrieveProxy<T>(string proxyName) where T : class, IProxy
    {
        if (_proxyMap.TryGetValue(proxyName, out var proxy))
        {
            return proxy as T;
        }

        return null;
    }

    public T RetrieveMediator<T>(string mediatorName) where T : class, IMediator
    {
        if (_mediatorMap.TryGetValue(mediatorName, out var mediator))
        {
            return mediator as T;
        }

        return null;
    }

    public void SendNotification(string notificationName, object body = null)
    {
        var notification = new Notification(notificationName, body);

        if (_commandMap.TryGetValue(notificationName, out var commandFactory))
        {
            ICommand command = commandFactory.Invoke();
            command.Execute(notification);
        }

        foreach (var mediator in _mediatorMap.Values)
        {
            var interests = mediator.ListNotificationInterests();
            if (interests.Contains(notificationName))
            {
                mediator.HandleNotification(notification);
            }
        }
    }
}