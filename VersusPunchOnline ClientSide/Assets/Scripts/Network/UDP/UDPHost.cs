using System.Net;
using System.Threading.Tasks;

public class UDPHost<T> : UDPComponent<T> where T : SimpleMessage {
    #region Variables
    private int _guestLimit;
    #endregion


    public UDPHost(int guestLimit, UDPConnection<T> connection) {
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
}