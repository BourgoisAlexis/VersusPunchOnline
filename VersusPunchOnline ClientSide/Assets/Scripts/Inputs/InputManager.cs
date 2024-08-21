using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class InputManager : MonoBehaviour {
    #region Variables
    [SerializeField] private TextMeshProUGUI _tmproFPS;
    [SerializeField] private TextMeshProUGUI _tmproCurrentSnapShot;
    [SerializeField] private TextMeshProUGUI _tmproPing;

    private List<PlayerController> _playerControllers = new List<PlayerController>();
    private Dictionary<int, List<SnapShot>> _snapShots = new Dictionary<int, List<SnapShot>>() {
        { 0, new List<SnapShot>()},
        { 1, new List<SnapShot>()},
    };

    private Action<SnapShot> _processSnapShot;
    private GameStates _gameState = GameStates.Default;

    private int _inputDelay = 0;
    private int _currentShotIndex = 0;
    private double _currentPing = 0;

    private bool isLocal => GlobalManager.Instance.isLocal;
    private int self => GlobalManager.Instance.selfID;
    private int opponent => GlobalManager.Instance.selfID == 0 ? 1 : 0;

    private InputCondition[] _inputConditions = new InputCondition[] {
        new InputCondition("LeftStickX", InputAction.Left, true, -1),
        new InputCondition(KeyCode.LeftArrow, InputAction.Left, true),

        new InputCondition("LeftStickX", InputAction.Right, true, 1),
        new InputCondition(KeyCode.RightArrow, InputAction.Right, true),

        new InputCondition("A", InputAction.Jump, false),
        new InputCondition(KeyCode.UpArrow, InputAction.Jump, false),

        new InputCondition("X", InputAction.Punch, false),
        new InputCondition(KeyCode.DownArrow, InputAction.Punch, false),

        new InputCondition("A", InputAction.Valid, false),
        new InputCondition(KeyCode.DownArrow, InputAction.Valid, false),

        new InputCondition("B", InputAction.Cancel, false),
        new InputCondition(KeyCode.Escape, InputAction.Cancel, false),
    };
    #endregion


    private void Start() {
        _gameState = GameStates.MainScreen;
        MainScreenInit();
    }

    private void Update() {
        if (_gameState == GameStates.Default)
            return;

        //Loop from 0 to playerN to get inputs for every players
        List<InputAction> validInputs = new List<InputAction>();

        foreach (InputCondition condition in _inputConditions)
            if (condition.IsValid())
                validInputs.Add(condition.action);

        while (_snapShots[self].Count < _currentShotIndex + 1) {
            if (_snapShots[self].Count > 0)
                _snapShots[self].Add(_snapShots[self].Last());
            else
                _snapShots[self].Add(new SnapShot(_currentShotIndex));
        }

        SnapShot snapShot = GetCurrentSnapShot(self);

        if (snapShot.index != _currentShotIndex)
            snapShot = _snapShots[self][_currentShotIndex] = new SnapShot(_currentShotIndex);

        foreach (InputAction input in validInputs)
            snapShot.AddInput(input);
    }


    private void MainScreenInit() {
        ClearInputs();
        _inputDelay = 0;

        Action onLoad = () => {
            GlobalManager.Instance.onCustomUpdate -= ProcessMainScreen;
            ClearInputs();
            _gameState = GameStates.Default;
        };

        Action onLoaded = () => {
            GlobalManager.Instance.NavigationManager.onLoadScene -= onLoad;
        };

        GlobalManager.Instance.onCustomUpdate += ProcessMainScreen;
        GlobalManager.Instance.NavigationManager.onLoadScene += onLoad;
        GlobalManager.Instance.NavigationManager.onSceneLoaded += onLoaded;
    }

    private void ProcessMainScreen() {
        LogCurrentShot();

        MainScreenManager m = GlobalManager.Instance.SceneManager as MainScreenManager;
        SnapShot s = GetCurrentSnapShot(0);

        if (m != null && s != null)
            m.ExecuteInputs(s.inputs);

        _currentShotIndex++;
    }


    public void GameplayInit(List<PlayerController> players) {
        _playerControllers = players;
        _inputDelay = isLocal ? 0 : AppConst.inputDelay;

        GlobalManager.Instance.onCustomUpdate += ProcessGameplay;
        GlobalManager.Instance.onSecondaryCustomUpdate += () => { _tmproFPS.text = (1f / Time.fixedDeltaTime).ToString("0.00"); };
        GlobalManager.Instance.onSecondaryCustomUpdate += () => { _tmproCurrentSnapShot.text = _currentShotIndex.ToString("0.00"); };

        if (!isLocal) {
            GlobalManager.Instance.ConnectionManager.onMessageReceived += AddSnapShot;
            GlobalManager.Instance.onSecondaryCustomUpdate += () => { _tmproPing.text = _currentPing.ToString("0.00"); };
        }

        _gameState = GameStates.Gameplay;
    }

    private void ProcessGameplay() {
        LogCurrentShot();

        ExecuteInputs();

        if (!isLocal)
            SendInput();

        _currentShotIndex++;
    }


    private void ExecuteInputs() {
        for (int i = 0; i < _playerControllers.Count; i++) {
            SnapShot snapShot = GetSnapShot(i, _currentShotIndex - _inputDelay);

            if (snapShot != null && snapShot.inputs != null) {
                string json = JsonUtility.ToJson(snapShot);
                Utils.Log(this, "ExecuteInputs", $"player : {i} / snapShot : {_currentShotIndex} / {json}");
                _playerControllers[i].ExecuteInputs(snapShot.inputs);
            }
            else if (isLocal) {
                //What do we do if input is missing on local ?
                _playerControllers[i].ExecuteInputs(new List<string>());
            }
            else {
                int index = 0;
                while (snapShot == null || snapShot.inputs == null) {
                    snapShot = GetSnapShot(i, _currentShotIndex - _inputDelay - index);
                    index++;

                    if (index >= 10)
                        break;
                }

                if (snapShot == null)
                    Utils.LogError(this, "ExecuteInputs", $"No input for player : {i} / snapShot : {_currentShotIndex - _inputDelay}");
                else
                    _playerControllers[i].ExecuteInputs(snapShot.inputs);
            }
        }
    }

    private void SendInput() {
        SnapShot snapShot = GetCurrentSnapShot(self);

        if (snapShot != null) {
            try {
                GlobalManager.Instance.ConnectionManager.SendMessage(snapShot);
            }
            catch (Exception ex) {
                Utils.LogError(this, "SendInput", $"{ex.Message} {_currentShotIndex} {GlobalManager.Instance.ConnectionManager == null}");
                return;
            }
        }
    }

    public void AddSnapShot(SnapShot snapShot) {
        string json = JsonUtility.ToJson(snapShot);
        Utils.Log(this, "AddSnapShot", json);

        _currentPing = DateTime.Now.TimeOfDay.TotalMilliseconds - snapShot.time;

        while (_snapShots[opponent].Count < snapShot.index + 1) {
            if (_snapShots[opponent].Count > 0)
                _snapShots[opponent].Add(_snapShots[opponent].Last());
            else
                _snapShots[opponent].Add(new SnapShot(_currentShotIndex));
        }

        _snapShots[opponent][snapShot.index] = snapShot;

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

    private SnapShot GetCurrentSnapShot(int playerIndex) {
        List<SnapShot> snapShots = _snapShots[playerIndex];

        if (_currentShotIndex >= snapShots.Count)
            return null;

        Utils.Log(this, "GetCurrentSnapShot", $"{_currentShotIndex} / {snapShots.Count} / {snapShots[_currentShotIndex] == null}", true);

        return snapShots[_currentShotIndex];
    }

    private SnapShot GetSnapShot(int playerIndex, int index) {
        List<SnapShot> snapShots = _snapShots[playerIndex];

        if (snapShots == null || snapShots.Count <= 0)
            return null;

        if (index >= snapShots.Count || index < 0)
            return null;

        return snapShots[index];
    }


    private void ClearInputs() {
        foreach (KeyValuePair<int, List<SnapShot>> list in _snapShots)
            list.Value.Clear();

        _currentShotIndex = 0;
    }

    private void LogCurrentShot() {
        Debug.Log($"=========={_currentShotIndex}==========");
    }
}
