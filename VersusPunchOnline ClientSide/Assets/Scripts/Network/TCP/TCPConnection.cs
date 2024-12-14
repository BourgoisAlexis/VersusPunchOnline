using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.Threading.Tasks;
using System;

public abstract class TCPConnection {
    #region Variables
    public Action<SimpleMessage> OnMessageReceived;

    protected Stopwatch _swSend;
    protected Stopwatch _swReceived;
    private TCPHost _tcpHost;
    private TCPGuest _tcpGuest;

    private bool _running = false;
    #endregion


    public void Init(Action<IPEndPoint> onInitialized, bool host = false, int guestLimit = 1) {
        _running = true;
        _swSend = Stopwatch.StartNew();
        _swReceived = Stopwatch.StartNew();

        IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(GetIPAddress()), GetPort());

        if (host) {
            _tcpHost = new TCPHost(guestLimit, this);
            _tcpHost.OpenConnection(iPEndPoint);
        }

        onInitialized?.Invoke(iPEndPoint);
    }

    public void Connect(IPEndPoint iPEndPoint, Action onSuccess) {
        _tcpGuest = new TCPGuest(this);
        _tcpGuest.Connect(iPEndPoint, onSuccess);
    }

    public void CloseConnection() {
        _tcpHost?.CloseConnection();
        _tcpGuest?.CloseConnection();

        _running = false;
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


    private string GetIPAddress() {
        string result = string.Empty;

        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
            if (ip.AddressFamily == AddressFamily.InterNetwork) {
                result = ip.ToString();
                break;
            }

        if (string.IsNullOrEmpty(result))
            Utils.LogError($"IPAdress not found");

        return result;
    }

    private int GetPort() {
        TcpListener l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        int result = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();

        return result;
    }


    public void SendMessage(object obj) {
        _tcpHost?.SendMessage(obj);  //Sends message as a server
        _tcpGuest?.SendMessage(obj); //Sends message as a client
    }

    public abstract Task SendMessage(object obj, params TcpClient[] clients);

    protected abstract Task ReadMessage(NetworkStream stream);
}