using LiteNetLib;
using System;

public class UDPGameplay : UDPConnectionManager {
    public Action<InputMessage> onMessageReceived;

    public override void SendMessage(object obj) {
        if (_host.ConnectedPeersCount <= 0 || _writer == null)
            return;

        string message = obj.ToString();
        _writer.Reset();
        _writer.Put(message);

        foreach (NetPeer peer in _host.ConnectedPeerList)
            peer.Send(_writer, DeliveryMethod.ReliableOrdered);

        Utils.Log(this, $"{message}");
    }

    protected override void ReadMessage(NetPeer fromPeer, NetPacketReader dataReader, byte deliveryMethod, DeliveryMethod channel) {
        string json = dataReader.GetString(150);
        dataReader.Recycle();
        Utils.Log(this, json);

        onMessageReceived?.Invoke(InputMessage.FromString(json));
    }
}
