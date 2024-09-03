using System;
using System.Collections.Generic;

public class GameStateManager {
    private Dictionary<int, FrameInfo> _frames;
    private int _currentIndex;
    //Add event for gamestate change

    public FrameInfo CurrentFrame => _frames[_currentIndex];
    public int CurrentIndex => _currentIndex;
    public GameState State { get; private set; }

    private Action _executeInputs;

    public GameStateManager() {
        _frames = new Dictionary<int, FrameInfo>();
        State = GameState.Default;

        GlobalManager.Instance.onCustomUpdate += Update;
        GlobalManager.Instance.NavigationManager.onLoadScene += Clear;
        GlobalManager.Instance.NavigationManager.onLoadScene += () => { ChangeGameState(GameState.Default); };
    }


    private void Update() {
        Utils.Log(this, $"=========={_currentIndex}==========");
        _executeInputs?.Invoke();
        _currentIndex++;
        _frames.Add(_currentIndex, new FrameInfo(_currentIndex));
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

    public FrameInfo GetFrameInfoAtIndex(int index) {
        if (_frames == null)
            return null;

        if (!_frames.ContainsKey(index))
            return null;

        return _frames[index];
    }

    public void AddInput(InputAction action, int playerIndex = 0) {
        CurrentFrame.AddInput(action, playerIndex);
    }

    private void Clear() {
        _frames.Clear();
        _currentIndex = 0;
    }
}
