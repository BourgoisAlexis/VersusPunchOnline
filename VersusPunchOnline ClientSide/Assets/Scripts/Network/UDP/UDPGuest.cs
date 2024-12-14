using System;
using System.Net;
using System.Threading.Tasks;

public class UDPGuest<T> : UDPComponent<T> where T : SimpleMessage {
    public UDPGuest(UDPConnection<T> connection) {
        _connection = connection;
    }

    public async void Connect(IPEndPoint iPEndPoint, Action onSuccess) {
        CommonInit();

        _netManager.Start();
        _netManager.Connect(iPEndPoint.Address.ToString(), iPEndPoint.Port, "");

        _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) => {
            _connection.ReadMessage(fromPeer, dataReader, deliveryMethod, channel);
        };

        onSuccess?.Invoke();

        while (_running) {
            _netManager.PollEvents();
            await Task.Delay(AppConst.pollRate);
        }
    }
}
