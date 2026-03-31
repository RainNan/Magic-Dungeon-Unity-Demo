public class Proxy : IProxy
{
    public virtual string ProxyName { get; protected set; }
    public virtual object Data { get; set; }

    public Proxy(string proxyName, object data = null)
    {
        ProxyName = proxyName;
        Data = data;
    }
}