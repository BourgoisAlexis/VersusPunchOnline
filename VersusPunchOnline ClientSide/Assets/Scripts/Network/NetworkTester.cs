using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

public class NetworkPlayer {
    public TCPConnection tcpConnection;
    public UDPConnection<SimpleMessage> udpConnection;
    public IPEndPoint endPoint;
}

public class NetworkTester : SceneManager {
    #region Variables
    private List<NetworkPlayer> _players = new List<NetworkPlayer>();
    private int _messageIndex = 0;
    private double _maxDelay = 0;

    private bool _tcp = false;
    private int _hostIndex = 0;
    private int _guestIndex = 1;
    #endregion


    private async void Start() {
        InitializedHost();
        await Task.Delay(2000);
        InitializeGuest();

        Utils.Log(this, "Start");

        while (Application.isPlaying && _messageIndex < 200) {
            await Task.Delay(5);
            SendMessage(_guestIndex);
            _messageIndex++;
        }

        Utils.Log(this, "Start", $"maxdelay : {_maxDelay}");
    }

    private void InitializedHost() {
        NetworkPlayer p = new NetworkPlayer();
        _players.Add(p);

        if (_tcp) {
            p.tcpConnection = new SimpleTCPConnection();
            p.tcpConnection.Init((iPEndPoint) => {
                p.endPoint = iPEndPoint;
            }, true, 1);
        }
        else {
            p.udpConnection = new SimpleUDPConnection();
            p.udpConnection.Init((iPEndPoint) => {
                p.endPoint = iPEndPoint;
            }, true, 1);
        }
    }

    private void InitializeGuest() {
        NetworkPlayer p = new NetworkPlayer();
        _players.Add(p);

        if (_tcp) {
            p.tcpConnection = new SimpleTCPConnection();
            p.tcpConnection.Init((iPEndPoint) => {
                p.endPoint = iPEndPoint;
                p.tcpConnection.Connect(_players[_hostIndex].endPoint, () => Debug.Log("Connected"));
                _players[_hostIndex].tcpConnection.OnMessageReceived += MessageRead;
            });
        }
        else {
            p.udpConnection = new SimpleUDPConnection();
            p.udpConnection.Init((iPEndPoint) => {
                p.endPoint = iPEndPoint;
                p.udpConnection.Connect(_players[_hostIndex].endPoint, () => Debug.Log("Connected"));
                _players[_hostIndex].udpConnection.OnMessageReceived += MessageRead;
            });
        }
    }


    private void SendMessage(int senderIndex) {
        if (_tcp) {
            _players[senderIndex].tcpConnection.SendMessage(new SimpleMessage(_messageIndex.ToString()));
        }
        else {
            _players[senderIndex].udpConnection.SendMessage(new SimpleMessage(_messageIndex.ToString()));
        }
    }

    private void MessageRead(SimpleMessage message) {
        double delay = DateTime.Now.TimeOfDay.TotalMilliseconds - message.Time;
        if (delay > _maxDelay)
            _maxDelay = delay;

        Utils.Log(this, "MessageRead", $"delay : {string.Format("{0:0.00}", delay)}");
    }

    private void OnApplicationQuit() {
        foreach (NetworkPlayer p in _players) {
            p.tcpConnection?.CloseConnection();
            p.udpConnection?.CloseConnection();
        }
    }
}
