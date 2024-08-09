using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[Serializable]
public class FrameData {
    public int frameIndex;
    public string inputs;
    public double time; //used for ping

    public FrameData(int frameIndex, string input, TimeSpan time) {
        this.frameIndex = frameIndex;
        this.inputs = input;
        this.time = time.TotalMilliseconds;
    }
}

public class InputManager : MonoBehaviour {
    #region Variables
    [SerializeField] private TextMeshProUGUI _tmproFPS;
    [SerializeField] private TextMeshProUGUI _tmproCurrentFrame;
    [SerializeField] private TextMeshProUGUI _tmproPing;

    private List<PlayerController> _playerControllers = new List<PlayerController>();
    private Dictionary<int, List<FrameData>> _frameDatas = new Dictionary<int, List<FrameData>>() {
        { 0, new List<FrameData>()},
        { 1, new List<FrameData>()},
    };

    private int _inputDelay = 0;
    private int _currentFrameIndex = 0;
    private double _currentPing = 0;

    private bool isLocal => GlobalManager.Instance.isLocal;
    private int self => GlobalManager.Instance.selfID;
    private int opponent => GlobalManager.Instance.selfID == 0 ? 1 : 0;

    private InputCondition[] _inputConditions = new InputCondition[] {
        new InputCondition(KeyCode.LeftArrow, true),
        new InputCondition(KeyCode.RightArrow, true),
        new InputCondition(KeyCode.UpArrow, false),
        new InputCondition(KeyCode.LeftArrow, true),
        new InputCondition(KeyCode.Space, false),
    };
    #endregion


    public void Init() {
        List<PlayerController> ctrls = FindObjectsOfType<PlayerController>().ToList();
        _playerControllers.Add(ctrls.Find(x => x.playerIndex == 0));
        _playerControllers.Add(ctrls.Find(x => x.playerIndex == 1));

        foreach (PlayerController c in _playerControllers)
            c.Init();

        GlobalManager.Instance.onCustomUpdate += isLocal ? ProcessLocal : processOnline;
        GlobalManager.Instance.onSecondaryCustomUpdate += () => { _tmproFPS.text = (1f / Time.fixedDeltaTime).ToString("0.00"); };
        GlobalManager.Instance.onSecondaryCustomUpdate += () => { _tmproCurrentFrame.text = _currentFrameIndex.ToString("0.00"); };
        _inputDelay = isLocal ? 0 : AppConst.inputDelay;

        if (isLocal)
            return;

        GlobalManager.Instance.ConnectionManager.onMessageReceived += AddFrame;
        GlobalManager.Instance.onSecondaryCustomUpdate += () => { _tmproPing.text = _currentPing.ToString("0.00"); };
    }

    private void Update() {
        List<KeyCode> validInputs = new List<KeyCode>();

        foreach (InputCondition cond in _inputConditions) {
            KeyCode code = cond.code;
            if (Input.GetKeyDown(code)) {
                cond.pressed = true;
                if (!cond.canMaintain)
                    validInputs.Add(code);
            }
            else if (Input.GetKeyUp(code))
                cond.pressed = false;

            if (cond.canMaintain && cond.pressed)
                validInputs.Add(code);
        }

        Utils.Log(this, "Update", $"currentFrame : {_currentFrameIndex}", true);

        while (_frameDatas[self].Count < _currentFrameIndex + 1) {
            if (_frameDatas[self].Count > 0)
                _frameDatas[self].Add(_frameDatas[self].Last());
            else
                _frameDatas[self].Add(new FrameData(_currentFrameIndex, "None", DateTime.Now.TimeOfDay));
        }

        FrameData frame = GetCurrentFrame(self);

        if (frame.frameIndex != _currentFrameIndex)
            frame = _frameDatas[self][_currentFrameIndex] = new FrameData(_currentFrameIndex, "None", DateTime.Now.TimeOfDay);

        foreach (KeyCode code in validInputs)
            if (Input.GetKey(code))
                if (!frame.inputs.Contains(code.ToString()))
                    frame.inputs += $";{code}";
    }


    private void ProcessLocal() {
        Debug.Log($"=========={_currentFrameIndex}==========");

        ProcessInputs();
        _currentFrameIndex++;
    }

    private void processOnline() {
        Debug.Log($"=========={_currentFrameIndex}==========");

        ProcessInputs();
        SendInput();
        _currentFrameIndex++;
    }


    private void SendInput() {
        FrameData frame = GetCurrentFrame(self);

        if (frame != null) {
            try {
                GlobalManager.Instance.ConnectionManager.SendMessage(frame);
            }
            catch (Exception ex) {
                Utils.LogError(this, "SendInput", $"{ex.Message} {_currentFrameIndex} {GlobalManager.Instance.ConnectionManager == null}");
                return;
            }
        }
    }

    private void ProcessInputs() {
        for (int i = 0; i < _playerControllers.Count; i++) {
            FrameData frame = GetFrame(i, _currentFrameIndex - _inputDelay);

            if (frame != null && frame.inputs != null) {
                string json = JsonUtility.ToJson(frame);
                Utils.Log(this, "ProcessInputs", $"player : {i} / frame : {_currentFrameIndex} / {json}");
                _playerControllers[i].ExecuteInputs(frame);
            }
            else if (isLocal) {
                //What do we do if input is missing on local ?
            }
            else {
                int index = 0;
                while (frame == null || frame.inputs == null) {
                    frame = GetFrame(i, _currentFrameIndex - _inputDelay - index);
                    index++;

                    if (index >= 10)
                        break;
                }

                if (frame == null)
                    Utils.LogError(this, "ProcessInputs", $"No input for player : {i} / frame : {_currentFrameIndex - _inputDelay}");
                else
                    _playerControllers[i].ExecuteInputs(frame);
            }
        }
    }

    public void AddFrame(FrameData frame) {
        string json = JsonUtility.ToJson(frame);
        Utils.Log(this, "AddFrame", json);

        _currentPing = DateTime.Now.TimeOfDay.TotalMilliseconds - frame.time;

        while (_frameDatas[opponent].Count < frame.frameIndex + 1) {
            if (_frameDatas[opponent].Count > 0)
                _frameDatas[opponent].Add(_frameDatas[opponent].Last());
            else
                _frameDatas[opponent].Add(new FrameData(_currentFrameIndex, "None", DateTime.Now.TimeOfDay));
        }

        _frameDatas[opponent][frame.frameIndex] = frame;

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

    private FrameData GetCurrentFrame(int playerIndex) {
        List<FrameData> frames = _frameDatas[playerIndex];

        if (_currentFrameIndex >= frames.Count)
            return null;

        Utils.Log(this, "GetCurrentFrame", $"{_currentFrameIndex} / {frames.Count} / {frames[_currentFrameIndex] == null}", true);

        return frames[_currentFrameIndex];
    }

    private FrameData GetFrame(int playerIndex, int frameIndex) {
        List<FrameData> frames = _frameDatas[playerIndex];

        if (frames == null || frames.Count <= 0)
            return null;

        if (frameIndex >= frames.Count || frameIndex < 0)
            return null;

        return frames[frameIndex];
    }
}
