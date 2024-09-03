using UnityEngine;
using System.Collections.Generic;
using PlayerIOClient;
using System;
using System.Linq;

public class PlayerIOManager {
    #region Variables
    private Connection _connection;
    private Client _client;
    private bool _processing;

    private List<Message> _messages = new List<Message>();
    private Dictionary<string, Action<string[]>> _handledMessagesDict = new Dictionary<string, Action<string[]>>();
    #endregion


    public void Init(string userId, Action onSuccess) {
        Application.runInBackground = true;

        PlayerIO.Authenticate(
            "versuspunchonline-hxzulsresk6ho8sj0rffgq", //Game ID         
            "public",                                   //Connection ID
            new Dictionary<string, string> {            //Auth arguments
				{ "userId", userId },
            },
            null,                                   //PlayerInsight segments
            (Client client) => {
                _client = client;
                AuthenticateSuccess();
                onSuccess?.Invoke();
            },
            (PlayerIOError error) => {
                Utils.LogError(this, error.Message);
            }
        );
    }

    private void AuthenticateSuccess() {
        Utils.Log(this);

        if (!CheckClient())
            return;

        if (GlobalManager.Instance.useLocalPlayerIO) {
            Utils.Log(this, "Create serverEndpoint");
            _client.Multiplayer.DevelopmentServer = new ServerEndpoint("localhost", 8184);
        }

        CreateJoinRoom();
    }

    //Room
    public void CreateJoinRoom() {
        Utils.Log(this);

        if (!CheckClient())
            return;

        _client.Multiplayer.ListRooms(
            AppConst.defaultRoomID,
            null,
            30,
            0,
            (RoomInfo[] rooms) => {
                if (rooms.Length == 0) {
                    CreateRoom($"{AppConst.defaultRoomID}_{0}");
                }
                else {
                    bool connected = false;
                    foreach (RoomInfo room in rooms) {
                        if (room.OnlineUsers < AppConst.userLimitPerRoom) {
                            JoinRoom(room.Id);
                            connected = true;
                            break;
                        }
                    }
                    if (!connected) {
                        CreateRoom($"{AppConst.defaultRoomID}_{rooms.Length}");
                    }
                }
            }
        );
    }

    private void CreateRoom(string roomId) {
        Utils.Log(this);

        if (!CheckClient())
            return;

        _client.Multiplayer.CreateRoom(
            roomId,
            "Lobby",
            true,
            null,
            (string roomId) => {
                JoinRoom(roomId);
            },
            (PlayerIOError error) => {
                Utils.LogError(this, error.Message);
            }
        );
    }

    private void JoinRoom(string roomId) {
        Utils.Log(this);

        if (!CheckClient())
            return;

        _client.Multiplayer.JoinRoom(
            roomId,                             //Room id. If set to null a random roomid is used
            new Dictionary<string, string> {
                { "gameVersion", $"{Application.version}" }
            },
            (Connection connection) => {
                _connection = connection;
                _connection.OnMessage += ReceiveMessage;
            },
            (PlayerIOError error) => {
                Utils.LogError(this, error.Message);
            }
        );
    }

    public void LeaveRoom() {
        Utils.Log(this, "LeaveRoom");

        if (_connection == null)
            return;

        _connection.Disconnect();
    }


    //Messages
    private void ReceiveMessage(object sender, Message m) {
        Utils.Log(this, $"{m.Type}");
        _messages.Add(m);

        if (_processing)
            return;

        ProcessMessages();
    }

    public void SendMessage(string type, params object[] parameters) {
        if (!CheckConnection())
            return;

        Message m = Message.Create(type, parameters);
        _connection.Send(m);
    }

    public async void ProcessMessages() {
        _processing = true;

        while (_messages.Count > 0) {
            Message m = _messages.First();

            if (_handledMessagesDict.ContainsKey(m.Type))
                _handledMessagesDict[m.Type]?.Invoke(m.GetString(0).Split(';'));

            _messages.Remove(m);
        }

        _processing = false;
    }

    public void AddMessageToHandle(string id, Action<string[]> action) {
        if (_handledMessagesDict.ContainsKey(id))
            _handledMessagesDict[id] += action;
        else
            _handledMessagesDict.Add(id, action);
    }


    //Null checks
    private bool CheckClient() {
        if (_client == null) {
            Debug.LogError("_client is null");
            return false;
        }

        return true;
    }

    private bool CheckConnection() {
        if (_connection == null) {
            Debug.LogError("_connection is null");
            return false;
        }

        return true;
    }
}
