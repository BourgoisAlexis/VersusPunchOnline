using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;

public abstract class PeerConnection<T> where T : PeerMessage {
    public Action<T> OnMessageReceived;

    protected Stopwatch _swSend;
    protected Stopwatch _swReceived;


    public abstract void Init(Action<IPEndPoint> onInitialized, bool host = false, int guestLimit = 1);
    public abstract void CloseConnection();
    public abstract void Connect(IPEndPoint iPEndPoint, Action onSuccess);
    public abstract void SendMessage(object obj);


    protected string GetIPAddress() {
        string result = string.Empty;

        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
            if (ip.AddressFamily == AddressFamily.InterNetwork) {
                result = ip.ToString();
                break;
            }

        if (string.IsNullOrEmpty(result))
            Utils.LogError($"IPAdress not found");

        return result;
    }

    protected int GetPort() {
        TcpListener l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        int result = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();

        return result;
    }
}
