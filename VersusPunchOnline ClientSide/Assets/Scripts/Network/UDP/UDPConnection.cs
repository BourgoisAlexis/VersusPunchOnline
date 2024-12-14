using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using LiteNetLib.Utils;

public abstract class UDPConnection {
    #region Variables
    public Action<SimpleMessage> OnMessageReceived;

    protected Stopwatch _swSend;
    protected Stopwatch _swReceived;
    protected UDPHost _udpHost;
    protected UDPGuest _udpGuest;
    #endregion


    //https://github.com/RevenantX/LiteNetLib?tab=readme-ov-file
    public void Init(Action<IPEndPoint> onInitialized, bool host = false, int guestLimit = 1) {
        _swSend = Stopwatch.StartNew();
        _swReceived = Stopwatch.StartNew();

        IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(GetIPAddress()), GetPort());

        if (host) {
            _udpHost = new UDPHost(guestLimit, this);
            _udpHost.OpenConnection(iPEndPoint);
        }

        onInitialized?.Invoke(iPEndPoint);
    }

    public void CloseConnection() {
        _udpHost?.CloseConnection();
        _udpGuest?.CloseConnection();
    }

    private string GetIPAddress() {
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

    private int GetPort() {
        TcpListener l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        int result = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();

        return result;
    }

    public void Connect(IPEndPoint iPEndPoint, Action onSuccess) {
        _udpGuest = new UDPGuest(this);
        _udpGuest.Connect(iPEndPoint, onSuccess);
    }

    public void SendMessage(object obj) {
        _udpHost?.SendMessage(obj);  //Sends message as a server
        _udpGuest?.SendMessage(obj); //Sends message as a client
    }

    public abstract void SendMessage(object obj, NetManager netManager, NetDataWriter writer);

    public abstract void ReadMessage(NetPeer fromPeer, NetPacketReader dataReader, byte deliveryMethod, DeliveryMethod channel);
}
