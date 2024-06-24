using System.Net.Sockets;
using System.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using UnityEngine;
using System.Threading.Tasks;

public class PeerToPeerManager<T> where T : class {
    private TcpListener _server = null;
    private List<TcpClient> _clients = new List<TcpClient>();
    public Action<T> onMessageReceived;

    private bool _closed = true;

    public void CloseConnection() {
        _server.Server.Close();
        _server.Stop();

        foreach (TcpClient client in _clients) {
            client.GetStream().Close();
            client.Close();
        }

        _clients.Clear();

        _closed = true;
    }

    public void Setup(Action<IPEndPoint> onConnectionOpened) {
        _closed = false;
        IPAddress ip = IPAddress.Parse(GetLocalIPAddress());
        int port = GetPort();

        IPEndPoint iPEndPoint = new IPEndPoint(ip, port);

        OpenConnection(iPEndPoint);

        onConnectionOpened?.Invoke(iPEndPoint);

        Utils.Log(this, "Setup", "Connected");
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

                if (_clients.Count > 0)
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

        Stopwatch sw = new Stopwatch();
        NetworkStream stream = client.GetStream();

        while (!_closed && client.Connected) {
            sw.Restart();

            try {
                byte[] lengthPrefix = new byte[4];
                await stream.ReadAsync(lengthPrefix, 0, lengthPrefix.Length);
                int dataLength = BitConverter.ToInt32(lengthPrefix, 0);

                byte[] dataArray = new byte[dataLength];
                int bytesRead = 0;

                while (bytesRead < dataLength) {
                    int read = await stream.ReadAsync(dataArray, bytesRead, dataLength - bytesRead);
                    if (read == 0)
                        throw new EndOfStreamException("Connection closed before full message was received.");
                    bytesRead += read;
                }

                string json = System.Text.Encoding.UTF8.GetString(dataArray);

                try {
                    T data = JsonUtility.FromJson<T>(json);
                    onMessageReceived?.Invoke(data);
                }
                catch (Exception ex) {
                    Utils.LogError(this, "ReadMessage", ex.Message);
                }
            }
            catch (ObjectDisposedException ex) {
                Utils.LogError(this, "ReadMessage", ex.Message);
            }

            sw.Stop();
            Utils.Log(this, "Reading Time", $"{sw.ElapsedMilliseconds}");
        }
    }

    private string GetLocalIPAddress() {
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


    public async Task SendMessage(object obj) {
        if (_clients.Count <= 0)
            throw new Exception("No client");

        Stopwatch sw = new Stopwatch();
        sw.Restart();

        try {
            string json = JsonUtility.ToJson(obj);
            byte[] dataArray = System.Text.Encoding.UTF8.GetBytes(json);
            int dataLength = dataArray.Length;
            byte[] lengthPrefix = BitConverter.GetBytes(dataLength);

            foreach (TcpClient client in _clients) {
                NetworkStream stream = client.GetStream();

                await stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);
                await stream.WriteAsync(dataArray, 0, dataArray.Length);
            }
        }
        catch (Exception ex) {
            Utils.LogError(this, "SendMessage", ex.Message);
            throw ex;
        }

        sw.Stop();
        Utils.Log(this, "Writing Time", $"{sw.ElapsedMilliseconds}");
    }
}