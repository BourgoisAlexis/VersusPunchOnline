using LiteNetLib;
using LiteNetLib.Utils;

public class GameplayUDPConnection : UDPConnection<InputMessage> {
    public override void SendMessage(object obj, NetManager netManager, NetDataWriter writer) {
        if (netManager.ConnectedPeersCount <= 0 || writer == null)
            return;

        string message = obj.ToString();
        writer.Reset();
        writer.Put(message);

        foreach (NetPeer peer in netManager.ConnectedPeerList)
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    public override void ReadMessage(NetPeer fromPeer, NetPacketReader dataReader, byte deliveryMethod, DeliveryMethod channel) {
        string json = dataReader.GetString(150);
        dataReader.Recycle();

        OnMessageReceived?.Invoke(SimpleMessage.FromString<InputMessage>(json));
    }
}
