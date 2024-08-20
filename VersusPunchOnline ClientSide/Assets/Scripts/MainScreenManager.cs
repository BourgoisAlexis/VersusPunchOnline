using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class Row {
    public List<UIButton> buttons;
}

public class MainScreenManager : SceneManager, IInputUser {
    #region Variables
    [SerializeField] private List<Row> _buttons = new List<Row>();
    private Vector2Int _currentButton;
    private int _currentDelay;
    private int _inputLimit = 10;

    public UIButton GetCurrentButton() => _buttons[_currentButton.y].buttons[_currentButton.x];
    #endregion


    public override void Init(params object[] parameters) {
        base.Init(parameters);
        GetCurrentButton().OnPointerEnter(null);
    }


    public void ExecuteInputs(List<string> inputs) {
        if (inputs == null)
            return;

        if (_currentDelay > 0) {
            _currentDelay--;
            return;
        }
        else if (inputs.Count > 1)
            _currentDelay = _inputLimit;

        if (inputs.Contains(InputAction.Valid.ToString())) {
            GetCurrentButton().OnPointerClick(null);
            return;
        }

        Vector2Int dir = new Vector2Int();

        if (inputs.Contains(InputAction.Left.ToString()))
            dir.x = -1;
        else if (inputs.Contains(InputAction.Right.ToString()))
            dir.x = 1;

        Navigate(dir);
    }

    public void Navigate(Vector2Int direction) {
        if (direction == Vector2Int.zero)
            return;

        Vector2Int result = _currentButton + direction;

        if (result.y < 0 || result.y >= _buttons.Count)
            return;

        if (result.x < 0 || result.x >= _buttons[result.y].buttons.Count)
            return;

        GetCurrentButton().OnPointerExit(null);
        _currentButton = result;
        GetCurrentButton().OnPointerEnter(null);
    }


    public void SetupLocal() {
        LoadGameplayScene(true);
    }

    public void SetupOnline() {
        ConnectToPlayerIO();
    }

    public async void SetupPhysics() {
        await GlobalManager.Instance.NavigationManager.LoadScene(2);
        GlobalManager.Instance.PhysicsManager.Setup();
    }

    public async void SetupNetwork() {
        await GlobalManager.Instance.NavigationManager.LoadScene(3);
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
        await GlobalManager.Instance.NavigationManager.LoadScene(1);
    }
}
