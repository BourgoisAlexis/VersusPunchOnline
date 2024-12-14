using System;
using System.Net;
using System.Threading.Tasks;

public class UDPGuest : UDPComponent{
    public UDPGuest(UDPConnection connection) {
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

    public void CloseConnection() {
        _netManager?.Stop();
        _running = false;
    }

    public void SendMessage(object obj) {
        _connection.SendMessage(obj, _netManager, _writer);
    }
}
