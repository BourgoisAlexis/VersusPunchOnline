using System.Net.Sockets;
using System.Net;
using System;

public abstract class ConnectionManager<T> {
    public abstract void Init(Action<T> onInit);
    public abstract void CloseConnection();
    /// <summary>
    /// Connect to a host
    /// </summary>
    /// <param name="iPEndPoint"></param>
    /// <param name="onSuccess"></param>
    public abstract void Connect(IPEndPoint iPEndPoint, Action onSuccess);

    protected string GetIPAddress() {
        string result = string.Empty;

        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
            if (ip.AddressFamily == AddressFamily.InterNetwork) {
                result = ip.ToString();
                break;
            }

        if (string.IsNullOrEmpty(result))
            Utils.LogError(this, "GetLocalIPAddress", $"ip address not found");

        Utils.Log(this, "GetLocalIPAddress", $"{result}");
        return result;
    }

    protected int GetPort() {
        TcpListener l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        int port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();

        Utils.Log(this, "GetPort", $"{port}");
        return port;
    }
}
