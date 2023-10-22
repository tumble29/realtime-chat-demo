namespace SignalRChatDemo.Models;

//Reference: https://learn.microsoft.com/en-us/aspnet/signalr/overview/guide-to-the-api/mapping-users-to-connections
public class ConnectionMapping<T>
{
    private readonly Dictionary<T, HashSet<string>> _connections = new Dictionary<T, HashSet<string>>();

    public int Count
    {
        get { return _connections.Count; }
    }

    public void Add(T key, string connectedId)
    {
        //lock to prevent race condition
        lock (_connections)
        {
            HashSet<string>? connections;
            if (!_connections.TryGetValue(key, out connections))
            {
                connections = new HashSet<string>();
                _connections.Add(key, connections);
            }
            lock (connections)
            {
                connections.Add(connectedId);
            }
        }
    }

    public IEnumerable<string> GetConnections(T key)
    {
        HashSet<string>? connections;
        if (_connections.TryGetValue(key, out connections))
        {
            return connections;
        }
        return Enumerable.Empty<string>();
    }

    public IEnumerable<T> GetUsers()
    {
        return _connections.Keys.ToList();
    }

    public void Remove(T key, string connectionId)
    {
        lock (_connections)
        {
            HashSet<string>? connections;
            if (!_connections.TryGetValue(key, out connections))
            {
                return;
            }
            lock (connections)
            {
                connections.Remove(connectionId);
                if (connections.Count == 0)
                {
                    _connections.Remove(key);
                }
            }
        }
    }
}
