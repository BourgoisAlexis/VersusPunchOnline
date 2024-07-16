using System.Net.Sockets;
using System.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

public abstract class BasicP2P {
    #region Variables
    protected Stopwatch _swSend;
    protected Stopwatch _swReceived;
    protected List<TcpClient> _clients = new List<TcpClient>();
    protected int _maxClients = 1;

    private TcpListener _server = null;
    private bool _closed = true;
    #endregion


    public void CloseConnection() {
        int i = 0;

        Utils.Log(this, "CloseConnection", $"{i++}");
        _server.Server.Close();
        Utils.Log(this, "CloseConnection", $"{i++}");
        _server.Stop();

        Utils.Log(this, "CloseConnection", $"{i++}");
        foreach (TcpClient client in _clients) {
            if (client.Connected)
                client.GetStream().Close();

            client.Close();
        }

        Utils.Log(this, "CloseConnection", $"{i++}");
        _clients.Clear();

        Utils.Log(this, "CloseConnection", $"{i++}");
        _closed = true;

        Utils.Log(this, "CloseConnection", $"{i++}");
    }

    public void Setup(Action<IPEndPoint> onConnectionOpened) {
        _closed = false;
        IPAddress ip = IPAddress.Parse(GetIPAddress());
        int port = GetPort();

        IPEndPoint iPEndPoint = new IPEndPoint(ip, port);

        OpenConnection(iPEndPoint);

        onConnectionOpened?.Invoke(iPEndPoint);

        _swSend = Stopwatch.StartNew();
        _swReceived = Stopwatch.StartNew();
    }

    private void OpenConnection(IPEndPoint iPEndPoint) {
        _server = new TcpListener(iPEndPoint);
        _server.Start();

        WaitingForConnections();
    }

    private async void WaitingForConnections() {
        while (!_closed) {
            try {
                TcpClient client = await _server.AcceptTcpClientAsync();
                _clients.Add(client);

                Utils.Log(this, "Connection", client.Client.RemoteEndPoint.ToString());

                if (_clients.Count >= _maxClients)
                    return;
            }
            catch (ObjectDisposedException ex) {
                Utils.LogError(this, "WaitingForConnections", ex.Message);
                break;
            }
        }
    }

    public async void Connect(IPEndPoint iPEndPoint, Action onSuccess) {
        using TcpClient client = new TcpClient();
        //Connect to a remote host
        await client.ConnectAsync(iPEndPoint.Address, iPEndPoint.Port);
        onSuccess?.Invoke();

        NetworkStream stream = client.GetStream();

        while (!_closed && client.Connected) {
            try {
                await ReadMessage(stream);
            }
            catch (Exception ex) {
                Utils.LogError(this, "ReadMessage", $"{_closed} {ex.Message}");
            }
        }
    }


    private string GetIPAddress() {
        string result = string.Empty;

        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
            if (ip.AddressFamily == AddressFamily.InterNetwork) {
                result = ip.ToString();
                break;
            }

        if (string.IsNullOrEmpty(result))
            Utils.LogError(this, "GetLocalIPAddress", $"ip address not found");

        Utils.Log(this, "GetLocalIPAddress", $"{result}");
        return result;
    }

    private int GetPort() {
        TcpListener l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        int port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();

        Utils.Log(this, "GetPort", $"{port}");
        return port;
    }


    protected abstract Task ReadMessage(NetworkStream stream);

    public abstract Task SendMessage(object obj);
}