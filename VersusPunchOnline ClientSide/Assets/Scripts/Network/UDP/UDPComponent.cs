using LiteNetLib.Utils;
using LiteNetLib;

public class UDPComponent<T> where T : SimpleMessage {
    protected UDPConnection<T> _connection;
    protected EventBasedNetListener _listener;
    protected NetManager _netManager;
    protected NetDataWriter _writer;
    protected bool _running = false;

    protected void CommonInit() {
        _running = true;

        _listener = new EventBasedNetListener();
        _netManager = new NetManager(_listener);
        _writer = new NetDataWriter();
    }

    public void CloseConnection() {
        _netManager?.Stop();
        _running = false;
    }

    public void SendMessage(object obj) {
        _connection.SendMessage(obj, _netManager, _writer);
    }
}
