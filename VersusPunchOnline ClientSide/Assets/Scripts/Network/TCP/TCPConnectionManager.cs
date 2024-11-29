using System.Net.Sockets;
using System.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

public abstract class TCPConnectionManager : ConnectionManager<IPEndPoint> {
    #region Variables
    protected Stopwatch _swSend;
    protected Stopwatch _swReceived;
    protected List<TcpClient> _guests = new List<TcpClient>();
    protected int _maxClients = 1;

    private TcpListener _host = null;
    private bool _closed = true;
    #endregion


    public override void Init(Action<IPEndPoint> onInit) {
        _closed = false;
        IPAddress ip = IPAddress.Parse(GetIPAddress());
        int port = GetPort();

        IPEndPoint iPEndPoint = new IPEndPoint(ip, port);

        OpenConnection(iPEndPoint);

        onInit?.Invoke(iPEndPoint);

        _swSend = Stopwatch.StartNew();
        _swReceived = Stopwatch.StartNew();
    }

    public override void CloseConnection() {
        _host.Server.Close();
        _host.Stop();

        foreach (TcpClient client in _guests) {
            if (client.Connected)
                client.GetStream().Close();

            client.Close();
        }

        _guests.Clear();
        _closed = true;
    }


    private void OpenConnection(IPEndPoint iPEndPoint) {
        _host = new TcpListener(iPEndPoint);
        _host.Start();

        WaitingForConnections();
    }

    private async void WaitingForConnections() {
        while (!_closed) {
            try {
                TcpClient client = await _host.AcceptTcpClientAsync();
                _guests.Add(client);

                Utils.Log(this, "WaitingForConnections", client.Client.RemoteEndPoint.ToString());

                if (_guests.Count >= _maxClients)
                    return;
            }
            catch (ObjectDisposedException ex) {
                Utils.LogError(this, ex.Message);
                break;
            }
        }
    }

    /// <summary>
    /// Connect to a host
    /// </summary>
    /// <param name="iPEndPoint"></param>
    /// <param name="onSuccess"></param>
    public override async void Connect(IPEndPoint iPEndPoint, Action onSuccess) {
        using TcpClient client = new TcpClient();
        await client.ConnectAsync(iPEndPoint.Address, iPEndPoint.Port);
        onSuccess?.Invoke();

        NetworkStream stream = client.GetStream();

        while (!_closed && client.Connected) {
            try {
                await ReadMessage(stream);
            }
            catch (Exception ex) {
                Utils.LogError(this, $"{_closed} {ex.Message}");
            }
        }
    }


    protected abstract Task ReadMessage(NetworkStream stream);

    public abstract Task SendMessage(object obj);
}