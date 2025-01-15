using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.Threading.Tasks;
using System;
using UnityEngine;
using System.IO;

public abstract class TCPConnection<T> : PeerConnection<T> where T : PeerMessage {
    private TCPHost<T> _tcpHost;
    private TCPGuest<T> _tcpGuest;
    private bool _running = false;


    public override void Init(Action<IPEndPoint> onInitialized, bool host = false, int guestLimit = 1) {
        _running = true;
        _swSend = Stopwatch.StartNew();
        _swReceived = Stopwatch.StartNew();

        IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(GetIPAddress()), GetPort());

        if (host) {
            _tcpHost = new TCPHost<T>(guestLimit, this);
            _tcpHost.OpenConnection(iPEndPoint);
        }

        onInitialized?.Invoke(iPEndPoint);
    }

    public override void Connect(IPEndPoint iPEndPoint, Action onSuccess) {
        _tcpGuest = new TCPGuest<T>(this);
        _tcpGuest.Connect(iPEndPoint, onSuccess);
    }

    public override void CloseConnection() {
        _tcpHost?.CloseConnection();
        _tcpGuest?.CloseConnection();

        _running = false;
    }


    public override void SendMessage(object obj) {
        _tcpHost?.SendMessage(obj);  //Sends message as a server
        _tcpGuest?.SendMessage(obj); //Sends message as a client
    }

    public virtual async Task SendMessage(object obj, TcpClient[] clients) {
        if (clients.Length <= 0)
            throw new Exception("No client connected");

        try {
            string json = JsonUtility.ToJson(obj);
            byte[] dataArray = System.Text.Encoding.UTF8.GetBytes(json);
            int dataLength = dataArray.Length;
            byte[] lengthPrefix = BitConverter.GetBytes(dataLength);

            foreach (TcpClient client in clients) {
                NetworkStream stream = client.GetStream();

                await stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);
                await stream.WriteAsync(dataArray, 0, dataArray.Length);
            }
        }
        catch (Exception ex) {
            Utils.LogError($"{ex.Message}");
            throw ex;
        }
    }

    public async void ReadLoop(TcpClient client) {
        NetworkStream stream = client.GetStream();

        while (_running && client.Connected) {
            try {
                await ReadMessage(stream);
            }
            catch (Exception ex) {
                Utils.LogError($"{_running} {ex.Message}");
            }
        }
    }

    protected virtual async Task ReadMessage(NetworkStream stream) {
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
            OnMessageReceived?.Invoke(PeerMessage.FromString<T>(json));
        }
        catch (Exception ex) {
            throw ex;
        }
    }
}