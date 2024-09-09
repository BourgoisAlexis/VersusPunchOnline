using System;
using System.Collections.Generic;

public class GameStateManager {
    //A list representing every players and a dictionnary for every inputs per player
    private List<Dictionary<int, InputMessage>> _inputs;
    private int _currentIndex;
    //Add event for gamestate change

    public int CurrentIndex => _currentIndex;
    public GameState State { get; private set; }

    private Action _executeInputs;

    public GameStateManager() {
        _inputs = new List<Dictionary<int, InputMessage>>();

        for (int i = 0; i < 4; i++)
            _inputs.Add(new Dictionary<int, InputMessage>());

        State = GameState.Default;

        GlobalManager.Instance.onCustomUpdate += Update;
        GlobalManager.Instance.NavigationManager.onLoadScene += Clear;
        GlobalManager.Instance.NavigationManager.onLoadScene += () => { ChangeGameState(GameState.Default); };
    }


    private void Update() {
        Utils.Log(this, $"=========={_currentIndex}==========");
        _executeInputs?.Invoke();
        _currentIndex++;

        for (int i = 0; i < _inputs.Count; i++)
            if (!_inputs[i].ContainsKey(_currentIndex))
                _inputs[i].Add(_currentIndex, new InputMessage(_currentIndex, i));
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
        return GetInput(playerIndex, _currentIndex);
    }

    public void AddInput(InputAction action, int playerIndex = 0) {
        _inputs[playerIndex][_currentIndex].AddInput(action, playerIndex);
    }

    //Synchronize first frame
    public void AddInputFromMessage(InputMessage input) {
        int playerIndex = input.playerIndex;
        int frameIndex = input.frameIndex;
        Dictionary<int, InputMessage> dic = _inputs[playerIndex];

        if (!dic.ContainsKey(frameIndex))
            dic.Add(frameIndex, input);
        else
            foreach (string s in input.inputs)
                _inputs[playerIndex][frameIndex].AddInput(s);

        int frameDiff = _currentIndex - frameIndex;
        double timeDiff = input.time - GetCurrentInput(input.playerIndex == 0 ? 1 : 0).time;

        Utils.Log(this, $"frame diff > {frameDiff} f | time diff > {timeDiff} ms");

        if (_currentIndex > AppConst.synchroDuration)
            return;

        if (frameDiff > 1)
            _currentIndex--;
    }

    private void Clear() {
        foreach (Dictionary<int, InputMessage> dic in _inputs)
            dic.Clear();

        _currentIndex = 0;
    }
}
