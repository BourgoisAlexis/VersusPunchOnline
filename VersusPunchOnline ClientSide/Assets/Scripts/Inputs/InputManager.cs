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
    private bool isLocal => GlobalManager.Instance.isLocal;
    private int self => GlobalManager.Instance.selfID;
    private int opponent => GlobalManager.Instance.selfID == 0 ? 1 : 0;
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
            GlobalManager.Instance.GameStateManager.AddInput(input);
    }


    public void InitMainScreen() {
        _inputDelay = 0;
    }

    public void InitGameplay(List<PlayerController> players) {
        _playerControllers = players;
        _inputDelay = isLocal ? 0 : AppConst.inputDelay;
        GlobalManager manager = GlobalManager.Instance;

        manager.onSecondaryCustomUpdate += () => { _tmproFPS.text = (1f / Time.fixedDeltaTime).ToString("0.00"); };
        manager.onSecondaryCustomUpdate += () => { _tmproCurrentSnapShot.text = _stateManager.CurrentIndex.ToString("0.00"); };

        if (!isLocal) {
            manager.ConnectionManager.onMessageReceived += AddSnapShot;
            manager.onSecondaryCustomUpdate += () => { _tmproPing.text = _currentPing.ToString("0.00"); };
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
        FrameInfo frame = GlobalManager.Instance.GameStateManager.CurrentFrame;
        Debug.Log(frame.ToString());

        List<IInputUser> users = new List<IInputUser>(_listeners);

        foreach (IInputUser user in users)
            user.ExecuteInputs(frame.inputs[0]);
    }


    private void ExecuteInputs() {
        for (int i = 0; i < _playerControllers.Count; i++) {
            int shotIndex = _stateManager.CurrentIndex;
            FrameInfo frame = _stateManager.GetFrameInfoAtIndex(shotIndex - _inputDelay);

            Debug.Log(frame.ToString());

            if (frame != null && frame.inputs != null) {
                string json = JsonUtility.ToJson(frame);
                Utils.Log(this, $"player : {i} / snapShot : {shotIndex} / {json}");
                _playerControllers[i].ExecuteInputs(frame.inputs[i]);
            }
            else if (isLocal) {
                //What do we do if input is missing on local ?
                _playerControllers[i].ExecuteInputs(new List<string>());
            }
            else {
                int index = 0;
                while (frame == null || frame.inputs == null) {
                    frame = _stateManager.GetFrameInfoAtIndex(shotIndex - _inputDelay - index);
                    index++;

                    if (index >= 10)
                        break;
                }

                if (frame == null)
                    Utils.LogError(this, $"No input for player : {i} / snapShot : {shotIndex - _inputDelay}");
                else
                    _playerControllers[i].ExecuteInputs(frame.inputs[i]);
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
        FrameInfo frame = GlobalManager.Instance.GameStateManager.CurrentFrame;

        if (frame == null) {
            Utils.LogError(this, "Frame is null");
            return;
        }

        //try {
        //    GlobalManager.Instance.ConnectionManager.SendMessage(snapShot);
        //}
        //catch (Exception ex) {
        //    Utils.LogError(this, "SendInput", $"{ex.Message} {_currentShotIndex} {GlobalManager.Instance.ConnectionManager == null}");
        //    return;
        //}
    }

    public void AddSnapShot(FrameInfo frame) {
        //string json = JsonUtility.ToJson(snapShot);
        //Utils.Log(this, "AddSnapShot", json);

        //_currentPing = DateTime.Now.TimeOfDay.TotalMilliseconds - snapShot.time;

        //while (_snapShots[opponent].Count < snapShot.index + 1) {
        //    if (_snapShots[opponent].Count > 0)
        //        _snapShots[opponent].Add(_snapShots[opponent].Last());
        //    else
        //        _snapShots[opponent].Add(new SnapShot(_currentShotIndex));
        //}

        //_snapShots[opponent][snapShot.index] = snapShot;

        //if (frame.frameIndex % 5 == 0) {
        //    FrameData d = _frameDatas[self][frame.frameIndex];
        //    int frameDiff = _currentFrameIndex - frame.frameIndex;
        //    double timeDiff = frame.time - d.time;

        //    Utils.Log(this, "Addframe", $"frame diff > {frameDiff} f | time diff > {timeDiff} ms");

        //    //Si timeDiff > 0 alors on est en avance donc on doit "ralentir"
        //    if (timeDiff > 0) {
        //        int step = frameDiff * 2;
        //        for (int i = frame.frameIndex - step; i < frame.frameIndex + step; i++) {

        //        }
        //    }
        //}
    }
}
