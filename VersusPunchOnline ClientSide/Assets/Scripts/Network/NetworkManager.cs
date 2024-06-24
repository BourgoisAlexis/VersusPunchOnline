using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class NetWorkPlayer {
    public PeerToPeerManager<FrameData> peerToPeerManager;
    public string ip;
    public string port;
}

public class NetworkTestMessage {
    public int messageIndex;
    public string message;
    public double time;

    public NetworkTestMessage(int messageIndex, string message, double time) {
        this.messageIndex = messageIndex;
        this.message = message;
        this.time = time;
    }
}

public class NetworkManager : SceneManager {
    #region Variables
    private List<NetWorkPlayer> _players = new List<NetWorkPlayer>();
    private int _messageIndex = 0;
    #endregion


    private async void Start() {
        for (int i = 0; i < 2; i++) {
            NetWorkPlayer p = new NetWorkPlayer();
            _players.Add(p);
            p.peerToPeerManager = new PeerToPeerManager<FrameData>();
            p.peerToPeerManager.Setup((iPEndPoint) => {
                p.ip = iPEndPoint.Address.ToString();
                p.port = iPEndPoint.Port.ToString();
            });
            p.peerToPeerManager.onMessageReceived += ShowMessage;
        }

        await Connect(_players[0], _players[1]);
        await Connect(_players[1], _players[0]);

        Utils.Log(this, "Start", "Setuped");

        while (Application.isPlaying) {
            await TestMessage();
            _messageIndex++;
        }
    }

    private async Task Connect(NetWorkPlayer p1, NetWorkPlayer p2) {
        IPAddress ip = IPAddress.Parse(p1.ip);
        int port = int.Parse(p1.port);
        IPEndPoint endPoint = new IPEndPoint(ip, port);

        bool goOn = false;
        p2.peerToPeerManager.Connect(endPoint, () => {
            goOn = true;
        });

        while (!goOn)
            await Task.Yield();
    }

    private void ShowMessage(FrameData message) {
        Utils.Log(this, "ShowMessage", $"messageIndex : {message.frameIndex} / delay : {DateTime.Now.TimeOfDay.TotalMilliseconds - message.time}");
    }

    private async Task SendMessage(int index, FrameData message) {
        await _players[index].peerToPeerManager.SendMessage(message);
    }

    private async Task TestMessage() {
        int r = UnityEngine.Random.Range(0, 100);
        FrameData message = new FrameData(_messageIndex, r.ToString(), DateTime.Now.TimeOfDay);

        await SendMessage(0, message);
        await SendMessage(1, message);
    }

    private void OnApplicationQuit() {
        foreach (NetWorkPlayer p in _players)
            p.peerToPeerManager.CloseConnection();
    }

    private void OnDestroy() {
        foreach (NetWorkPlayer p in _players)
            p.peerToPeerManager.CloseConnection();
    }
}
