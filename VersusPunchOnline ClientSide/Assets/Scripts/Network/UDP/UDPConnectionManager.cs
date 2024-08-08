using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;

public abstract class UDPConnectionManager : ConnectionManager<IPEndPoint> {
    #region Variables
    protected Stopwatch _swSend;
    protected Stopwatch _swReceived;

    protected int _maxClients = 1;
    protected NetManager _host = null;
    protected EventBasedNetListener _listenerHost = null;
    protected NetDataWriter _writer = null;

    protected NetManager _guest = null;
    protected EventBasedNetListener _listenerGuest = null;

    private bool _closed = true;
    #endregion

    //https://github.com/RevenantX/LiteNetLib?tab=readme-ov-file
    public override void Init(Action<IPEndPoint> onInit) {
        Utils.Log(this, "Init");

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
        _host.Stop();

        //foreach (TcpClient client in _guests) {
        //    if (client.Connected)
        //        client.GetStream().Close();

        //    client.Close();
        //}

        //_guests.Clear();
        _closed = true;
    }


    private async void OpenConnection(IPEndPoint iPEndPoint) {
        Utils.Log(this, "OpenConnection");

        _listenerHost = new EventBasedNetListener();
        _host = new NetManager(_listenerHost);
        _host.Start(iPEndPoint.Port);

        _listenerHost.ConnectionRequestEvent += request => {
            Utils.Log(this, "OpenConnection", $"{request.RemoteEndPoint} Tried to connect");

            if (_host.ConnectedPeersCount < _maxClients)
                request.Accept();
            else
                request.Reject();
        };

        _listenerHost.PeerConnectedEvent += peer => {
            Utils.Log(this, "OpenConnection", $"We got connection: {peer}");
            _writer = new NetDataWriter();
            SendMessage("Hello client");
        };

        while (!_closed) {
            _host.PollEvents();
            await Task.Delay(AppConst.pollRate);
        }
    }

    /// <summary>
    /// Connect to a host
    /// </summary>
    /// <param name="iPEndPoint"></param>
    /// <param name="onSuccess"></param>
    public override async void Connect(IPEndPoint iPEndPoint, Action onSuccess) {
        Utils.Log(this, "Connect");

        _listenerGuest = new EventBasedNetListener();
        _guest = new NetManager(_listenerGuest);
        _guest.Start();
        _guest.Connect(iPEndPoint.Address.ToString(), iPEndPoint.Port, "");

        _listenerGuest.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) => {
            ReadMessage(fromPeer, dataReader, deliveryMethod, channel);
        };

        onSuccess?.Invoke();

        while (!_closed) {
            _guest.PollEvents();
            await Task.Delay(AppConst.pollRate);
        }
    }


    protected abstract void ReadMessage(NetPeer fromPeer, NetPacketReader dataReader, byte deliveryMethod, DeliveryMethod channel);

    public abstract void SendMessage(object obj);
}
