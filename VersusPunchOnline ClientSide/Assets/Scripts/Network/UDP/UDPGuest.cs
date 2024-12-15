using System;
using System.Net;

public class UDPGuest<T> : UDPPeer<T> where T : PeerMessage {
    public UDPGuest(UDPConnection<T> connection) {
        _connection = connection;
    }

    public void Connect(IPEndPoint iPEndPoint, Action onSuccess) {
        PeerInit();

        _netManager.Start();
        _netManager.Connect(iPEndPoint.Address.ToString(), iPEndPoint.Port, "");

        onSuccess?.Invoke();

        ReadLoop();
    }
}
