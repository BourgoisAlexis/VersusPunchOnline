using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

public class NetworkPlayer {
    public TCPConnection<PeerMessage> tcpConnection;
    public UDPConnection<PeerMessage> udpConnection;
    public IPEndPoint endPoint;
}

public class NetworkTester : SceneManager {
    #region Variables
    private List<NetworkPlayer> _players = new List<NetworkPlayer>();
    private bool _tcp = false;
    private int _messageNumber = 500;
    private int _delayBetweenMessages = 1;

    private int _messageIndex = 0;
    private double _totalDelay = 0;
    private double _maxDelay = 0;
    private int _hostIndex = 0;
    private int _guestIndex = 1;
    #endregion


    private async void Start() {
        InitializedHost();
        InitializeGuest();

        Utils.Log("Connection test start");

        while (Application.isPlaying && _messageIndex < _messageNumber) {
            await Task.Delay(_delayBetweenMessages);
            SendMessage(_guestIndex);
            _messageIndex++;
        }

        Utils.Log($"Connection test results: maxdelay={string.Format("{0:0.00}", _maxDelay)} ms / averagedelay={string.Format("{0:0.00}", _totalDelay / _messageNumber)} ms");
    }

    private void InitializedHost() {
        NetworkPlayer p = new NetworkPlayer();
        _players.Add(p);

        if (_tcp) {
            p.tcpConnection = new SimpleTCPConnection<PeerMessage>();
            p.tcpConnection.Init((iPEndPoint) => {
                p.endPoint = iPEndPoint;
            }, true, 1);
        }
        else {
            p.udpConnection = new SimpleUDPConnection<PeerMessage>();
            p.udpConnection.Init((iPEndPoint) => {
                p.endPoint = iPEndPoint;
            }, true, 1);
        }
    }

    private void InitializeGuest() {
        NetworkPlayer p = new NetworkPlayer();
        _players.Add(p);

        if (_tcp) {
            p.tcpConnection = new SimpleTCPConnection<PeerMessage>();
            p.tcpConnection.Init((iPEndPoint) => {
                p.endPoint = iPEndPoint;
                p.tcpConnection.Connect(_players[_hostIndex].endPoint, () => Debug.Log("Connected"));
                _players[_hostIndex].tcpConnection.OnMessageReceived += MessageRead;
            });
        }
        else {
            p.udpConnection = new SimpleUDPConnection<PeerMessage>();
            p.udpConnection.Init((iPEndPoint) => {
                p.endPoint = iPEndPoint;
                p.udpConnection.Connect(_players[_hostIndex].endPoint, () => Debug.Log("Connected"));
                _players[_hostIndex].udpConnection.OnMessageReceived += MessageRead;
            });
        }
    }


    private void SendMessage(int senderIndex) {
        if (_tcp) {
            _players[senderIndex].tcpConnection.SendMessage(new PeerMessage(_messageIndex.ToString()));
        }
        else {
            _players[senderIndex].udpConnection.SendMessage(new PeerMessage(_messageIndex.ToString()));
        }
    }

    private void MessageRead(PeerMessage message) {
        double delay = DateTime.Now.TimeOfDay.TotalMilliseconds - message.Time;
        _totalDelay += delay;
        if (delay > _maxDelay)
            _maxDelay = delay;

        Utils.Log(this, "MessageRead", $"delay : {string.Format("{0:0.00}", delay)} ms");
    }

    private void OnApplicationQuit() {
        foreach (NetworkPlayer p in _players) {
            p.tcpConnection?.CloseConnection();
            p.udpConnection?.CloseConnection();
        }
    }
}
