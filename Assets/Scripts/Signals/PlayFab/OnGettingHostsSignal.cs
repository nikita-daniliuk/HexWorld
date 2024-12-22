using System.Collections.Generic;

public class OnGettingHostsSignal
{
    public readonly Dictionary<string, object> Hosts = new Dictionary<string, object>();

    public OnGettingHostsSignal(Dictionary<string, object> Hosts)
    {
        this.Hosts = new Dictionary<string, object>(Hosts);
    }
}