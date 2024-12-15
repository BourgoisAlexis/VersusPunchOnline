using System;
using System.Diagnostics;
using System.Net;
using LiteNetLib;
using LiteNetLib.Utils;

public abstract class UDPConnection<T> : PeerConnection<T> where T : PeerMessage {
    protected UDPHost<T> _udpHost;
    protected UDPGuest<T> _udpGuest;


    //https://github.com/RevenantX/LiteNetLib?tab=readme-ov-file
    public override void Init(Action<IPEndPoint> onInitialized, bool host = false, int guestLimit = 1) {
        _swSend = Stopwatch.StartNew();
        _swReceived = Stopwatch.StartNew();

        IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(GetIPAddress()), GetPort());

        if (host) {
            _udpHost = new UDPHost<T>(guestLimit, this);
            _udpHost.OpenConnection(iPEndPoint);
        }

        onInitialized?.Invoke(iPEndPoint);
    }

    public override void CloseConnection() {
        _udpHost?.CloseConnection();
        _udpGuest?.CloseConnection();
    }

    public override void Connect(IPEndPoint iPEndPoint, Action onSuccess) {
        _udpGuest = new UDPGuest<T>(this);
        _udpGuest.Connect(iPEndPoint, onSuccess);
    }


    public override void SendMessage(object obj) {
        _udpHost?.SendMessage(obj);  //Sends message as a server
        _udpGuest?.SendMessage(obj); //Sends message as a client
    }

    public virtual void SendMessage(object obj, NetManager netManager, NetDataWriter writer) {
        if (netManager.ConnectedPeersCount <= 0 || writer == null)
            return;

        string message = obj.ToString();
        writer.Reset();
        writer.Put(message);

        foreach (NetPeer peer in netManager.ConnectedPeerList)
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    public virtual void ReadMessage(NetPeer fromPeer, NetPacketReader dataReader, byte deliveryMethod, DeliveryMethod channel) {
        string json = dataReader.GetString(150);
        dataReader.Recycle();

        OnMessageReceived?.Invoke(PeerMessage.FromString<T>(json));
    }
}
