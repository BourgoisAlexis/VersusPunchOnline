using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System;

public class TCPHost<T> where T : PeerMessage {
    #region Variables
    private TCPConnection<T> _connection;
    private int _guestLimit;
    private TcpListener _listener;
    private List<TcpClient> _guests = new List<TcpClient>();
    private bool _running = false;
    #endregion


    public TCPHost(int guestLimit, TCPConnection<T> connection) {
        _guestLimit = guestLimit;
        _connection = connection;
    }


    public void OpenConnection(IPEndPoint iPEndPoint) {
        _running = true;
        _listener = new TcpListener(iPEndPoint);
        _listener.Start();

        WaitForConnectionLoop();
    }

    private async void WaitForConnectionLoop() {
        while (_running) {
            try {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                _guests.Add(client);
                _connection.ReadLoop(client);

                if (_guests.Count >= _guestLimit)
                    return;
            }
            catch (ObjectDisposedException ex) {
                Utils.LogError($"{ex.Message}");
                break;
            }
        }
    }

    public void CloseConnection() {
        _listener.Server.Close();
        _listener.Stop();

        foreach (TcpClient client in _guests) {
            if (client.Connected)
                client.GetStream().Close();

            client.Close();
        }

        _guests.Clear();
        _running = false;
    }

    public void SendMessage(object obj) {
        _connection.SendMessage(obj, _guests.ToArray());
    }
}
