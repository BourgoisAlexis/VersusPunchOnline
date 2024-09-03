using LiteNetLib;
using System;
using UnityEngine;

public class UDPTester : UDPConnectionManager {
    public Action<FrameInfo> onMessageReceived;

    public override void SendMessage(object obj) {
        if (_host.ConnectedPeersCount <= 0 || _writer == null)
            return;

        string json = JsonUtility.ToJson(obj);
        _writer.Reset();
        _writer.Put(json);

        foreach (NetPeer peer in _host.ConnectedPeerList) {
            peer.Send(_writer, DeliveryMethod.ReliableOrdered);
            //Utils.Log(this, "Ping", $"{peer.Ping}");
        }
    }

    protected override void ReadMessage(NetPeer fromPeer, NetPacketReader dataReader, byte deliveryMethod, DeliveryMethod channel) {
        string json = dataReader.GetString(100);
        Utils.Log(this, json);
        dataReader.Recycle();

        onMessageReceived?.Invoke(FrameInfo.FromString(json));
    }
}
