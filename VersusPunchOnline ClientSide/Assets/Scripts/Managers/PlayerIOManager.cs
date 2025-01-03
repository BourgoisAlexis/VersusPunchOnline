using UnityEngine;
using System.Collections.Generic;
using PlayerIOClient;
using System;
using System.Linq;

public class HandledMessage {
    public Action<string[]> OnMessageHandled;
    public int InfosLength;

    public HandledMessage(int infosLength) {
        InfosLength = infosLength;
    }
}

public class PlayerIOManager {
    #region Variables
    private Connection _connection;
    private Client _client;

    private bool _processing;
    private List<Message> _messages = new List<Message>();
    private Dictionary<string, HandledMessage> _handledMessageTypes = new Dictionary<string, HandledMessage>();
    #endregion


    public void Init(string gameID, string userID, Action onSuccess) {
        if (string.IsNullOrEmpty(gameID)) {
            Utils.ErrorOnParams("PlayerIOManager", "Init");
            return;
        }

        Application.runInBackground = true;

        PlayerIO.Authenticate(
            gameID,                                     //Game ID         
            "public",                                   //Connection ID
            new Dictionary<string, string> {            //Auth arguments
				{ "userId", userID },
            },
            null,                                   //PlayerInsight segments
            (Client client) => {
                _client = client;
                AuthenticateSuccess();
                onSuccess?.Invoke();
            },
            (PlayerIOError error) => {
                Utils.LogError(this, "Init", error.Message);
            }
        );
    }

    private void AuthenticateSuccess() {
        Utils.Log(this, "AuthenticateSuccess");

        if (!CheckClient())
            return;

        if (GlobalManager.Instance.UseLocalPlayerIO) {
            Utils.Log(this, "AuthenticateSuccess", "Create serverEndpoint");
            _client.Multiplayer.DevelopmentServer = new ServerEndpoint("localhost", 8184);
        }
    }

    //Room
    public void CreateRoom(Action<string> onSuccess, Action onError) {
        Utils.Log(this, "CreateRoom");

        if (!CheckClient())
            return;

        _client.Multiplayer.ListRooms(AppConst.DEFAULT_ROOM_ID,
            null,
            10,
            0,
            (RoomInfo[] infos) => {
                JoinCreate(infos, onSuccess, onError);
            }
        );
    }

    private void JoinCreate(RoomInfo[] rooms, Action<string> onSuccess, Action onError) {
        if (rooms != null && rooms.Length > 0) {
            Utils.Log($"{rooms[0].Id}");
            JoinRoom(rooms[0].Id, null, onError);
            onSuccess?.Invoke(rooms[0].Id);
            return;
        }

        _client.Multiplayer.CreateRoom(
            null,
            AppConst.DEFAULT_ROOM_ID,
            true,
            new Dictionary<string, string> {
                //{ cc.numberOfPlayerKey, $"{GlobalManager.Instance.numberOfPlayer}" },
                { AppConst.VERSION_KEY, AppConst.VERSION }
            },
            (string roomID) => {
                Utils.Log(this, "CreateRoom", roomID);
                JoinRoom(roomID, null, onError);
                onSuccess?.Invoke(roomID);
            },
            (PlayerIOError error) => {
                Utils.LogError(this, "CreateRoom", error.Message);
                onError?.Invoke();
            }
        );
    }

    public void JoinRoom(string roomID, Action onSuccess, Action onError) {
        if (string.IsNullOrEmpty(roomID)) {
            Utils.ErrorOnParams("PlayerIOManager", "Init");
            onError?.Invoke();
            return;
        }

        Utils.Log(this, "JoinRoom");

        if (!CheckClient()) {
            onError?.Invoke();
            return;
        }

        _client.Multiplayer.JoinRoom(
            roomID,                             //Room id. If set to null a random roomid is used
            new Dictionary<string, string> {
                { AppConst.VERSION_KEY, AppConst.VERSION }
            },
            (Connection connection) => {
                _connection = connection;
                _connection.OnMessage += ReceiveMessage;
                onSuccess?.Invoke();
            },
            (PlayerIOError error) => {
                Utils.LogError(this, "JoinRoom", error.Message);
                onError?.Invoke();
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
        if (sender == null || m == null) {
            Utils.ErrorOnParams("PlayerIOManager", "ReceiveMessage");
            return;
        }

        Utils.LogMessage(m);

        _messages.Add(m);

        if (_processing)
            return;

        ProcessMessages();
    }

    public void SendMessage(string type, params object[] parameters) {
        if (string.IsNullOrEmpty(type)) {
            Utils.ErrorOnParams("PlayerIOManager", "SendMessage");
            return;
        }

        if (!CheckConnection())
            return;

        Message m = Message.Create(type, parameters);
        _connection.Send(m);
    }

    public void ProcessMessages() {
        _processing = true;

        while (_messages.Count > 0) {
            Message m = _messages.First();

            if (_handledMessageTypes.ContainsKey(m.Type)) {
                string[] infos = Utils.GetMessageParams(m);
                if (infos == null || infos.Length < _handledMessageTypes[m.Type].InfosLength) {
                    Utils.LogMessage(m);
                    Utils.LogError(this, "ProcessMessages", "infos are wrong");
                }
                else
                    _handledMessageTypes[m.Type].OnMessageHandled?.Invoke(infos);
            }
            else {
                Utils.LogError(this, "ProcessMessages", $"message of type {m.Type} is not handled");
            }

            _messages.Remove(m);
        }

        _processing = false;
    }

    public void HandleMessage(string id, Action<string[]> action, int infosLength = 0) {
        if (string.IsNullOrEmpty(id) || action == null) {
            Utils.ErrorOnParams("PlayerIOManager", "HandleMessage");
            return;
        }

        HandledMessage m = null;

        if (_handledMessageTypes.ContainsKey(id)) {
            m = _handledMessageTypes[id];
        }
        else {
            m = new HandledMessage(infosLength);
            _handledMessageTypes.Add(id, m);
        }

        m.OnMessageHandled += action;
    }

    public void UnhandleMessage(string id, Action<string[]> action) {
        if (string.IsNullOrEmpty(id) || action == null) {
            Utils.ErrorOnParams("PlayerIOManager", "RemoveMessage");
            return;
        }

        if (_handledMessageTypes.ContainsKey(id))
            _handledMessageTypes[id].OnMessageHandled -= action;
    }


    //Null checks
    private bool CheckClient() {
        if (_client == null) {
            Utils.LogError(this, "CheckClient", "_client is null");
            return false;
        }

        return true;
    }

    private bool CheckConnection() {
        if (_connection == null) {
            Utils.LogError(this, "CheckConnection", "_connection is null");
            return false;
        }

        return true;
    }
}
