using LiteNetLib.Utils;
using LiteNetLib;

public class UDPComponent {
    protected UDPConnection _connection;
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
}
