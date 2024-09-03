using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class Row {
    public List<UIButton> buttons;
}

public class MainScreenManager : SceneManager {
    public override void Init(params object[] parameters) {
        base.Init(parameters);
        GlobalManager.Instance.InputManager.InitMainScreen();
        GlobalManager.Instance.GameStateManager.ChangeGameState(GameState.MainScreen);
    }

    public void SetupLocal() {
        LoadGameplayScene(true);
    }

    public void SetupOnline() {
        ConnectToPlayerIO();
    }

    public async void SetupPhysics() {
        await GlobalManager.Instance.NavigationManager.LoadScene(3);
        GlobalManager.Instance.PhysicsManager.Init();
    }

    public async void SetupNetwork() {
        await GlobalManager.Instance.NavigationManager.LoadScene(4);
    }

    private void ConnectToPlayerIO() {
        GlobalManager.Instance.PlayerIOManager.AddMessageToHandle(AppConst.serverMessageAskForConnectionInfos, OpenConnection);
        GlobalManager.Instance.PlayerIOManager.AddMessageToHandle(AppConst.serverMessageConnectionInfos, ConnectToHost);
        GlobalManager.Instance.PlayerIOManager.Init("Alexis", null);

        _viewManager.ShowView(1);
    }

    private void OpenConnection(string[] infos) {
        string receiverID = infos[0];
        GlobalManager.Instance.selfID = int.Parse(infos[1]);

        GlobalManager.Instance.ConnectionManager.Init((iPEndPoint) => {
            string ip = iPEndPoint.Address.ToString();
            string port = iPEndPoint.Port.ToString();
            GlobalManager.Instance.PlayerIOManager.SendMessage(AppConst.userMessageP2POpen, receiverID, ip, port);
        });
    }

    private async void ConnectToHost(string[] infos) {
        IPAddress ip = IPAddress.Parse(infos[0]);
        int port = int.Parse(infos[1]);
        IPEndPoint endPoint = new IPEndPoint(ip, port);

        bool goOn = false;
        GlobalManager.Instance.ConnectionManager.Connect(endPoint, () => {
            goOn = true;
        });

        while (!goOn)
            await Task.Yield();

        LoadGameplayScene(false);
    }

    private async void LoadGameplayScene(bool isLocal) {
        GlobalManager.Instance.isLocal = isLocal;
        await GlobalManager.Instance.NavigationManager.LoadScene(2);
    }
}
