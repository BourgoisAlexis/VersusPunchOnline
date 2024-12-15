using System.Net;

public class UDPHost<T> : UDPPeer<T> where T : PeerMessage {
    #region Variables
    private int _guestLimit;
    #endregion


    public UDPHost(int guestLimit, UDPConnection<T> connection) {
        _guestLimit = guestLimit;
        _connection = connection;
    }


    public void OpenConnection(IPEndPoint iPEndPoint) {
        PeerInit();
        _netManager.Start(iPEndPoint.Port);

        _listener.ConnectionRequestEvent += request => {
            if (_netManager.ConnectedPeersCount < _guestLimit)
                request.Accept();
            else
                request.Reject();
        };

        ReadLoop();
    }
}