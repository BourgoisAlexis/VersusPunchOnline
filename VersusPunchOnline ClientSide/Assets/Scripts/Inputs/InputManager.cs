using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InputManager : MonoBehaviour {
    #region Variables
    [SerializeField] private TextMeshProUGUI _tmproFPS;
    [SerializeField] private TextMeshProUGUI _tmproCurrentSnapShot;
    [SerializeField] private TextMeshProUGUI _tmproPing;

    private List<PlayerController> _playerControllers = new List<PlayerController>();
    private List<IInputUser> _listeners = new List<IInputUser>();

    private int _inputDelay = 0;
    private double _currentPing = 0;
    private Cheats _cheat = new Cheats();

    private InputCondition[] _inputConditions = new InputCondition[] {
        new InputCondition("LeftStickX", InputAction.Left, true, -1),
        new InputCondition(KeyCode.LeftArrow, InputAction.Left, true),

        new InputCondition("LeftStickX", InputAction.Right, true, 1),
        new InputCondition(KeyCode.RightArrow, InputAction.Right, true),

        new InputCondition("X", InputAction.Punch, false),
        new InputCondition(KeyCode.DownArrow, InputAction.Punch, false),

        new InputCondition("A", InputAction.Jump, false),
        new InputCondition(KeyCode.UpArrow, InputAction.Jump, false),

        new InputCondition("B", InputAction.Bonus, false),
        new InputCondition(KeyCode.Space, InputAction.Bonus, false),

        new InputCondition("A", InputAction.Valid, false),
        new InputCondition(KeyCode.DownArrow, InputAction.Valid, false),

        new InputCondition("B", InputAction.Cancel, false),
        new InputCondition(KeyCode.Escape, InputAction.Cancel, false),
    };


    //Accessors
    private bool isLocal => GlobalManager.Instance.IsLocal;
    private int self => GlobalManager.Instance.SelfID;
    private GameStateManager _stateManager => GlobalManager.Instance.GameStateManager;
    #endregion


    private void Update() {
        if (_stateManager.State == GameState.Default)
            return;

        //Loop from 0 to playerN to get inputs for every players
        List<InputAction> validInputs = new List<InputAction>();

        foreach (InputCondition condition in _inputConditions)
            if (condition.IsValid())
                validInputs.Add(condition.action);

        foreach (InputAction input in validInputs)
            GlobalManager.Instance.GameStateManager.AddInput(input, self);

        _cheat.Update();
    }


    public void InitMainScreen() {
        _inputDelay = 0;
    }

    public void InitGameplay(List<PlayerController> players) {
        _playerControllers = players;
        _inputDelay = isLocal ? 0 : AppConst.ONLINE_INPUT_DELAY;
        GlobalManager manager = GlobalManager.Instance;

        manager.OnSecondaryCustomUpdate += () => {
            _tmproFPS.text = $"{(1f / Time.fixedDeltaTime).ToString("0")} fps";
        };

        manager.OnSecondaryCustomUpdate += () => {
            _tmproCurrentSnapShot.text = $"frame : {_stateManager.CurrentIndex.ToString("0")}";
        };

        if (!isLocal) {
            manager.Connection.OnMessageReceived += AddInput;
            manager.OnSecondaryCustomUpdate += () => {
                _tmproPing.text = $"{_currentPing.ToString("0.00")} ms";
            };
        }
    }


    public void ProcessMainScreen() {
        CommonProcess();
    }

    public void ProcessGameplay() {
        ExecuteInputs();
        CommonProcess();

        if (!isLocal)
            SendInput();
    }

    private void CommonProcess() {
        if (_stateManager.CurrentIndex < AppConst.SYNCHRO_DURATION)
            return;

        InputMessage message = GlobalManager.Instance.GameStateManager.GetCurrentInput(0);
        if (GlobalManager.Instance.ShowLowPriorityLogs)
            Utils.Log(this, "CommonProcess", message.ToString());

        List<IInputUser> tempoCopy = new List<IInputUser>(_listeners);

        foreach (IInputUser user in tempoCopy)
            user.ExecuteInputs(message.Inputs);
    }


    private void ExecuteInputs() {
        if (_stateManager.CurrentIndex < AppConst.SYNCHRO_DURATION)
            return;

        for (int i = 0; i < _playerControllers.Count; i++) {
            int frameIndex = _stateManager.CurrentIndex;
            InputMessage message = _stateManager.GetInput(i, frameIndex - _inputDelay);

            if (GlobalManager.Instance.ShowLowPriorityLogs)
                Utils.Log(this, "ExecuteInputs", message.ToString());

            if (message != null && message.Inputs != null) {
                _playerControllers[i].ExecuteInputs(message.Inputs);
            }
            else if (isLocal) {
                //What do we do if input is missing on local ?
                _playerControllers[i].ExecuteInputs(new List<string>());
            }
            else {
                int index = 0;
                while (message == null || message.Inputs == null) {
                    message = _stateManager.GetInput(i, frameIndex - _inputDelay - index);
                    index++;

                    if (index >= 10)
                        break;
                }

                if (message == null)
                    Utils.LogError(this, $"No input for player : {i} / inputData : {frameIndex - _inputDelay}");
                else
                    _playerControllers[i].ExecuteInputs(message.Inputs);
            }
        }
    }

    public void AddListener(IInputUser listener) {
        if (listener == null)
            return;

        if (_listeners.Contains(listener))
            return;

        _listeners.Add(listener);
    }

    public void RemoveListener(IInputUser listener) {
        if (listener == null)
            return;

        if (!_listeners.Contains(listener))
            return;

        _listeners.Remove(listener);
    }

    private void SendInput() {
        InputMessage message = GlobalManager.Instance.GameStateManager.GetCurrentInput(self);

        if (message == null) {
            Utils.LogError(this, "Frame is null");
            return;
        }

        try {
            GlobalManager.Instance.Connection.SendMessage(message);
        }
        catch (Exception ex) {
            Utils.LogError(this, $"{ex.Message} {message.FrameIndex} {GlobalManager.Instance.Connection == null}");
            return;
        }
    }

    public void AddInput(InputMessage message) {
        string json = message.ToString();
        Utils.Log(this, "AddInput", json);

        _currentPing = DateTime.Now.TimeOfDay.TotalMilliseconds - message.Time;
        GlobalManager.Instance.GameStateManager.AddInputFromMessage(message);
    }
}
