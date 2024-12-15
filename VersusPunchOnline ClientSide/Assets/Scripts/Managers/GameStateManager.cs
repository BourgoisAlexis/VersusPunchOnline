using System;
using System.Collections.Generic;

public class GameStateManager {
    public int CurrentIndex { get; private set; }
    public GameState State { get; private set; }

    //A list representing every players and a dictionnary for every inputs per player
    private List<Dictionary<int, InputMessage>> _inputs;
    private Action _executeInputs;


    public GameStateManager() {
        _inputs = new List<Dictionary<int, InputMessage>>();

        for (int i = 0; i < 4; i++)
            _inputs.Add(new Dictionary<int, InputMessage>());

        State = GameState.Default;

        GlobalManager.Instance.OnCustomUpdate += Update;
        GlobalManager.Instance.NavigationManager.OnLoad += Clear;
        GlobalManager.Instance.NavigationManager.OnLoad += () => { ChangeGameState(GameState.Default); };
    }


    private void Update() {
        if (GlobalManager.Instance.ShowLowPriorityLogs)
            Utils.Log(this, "Update", $"=========={CurrentIndex}==========");

        _executeInputs?.Invoke();
        CurrentIndex++;

        for (int i = 0; i < _inputs.Count; i++)
            if (!_inputs[i].ContainsKey(CurrentIndex))
                _inputs[i].Add(CurrentIndex, new InputMessage(CurrentIndex, i));
    }


    public void ChangeGameState(GameState state) {
        if (State == state)
            return;

        switch (state) {
            case GameState.Default:
                _executeInputs -= GlobalManager.Instance.InputManager.ProcessMainScreen;
                _executeInputs -= GlobalManager.Instance.InputManager.ProcessGameplay;
                break;

            case GameState.MainScreen:
                _executeInputs += GlobalManager.Instance.InputManager.ProcessMainScreen;
                _executeInputs -= GlobalManager.Instance.InputManager.ProcessGameplay;
                break;

            case GameState.Gameplay:
                _executeInputs -= GlobalManager.Instance.InputManager.ProcessMainScreen;
                _executeInputs += GlobalManager.Instance.InputManager.ProcessGameplay;
                break;
        }

        State = state;
    }

    public InputMessage GetInput(int playerIndex, int frameIndex) {
        if (_inputs == null || _inputs.Count <= 0)
            return null;

        if (!_inputs[playerIndex].ContainsKey(frameIndex))
            return null;

        return _inputs[playerIndex][frameIndex];
    }

    public InputMessage GetCurrentInput(int playerIndex) {
        return GetInput(playerIndex, CurrentIndex);
    }

    public void AddInput(InputAction action, int playerIndex = 0) {
        _inputs[playerIndex][CurrentIndex].AddInput(action, playerIndex);
    }

    //Synchronize first frame
    public void AddInputFromMessage(InputMessage input) {
        int playerIndex = input.PlayerIndex;
        int frameIndex = input.FrameIndex;
        Dictionary<int, InputMessage> dic = _inputs[playerIndex];

        if (!dic.ContainsKey(frameIndex))
            dic.Add(frameIndex, input);
        else
            foreach (string s in input.Inputs)
                _inputs[playerIndex][frameIndex].AddInput(s);

        int frameDiff = CurrentIndex - frameIndex;
        double timeDiff = input.Time - GetCurrentInput(input.PlayerIndex == 0 ? 1 : 0).Time;

        Utils.Log(this, "AddInputFromMessage", $"frame diff > {frameDiff} f | time diff > {timeDiff} ms");

        if (CurrentIndex > AppConst.SYNCHRO_DURATION)
            return;

        if (frameDiff > 1)
            CurrentIndex--;
    }

    private void Clear() {
        foreach (Dictionary<int, InputMessage> dic in _inputs)
            dic.Clear();

        CurrentIndex = 0;
    }
}
