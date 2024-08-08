using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

public class NetWorkPlayer {
    public TCPConnectionManager tcpConnection;
    public UDPConnectionManager udpConnection;
    public IPEndPoint endPoint;
}

public class NetworkTester : SceneManager {
    #region Variables
    private List<NetWorkPlayer> _players = new List<NetWorkPlayer>();
    private int _messageIndex = 0;
    private double _maxDelay = 0;

    private bool _tcp = false;
    #endregion


    private async void Start() {
        for (int i = 0; i < 2; i++) {
            NetWorkPlayer p = new NetWorkPlayer();
            _players.Add(p);

            if (_tcp) {
                p.tcpConnection = new TCPBandWidthTester();
                p.tcpConnection.Init((iPEndPoint) => {
                    p.endPoint = iPEndPoint;
                });
            }
            else {
                p.udpConnection = new UDPTester();
                p.udpConnection.Init((iPEndPoint) => {
                    p.endPoint = iPEndPoint;
                });

                (p.udpConnection as UDPTester).onMessageRead += MessageRead;
            }
        }

        await Connect(_players[0], _players[1]);

        Utils.Log(this, "Start");

        while (Application.isPlaying && _messageIndex < 1000) {
            await SendMessage(0);
            _messageIndex++;
        }

        Utils.Log(this, "End", $"maxdelay : {_maxDelay}");
    }

    private async Task Connect(NetWorkPlayer host, NetWorkPlayer guest) {
        bool goOn = false;

        if (_tcp) {
            guest.tcpConnection.Connect(host.endPoint, () => {
                goOn = true;
            });
        }
        else {
            guest.udpConnection.Connect(host.endPoint, () => {
                goOn = true;
            });
        }

        while (!goOn)
            await Task.Yield();
    }

    private async Task SendMessage(int senderIndex) {
        if (_tcp)
            await _players[senderIndex].tcpConnection.SendMessage(null);
        else {
            await Task.Delay(AppConst.pollRate);
            _players[senderIndex].udpConnection.SendMessage(new FrameData(_messageIndex, KeyCode.UpArrow.ToString(), DateTime.Now.TimeOfDay));
        }
    }

    private void MessageRead(FrameData frame) {
        double delay = DateTime.Now.TimeOfDay.TotalMilliseconds - frame.time;
        if (delay > _maxDelay)
            _maxDelay = delay;

        Utils.Log(this, "MessageRead", $"delay : {string.Format("{0:0.00}", delay)}");
    }

    private void OnApplicationQuit() {
        foreach (NetWorkPlayer p in _players) {
            p.tcpConnection?.CloseConnection();
            p.udpConnection?.CloseConnection();
        }
    }
}
