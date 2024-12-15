using LiteNetLib.Utils;
using LiteNetLib;
using System.Threading.Tasks;

public abstract class UDPPeer<T> where T : PeerMessage {
    #region Variables
    protected UDPConnection<T> _connection;
    protected EventBasedNetListener _listener;
    protected NetManager _netManager;
    protected NetDataWriter _writer;
    protected bool _running = false;
    #endregion


    protected void PeerInit() {
        _running = true;

        _listener = new EventBasedNetListener();
        _netManager = new NetManager(_listener);
        _writer = new NetDataWriter();
    }

    public void CloseConnection() {
        _netManager?.Stop();
        _running = false;
    }


    protected async void ReadLoop() {
        _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) => {
            ReadMessage(fromPeer, dataReader, deliveryMethod, channel);
        };

        while (_running) {
            _netManager.PollEvents();
            await Task.Delay(AppConst.POLL_RATE);
        }
    }

    public void SendMessage(object obj) {
        _connection.SendMessage(obj, _netManager, _writer);
    }

    private void ReadMessage(NetPeer fromPeer, NetPacketReader dataReader, byte deliveryMethod, DeliveryMethod channel) {
        _connection.ReadMessage(fromPeer, dataReader, deliveryMethod, channel);
    }
}
