using System.Net;
using System.Threading.Tasks;

public class UDPHost : UDPComponent {
    #region Variables
    private int _guestLimit;
    #endregion


    public UDPHost(int guestLimit, UDPConnection connection) {
        _guestLimit = guestLimit;
        _connection = connection;
    }


    public async void OpenConnection(IPEndPoint iPEndPoint) {
        CommonInit();
        _netManager.Start(iPEndPoint.Port);

        _listener.ConnectionRequestEvent += request => {
            if (_netManager.ConnectedPeersCount < _guestLimit)
                request.Accept();
            else
                request.Reject();
        };

        _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) => {
            _connection.ReadMessage(fromPeer, dataReader, deliveryMethod, channel);
        };

        while (_running) {
            _netManager.PollEvents();
            await Task.Delay(AppConst.pollRate);
        }
    }


    public void CloseConnection() {
        _netManager?.Stop();
        _running = false;
    }

    public void SendMessage(object obj) {
        _connection.SendMessage(obj, _netManager, _writer);
    }
}