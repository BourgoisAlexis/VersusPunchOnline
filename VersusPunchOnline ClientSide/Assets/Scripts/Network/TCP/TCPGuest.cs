using System;
using System.Net;
using System.Net.Sockets;

public class TCPGuest {
    private TCPConnection _connection;
    private TcpClient _host;


    public TCPGuest(TCPConnection connection) {
        _connection = connection;
    }


    public async void Connect(IPEndPoint iPEndPoint, Action onSuccess) {
        TcpClient client = new TcpClient();
        await client.ConnectAsync(iPEndPoint.Address, iPEndPoint.Port);
        onSuccess?.Invoke();

        _host = client;
        _connection.ReadLoop(client);
    }

    public void CloseConnection() {
        if (_host.Connected)
            _host.GetStream().Close();

        _host.Close();

        _host = null;
    }

    public void SendMessage(object obj) {
        _connection.SendMessage(obj, _host);
    }
}
